using CitizenFX.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using VorpInventory.Models;
using VorpInventory.Scripts;

namespace VorpInventory.Database
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
        public static Dictionary<string, Dictionary<string, ItemClass>> UserInventory = new Dictionary<string, Dictionary<string, ItemClass>>();

        public static Dictionary<int, WeaponClass> UserWeapons = new Dictionary<int, WeaponClass>();
        public static Dictionary<string, Items> ServerItems = new Dictionary<string, Items>();

        public void Init()
        {
            SetupItems();
            SetupLoadouts();
        }

        public static Dictionary<string, ItemClass> GetInventory(string identifier)
        {
            if (!UserInventory.ContainsKey(identifier)) return null;
            return UserInventory[identifier];
        }

        public static Items GetItem(string itemName)
        {
            if (!ServerItems.ContainsKey(itemName)) return null;
            return ServerItems[itemName];
        }

        public static ItemClass GetUserItem(string identifier, string itemName)
        {
            Dictionary<string, ItemClass> userItems = GetInventory(identifier);
            if (userItems == null) return null;
            if (!userItems.ContainsKey(itemName)) return null;
            return userItems[itemName];
        }

        public void SetupItems()
        {
            Logger.Debug($"Setting up Items");
            Exports["ghmattimysql"].execute("SELECT * FROM items", new Action<dynamic>((result) =>
            {
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
                        ServerItems.Add(item.item.ToString(), new Items(item.item, item.label, int.Parse(item.limit.ToString()), item.can_remove, item.type, item.usable));
                    }
                }
            }));
            Logger.Debug($"Item setup completed");
        }

        public void SetupLoadouts()
        {
            Logger.Debug($"Setting up Loadouts");
            Exports["ghmattimysql"].execute("SELECT * FROM loadout;", new object[] { }, new Action<dynamic>((loadout) =>
            {
                if (loadout == null)
                {
                    Logger.Warn($"No loadouts returned from the database");
                    return;
                }

                if (loadout.Count != 0)
                {
                    WeaponClass wp;
                    foreach (var row in loadout)
                    {
                        try
                        {
                            JObject ammo = JsonConvert.DeserializeObject(row.ammo.ToString());
                            JArray comp = JsonConvert.DeserializeObject(row.components.ToString());
                            int charId = -1;
                            if (row.charidentifier != null)
                            {
                                charId = row.charidentifier;
                            }
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
                            wp = new WeaponClass(int.Parse(row.id.ToString()), row.identifier.ToString(), row.name.ToString(), amunition, components, auused, auused2, charId);
                            UserWeapons[wp.getId()] = wp;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
                Logger.Debug($"Loadouts setup completed");
            }));
        }
    }
}