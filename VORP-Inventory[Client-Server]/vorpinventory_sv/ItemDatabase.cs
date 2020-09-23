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
                    Debug.WriteLine("There`re no items in database");
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
        }
    }
}