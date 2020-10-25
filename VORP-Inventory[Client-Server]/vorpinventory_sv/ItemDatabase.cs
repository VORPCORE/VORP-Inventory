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
        public static Dictionary<string, Dictionary<string, Item>> usersInventory = new Dictionary<string, Dictionary<string, Item>>();
        public static Dictionary<int, WeaponClass> userWeapons = new Dictionary<int, WeaponClass>();
        public static Dictionary<string, Item> svItems = new Dictionary<string, Item>();
        public ItemDatabase()
        {
            LoadDatabase();
        }

        //TO-DO:
        //Revisar la clase ItemClass para refactorizarla
        // Agrear las nuevos metodos
        // revisar que no explote ;)
        public static void LoadItemTemplate()
        {
            foreach (var item in Config.config["Items"])
            {
                svItems.Add(item["Name"].ToString(), new Item(item["Name"].ToString(), item["Label"].ToString(), item["Type"].ToString(), item["Model"].ToString(), 0, item["Limit"].ToObject<int>(), item["Weight"].ToObject<double>(), item["CanUse"].ToObject<bool>(), item["CanRemove"].ToObject<bool>(), item["DropOnDeath"].ToObject<bool>()));
            }
        }

        private async void LoadDatabase()
        {
            await Delay(5000);

            Exports["ghmattimysql"].execute("SELECT * FROM loadout;", new object[] {  }, new Action<dynamic>((loadout) =>
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
                            wp = new WeaponClass(int.Parse(row.id.ToString()), row.identifier.ToString(), row.name.ToString(), amunition, components, auused, charId);
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