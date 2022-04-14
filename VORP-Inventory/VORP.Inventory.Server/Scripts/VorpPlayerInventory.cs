using CitizenFX.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VORP.Inventory.Shared;
using VORP.Inventory.Shared.Models;
using VORP.Inventory.Server.Database;
using VORP.Inventory.Server.Extensions;
using VORP.Inventory.Server.Models;

namespace VORP.Inventory.Server.Scripts
{
    public class VorpPlayerInventory : Manager
    {
        PlayerList PlayerList => PluginManager.PlayerList;

        public static Dictionary<int, Dictionary<string, dynamic>> Pickups = new Dictionary<int, Dictionary<string, dynamic>>();
        public static Dictionary<int, Dictionary<string, dynamic>> PickupsMoney = new Dictionary<int, Dictionary<string, dynamic>>();

        public void Init()
        {
            AddEvent("vorpinventory:getItemsTable", new Action<Player>(OnGetItemsTableAsync));
            AddEvent("vorpinventory:getInventory", new Action<Player>(OnGetInventoryAsync));
            AddEvent("vorpinventory:serverGiveItem", new Action<Player, string, int, int>(OnServerGiveItemAsync));
            AddEvent("vorpinventory:serverGiveWeapon", new Action<Player, int, int>(OnServerGiveWeapon));
            AddEvent("vorpinventory:serverDropItem", new Action<Player, string, int>(OnServerDropItemAsync));
            AddEvent("vorpinventory:serverDropMoney", new Action<Player, double>(OnServerDropMoneyAsync));
            AddEvent("vorpinventory:serverDropAllMoney", new Action<Player>(OnServerDropAllMoneyAsync));
            AddEvent("vorpinventory:serverDropWeapon", new Action<Player, int>(OnServerDropWeapon));
            AddEvent("vorpinventory:sharePickupServer", new Action<string, int, int, Vector3, int>(OnSharePickupServer));
            AddEvent("vorpinventory:shareMoneyPickupServer", new Action<int, double, Vector3>(OnShareMoneyPickupServer));
            AddEvent("vorpinventory:onPickup", new Action<Player, int>(OnPickup));
            AddEvent("vorpinventory:onPickupMoney", new Action<Player, int>(OnPickupMoney));
            AddEvent("vorpinventory:setUsedWeapon", new Action<Player, int, bool, bool>(OnUsedWeapon));
            AddEvent("vorpinventory:setWeaponBullets", new Action<Player, int, string, int>(OnSetWeaponBullets));
            AddEvent("vorp_inventory:giveMoneyToPlayer", new Action<Player, int, double>(OnGiveMoneyToPlayerAsync));
            AddEvent("vorp_NewCharacter", new Action<int>(OnNewCharacter));
        }

        private async void OnNewCharacter(int playerId)
        {
            await Delay(5000);

            Player player = PlayerList[playerId];
            if (player == null)
            {
                Logger.Error($"Player '{playerId}' was not found.");
                return;
            }

            string identifier = "steam:" + player.Identifiers["steam"];

            // Attempt to add all starter items/weapons from the Config.json
            try
            {
                Dictionary<string, int> startItems = Configuration.Config.StartItems;

                foreach (KeyValuePair<string, int> item in startItems)
                {
                    TriggerEvent("vorpCore:addItem", playerId, item.Key, item.Value);
                }

                Dictionary<string, Dictionary<string, double>> startWeapons = Configuration.Config.StartWeapons;

                foreach (KeyValuePair<string, Dictionary<string, double>> weaponData in startWeapons)
                {
                    List<string> auxiliaryBullets = new List<string>();
                    Dictionary<string, int> receivedBullets = new Dictionary<string, int>();

                    Weapon weapon = Configuration.Config.Weapons.FirstOrDefault(x => x.HashName == weaponData.Key);

                    Dictionary<string, double> ammoHash = weapon.AmmoHash;
                    foreach (KeyValuePair<string, double> bullets in ammoHash)
                    {
                        auxiliaryBullets.Add(bullets.Key);
                    }

                    foreach (KeyValuePair<string, double> bullet in weaponData.Value)
                    {
                        if (auxiliaryBullets.Contains(bullet.Key))
                        {
                            receivedBullets.Add(bullet.Key, int.Parse(bullet.Value.ToString()));
                        }
                    }

                    TriggerEvent("vorpCore:registerWeapon", playerId, (object)weaponData.Key, receivedBullets);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"OnNewCharacter: {ex.Message}");
            }
        }

        private async void OnServerDropMoneyAsync([FromSource] Player player, double amount)
        {
            try
            {
                int _source = int.Parse(player.Handle);

                dynamic coreUserCharacter = await player.GetCoreUserCharacterAsync();
                if (coreUserCharacter == null)
                {
                    Logger.Error($"serverDropMoney: Player '{player}' CORE User does not exist.");
                    return;
                }

                double sourceMoney = coreUserCharacter.money;

                if (amount <= 0)
                {
                    player.TriggerEvent("vorp:TipRight", Configuration.GetTranslation("TryExploits"), 3000);
                }
                else if (sourceMoney < amount)
                {
                    player.TriggerEvent("vorp:TipRight", Configuration.GetTranslation("NotEnoughMoney"), 3000);
                }
                else
                {
                    coreUserCharacter.removeCurrency(0, amount);
                    player.TriggerEvent("vorpInventory:createMoneyPickup", amount);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"serverDropMoney");
            }
        }

        private async void OnServerDropAllMoneyAsync([FromSource] Player player)
        {
            try
            {
                int _source = int.Parse(player.Handle);

                dynamic coreUserCharacter = await player.GetCoreUserCharacterAsync();
                if (coreUserCharacter == null)
                {
                    Logger.Error($"serverDropAllMoney: Player '{player.Handle}' CORE User does not exist.");
                    return;
                }

                double sourceMoney = coreUserCharacter.money;

                if (sourceMoney > 0)
                {
                    coreUserCharacter.removeCurrency(0, sourceMoney);
                    Logger.Trace($"vorpInventory:createMoneyPickup({sourceMoney})");
                    player.TriggerEvent("vorpInventory:createMoneyPickup", sourceMoney);
                    await BaseScript.Delay(100);
                    player.TriggerEvent("vorp:inventory:ux:update", 0);
                    Logger.Trace($"vorp:inventory:ux:update({0})");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"serverDropAllMoney");
            }
        }

        private async void OnGiveMoneyToPlayerAsync([FromSource] Player player, int target, double amount)
        {
            try
            {
                Player _target = PlayerList[target];

                if (_target == null)
                {
                    Logger.Error($"giveMoneyToPlayer: Target Player '{_target} does not exist.");
                    return;
                }

                dynamic coreUserCharacter = await player.GetCoreUserCharacterAsync();
                if (coreUserCharacter == null)
                {
                    Logger.Error($"giveMoneyToPlayer: Player '{player.Handle}' CORE User does not exist.");
                    return;
                }

                if (!Common.HasProperty(coreUserCharacter, "money"))
                {
                    Logger.Error($"giveMoneyToPlayer: Player '{player.Handle}' CORE User Character missing property 'money'.");
                    return;
                }

                double sourceMoney = coreUserCharacter.money;

                if (amount <= 0)
                {
                    player.TriggerEvent("vorp:TipRight", Configuration.GetTranslation("TryExploits"), 3000);
                    await Delay(3000);
                    player.TriggerEvent("vorp_inventory:ProcessingReady");
                }
                else if (sourceMoney < amount)
                {
                    player.TriggerEvent("vorp:TipRight", Configuration.GetTranslation("NotEnoughMoney"), 3000);
                    await Delay(3000);
                    player.TriggerEvent("vorp_inventory:ProcessingReady");

                }
                else
                {
                    coreUserCharacter.removeCurrency(0, amount);
                    dynamic core = await Common.GetCoreUserAsync(target);
                    dynamic targetCoreUserCharacter = core.getUsedCharacter;

                    if (targetCoreUserCharacter == null)
                    {
                        Logger.Error($"giveMoneyToPlayer: Target Player '{target}' CORE User does not exist.");
                        return;
                    }

                    targetCoreUserCharacter.addCurrency(0, amount);
                    player.TriggerEvent("vorp:TipRight", string.Format(Configuration.GetTranslation("YouPaid"), amount.ToString(), _target.Name), 3000);
                    _target.TriggerEvent("vorp:TipRight", string.Format(Configuration.GetTranslation("YouReceived"), amount.ToString(), player.Name), 3000);
                    TriggerEvent("vorpinventory:moneylog", player.Handle, _target.Handle, amount);
                    await Delay(3000);
                    player.TriggerEvent("vorp_inventory:ProcessingReady");

                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "giveMoneyToPlayer");
            }
        }

        private void OnSetWeaponBullets([FromSource] Player player, int weaponId, string type, int bullet)
        {
            try
            {
                if (ItemDatabase.UserWeapons.ContainsKey(weaponId))
                {
                    ItemDatabase.UserWeapons[weaponId].setAmmo(bullet, type);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"setWeaponBullets");
            }
        }

        public async Task SaveInventoryItemsSupportAsync(string identifier, int coreUserCharacterId)
        {
            try
            {
                await Delay(0);

                bool result = await PluginManager.ScriptVorpCoreInventoryApi.SaveInventoryItemsSupportAsync(identifier, coreUserCharacterId);

                if (!result)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Method: SaveInventoryItemsSupport\n");
                    sb.Append("Message: Player inventory not saved\n");
                    sb.Append($"Player SteamID: {identifier}\n");
                    sb.Append($"Player CharacterId: {coreUserCharacterId}\n");
                    sb.Append($"If CharacterId = -1, then the Core did not return the character.\n");
                    sb.Append($"Inventory: {JsonConvert.SerializeObject(ItemDatabase.UserInventory[identifier])}");
                    Logger.Warn($"{sb}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"SaveInventoryItemsSupport");
            }
        }

        private void OnUsedWeapon([FromSource] Player source, int id, bool used, bool used2)
        {
            try
            {
                int Used = used ? 1 : 0;
                int Used2 = used2 ? 1 : 0;
                Exports["ghmattimysql"]
                    .execute(
                        $"UPDATE loadout SET used = ? , used2 = ? WHERE id=?",
                        new[] { Used, Used2, id });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"usedWeapon");
            }
        }

        //Sub items for other scripts
        private async Task SubItemAsync(int source, string name, int cuantity)
        {
            try
            {
                Player player = PlayerList[source];

                if (player == null)
                {
                    Logger.Error($"Player '{source}' does not exist.");
                    return;
                }

                string identifier = "steam:" + player.Identifiers["steam"];
                int coreUserCharacterId = await player.GetCoreUserCharacterIdAsync();

                Dictionary<string, ItemClass> userInventory = ItemDatabase.GetInventory(identifier);

                if (userInventory.ContainsKey(name))
                {
                    ItemClass itemClass = userInventory[name];
                    if (cuantity <= itemClass.getCount())
                    {
                        itemClass.Subtract(cuantity);
                        await SaveInventoryItemsSupportAsync(identifier, coreUserCharacterId);
                    }

                    if (itemClass.getCount() == 0)
                    {
                        userInventory.Remove(name);
                        await SaveInventoryItemsSupportAsync(identifier, coreUserCharacterId);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"subItem");
            }
        }

        //For other scripts add items
        private async void addItem(int source, string name, int cuantity)
        {
            try
            {
                Player player = PlayerList[source];

                if (player == null)
                {
                    Logger.Error($"Player '{source}' does not exist.");
                    return;
                }

                string identifier = "steam:" + player.Identifiers["steam"];
                int coreUserCharacterId = await player.GetCoreUserCharacterIdAsync();
                if (ItemDatabase.UserInventory.ContainsKey(identifier))
                {
                    if (ItemDatabase.UserInventory[identifier].ContainsKey(name))
                    {
                        if (cuantity > 0)
                        {
                            ItemDatabase.UserInventory[identifier][name].addCount(cuantity);
                            await SaveInventoryItemsSupportAsync(identifier, coreUserCharacterId);
                        }
                    }
                    else
                    {
                        if (ItemDatabase.ServerItems.ContainsKey(name))
                        {
                            ItemDatabase.UserInventory[identifier].Add(name, new ItemClass(cuantity, ItemDatabase.ServerItems[name].getLimit(),
                                ItemDatabase.ServerItems[name].getLabel(), name, "item_inventory", true, ItemDatabase.ServerItems[name].getCanRemove()));
                            await SaveInventoryItemsSupportAsync(identifier, coreUserCharacterId);
                        }
                    }
                }
                else
                {
                    Dictionary<string, ItemClass> userinv = new Dictionary<string, ItemClass>();
                    ItemDatabase.UserInventory.Add(identifier, userinv);
                    if (ItemDatabase.ServerItems.ContainsKey(name))
                    {
                        ItemDatabase.UserInventory[identifier].Add(name, new ItemClass(cuantity, ItemDatabase.ServerItems[name].getLimit(),
                            ItemDatabase.ServerItems[name].getLabel(), name, "item_inventory", true, ItemDatabase.ServerItems[name].getCanRemove()));
                        await SaveInventoryItemsSupportAsync(identifier, coreUserCharacterId);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"addItem: Possible player dropped?");
            }
        }

        private async void AddWeaponAsync(int source, int weapId)
        {
            try
            {
                Player player = PlayerList[source];

                if (player == null)
                {
                    Logger.Error($"Player '{source}' does not exist.");
                    return;
                }

                string identifier = "steam:" + player.Identifiers["steam"];
                if (ItemDatabase.UserWeapons.ContainsKey(weapId))
                {
                    ItemDatabase.UserWeapons[weapId].setPropietary(identifier);

                    dynamic coreUserCharacter = await player.GetCoreUserCharacterAsync();
                    if (coreUserCharacter == null)
                    {
                        Logger.Error($"addWeapon: Player '{player.Handle}' CORE User does not exist.");
                        return;
                    }

                    int charIdentifier = coreUserCharacter.charIdentifier;
                    Exports["ghmattimysql"]
                        .execute(
                            $"UPDATE loadout SET identifier = '{ItemDatabase.UserWeapons[weapId].getPropietary()}', charidentifier = '{charIdentifier}' WHERE id=?",
                            new[] { weapId });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"addWeapon");
            }
        }

        private async void SubtractWeaponAsync(int source, int weapId)
        {
            try
            {
                Player player = PlayerList[source];

                if (player == null)
                {
                    Logger.Error($"Player '{source}' does not exist.");
                    return;
                }

                string identifier = "steam:" + player.Identifiers["steam"];
                if (ItemDatabase.UserWeapons.ContainsKey(weapId))
                {
                    ItemDatabase.UserWeapons[weapId].setPropietary("");

                    dynamic coreUserCharacter = await player.GetCoreUserCharacterAsync();
                    if (coreUserCharacter == null)
                    {
                        Logger.Error($"subWeapon: Player '{player.Handle}' CORE User does not exist.");
                        return;
                    }

                    int charIdentifier = coreUserCharacter.charIdentifier;
                    Exports["ghmattimysql"]
                        .execute(
                            $"UPDATE loadout SET identifier = '{ItemDatabase.UserWeapons[weapId].getPropietary()}', charidentifier = '{charIdentifier}' WHERE id=?",
                            new[] { weapId });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"subWeapon");
            }
        }

        private async void OnPickup([FromSource] Player player, int obj)
        {
            try
            {
                string identifier = "steam:" + player.Identifiers["steam"];
                int source = int.Parse(player.Handle);

                dynamic coreUserCharacter = await player.GetCoreUserCharacterAsync();
                if (coreUserCharacter == null)
                {
                    Logger.Error($"onPickup: Player '{player.Handle}' CORE User does not exist.");
                    return;
                }

                int charIdentifier = coreUserCharacter.charIdentifier;
                if (Pickups.ContainsKey(obj))
                {
                    if (Pickups[obj]["weaponid"] == 1)
                    {
                        if (ItemDatabase.UserInventory.ContainsKey(identifier))
                        {

                            if (ItemDatabase.ServerItems[Pickups[obj]["name"]].getLimit() != -1)
                            {
                                if (ItemDatabase.UserInventory[identifier].ContainsKey(Pickups[obj]["name"]))
                                {
                                    int totalcount = Pickups[obj]["amount"] + ItemDatabase.UserInventory[identifier][Pickups[obj]["name"]].getCount();

                                    if (ItemDatabase.ServerItems[Pickups[obj]["name"]].getLimit() < totalcount)
                                    {
                                        TriggerClientEvent(player, "vorp:TipRight", Configuration.GetTranslation("fullInventory"), 2000);
                                        return;
                                    }
                                }
                                //int totalcount = Pickups[obj]["amount"] ItemDatabase.usersInventory[identifier];
                                //totalcount += Pickups[obj]["amount"];
                                //ItemDatabase.svItems[Pickups[obj]["name"]].getCount();

                            }

                            if (Configuration.INVENTORY_MAX_ITEMS != 0)
                            {
                                int totalcount = VorpCoreInventoryAPI.GetTotalAmountOfItems(identifier);
                                totalcount += Pickups[obj]["amount"];
                                if (totalcount <= Configuration.INVENTORY_MAX_ITEMS)
                                {
                                    addItem(source, Pickups[obj]["name"], Pickups[obj]["amount"]);
                                    TriggerClientEvent("vorpInventory:sharePickupClient", Pickups[obj]["name"], Pickups[obj]["obj"],
                                        Pickups[obj]["amount"], Pickups[obj]["coords"], 2, Pickups[obj]["weaponid"]);
                                    TriggerClientEvent("vorpInventory:removePickupClient", Pickups[obj]["obj"]);
                                    player.TriggerEvent("vorpinventory:receiveItem", Pickups[obj]["name"], Pickups[obj]["amount"]);
                                    player.TriggerEvent("vorpInventory:playerAnim", obj);
                                    Pickups.Remove(obj);
                                }
                                else
                                {
                                    TriggerClientEvent(player, "vorp:TipRight", Configuration.GetTranslation("fullInventory"), 2000);
                                }
                            }
                            else
                            {
                                addItem(source, Pickups[obj]["name"], Pickups[obj]["amount"]);
                                TriggerClientEvent("vorpInventory:sharePickupClient", Pickups[obj]["name"], Pickups[obj]["obj"],
                                    Pickups[obj]["amount"], Pickups[obj]["coords"], 2, Pickups[obj]["weaponid"]);
                                TriggerClientEvent("vorpInventory:removePickupClient", Pickups[obj]["obj"]);
                                player.TriggerEvent("vorpinventory:receiveItem", Pickups[obj]["name"], Pickups[obj]["amount"]);
                                player.TriggerEvent("vorpInventory:playerAnim", obj);
                                Pickups.Remove(obj);
                            }

                        }
                    }
                    else
                    {
                        if (Configuration.INVENTORY_MAX_WEAPONS != 0)
                        {
                            int totalcount = VorpCoreInventoryAPI.getUserTotalCountWeapons(identifier, charIdentifier);
                            totalcount += 1;
                            if (totalcount <= Configuration.INVENTORY_MAX_WEAPONS)
                            {
                                int weaponId = Pickups[obj]["weaponid"];
                                AddWeaponAsync(source, Pickups[obj]["weaponid"]);
                                TriggerEvent("syn_weapons:onpickup", Pickups[obj]["weaponid"]);
                                TriggerClientEvent("vorpInventory:sharePickupClient", Pickups[obj]["name"], Pickups[obj]["obj"],
                                    Pickups[obj]["amount"], Pickups[obj]["coords"], 2, Pickups[obj]["weaponid"]);
                                TriggerClientEvent("vorpInventory:removePickupClient", Pickups[obj]["obj"]);
                                player.TriggerEvent("vorpinventory:receiveWeapon", weaponId, ItemDatabase.UserWeapons[weaponId].getPropietary(),
                                    ItemDatabase.UserWeapons[weaponId].getName(), ItemDatabase.UserWeapons[weaponId].getAllAmmo(), ItemDatabase.UserWeapons[weaponId].getAllComponents());
                                player.TriggerEvent("vorpInventory:playerAnim", obj);
                                Pickups.Remove(obj);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"onPickup");
            }
        }

        private void OnPickupMoney([FromSource] Player player, int obj)
        {
            try
            {
                string identifier = "steam:" + player.Identifiers["steam"];
                int source = int.Parse(player.Handle);
                if (PickupsMoney.ContainsKey(obj))
                {
                    TriggerClientEvent("vorpInventory:shareMoneyPickupClient", PickupsMoney[obj]["obj"],
                    PickupsMoney[obj]["amount"], PickupsMoney[obj]["coords"], 2);
                    TriggerClientEvent("vorpInventory:removePickupClient", PickupsMoney[obj]["obj"]);
                    player.TriggerEvent("vorpInventory:playerAnim", obj);
                    TriggerEvent("vorp:addMoney", source, 0, PickupsMoney[obj]["amount"]);

                    if (!PickupsMoney.ContainsKey(obj)) return;
                    PickupsMoney.Remove(obj);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"onPickupMoney");
            }
        }

        private void OnSharePickupServer(string name, int obj, int amount, Vector3 position, int weaponId)
        {
            try
            {
                TriggerClientEvent("vorpInventory:sharePickupClient", name, obj, amount, position, 1, weaponId);
                Pickups.Add(obj, new Dictionary<string, dynamic>
                {
                    ["name"] = name,
                    ["obj"] = obj,
                    ["amount"] = amount,
                    ["weaponid"] = weaponId,
                    ["inRange"] = false,
                    ["coords"] = position
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"sharePickupServer");
            }
        }

        // is obj a networkid ?
        private void OnShareMoneyPickupServer(int obj, double amount, Vector3 position)
        {
            try
            {
                if (PickupsMoney.ContainsKey(obj)) return; // don't add or do anything, if it already exists

                TriggerClientEvent("vorpInventory:shareMoneyPickupClient", obj, amount, position, 1);
                PickupsMoney.Add(obj, new Dictionary<string, dynamic>
                {
                    ["name"] = "Dollars",
                    ["obj"] = obj,
                    ["amount"] = amount,
                    ["inRange"] = false,
                    ["coords"] = position
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"shareMoneyPickupServer");
            }
        }

        //Weapon methods
        private void OnServerDropWeapon([FromSource] Player source, int weaponId)
        {
            try
            {
                SubtractWeaponAsync(int.Parse(source.Handle), weaponId);
                source.TriggerEvent("vorpInventory:createPickup", ItemDatabase.UserWeapons[weaponId].getName(), 1, weaponId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"serverDropWeapon");
            }
        }

        //Items methods
        private async void OnServerDropItemAsync([FromSource] Player source, string itemname, int cuantity)
        {
            try
            {
                await SubItemAsync(int.Parse(source.Handle), itemname, cuantity);
                source.TriggerEvent("vorpInventory:createPickup", itemname, cuantity, 1);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"serverDropItem");
            }
        }

        private void OnServerGiveWeapon([FromSource] Player player, int weaponId, int target)
        {
            try
            {
                Player targetPlayer = PlayerList[target];

                if (targetPlayer == null)
                {
                    Logger.Error($"Target Player '{target}' does not exist.");
                    return;
                }

                string identifier = "steam:" + player.Identifiers["steam"];

                if (ItemDatabase.UserWeapons.ContainsKey(weaponId))
                {
                    SubtractWeaponAsync(int.Parse(player.Handle), weaponId);
                    AddWeaponAsync(int.Parse(targetPlayer.Handle), weaponId);
                    targetPlayer.TriggerEvent("vorpinventory:receiveWeapon", weaponId, ItemDatabase.UserWeapons[weaponId].getPropietary(),
                        ItemDatabase.UserWeapons[weaponId].getName(), ItemDatabase.UserWeapons[weaponId].getAllAmmo(), ItemDatabase.UserWeapons[weaponId].getAllComponents());
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"serverGiveWeapon");
            }
        }
        private async void OnServerGiveItemAsync([FromSource] Player player, string itemName, int amount, int targetHandle)
        {
            try
            {
                Player targetPlayer = PlayerList[targetHandle];

                if (targetPlayer == null)
                {
                    Logger.Error($"ServerGiveItem: Target Player '{targetHandle}' does not exist.");
                    return;
                }

                string identifier = "steam:" + player.Identifiers["steam"];
                string targetIdentifier = "steam:" + targetPlayer.Identifiers["steam"];

                int playerCharId = await player.GetCoreUserCharacterIdAsync();

                if (playerCharId == -1)
                {
                    Logger.Error($"ServerGiveItem: User '{player.Name}#{player.Handle}' character was not found.");
                    return;
                }

                int targetCharId = await targetPlayer.GetCoreUserCharacterIdAsync();

                if (targetCharId == -1)
                {
                    Logger.Error($"ServerGiveItem: User '{targetPlayer.Name}#{targetPlayer.Handle}' character was not found.");
                    return;
                }

                Dictionary<string, ItemClass> userInventory = ItemDatabase.GetInventory(identifier);

                if (userInventory is null)
                {
                    Logger.Error($"ServerGiveItem: User '{player.Name}#{player.Handle}' inventory was not found.");
                    return;
                }

                Dictionary<string, ItemClass> targetInventory = ItemDatabase.GetInventory(targetIdentifier);

                if (targetInventory is null)
                {
                    Logger.Error($"ServerGiveItem: Target User '{targetPlayer.Name}#{targetPlayer.Handle}' inventory was not found.");
                    return;
                }

                if (!userInventory.ContainsKey(itemName))
                {
                    TriggerClientEvent(player, "vorp:TipRight", Configuration.GetTranslation("itemerror"), 2000);
                    Logger.Error($"ServerGiveItem: User '{player.Name}#{player.Handle}' inventory item '{itemName}' was not found.");
                    return;
                }

                ItemClass item = userInventory[itemName];
                int itemCount = item.getCount();

                int targetTotalItems = 0;
                int targetItemLimit = 0;
                int targetTotalCountOfItems = VorpCoreInventoryAPI.GetTotalAmountOfItems(targetIdentifier);

                bool canGiveItemToTarget = true;

                ItemClass targetItem = null;
                if (targetInventory.ContainsKey(itemName))
                {
                    targetItem = targetInventory[itemName];
                    targetTotalItems = targetItem.getCount();
                    targetItemLimit = targetItem.getLimit();

                    if (targetTotalItems + amount >= targetItemLimit)
                        canGiveItemToTarget = false;
                }

                int newTotalAmount = targetTotalCountOfItems + amount;
                if (newTotalAmount > Configuration.INVENTORY_MAX_ITEMS)
                    canGiveItemToTarget = false;

                if (!canGiveItemToTarget)
                {
                    TriggerClientEvent(player, "vorp:TipRight", Configuration.GetTranslation("fullInventoryGive"), 2000);
                    TriggerClientEvent(targetPlayer, "vorp:TipRight", Configuration.GetTranslation("fullInventory"), 2000);
                    return;
                }

                if (targetItem is not null)
                {
                    targetItem.addCount(amount);
                }
                else
                {
                    if (ItemDatabase.ServerItems.ContainsKey(itemName))
                    {
                        Items serverItem = ItemDatabase.ServerItems[itemName];
                        targetInventory.Add(itemName, new ItemClass(amount, serverItem.getLimit(), serverItem.getLabel(), itemName, "item_inventory", true, serverItem.getCanRemove()));
                    }
                    else
                    {
                        Logger.Error($"ServerGiveItem: Server items does not contain '{itemName}'.");
                        return;
                    }
                }

                await SaveInventoryItemsSupportAsync(targetIdentifier, targetCharId);

                item.Subtract(amount);

                if (item.getCount() == 0)
                    userInventory.Remove(itemName);

                await SaveInventoryItemsSupportAsync(identifier, playerCharId);

                targetPlayer.TriggerEvent("vorpinventory:receiveItem", itemName, amount);
                player.TriggerEvent("vorpinventory:receiveItem2", itemName, amount);
                TriggerClientEvent(player, "vorp:TipRight", Configuration.GetTranslation("yougaveitem"), 2000);
                TriggerClientEvent(targetPlayer, "vorp:TipRight", Configuration.GetTranslation("YouReceiveditem"), 2000);
                TriggerEvent("vorpinventory:itemlog", player.Handle, targetPlayer.Handle, itemName, amount);

            }
            catch (Exception ex)
            {
                Logger.Error("ServerGiveItem: if NullReferenceException, possible player steam identity failed");
                Logger.Error(ex, $"ServerGiveItem");
            }
        }

        private async void OnGetItemsTableAsync([FromSource] Player source)
        {
            try
            {
                Logger.Trace($"Player '{source.Name}' requested Items Table.");

                // must have a better way
                int attempts = 0;
                while (PluginManager.ItemsDB.items is null)
                {
                    if (attempts > 10)
                    {
                        Logger.CriticalError($"Failed to generate Items table, possible there are no items in the database?");
                        break;
                    }

                    attempts++;
                    await BaseScript.Delay(500);
                }

                if (PluginManager.ItemsDB.items.Count > 0)
                {
                    source.TriggerEvent("vorpInventory:giveItemsTable", PluginManager.ItemsDB.items);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"getItemsTable");
            }
        }

        private async void OnGetInventoryAsync([FromSource] Player player)
        {
            try
            {
                Logger.Trace($"OnGetInventoryAsync: Player '{player.Name}' requested their Inventory.");

                string identifier = "steam:" + player.Identifiers["steam"];
                dynamic coreUserCharacter = await player.GetCoreUserCharacterAsync();

                if (coreUserCharacter == null)
                {
                    Logger.Error($"getInventory: Core User '{player.Handle}' could not be found.");
                    return;
                }

                int charIdentifier = coreUserCharacter.charIdentifier;
                string inventory = coreUserCharacter.inventory;

                Dictionary<string, ItemClass> userinv = new Dictionary<string, ItemClass>();
                List<WeaponClass> userwep = new List<WeaponClass>();

                if (!PluginManager.ActiveCharacters.ContainsKey(player.Handle))
                    PluginManager.ActiveCharacters.Add(player.Handle, charIdentifier);

                if (PluginManager.ActiveCharacters[player.Handle] != charIdentifier)
                    PluginManager.ActiveCharacters[player.Handle] = charIdentifier;

                if (inventory is not null)
                {
                    // turn this into a class
                    dynamic coreInventory = JsonConvert.DeserializeObject<dynamic>(inventory);
                    // turn this into a class, its horrible like this, nothing means anything
                    foreach (dynamic itemname in PluginManager.ItemsDB.items)
                    {
                        if (coreInventory[itemname.item.ToString()] != null)
                        {
                            ItemClass item = new ItemClass(int.Parse(coreInventory[itemname.item.ToString()].ToString()), int.Parse(itemname.limit.ToString()),
                                itemname.label, itemname.item, itemname.type, itemname.usable, itemname.can_remove);
                            userinv.Add(itemname.item.ToString(), item);
                        }
                    }
                }

                if (!ItemDatabase.UserInventory.ContainsKey(identifier))
                {
                    ItemDatabase.UserInventory.Add(identifier, userinv);
                }
                ItemDatabase.UserInventory[identifier] = userinv;

                Logger.Trace($"OnGetInventoryAsync[{identifier}]: {inventory}");

                player.TriggerEvent("vorpInventory:giveInventory", inventory);

                Exports["ghmattimysql"].execute("SELECT * FROM loadout WHERE `identifier` = ? AND `charidentifier` = ?;", new object[] { identifier, charIdentifier }, new Action<dynamic>((weaponsinvento) =>
                {
                    if (weaponsinvento.Count > 0)
                    {
                        WeaponClass wp;
                        foreach (var row in weaponsinvento)
                        {
                            JObject ammo = JsonConvert.DeserializeObject(row.ammo.ToString());
                            JArray comp = JsonConvert.DeserializeObject(row.components.ToString());
                            Dictionary<string, int> amunition = new Dictionary<string, int>();
                            List<string> components = new List<string>();
                            foreach (JProperty ammos in ammo.Properties())
                            {
                                amunition.Add(ammos.Name, int.Parse(ammos.Value.ToString()));
                            }
                            foreach (JToken x in comp)
                            {
                                components.Add(x.ToString());
                            }

                            bool auused = false;
                            if (row.used == 1)
                            {
                                auused = true;
                            }

                            bool auused2 = false;
                            if (row.used2 == 1)
                            {
                                auused2 = true;
                            }
                            wp = new WeaponClass(int.Parse(row.id.ToString()), row.identifier.ToString(), row.name.ToString(), amunition, components, auused, auused2, charIdentifier);
                            ItemDatabase.UserWeapons[wp.getId()] = wp;
                        }

                        Logger.Trace($"OnGetInventoryAsync[{identifier}]: {weaponsinvento}");

                        // is there something wrong with returning an empty list?
                        player.TriggerEvent("vorpInventory:giveLoadout", weaponsinvento);
                    }

                }));
            }
            catch (NullReferenceException nEX)
            {
                Logger.Error(nEX, $"getInventory: Player dropped or connecting?");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"getInventory");
            }
        }
    }
}
