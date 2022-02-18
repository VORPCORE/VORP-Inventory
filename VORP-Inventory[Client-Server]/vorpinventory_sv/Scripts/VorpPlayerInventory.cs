using CitizenFX.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VorpInventory.Database;
using VorpInventory.Diagnostics;
using VorpInventory.Extensions;
using VorpInventory.Models;

namespace VorpInventory.Scripts
{
    public class VorpPlayerInventory : BaseScript
    {
        PlayerList PlayerList => PluginManager.PlayerList;

        public static Dictionary<int, Dictionary<string, dynamic>> Pickups = new Dictionary<int, Dictionary<string, dynamic>>();
        public static Dictionary<int, Dictionary<string, dynamic>> PickupsMoney = new Dictionary<int, Dictionary<string, dynamic>>();

        internal VorpPlayerInventory()
        {
            EventHandlers["vorpinventory:getItemsTable"] += new Action<Player>(getItemsTable);
            EventHandlers["vorpinventory:getInventory"] += new Action<Player>(getInventory);
            EventHandlers["vorpinventory:serverGiveItem"] += new Action<Player, string, int, int>(ServerGiveItem);
            EventHandlers["vorpinventory:serverGiveWeapon"] += new Action<Player, int, int>(serverGiveWeapon);
            EventHandlers["vorpinventory:serverDropItem"] += new Action<Player, string, int>(serverDropItem);
            EventHandlers["vorpinventory:serverDropMoney"] += new Action<Player, double>(serverDropMoney);
            EventHandlers["vorpinventory:serverDropAllMoney"] += new Action<Player>(serverDropAllMoney);
            EventHandlers["vorpinventory:serverDropWeapon"] += new Action<Player, int>(serverDropWeapon);
            EventHandlers["vorpinventory:sharePickupServer"] += new Action<string, int, int, Vector3, int>(sharePickupServer);
            EventHandlers["vorpinventory:shareMoneyPickupServer"] += new Action<int, double, Vector3>(shareMoneyPickupServer);
            EventHandlers["vorpinventory:onPickup"] += new Action<Player, int>(onPickup);
            EventHandlers["vorpinventory:onPickupMoney"] += new Action<Player, int>(onPickupMoney);
            EventHandlers["vorpinventory:setUsedWeapon"] += new Action<Player, int, bool, bool>(usedWeapon);
            EventHandlers["vorpinventory:setWeaponBullets"] += new Action<Player, int, string, int>(setWeaponBullets);
            EventHandlers["vorp_inventory:giveMoneyToPlayer"] += new Action<Player, int, double>(giveMoneyToPlayer);
        }

        private async void serverDropMoney([FromSource] Player player, double amount)
        {try
            {
                int _source = int.Parse(player.Handle);

                dynamic coreUserCharacter = player.GetCoreUserCharacter();
                if (coreUserCharacter == null)
                {
                    Logger.Error($"serverDropMoney: Player '{player}' CORE User does not exist.");
                    return;
                }

                double sourceMoney = coreUserCharacter.money;

                if (amount <= 0)
                {
                    player.TriggerEvent("vorp:TipRight", Config.lang["TryExploits"], 3000);
                }
                else if (sourceMoney < amount)
                {
                    player.TriggerEvent("vorp:TipRight", Config.lang["NotEnoughMoney"], 3000);
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

        private async void serverDropAllMoney([FromSource] Player player)
        {
            try
            {
                int _source = int.Parse(player.Handle);

                dynamic coreUserCharacter = player.GetCoreUserCharacter();
                if (coreUserCharacter == null)
                {
                    Logger.Error($"serverDropAllMoney: Player '{player.Handle}' CORE User does not exist.");
                    return;
                }

                double sourceMoney = coreUserCharacter.money;

                if (sourceMoney > 0)
                {
                    coreUserCharacter.removeCurrency(0, sourceMoney);
                    player.TriggerEvent("vorpInventory:createMoneyPickup", sourceMoney);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"serverDropAllMoney");
            }
        }

        private async void giveMoneyToPlayer([FromSource] Player player, int target, double amount)
        {
            try
            {
                Player _target = PlayerList[target];

                if (_target == null)
                {
                    Logger.Error($"giveMoneyToPlayer: Target Player '{_target} does not exist.");
                    return;
                }

                dynamic coreUserCharacter = player.GetCoreUserCharacter();
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
                    player.TriggerEvent("vorp:TipRight", Config.lang["TryExploits"], 3000);
                    await Delay(3000);
                    player.TriggerEvent("vorp_inventory:ProcessingReady");
                }
                else if (sourceMoney < amount)
                {
                    player.TriggerEvent("vorp:TipRight", Config.lang["NotEnoughMoney"], 3000);
                    await Delay(3000);
                    player.TriggerEvent("vorp_inventory:ProcessingReady");

                }
                else
                {
                    coreUserCharacter.removeCurrency(0, amount);
                    dynamic targetCoreUserCharacter = PluginManager.CORE.getUser(target).getUsedCharacter;

                    if (targetCoreUserCharacter == null)
                    {
                        Logger.Error($"giveMoneyToPlayer: Target Player '{target}' CORE User does not exist.");
                        return;
                    }

                    targetCoreUserCharacter.addCurrency(0, amount);
                    player.TriggerEvent("vorp:TipRight", string.Format(Config.lang["YouPaid"], amount.ToString(), _target.Name), 3000);
                    _target.TriggerEvent("vorp:TipRight", string.Format(Config.lang["YouReceived"], amount.ToString(), player.Name), 3000);
                    await Delay(3000);
                    player.TriggerEvent("vorp_inventory:ProcessingReady");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "giveMoneyToPlayer");
            }
        }

        private void setWeaponBullets([FromSource] Player player, int weaponId, string type, int bullet)
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

        public async Task SaveInventoryItemsSupport(string identifier, int coreUserCharacterId)
        {
            try
            {
                await Delay(0);

                bool result = await PluginManager._scriptVorpCoreInventoryApi.SaveInventoryItemsSupport(identifier, coreUserCharacterId);

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

        private void usedWeapon([FromSource] Player source, int id, bool used, bool used2)
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
        private async void subItem(int source, string name, int cuantity)
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
                int coreUserCharacterId = player.GetCoreUserCharacterId();

                Dictionary<string, ItemClass> userInventory = ItemDatabase.GetInventory(identifier);

                if (userInventory.ContainsKey(name))
                {
                    ItemClass itemClass = userInventory[name];
                    if (cuantity <= itemClass.getCount())
                    {
                        itemClass.Subtract(cuantity);
                        await SaveInventoryItemsSupport(identifier, coreUserCharacterId);
                    }

                    if (itemClass.getCount() == 0)
                    {
                        userInventory.Remove(name);
                        await SaveInventoryItemsSupport(identifier, coreUserCharacterId);
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
                int coreUserCharacterId = player.GetCoreUserCharacterId();
                if (ItemDatabase.UserInventory.ContainsKey(identifier))
                {
                    if (ItemDatabase.UserInventory[identifier].ContainsKey(name))
                    {
                        if (cuantity > 0)
                        {
                            ItemDatabase.UserInventory[identifier][name].addCount(cuantity);
                            await SaveInventoryItemsSupport(identifier, coreUserCharacterId);
                        }
                    }
                    else
                    {
                        if (ItemDatabase.ServerItems.ContainsKey(name))
                        {
                            ItemDatabase.UserInventory[identifier].Add(name, new ItemClass(cuantity, ItemDatabase.ServerItems[name].getLimit(),
                                ItemDatabase.ServerItems[name].getLabel(), name, "item_inventory", true, ItemDatabase.ServerItems[name].getCanRemove()));
                            await SaveInventoryItemsSupport(identifier, coreUserCharacterId);
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
                        await SaveInventoryItemsSupport(identifier, coreUserCharacterId);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"addItem: Possible player dropped?");
            }
        }

        private async void addWeapon(int source, int weapId)
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

                    dynamic coreUserCharacter = player.GetCoreUserCharacter();
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

        private async void subWeapon(int source, int weapId)
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

                    dynamic coreUserCharacter = player.GetCoreUserCharacter();
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

        private async void onPickup([FromSource] Player player, int obj)
        {
            try
            {
                string identifier = "steam:" + player.Identifiers["steam"];
                int source = int.Parse(player.Handle);

                dynamic coreUserCharacter = player.GetCoreUserCharacter();
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
                                        TriggerClientEvent(player, "vorp:TipRight", Config.lang["fullInventory"], 2000);
                                        return;
                                    }
                                }
                                //int totalcount = Pickups[obj]["amount"] ItemDatabase.usersInventory[identifier];
                                //totalcount += Pickups[obj]["amount"];
                                //ItemDatabase.svItems[Pickups[obj]["name"]].getCount();

                            }

                            if (Config.MaxItems != 0)
                            {
                                int totalcount = VorpCoreInventoryAPI.GetTotalAmountOfItems(identifier);
                                totalcount += Pickups[obj]["amount"];
                                if (totalcount <= Config.MaxItems)
                                {
                                    addItem(source, Pickups[obj]["name"], Pickups[obj]["amount"]);
                                    Debug.WriteLine($"añado {Pickups[obj]["amount"]}");
                                    TriggerClientEvent("vorpInventory:sharePickupClient", Pickups[obj]["name"], Pickups[obj]["obj"],
                                        Pickups[obj]["amount"], Pickups[obj]["coords"], 2, Pickups[obj]["weaponid"]);
                                    TriggerClientEvent("vorpInventory:removePickupClient", Pickups[obj]["obj"]);
                                    player.TriggerEvent("vorpinventory:receiveItem", Pickups[obj]["name"], Pickups[obj]["amount"]);
                                    player.TriggerEvent("vorpInventory:playerAnim", obj);
                                    Pickups.Remove(obj);
                                }
                                else
                                {
                                    TriggerClientEvent(player, "vorp:TipRight", Config.lang["fullInventory"], 2000);
                                }
                            }
                            else
                            {
                                addItem(source, Pickups[obj]["name"], Pickups[obj]["amount"]);
                                Debug.WriteLine($"añado {Pickups[obj]["amount"]}");
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
                        if (Config.MaxWeapons != 0)
                        {
                            int totalcount = VorpCoreInventoryAPI.getUserTotalCountWeapons(identifier, charIdentifier);
                            totalcount += 1;
                            if (totalcount <= Config.MaxWeapons)
                            {
                                int weaponId = Pickups[obj]["weaponid"];
                                addWeapon(source, Pickups[obj]["weaponid"]);
                                TriggerEvent("syn_weapons:onpickup", Pickups[obj]["weaponid"]);
                                //Debug.WriteLine($"añado {ItemDatabase.userWeapons[Pickups[obj]["weaponid"].ToString()].getPropietary()}");
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

        private void onPickupMoney([FromSource] Player player, int obj)
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

        private void sharePickupServer(string name, int obj, int amount, Vector3 position, int weaponId)
        {
            try
            {
                TriggerClientEvent("vorpInventory:sharePickupClient", name, obj, amount, position, 1, weaponId);
                Debug.WriteLine(obj.ToString());
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
        private void shareMoneyPickupServer(int obj, double amount, Vector3 position)
        {
            try
            {
                if (PickupsMoney.ContainsKey(obj)) return; // don't add or do anything, if it already exists

                TriggerClientEvent("vorpInventory:shareMoneyPickupClient", obj, amount, position, 1);
                Debug.WriteLine(obj.ToString());

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
        private void serverDropWeapon([FromSource] Player source, int weaponId)
        {
            try
            {
                subWeapon(int.Parse(source.Handle), weaponId);
                source.TriggerEvent("vorpInventory:createPickup", ItemDatabase.UserWeapons[weaponId].getName(), 1, weaponId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"serverDropWeapon");
            }
        }

        //Items methods
        private void serverDropItem([FromSource] Player source, string itemname, int cuantity)
        {
            try
            {
                subItem(int.Parse(source.Handle), itemname, cuantity);
                source.TriggerEvent("vorpInventory:createPickup", itemname, cuantity, 1);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"serverDropItem");
            }
        }

        private void serverGiveWeapon([FromSource] Player player, int weaponId, int target)
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
                    subWeapon(int.Parse(player.Handle), weaponId);
                    addWeapon(int.Parse(targetPlayer.Handle), weaponId);
                    targetPlayer.TriggerEvent("vorpinventory:receiveWeapon", weaponId, ItemDatabase.UserWeapons[weaponId].getPropietary(),
                        ItemDatabase.UserWeapons[weaponId].getName(), ItemDatabase.UserWeapons[weaponId].getAllAmmo(), ItemDatabase.UserWeapons[weaponId].getAllComponents());
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"serverGiveWeapon");
            }
        }
        private void ServerGiveItem([FromSource] Player player, string itemname, int amount, int target)
        {
            try
            {
                bool give = true;

                Player targetPlayer = PlayerList[target];

                if (targetPlayer == null)
                {
                    Logger.Error($"Target Player '{target}' does not exist.");
                    return;
                }

                string identifier = "steam:" + player.Identifiers["steam"];
                string targetIdentifier = "steam:" + targetPlayer.Identifiers["steam"];

                if (ItemDatabase.UserInventory.ContainsKey(identifier) && ItemDatabase.UserInventory.ContainsKey(targetIdentifier) && ItemDatabase.UserInventory[identifier].ContainsKey(itemname))
                {
                    if (ItemDatabase.UserInventory[identifier][itemname].getCount() >= amount)
                    {
                        if (ItemDatabase.UserInventory[targetIdentifier].ContainsKey(itemname))
                        {
                            if (ItemDatabase.UserInventory[targetIdentifier][itemname].getCount() + amount
                                >= ItemDatabase.UserInventory[targetIdentifier][itemname].getLimit())
                            {
                                give = false;
                            }

                        }
                        int totalcount = VorpCoreInventoryAPI.GetTotalAmountOfItems(targetIdentifier);
                        totalcount += amount;
                        if (totalcount > Config.MaxItems)

                        {
                            give = false;
                        }

                        if (give)
                        {
                            addItem(int.Parse(targetPlayer.Handle), itemname, amount);
                            subItem(int.Parse(player.Handle), itemname, amount);
                            targetPlayer.TriggerEvent("vorpinventory:receiveItem", itemname, amount);
                            player.TriggerEvent("vorpinventory:receiveItem2", itemname, amount);
                            TriggerClientEvent(player, "vorp:TipRight", Config.lang["yougaveitem"], 2000);
                            TriggerClientEvent(targetPlayer, "vorp:TipRight", Config.lang["YouReceiveditem"], 2000);
                        }
                        else
                        {

                            TriggerClientEvent(player, "vorp:TipRight", Config.lang["fullInventoryGive"], 2000);
                            TriggerClientEvent(targetPlayer, "vorp:TipRight", Config.lang["fullInventory"], 2000);
                        }

                    }
                    else
                    {
                        TriggerClientEvent(player, "vorp:TipRight", Config.lang["itemerror"], 2000);
                        TriggerClientEvent(targetPlayer, "vorp:TipRight", Config.lang["itemerror"], 2000);
                    }
                }
                else
                {
                    Debug.WriteLine("Error Occured");
                    TriggerClientEvent(player, "vorp:TipRight", Config.lang["itemerror"], 2000);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ServerGiveItem: if NullReferenceException, possible player steam identity failed");
                Logger.Error(ex, $"ServerGiveItem");
            }
        }

        private async void getItemsTable([FromSource] Player source)
        {
            try
            {
                // must have a better way
                while (PluginManager.ItemsDB.items is null)
                {
                    await BaseScript.Delay(500);
                }

                if (PluginManager.ItemsDB.items.Count != 0)
                {
                    source.TriggerEvent("vorpInventory:giveItemsTable", PluginManager.ItemsDB.items);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"getItemsTable");
            }
        }

        private async void getInventory([FromSource] Player player)
        {
            try
            {
                string identifier = "steam:" + player.Identifiers["steam"];
                dynamic coreUserCharacter = player.GetCoreUserCharacter();

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

                if (inventory != null)
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

                player.TriggerEvent("vorpInventory:giveInventory", inventory);

                Exports["ghmattimysql"].execute("SELECT * FROM loadout WHERE `identifier` = ? AND `charidentifier` = ?;", new object[] { identifier, charIdentifier }, new Action<dynamic>((weaponsinvento) =>
                {
                    if (weaponsinvento.Count > 0)
                    {
                        WeaponClass wp;
                        foreach (var row in weaponsinvento)
                        {
                            JObject ammo = Newtonsoft.Json.JsonConvert.DeserializeObject(row.ammo.ToString());
                            JArray comp = Newtonsoft.Json.JsonConvert.DeserializeObject(row.components.ToString());
                            Dictionary<string, int> amunition = new Dictionary<string, int>();
                            List<string> components = new List<string>();
                            foreach (JProperty ammos in ammo.Properties())
                            {
                            //Debug.WriteLine(ammos.Name);
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

                    // is there something wrong with returning an empty list?
                    player.TriggerEvent("vorpInventory:giveLoadout", weaponsinvento);
                    }

                }));
            }
            catch(NullReferenceException nEX)
            {
                Logger.Error(nEX, $"getInventory: Player dropped or connecting?");
            }
            catch(Exception ex)
            {
                Logger.Error(ex, $"getInventory");
            }
        }
    }
}
