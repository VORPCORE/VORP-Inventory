﻿using CitizenFX.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using VORP.Inventory.Server.Models;
using VORP.Inventory.Server.Scripts;

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
        public static Dictionary<string, Dictionary<string, ItemClass>> UserInventory = new Dictionary<string, Dictionary<string, ItemClass>>();

        public static Dictionary<int, WeaponClass> UserWeapons = new Dictionary<int, WeaponClass>();
        public static Dictionary<string, ItemClass> ServerItems = new Dictionary<string, ItemClass>();

        public void Init()
        {
            Logger.Trace($"ItemDatabase Init");
            SetupItems();
            SetupLoadouts();
        }

        public static Dictionary<string, ItemClass> GetInventory(string identifier)
        {
            if (!UserInventory.ContainsKey(identifier)) return null;
            return UserInventory[identifier];
        }

        public static ItemClass GetItem(string itemName)
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
                            ServerItems.Add(item.item.ToString(), new ItemClass
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

                            wp = new WeaponClass
                            {
                                Id = int.Parse(row.id.ToString()), 
                                Propietary = row.identifier.ToString(), 
                                Name = row.name.ToString(), 
                                Ammo = amunition, 
                                Components = components, 
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