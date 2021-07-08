using CitizenFX.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace vorpinventory_sv
{
    public class ItemDatabase : BaseScript
    {
        //Lista de items con sus labels para que el cliente conozca el label de cada item
        public static dynamic items;
        //Lista de itemclass con el nombre de su dueño para poder hacer todo el tema de añadir y quitar cuando se robe y demas
        public static Dictionary<string, Dictionary<string, ItemClass>> usersInventory = new Dictionary<string, Dictionary<string, ItemClass>>();
        public static Dictionary<int, WeaponClass> userWeapons = new Dictionary<int, WeaponClass>();
        public static Dictionary<string, Items> svItems = new Dictionary<string, Items>();
        public ItemDatabase()
        {
            LoadDatabase();
        }

        private async void LoadDatabase()
        {
            await Delay(5000);
            Exports["ghmattimysql"].execute("SELECT * FROM items", new Action<dynamic>((result) =>
            {
                if (result.Count == 0)
                {
                    Debug.WriteLine("No items in database");
                }
                else
                {
                    items = result;
                    foreach (dynamic item in items)
                    {
                        svItems.Add(item.item.ToString(), new Items(item.item, item.label, int.Parse(item.limit.ToString()), item.can_remove, item.type, item.usable));
                    }
                }
            }));

            Exports["ghmattimysql"].execute("SELECT * FROM loadout;", new object[] { }, new Action<dynamic>((loadout) =>
            {
                if (loadout.Count != 0)
                {
                    WeaponClass wp;
                    foreach (var row in loadout)
                    {
                        try
                        {
                            JObject ammo = Newtonsoft.Json.JsonConvert.DeserializeObject(row.ammo.ToString());
                            JArray comp = Newtonsoft.Json.JsonConvert.DeserializeObject(row.components.ToString());
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
                            wp = new WeaponClass(int.Parse(row.id.ToString()), row.identifier.ToString(), row.name.ToString(), row.label.ToString(), amunition, components, auused, auused2, charId);
                            ItemDatabase.userWeapons[wp.getId()] = wp;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }

                }

            }));

        }
    }
}