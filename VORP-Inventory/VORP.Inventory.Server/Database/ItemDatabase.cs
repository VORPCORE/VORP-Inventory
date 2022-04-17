using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using VORP.Inventory.Server.Scripts;
using VORP.Inventory.Shared.Models;

namespace VORP.Inventory.Server.Database
{
    public class ItemDatabase : Manager
    {
        // List of items with their labels so that the client knows the label of each item
        private dynamic _items;
        public dynamic items // hack till refactor can be complete; items is being cleared by the garbage collector
        {
            get
            {
                if (_items == null)
                    SetupItems();

                return _items;
            }
            set { _items = value; }
        }
        // List of itemclass with the name of its owner to be able to do the whole theme of adding and removing when it is stolen and others
        public static Dictionary<string, Dictionary<string, Item>> UserInventory = new Dictionary<string, Dictionary<string, Item>>();

        public static Dictionary<int, Weapon> UserWeapons = new Dictionary<int, Weapon>();
        public static Dictionary<string, Item> ServerItems = new Dictionary<string, Item>();

        public void Init()
        {
            Logger.Trace($"ItemDatabase Init");
            SetupItems();
            SetupLoadouts();
        }

        public static Dictionary<string, Item> GetInventory(string identifier)
        {
            if (!UserInventory.ContainsKey(identifier)) return null;
            return UserInventory[identifier];
        }

        public static Item GetItem(string itemName)
        {
            if (!ServerItems.ContainsKey(itemName)) return null;
            return ServerItems[itemName];
        }

        public static Item GetUserItem(string identifier, string itemName)
        {
            Dictionary<string, Item> userItems = GetInventory(identifier);
            if (userItems == null) return null;
            if (!userItems.ContainsKey(itemName)) return null;
            return userItems[itemName];
        }

        public void SetupItems()
        {
            try
            {
                Logger.Trace($"Setting up Items");
                Exports["ghmattimysql"].execute("SELECT * FROM items", new Action<dynamic>(async (result) =>
                {
                    await BaseScript.Delay(0);
                    if (result == null)
                    {
                        Logger.Warn($"No items returned from the database");
                        return;
                    }

                    if (result.Count == 0)
                    {
                        Logger.Warn("No items in the items table of the database.");
                    }
                    else
                    {
                        items = result;
                        foreach (dynamic item in items)
                        {
                            ServerItems.Add(item.item.ToString(), new Item
                            {
                                Name = item.item,
                                Label = item.label,
                                Limit = int.Parse(item.limit.ToString()),
                                CanRemove = item.can_remove,
                                Type = item.type,
                                Usable = item.usable
                            });
                        }
                    }

                    Logger.Trace($"Item setup completed; found {ServerItems.Count} items.");
                }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"SetupItems");
            }
        }

        public void SetupLoadouts()
        {
            Logger.Trace($"Setting up Loadouts");
            Exports["ghmattimysql"].execute("SELECT * FROM loadout;", new object[] { }, new Action<dynamic>(async loadout =>
            {
            await BaseScript.Delay(0);
            if (loadout == null)
            {
                Logger.Warn($"No loadouts returned from the database");
                return;
            }

            if (loadout.Count != 0)
            {
                Weapon wp;
                    foreach (var row in loadout)
                    {
                        try
                        {
                            string ammo = row.ammo.ToString();
                            Dictionary<string, int> amunition = new();
                            string components = row.components.ToString();
                            List<string> lstComponents = new();

                            if (!string.IsNullOrEmpty(ammo))
                            {
                                amunition = JsonConvert.DeserializeObject<Dictionary<string, int>>(row.ammo.ToString());
                            }

                            if (!string.IsNullOrEmpty(components))
                            {
                                lstComponents = JsonConvert.DeserializeObject<List<string>>(row.components.ToString());
                            }

                            int charId = -1;
                            if (row.charidentifier != null)
                            {
                                charId = row.charidentifier;
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

                            wp = new Weapon
                            {
                                Id = int.Parse(row.id.ToString()),
                                Propietary = row.identifier.ToString(),
                                Name = row.name.ToString(),
                                Ammo = amunition,
                                Components = lstComponents,
                                Used = auused,
                                Used2 = auused2,
                                CharId = charId
                            };

                            UserWeapons[wp.Id] = wp;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "SetupLoadouts");
                        }
                    }
                }

                Logger.Trace($"Loadouts setup completed; found {UserWeapons.Count} loadouts.");
            }));
        }
    }
}