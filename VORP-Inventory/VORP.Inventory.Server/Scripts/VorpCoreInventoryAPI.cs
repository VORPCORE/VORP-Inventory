using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using VORP.Inventory.Server.Database;
using VORP.Inventory.Server.Extensions;
using VORP.Inventory.Shared;
using VORP.Inventory.Shared.Models;

namespace VORP.Inventory.Server.Scripts
{
    public class VorpCoreInventoryAPI : Manager
    {
        public static Dictionary<string, CallbackDelegate> usableItemsFunctions = new Dictionary<string, CallbackDelegate>();

        PlayerList PlayerList => PluginManager.PlayerList;

        public void Init()
        {
            Logger.Trace($"VorpCoreInventoryAPI Init");

            AddEvent("vorpCore:subWeapon", new Action<int, int>(OnSubtractWeaponAsync));
            AddEvent("vorpCore:giveWeapon", new Action<int, int, int>(OnGiveWeaponAsync));
            AddEvent("vorpCore:registerWeapon", new Action<int, string, ExpandoObject, ExpandoObject>(OnRegisterWeaponAsync));
            AddEvent("vorpCore:addItem", new Action<int, string, int>(OnAddItemAsync));
            AddEvent("vorpCore:subItem", new Action<int, string, int>(OnSubtractItemAsync));
            AddEvent("vorpCore:getItemCount", new Action<int, CallbackDelegate, string>(OnGetItems));
            AddEvent("vorpCore:getUserInventory", new Action<int, CallbackDelegate>(OnGetInventory));
            AddEvent("vorpCore:canCarryItems", new Action<int, int, CallbackDelegate>(OnCanCarryAmountItem));
            AddEvent("vorpCore:canCarryItem", new Action<int, string, int, CallbackDelegate>(OnUserCanCarryItem));
            AddEvent("vorpCore:canCarryWeapons", new Action<int, int, CallbackDelegate>(OnCanCarryAmountWeaponsAsync));
            AddEvent("vorpCore:subBullets", new Action<int, int, string, int>(OnSubtractBullets));
            AddEvent("vorpCore:addBullets", new Action<int, int, string, int>(OnAddBullets));

            AddEvent("vorpCore:subAmmo", new Action<int, int, string, int>(OnSubtractAmmo));
            AddEvent("vorpCore:addAmmo", new Action<int, int, string, int>(OnAddAmmo));

            AddEvent("vorpCore:getWeaponComponents", new Action<int, CallbackDelegate, int>(OnGetWeaponComponents));
            AddEvent("vorpCore:getWeaponBullets", new Action<int, CallbackDelegate, int>(OnGetWeaponBullets));
            AddEvent("vorpCore:getUserWeapons", new Action<int, CallbackDelegate>(OnGetUserWeaponsAsync));
            AddEvent("vorpCore:addComponent", new Action<int, int, string, CallbackDelegate>(OnAddComponent));
            AddEvent("vorpCore:getUserWeapon", new Action<int, CallbackDelegate, int>(OnGetUserWeapon));
            AddEvent("vorpCore:registerUsableItem", new Action<string, CallbackDelegate>(OnRegisterUsableItem));
            AddEvent("vorp:use", new Action<Player, string, object[]>(OnUseItem));

            Export.Add("CanCarryWeapon", ExportUserCanCarryWeaponAsync);
        }

        public async Task<bool> SaveInventoryItemsSupportAsync(string steamIdendifier, int coreCharacterId)
        {
            Dictionary<string, int> items = new Dictionary<string, int>();
            string json = "{}";
            try
            {
                await Delay(0);

                if (string.IsNullOrEmpty(steamIdendifier)) return false; // no steamId provided

                if (coreCharacterId == -1) return false; // no characterId provided

                Dictionary<string, Item> itemClasses = ItemDatabase.GetInventory(steamIdendifier);

                if (itemClasses is not null)
                {
                    foreach (KeyValuePair<string, Item> item in itemClasses)
                    {
                        items.Add(item.Key, item.Value.Count);
                    }
                }

                if (items.Count > 0)
                    json = JsonConvert.SerializeObject(items);

                // why?! is the steamID required? when the Character ID is unique?! why?!
                Exports["ghmattimysql"].execute($"UPDATE characters SET `inventory` = ? WHERE `identifier` = ? AND `charidentifier` = ?;", new object[] { json, steamIdendifier, coreCharacterId });
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("SaveInventoryItemsSupport");
                Logger.Error($"SteamID: {steamIdendifier}");
                Logger.Error($"CharacterId: {coreCharacterId}");
                Logger.Error($"Items: '{json}'");
                Logger.Error("SaveInventoryItemsSupport");
                Logger.Error(ex, "SaveInventoryItemsSupport");
                return false;
            }
        }

        private async void OnCanCarryAmountWeaponsAsync(int source, int quantity, CallbackDelegate cb)
        {
            try
            {
                bool result = await ExportUserCanCarryWeaponAsync(source, quantity);
                cb.Invoke(result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"canCarryAmountWeapons");
            }
        }

        private async Task<bool> ExportUserCanCarryWeaponAsync(int playerServerId, int quantity)
        {
            try
            {
                Player player = PlayerList[playerServerId];

                if (player == null)
                {
                    Logger.Error($"canCarryAmountWeapons: Player '{playerServerId}' does not exist.");
                    return false;
                }

                string identifier = "steam:" + player.Identifiers["steam"];

                dynamic coreUserCharacter = await player.GetCoreUserCharacterAsync();
                if (coreUserCharacter == null) return false;

                int charIdentifier = coreUserCharacter.charIdentifier;
                int totalcount = GetUserTotalCountWeapons(identifier, charIdentifier) + quantity;
                if (Configuration.INVENTORY_MAX_WEAPONS != -1)
                {
                    if (totalcount <= Configuration.INVENTORY_MAX_WEAPONS)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"UserCanCarryWeapon");
                return false;
            }
        }

        private void OnCanCarryAmountItem(int source, int quantity, CallbackDelegate cb)
        {
            try
            {
                Player player = PlayerList[source];

                if (player == null)
                {
                    Logger.Error($"canCarryAmountItem: Player '{source}' does not exist.");
                    return;
                }

                string identifier = "steam:" + player.Identifiers["steam"];
                if (ItemDatabase.UserInventory.ContainsKey(identifier) && Configuration.INVENTORY_MAX_ITEMS != -1)
                {
                    int totalcount = GetTotalAmountOfItems(identifier) + quantity;
                    if (totalcount <= Configuration.INVENTORY_MAX_ITEMS)
                    {
                        cb.Invoke(true);
                        return;
                    }
                    else
                    {
                        cb.Invoke(false);
                        return;
                    }
                }
                else
                {
                    cb.Invoke(true);
                    return;
                }
            }
            catch (NullReferenceException nrEx)
            {
                Logger.Error(nrEx, $"canCarryAmountItem: SOME HOW LOST STEAM?!");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"canCarryAmountItem");
            }
        }

        private void OnUserCanCarryItem(int source, string itemName, int amountToCarry, CallbackDelegate cb)
        {
            try
            {
                Player player = PlayerList[source];

                if (player == null)
                {
                    Logger.Error($"canCarryItem: Player '{source}' does not exist.");
                    cb.Invoke(false);
                }

                string identifier = "steam:" + player.Identifiers["steam"];

                Item item = ItemDatabase.GetItem(itemName);
                if (item == null)
                {
                    Logger.Error($"canCarryItem: Item '{itemName}' does not exist.");
                    cb.Invoke(false);
                }

                int maxLimitItem = item.Limit;
                Dictionary<string, Item> userInventory = ItemDatabase.GetInventory(identifier);

                int maxLimitConfig = Configuration.INVENTORY_MAX_ITEMS;
                int newTotalAmountOfCurrentItems = GetTotalAmountOfItems(identifier) + amountToCarry;

                bool result = false;

                // If the user has no inventory, then allow them to be given the item
                if (userInventory == null)
                {
                    result = CheckIfUserCanHaveItem(amountToCarry, maxLimitItem, maxLimitConfig, newTotalAmountOfCurrentItems);
                    cb.Invoke(result);
                    return;
                }

                // If the user does not have the item, then allow them to be given the item
                if (!userInventory.ContainsKey(itemName))
                {
                    result = CheckIfUserCanHaveItem(amountToCarry, maxLimitItem, maxLimitConfig, newTotalAmountOfCurrentItems);
                    cb.Invoke(result);
                    return;
                }

                // If the user has the item, we still check to see how many
                Item userItem = userInventory[itemName];
                int itemQuantity = userItem.Count;

                amountToCarry = itemQuantity + amountToCarry;
                result = CheckIfUserCanHaveItem(amountToCarry, maxLimitItem, maxLimitConfig, newTotalAmountOfCurrentItems);
                cb.Invoke(result);
                return;

                static bool CheckIfUserCanHaveItem(int amountToCarry, int maxLimitItem, int maxLimitConfig, int newTotalAmountOfCurrentItems)
                {
                    Logger.Debug($"amountToCarry: {amountToCarry} / maxLimitItem: {maxLimitItem}, maxLimitConfig: {maxLimitConfig}, newTotalAmountOfCurrentItems: {newTotalAmountOfCurrentItems}");
                    if (maxLimitConfig != -1)
                    {
                        if (amountToCarry > maxLimitItem) return false;
                        if (maxLimitConfig != -1)
                        {
                            if (newTotalAmountOfCurrentItems > maxLimitConfig) return false;
                            return true;
                        }
                        return true;
                    }
                    return true;
                }

            }
            catch (NullReferenceException nrEx)
            {
                Logger.Error(nrEx, $"canCarryAmountItem: SOME HOW LOST STEAM?!");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"canCarryAmountItem");
            }
        }

        private void OnGetInventory(int source, CallbackDelegate cb)
        {
            List<object> useritems = new List<object>();
            try
            {
                Player player = PlayerList[source];

                if (player == null)
                {
                    Logger.Error($"getInventory: Player doesn't exist, but why?!.");
                    return;
                }

                string identifier = "steam:" + player.Identifiers["steam"];
                if (ItemDatabase.UserInventory.ContainsKey(identifier))
                {
                    Dictionary<string, Item> itemsDBO = ItemDatabase.UserInventory[identifier];

                    if (itemsDBO == null)
                    {
                        Logger.Error($"getInventory: Player '{player.Name}' has no items.");

                        if (!string.IsNullOrEmpty(player?.EndPoint ?? ""))
                            cb.Invoke(useritems);
                    }

                    foreach (KeyValuePair<string, Item> items in itemsDBO)
                    {
                        Item itemClass = items.Value;
                        if (itemClass == null) continue;

                        Dictionary<string, object> item = new Dictionary<string, object>()
                        {
                            {"label", itemClass.Label},
                            {"name", itemClass.Name},
                            {"type", itemClass.Type},
                            {"count", itemClass.Count},
                            {"limit", itemClass.Limit},
                            {"usable", itemClass.Usable}
                        };

                        useritems.Add(item);
                    }

                    Logger.Trace($"User '{identifier}' Inventory: {JsonConvert.SerializeObject(useritems)}");

                    if (!string.IsNullOrEmpty(player?.EndPoint ?? ""))
                        cb.Invoke(useritems);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "getInventory");
                if (useritems.Count > 0)
                {
                    string itemStr = JsonConvert.SerializeObject(useritems);
                    Logger.Error($"Items at time of error: {itemStr}");
                }
            }
        }

        private void OnUseItem([FromSource] Player source, string itemName, params object[] args)
        {
            try
            {
                if (!usableItemsFunctions.ContainsKey(itemName))
                {
                    Logger.Error($"Item '{itemName}' doesn't exist as a usable item");
                    return;
                }

                Item item = ItemDatabase.GetItem(itemName);
                if (item == null)
                {
                    Logger.Error($"Item '{itemName}' not found in Server Items.");
                    return;
                }

                Dictionary<string, object> argumentos = new()
                {
                    { "source", int.Parse(source.Handle) },
                    { "item", ItemDatabase.ServerItems[itemName].GetItemDictionary() },
                    { "args", args }
                };
                usableItemsFunctions[itemName](argumentos);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"useItem");
            }
        }

        private void OnRegisterUsableItem(string name, CallbackDelegate cb)
        {
            usableItemsFunctions[name] = cb;
            Logger.Info($"{API.GetCurrentResourceName()}: Function callback of usable item: {name} registered!");
        }

        private void SubtractComponent(int player, int weaponId, string component, CallbackDelegate function)
        {
            try
            {
                Player p = PlayerList[player];

                if (p == null)
                {
                    Logger.Error($"subComponent: Player '{player}' does not exist.");
                    return;
                }

                string identifier = "steam:" + p.Identifiers["steam"];

                if (ItemDatabase.UserWeapons.ContainsKey(weaponId))
                {
                    if (ItemDatabase.UserWeapons[weaponId].Propietary == identifier)
                    {
                        ItemDatabase.UserWeapons[weaponId].QuitComponent(component);
                        Exports["ghmattimysql"]
                            .execute(
                                $"UPDATE loadout SET components = '{JsonConvert.SerializeObject(ItemDatabase.UserWeapons[weaponId].Components)}' WHERE id=?",
                                new[] { weaponId });
                        function.Invoke(true);
                        p.TriggerEvent("vorpCoreClient:subComponent", weaponId, component);
                    }
                    else
                    {
                        function.Invoke(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"subComponent");
            }
        }

        private void OnAddComponent(int player, int weaponId, string component, CallbackDelegate function)
        {
            try
            {
                Player p = PlayerList[player];

                if (p == null)
                {
                    Logger.Error($"addComponent: Player '{player}' does not exist.");
                    return;
                }

                string identifier = "steam:" + p.Identifiers["steam"];

                if (ItemDatabase.UserWeapons.ContainsKey(weaponId))
                {
                    if (ItemDatabase.UserWeapons[weaponId].Propietary == identifier)
                    {
                        ItemDatabase.UserWeapons[weaponId].Components.Add(component);

                        Exports["ghmattimysql"]
                            .execute(
                                $"UPDATE loadout SET components = '{Newtonsoft.Json.JsonConvert.SerializeObject(ItemDatabase.UserWeapons[weaponId].Components)}' WHERE id=?",
                                new[] { weaponId });
                        function.Invoke(true);
                        p.TriggerEvent("vorpCoreClient:addComponent", weaponId, component);
                    }
                    else
                    {
                        function.Invoke(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"addComponent");
            }
        }

        private void OnGetUserWeapon(int player, CallbackDelegate function, int weapId)
        {
            try
            {
                Player p = PlayerList[player];

                if (p == null)
                {
                    Logger.Error($"getUserWeapon: Player '{player}' does not exist.");
                    return;
                }

                string identifier = "steam:" + p.Identifiers["steam"];

                Dictionary<string, dynamic> weapons = new Dictionary<string, dynamic>();
                bool found = false;
                foreach (KeyValuePair<int, Weapon> weapon in ItemDatabase.UserWeapons)
                {
                    if (weapon.Value.Id == weapId && !found)
                    {
                        weapons.Add("name", weapon.Value.Name);
                        weapons.Add("id", weapon.Value.Id);
                        weapons.Add("propietary", weapon.Value.Propietary);
                        weapons.Add("used", weapon.Value.Used);
                        weapons.Add("ammo", weapon.Value.Ammo);
                        weapons.Add("components", weapon.Value.Components);
                        found = true;
                    }
                }

                function.Invoke(weapons);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"getUserWeapon");
            }
        }

        private async void OnGetUserWeaponsAsync(int source, CallbackDelegate function)
        {
            try
            {
                Player player = PlayerList[source];

                if (player == null)
                {
                    Logger.Error($"getUserWeapons: Player '{source}' does not exist.");
                    return;
                }

                string identifier = "steam:" + player.Identifiers["steam"];
                int charIdentifier;

                dynamic coreUserCharacter = await player.GetCoreUserCharacterAsync();
                if (coreUserCharacter == null)
                {
                    Logger.Error($"getUserWeapons: Player '{source}' CORE User does not exist.");
                    return;
                }

                charIdentifier = coreUserCharacter.charIdentifier;

                Dictionary<string, dynamic> weapons;
                List<Dictionary<string, dynamic>> userWeapons = new List<Dictionary<string, dynamic>>();

                foreach (KeyValuePair<int, Weapon> weapon in ItemDatabase.UserWeapons)
                {
                    if (weapon.Value.Propietary == identifier)
                    {

                        if (weapon.Value.CharId == charIdentifier)
                        {
                            weapons = new Dictionary<string, dynamic>
                            {
                                ["name"] = weapon.Value.Name,
                                ["id"] = weapon.Value.Id,
                                ["propietary"] = weapon.Value.Propietary,
                                ["used"] = weapon.Value.Used,
                                ["ammo"] = weapon.Value.Ammo,
                                ["components"] = weapon.Value.Components
                            };

                            userWeapons.Add(weapons);
                        }
                    }
                }

                function.Invoke(userWeapons);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"getUserWeapons");
            }
        }

        private void OnGetWeaponBullets(int player, CallbackDelegate function, int weaponId)
        {
            try
            {
                Player p = PlayerList[player];

                if (p == null)
                {
                    Logger.Error($"getWeaponBullets: Player '{player}' does not exist.");
                    return;
                }

                string identifier = "steam:" + p.Identifiers["steam"];

                if (ItemDatabase.UserWeapons.ContainsKey(weaponId))
                {
                    if (ItemDatabase.UserWeapons[weaponId].Propietary == identifier)
                    {
                        function.Invoke(ItemDatabase.UserWeapons[weaponId].Ammo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"getWeaponBullets");
            }
        }

        private void OnGetWeaponComponents(int player, CallbackDelegate function, int weaponId)
        {
            try
            {
                Player p = PlayerList[player];

                if (p == null)
                {
                    Logger.Error($"getWeaponComponents: Player '{player}' does not exist.");
                    return;
                }

                string identifier = "steam:" + p.Identifiers["steam"];

                if (ItemDatabase.UserWeapons.ContainsKey(weaponId))
                {
                    if (ItemDatabase.UserWeapons[weaponId].Propietary == identifier)
                    {
                        function.Invoke(ItemDatabase.UserWeapons[weaponId].Components);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"getWeaponComponents");
            }
        }

        private void OnAddBullets(int player, int weaponId, string bulletType, int cuantity)
        {
            try
            {
                Player p = PlayerList[player];

                if (p == null)
                {
                    Logger.Error($"addBullets: Player '{player}' does not exist.");
                    return;
                }

                string identifier = "steam:" + p.Identifiers["steam"];

                if (ItemDatabase.UserWeapons.ContainsKey(weaponId))
                {
                    if (ItemDatabase.UserWeapons[weaponId].Propietary == identifier)
                    {
                        ItemDatabase.UserWeapons[weaponId].AddAmmo(cuantity, bulletType);
                        p.TriggerEvent("vorpCoreClient:addBullets", weaponId, bulletType, cuantity);
                    }
                }
                else
                {
                    Logger.Trace("Weapon not found in DBa");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"addBullets");
            }
        }

        private void OnSubtractBullets(int player, int weaponId, string bulletType, int cuantity)
        {
            try
            {
                Player p = PlayerList[player];

                if (p == null)
                {
                    Logger.Error($"subBullets: Player '{player}' does not exist.");
                    return;
                }

                string identifier = "steam:" + p.Identifiers["steam"];

                if (ItemDatabase.UserWeapons.ContainsKey(weaponId))
                {
                    if (ItemDatabase.UserWeapons[weaponId].Propietary == identifier)
                    {
                        ItemDatabase.UserWeapons[weaponId].SubAmmo(cuantity, bulletType);
                        p.TriggerEvent("vorpCoreClient:subBullets", weaponId, bulletType, cuantity);
                    }
                }
                else
                {
                    Logger.Trace("Weapon not found in DB");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"subBullets");
            }
        }

        private void OnAddAmmo(int player, int weaponId, string bulletType, int cuantity)
        {
            try
            {
                Player p = PlayerList[player];

                if (p == null)
                {
                    Logger.Error($"addBullets: Player '{player}' does not exist.");
                    return;
                }

                string identifier = "steam:" + p.Identifiers["steam"];

                if (ItemDatabase.UserWeapons.ContainsKey(weaponId))
                {
                    if (ItemDatabase.UserWeapons[weaponId].Propietary == identifier)
                    {
                        ItemDatabase.UserWeapons[weaponId].AddAmmo(cuantity, bulletType);
                    }
                }
                else
                {
                    Logger.Trace("Weapon not found in DBa");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"addBullets");
            }
        }

        private void OnSubtractAmmo(int player, int weaponId, string bulletType, int cuantity)
        {
            try
            {
                Player p = PlayerList[player];

                if (p == null)
                {
                    Logger.Error($"subBullets: Player '{player}' does not exist.");
                    return;
                }

                string identifier = "steam:" + p.Identifiers["steam"];

                if (ItemDatabase.UserWeapons.ContainsKey(weaponId))
                {
                    if (ItemDatabase.UserWeapons[weaponId].Propietary == identifier)
                    {
                        ItemDatabase.UserWeapons[weaponId].SubAmmo(cuantity, bulletType);
                    }
                }
                else
                {
                    Logger.Trace("Weapon not found in DB");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"subBullets");
            }
        }

        private void OnGetItems(int source, CallbackDelegate funcion, string item)
        {
            try
            {
                Player player = PlayerList[source];

                if (player == null)
                {
                    Logger.Error($"getItems: Player '{source}' does not exist.");
                    return;
                }

                string identifier = "steam:" + player.Identifiers["steam"];

                if (ItemDatabase.UserInventory.ContainsKey(identifier))
                {
                    Dictionary<string, Item> inventory = ItemDatabase.GetInventory(identifier);

                    if (inventory == null)
                    {
                        funcion.Invoke(0);
                        return;
                    }

                    if (inventory.ContainsKey(item))
                    {
                        Item itemClass = inventory[item];
                        funcion.Invoke(itemClass.Count);
                    }
                    else
                    {
                        funcion.Invoke(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "getItems");
            }
        }

        private async void OnAddItemAsync(int source, string name, int quantity)
        {
            try
            {
                if (!ItemDatabase.ServerItems.ContainsKey(name))
                {
                    Logger.Trace($"addItem: Item: {name} not exist on Database please add this item on Table `Items`");
                    return;
                }

                Player player = PlayerList[source];

                if (player == null)
                {
                    Logger.Error($"addItem: Player '{source}' does not exist.");
                    return;
                }

                bool added = false;
                string identifier = "steam:" + player.Identifiers["steam"];
                int coreUserCharacterId = await player.GetCoreUserCharacterIdAsync();

                if (!ItemDatabase.UserInventory.ContainsKey(identifier))
                {
                    Dictionary<string, Item> userinv = new Dictionary<string, Item>();
                    ItemDatabase.UserInventory.Add(identifier, userinv);
                }

                if (ItemDatabase.UserInventory.ContainsKey(identifier))
                {
                    if (ItemDatabase.UserInventory[identifier].ContainsKey(name))
                    {
                        if (ItemDatabase.UserInventory[identifier][name].Count + quantity <= ItemDatabase.UserInventory[identifier][name].Limit)
                        {
                            if (quantity > 0)
                            {
                                if (Configuration.INVENTORY_MAX_ITEMS != 0)
                                {
                                    int totalcount = GetTotalAmountOfItems(identifier);
                                    totalcount += quantity;
                                    if (totalcount <= Configuration.INVENTORY_MAX_ITEMS)
                                    {
                                        added = true;
                                        ItemDatabase.UserInventory[identifier][name].AddCount(quantity);
                                    }
                                }
                                else
                                {
                                    added = true;
                                    ItemDatabase.UserInventory[identifier][name].AddCount(quantity);
                                }
                            }
                        }
                        else if (ItemDatabase.UserInventory[identifier][name].Limit == -1)
                        {
                            if (quantity > 0)
                            {
                                if (Configuration.INVENTORY_MAX_ITEMS != 0)
                                {
                                    int totalcount = GetTotalAmountOfItems(identifier);
                                    totalcount += quantity;
                                    if (totalcount <= Configuration.INVENTORY_MAX_ITEMS)
                                    {
                                        added = true;
                                        ItemDatabase.UserInventory[identifier][name].AddCount(quantity);
                                    }
                                }
                                else
                                {
                                    added = true;
                                    ItemDatabase.UserInventory[identifier][name].AddCount(quantity);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (quantity <= ItemDatabase.ServerItems[name].Limit)
                        {
                            added = true;

                            if (Configuration.INVENTORY_MAX_ITEMS != 0)
                            {
                                int totalcount = GetTotalAmountOfItems(identifier);
                                totalcount += quantity;
                                if (totalcount <= Configuration.INVENTORY_MAX_ITEMS)
                                {
                                    added = true;
                                    ItemDatabase.UserInventory[identifier].Add(name, new Item
                                    {
                                        Count = quantity,
                                        Limit = ItemDatabase.ServerItems[name].Limit,
                                        Label = ItemDatabase.ServerItems[name].Label,
                                        Name = name,
                                        Type = ItemDatabase.ServerItems[name].Type,
                                        Usable = true,
                                        CanRemove = ItemDatabase.ServerItems[name].CanRemove
                                    });
                                }
                            }
                            else
                            {
                                added = true;
                                ItemDatabase.UserInventory[identifier].Add(name, new Item
                                {
                                    Count = quantity,
                                    Limit = ItemDatabase.ServerItems[name].Limit,
                                    Label = ItemDatabase.ServerItems[name].Label,
                                    Name = name,
                                    Type = ItemDatabase.ServerItems[name].Type,
                                    Usable = true,
                                    CanRemove = ItemDatabase.ServerItems[name].CanRemove
                                });
                            }


                        }
                        else if (ItemDatabase.ServerItems[name].Limit == -1)
                        {
                            if (Configuration.INVENTORY_MAX_ITEMS != 0)
                            {
                                int totalcount = GetTotalAmountOfItems(identifier);
                                totalcount += quantity;
                                if (totalcount <= Configuration.INVENTORY_MAX_ITEMS)
                                {
                                    added = true;
                                    ItemDatabase.UserInventory[identifier].Add(name, new Item
                                    {
                                        Count = quantity,
                                        Limit = ItemDatabase.ServerItems[name].Limit,
                                        Label = ItemDatabase.ServerItems[name].Label,
                                        Name = name,
                                        Type = ItemDatabase.ServerItems[name].Type,
                                        Usable = true,
                                        CanRemove = ItemDatabase.ServerItems[name].CanRemove
                                    });
                                }
                            }
                            else
                            {
                                added = true;
                                ItemDatabase.UserInventory[identifier].Add(name, new Item
                                {
                                    Count = quantity,
                                    Limit = ItemDatabase.ServerItems[name].Limit,
                                    Label = ItemDatabase.ServerItems[name].Label,
                                    Name = name,
                                    Type = ItemDatabase.ServerItems[name].Type,
                                    Usable = true,
                                    CanRemove = ItemDatabase.ServerItems[name].CanRemove
                                });
                            }
                        }
                    }

                    if (ItemDatabase.UserInventory[identifier].ContainsKey(name) && added)
                    {
                        int limit = ItemDatabase.UserInventory[identifier][name].Limit;
                        string label = ItemDatabase.UserInventory[identifier][name].Label;
                        string type = ItemDatabase.UserInventory[identifier][name].Type;
                        bool usable = ItemDatabase.UserInventory[identifier][name].Usable;
                        bool canRemove = ItemDatabase.UserInventory[identifier][name].CanRemove;
                        player.TriggerEvent("vorpCoreClient:addItem", quantity, limit, label, name, type, usable, canRemove);//Pass item to client
                        bool result = await SaveInventoryItemsSupportAsync(identifier, coreUserCharacterId);
                        if (!result)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("Method: AddItem\n");
                            sb.Append("Message: Player inventory not saved\n");
                            sb.Append($"Player SteamID: {identifier}\n");
                            sb.Append($"Player CharacterId: {coreUserCharacterId}\n");
                            sb.Append($"If CharacterId = -1, then the Core did not return the character.\n");
                            sb.Append($"Inventory: {JsonConvert.SerializeObject(ItemDatabase.UserInventory[identifier])}");
                            Logger.Warn($"{sb}");
                        }
                    }
                    else
                    {
                        TriggerClientEvent(player, "vorp:Tip", Configuration.GetTranslation("fullInventory"), 2000);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "OnAddItemAsync");
            }
        }

        private async void OnSubtractItemAsync(int source, string itemName, int quantity)
        {
            try
            {
                if (ItemDatabase.GetItem(itemName) == null)
                {
                    Logger.Trace($"Item: {itemName} not exist on Database please add this item on Table `Items`");
                    return;
                }

                Player player = PlayerList[source];

                if (player == null)
                {
                    Logger.Error($"subItem: Player '{source}' does not exist.");
                    return;
                }

                string identifier = "steam:" + player.Identifiers["steam"];
                int coreUserCharacterId = await player.GetCoreUserCharacterIdAsync();

                Dictionary<string, Item> userInventory = ItemDatabase.GetInventory(identifier);
                if (userInventory == null)
                {
                    Logger.Error($"subItem: Player '{source}' inventory does not exist.");
                    return;
                }

                if (userInventory.ContainsKey(itemName))
                {
                    Item item = userInventory[itemName];
                    int itemCount = item.Count;

                    if (quantity <= itemCount)
                    {
                        itemCount = item.Subtract(quantity);
                    }

                    if (itemCount == 0)
                    {
                        userInventory.Remove(itemName);
                    }

                    player.TriggerEvent("vorpCoreClient:subItem", itemName, itemCount);
                    bool result = await SaveInventoryItemsSupportAsync(identifier, coreUserCharacterId);
                    if (!result)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Method: SubtractItem\n");
                        sb.Append("Message: Player inventory not saved\n");
                        sb.Append($"Player SteamID: {identifier}\n");
                        sb.Append($"Player CharacterId: {coreUserCharacterId}\n");
                        sb.Append($"If CharacterId = -1, then the Core did not return the character.\n");
                        sb.Append($"Inventory: {JsonConvert.SerializeObject(userInventory)}");
                        Logger.Warn($"{sb}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"VORP Inventory: subItem");
            }
        }

        private async void OnRegisterWeaponAsync(int target, string hashName, ExpandoObject ammos, ExpandoObject components)//Needs dirt level
        {
            try
            {
                Player targetPlayer = null;
                bool targetIsPlayer = false;
                foreach (Player pla in PlayerList)
                {
                    if (int.Parse(pla.Handle) == target)
                    {
                        targetPlayer = PlayerList[target];
                        targetIsPlayer = true;
                    }
                }

                if (targetPlayer == null)
                {
                    Logger.Error($"registerWeapon: Target Player '{target}' does not exist.");
                    return;
                }

                if (!Configuration.HasWeaponHashName(hashName))
                {
                    Logger.Error($"registerWeapon: Weapon name '{hashName}' does not exist.");
                    return;
                }

                string identifier;

                dynamic coreUserCharacter = await targetPlayer.GetCoreUserCharacterAsync();
                if (coreUserCharacter == null)
                {
                    Logger.Error($"registerWeapon: Player '{target}' CORE User does not exist.");
                    return;
                }
                int charIdentifier = coreUserCharacter.charIdentifier;

                if (targetIsPlayer)
                {
                    identifier = "steam:" + targetPlayer.Identifiers["steam"];
                    if (Configuration.INVENTORY_MAX_WEAPONS != 0)
                    {
                        int totalcount = GetUserTotalCountWeapons(identifier, charIdentifier);
                        totalcount += 1;
                        if (totalcount > Configuration.INVENTORY_MAX_WEAPONS)
                        {
                            Logger.Trace($"{targetPlayer.Name} Can't carry more weapons");
                            return;
                        }
                    }
                }
                else
                {
                    identifier = target.ToString();
                }

                Dictionary<string, int> ammoaux = new Dictionary<string, int>();
                if (ammos != null)
                {
                    foreach (KeyValuePair<string, object> ammo in ammos)
                    {
                        ammoaux.Add(ammo.Key, int.Parse(ammo.Value.ToString()));
                    }
                }

                List<string> auxcomponents = new List<string>();
                if (components != null)
                {
                    foreach (KeyValuePair<string, object> component in components)
                    {
                        auxcomponents.Add(component.Key);
                    }
                }

                Exports["ghmattimysql"].execute("INSERT INTO loadout (`identifier`,`charidentifier`,`name`,`ammo`,`components`) VALUES (?,?,?,?,?)", new object[] { identifier, charIdentifier, hashName, JsonConvert.SerializeObject(ammoaux), JsonConvert.SerializeObject(auxcomponents) }, new Action<dynamic>((result) =>
                {
                    int weaponId = result.insertId;
                    Weapon auxWeapon = new()
                    {
                        Id = weaponId,
                        Propietary = identifier,
                        Name = hashName,
                        Ammo = ammoaux,
                        Components = auxcomponents,
                        Used = false,
                        Used2 = false,
                        CharId = charIdentifier
                    };

                    ItemDatabase.UserWeapons.Add(weaponId, auxWeapon);
                    if (targetIsPlayer)
                    {
                        TriggerEvent("syn_weapons:registerWeapon", weaponId);
                        targetPlayer.TriggerEvent("vorpinventory:receiveWeapon", weaponId, ItemDatabase.UserWeapons[weaponId].Propietary,
                            ItemDatabase.UserWeapons[weaponId].Name, ItemDatabase.UserWeapons[weaponId].Ammo, ItemDatabase.UserWeapons[weaponId].Components);
                    }
                }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"registerWeapon");
            }
        }

        private async void OnGiveWeaponAsync(int source, int weapId, int target)
        {
            try
            {
                Player player = PlayerList[source];

                if (player == null)
                {
                    Logger.Error($"giveWeapon: Player '{source}' does not exist.");
                    return;
                }

                Player ptarget = null;
                bool targetIsPlayer = false;
                foreach (Player pla in PlayerList)
                {
                    if (int.Parse(pla.Handle) == target)
                    {
                        targetIsPlayer = true;
                    }
                }

                if (targetIsPlayer)
                {
                    ptarget = PlayerList[target];

                    if (ptarget == null)
                    {
                        Logger.Error($"giveWeapon: Target Player '{target}' does not exist.");
                        return;
                    }
                }

                string identifier = "steam:" + player.Identifiers["steam"];

                dynamic coreUserCharacter = await player.GetCoreUserCharacterAsync();
                if (coreUserCharacter == null)
                {
                    Logger.Error($"giveWeapon: Player '{source}' CORE User does not exist.");
                    return;
                }

                int charIdentifier = coreUserCharacter.charIdentifier;

                if (Configuration.INVENTORY_MAX_WEAPONS != 0)
                {
                    int totalcount = GetUserTotalCountWeapons(identifier, charIdentifier);
                    totalcount += 1;
                    if (totalcount > Configuration.INVENTORY_MAX_WEAPONS)
                    {
                        Logger.Trace($"{player.Name} Can't carry more weapons");
                        return;
                    }
                }

                if (ItemDatabase.UserWeapons.ContainsKey(weapId))
                {
                    ItemDatabase.UserWeapons[weapId].Propietary = identifier;
                    ItemDatabase.UserWeapons[weapId].CharId = charIdentifier;
                    Exports["ghmattimysql"]
                        .execute(
                            $"UPDATE loadout SET identifier = ?, charidentifier = ? WHERE id=?",
                            new object[] { ItemDatabase.UserWeapons[weapId].Propietary, charIdentifier, weapId });
                    player.TriggerEvent("vorpinventory:receiveWeapon", weapId, ItemDatabase.UserWeapons[weapId].Propietary,
                        ItemDatabase.UserWeapons[weapId].Name, ItemDatabase.UserWeapons[weapId].Ammo, ItemDatabase.UserWeapons[weapId].Components);
                    if (targetIsPlayer && ptarget != null)
                    {
                        ptarget.TriggerEvent("vorpCoreClient:subWeapon", weapId);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"giveWeapon: possible player has dropped?");
            }
        }

        private async void OnSubtractWeaponAsync(int source, int weapId)
        {
            try
            {
                Player player = PlayerList[source];

                if (player == null)
                {
                    Logger.Error($"subWeapon: Player '{source}' does not exist.");
                    return;
                }

                dynamic charIdentifier = await player.GetCoreUserCharacterIdAsync();

                if (charIdentifier == -1)
                {
                    Logger.Error($"subWeapon: Player '{source}' Core Character does not exist.");
                }

                string identifier = "steam:" + player.Identifiers["steam"];
                if (ItemDatabase.UserWeapons.ContainsKey(weapId))
                {
                    ItemDatabase.UserWeapons[weapId].Propietary = "";
                    Exports["ghmattimysql"]
                        .execute(
                            $"UPDATE loadout SET identifier = '{ItemDatabase.UserWeapons[weapId].Propietary}' , charidentifier = '{charIdentifier}' WHERE id=?",
                            new[] { weapId });
                }
                player.TriggerEvent("vorpCoreClient:subWeapon", weapId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"subWeapon");
            }
        }


        public static int GetTotalAmountOfItems(string identifier)
        {
            int t_count = 0;

            Dictionary<string, Item> userInventory = ItemDatabase.GetInventory(identifier);
            if (userInventory == null) return 0;

            foreach (Item item in userInventory.Values)
            {
                t_count += item.Count;
            }

            return t_count;
        }

        public static int GetUserTotalCountWeapons(string identifier, int charId)
        {
            int t_count = 0;
            foreach (var weapon in ItemDatabase.UserWeapons.Values)
            {
                if (weapon.Propietary.Contains(identifier) && weapon.CharId == charId)
                {
                    t_count += 1;
                }
            }

            return t_count;
        }
    }
}