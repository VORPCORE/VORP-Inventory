using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Dynamic;
using CitizenFX.Core.Native;

namespace vorpinventory_sv
{
    public class InventoryAPI : BaseScript
    {
        public static Dictionary<string,CallbackDelegate> usableItemsFunctions = new Dictionary<string, CallbackDelegate>();
        public InventoryAPI()
        {
            EventHandlers["vorpCore:subWeapon"] += new Action<int, int>(subWeapon);
            EventHandlers["vorpCore:giveWeapon"] += new Action<int, int, int>(giveWeapon);
            EventHandlers["vorpCore:registerWeapon"] += new Action<int, string,ExpandoObject,ExpandoObject>(registerWeapon);
            EventHandlers["vorpCore:addItem"] += new Action<int, string, int>(addItem);
            EventHandlers["vorpCore:subItem"] += new Action<int, string, int>(subItem);
            EventHandlers["vorpCore:getItemCount"] += new Action<int, CallbackDelegate, string>(getItems);
            EventHandlers["vorpCore:getUserInventory"] += new Action<int, CallbackDelegate>(getInventory);
            EventHandlers["vorpCore:subBullets"] += new Action<int, int, string, int>(subBullets);
            EventHandlers["vorpCore:addBullets"] += new Action<int, int, string, int>(addBullets);
            EventHandlers["vorpCore:getWeaponComponents"] += new Action<int, CallbackDelegate, int>(getWeaponComponents);
            EventHandlers["vorpCore:getWeaponBullets"] += new Action<int, CallbackDelegate, int>(getWeaponBullets);
            EventHandlers["vorpCore:getUserWeapons"] += new Action<int, CallbackDelegate>(getUserWeapons);
            EventHandlers["vorpCore:addComponent"] += new Action<int, int, string, CallbackDelegate>(addComponent);
            EventHandlers["vorpCore:getUserWeapon"] += new Action<int, CallbackDelegate, int>(getUserWeapon);
            EventHandlers["vorpCore:registerUsableItem"] += new Action<string,CallbackDelegate>(registerUsableItem);
            EventHandlers["vorp:use"] += new Action<Player,string,object[]>(useItem);
        }

        private void getInventory(int source, CallbackDelegate cb)
        {
            PlayerList pl = new PlayerList();
            Player p = pl[source];
            string identifier = "steam:" + p.Identifiers["steam"];
            if (ItemDatabase.usersInventory.ContainsKey(identifier))
            {
                List<object> useritems = new List<object>();
                
                foreach (var items in ItemDatabase.usersInventory[identifier])
                {
                    Dictionary<string, object> item = new Dictionary<string, object>()
                    {
                        {"label", items.Value.getLabel()},
                        {"name", items.Value.getName()},
                        {"type", items.Value.getType()},
                        {"count", items.Value.getCount()},
                        {"limit", items.Value.getLimit()},
                        {"usable", items.Value.getUsable()}
                    };
                    useritems.Add(item);
                }

                cb.Invoke(useritems);
            }
        }

        private void useItem([FromSource]Player source,string itemname ,params object []args)
        {
            string identifier = "steam:" + source.Identifiers["steam"];
            if (usableItemsFunctions.ContainsKey(itemname))
            {
                if (ItemDatabase.svItems.ContainsKey(itemname))
                {
                    Dictionary<string, object> argumentos = new Dictionary<string, object>()
                    {
                        {"source", int.Parse(source.Handle)},
                        {"item", ItemDatabase.svItems[itemname].getItemDictionary()},
                        {"args",args}
                    };
                    usableItemsFunctions[itemname](argumentos);
                }
                else
                {
                    Debug.WriteLine("Error");
                }
            }
        }

        private void registerUsableItem(string name, CallbackDelegate cb)
        {
            if (usableItemsFunctions.ContainsKey(name))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{API.GetCurrentResourceName()}: Function callback of item: {name} already registered!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                if (ItemDatabase.svItems.ContainsKey(name) && ItemDatabase.svItems[name].getUsable())
                {
                    usableItemsFunctions.Add(name,cb);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{API.GetCurrentResourceName()}: Function callback of item: {name} registered!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        private void subComponent(int player, int weaponId, string component, CallbackDelegate function)
        {
            PlayerList pl = new PlayerList();
            Player p = pl[player];
            string identifier = "steam:" + p.Identifiers["steam"];

            if (ItemDatabase.userWeapons.ContainsKey(weaponId))
            {
                if (ItemDatabase.userWeapons[weaponId].getPropietary() == identifier)
                {
                    ItemDatabase.userWeapons[weaponId].quitComponent(component);
                    Exports["ghmattimysql"]
                        .execute(
                            $"UPDATE loadout SET components = '{Newtonsoft.Json.JsonConvert.SerializeObject(ItemDatabase.userWeapons[weaponId].getAllComponents())}' WHERE id=?",
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

        private void addComponent(int player, int weaponId, string component, CallbackDelegate function)
        {
            PlayerList pl = new PlayerList();
            Player p = pl[player];
            string identifier = "steam:" + p.Identifiers["steam"];

            if (ItemDatabase.userWeapons.ContainsKey(weaponId))
            {
                if (ItemDatabase.userWeapons[weaponId].getPropietary() == identifier)
                {
                    ItemDatabase.userWeapons[weaponId].setComponent(component);
                    Exports["ghmattimysql"]
                        .execute(
                            $"UPDATE loadout SET components = '{Newtonsoft.Json.JsonConvert.SerializeObject(ItemDatabase.userWeapons[weaponId].getAllComponents())}' WHERE id=?",
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

        private void getUserWeapon(int player, CallbackDelegate function, int weapId)
        {
            PlayerList pl = new PlayerList();
            Player p = pl[player];
            string identifier = "steam:" + p.Identifiers["steam"];

            Dictionary<string, dynamic> weapons = new Dictionary<string, dynamic>();
            bool found = false;
            foreach (KeyValuePair<int, WeaponClass> weapon in ItemDatabase.userWeapons)
            {
                if (weapon.Value.getId() == weapId && !found)
                {
                    Debug.WriteLine("Entro a ver");
                    weapons.Add("name", weapon.Value.getName());
                    weapons.Add("id", weapon.Value.getId());
                    weapons.Add("propietary", weapon.Value.getPropietary());
                    weapons.Add("used", weapon.Value.getUsed());
                    weapons.Add("ammo", weapon.Value.getAllAmmo());
                    weapons.Add("components", weapon.Value.getAllComponents());
                    found = true;
                }
            }
            function.Invoke(weapons);
        }

        private void getUserWeapons(int player, CallbackDelegate function)
        {
            PlayerList pl = new PlayerList();
            Player p = pl[player];
            string identifier = "steam:" + p.Identifiers["steam"];

            Dictionary<string, dynamic> weapons;
            List<Dictionary<string, dynamic>> userWeapons = new List<Dictionary<string, dynamic>>();

            foreach (KeyValuePair<int, WeaponClass> weapon in ItemDatabase.userWeapons)
            {
                if (weapon.Value.getPropietary() == identifier)
                {
                    weapons = new Dictionary<string, dynamic>
                    {
                        ["name"] = weapon.Value.getName(),
                        ["id"] = weapon.Value.getId(),
                        ["propietary"] = weapon.Value.getPropietary(),
                        ["used"] = weapon.Value.getUsed(),
                        ["ammo"] = weapon.Value.getAllAmmo(),
                        ["components"] = weapon.Value.getAllComponents()
                    };
                    userWeapons.Add(weapons);
                }
            }
            function.Invoke(userWeapons);
        }

        private void getWeaponBullets(int player, CallbackDelegate function, int weaponId)
        {
            PlayerList pl = new PlayerList();
            Player p = pl[player];
            string identifier = "steam:" + p.Identifiers["steam"];

            if (ItemDatabase.userWeapons.ContainsKey(weaponId))
            {
                if (ItemDatabase.userWeapons[weaponId].getPropietary() == identifier)
                {
                    function.Invoke(ItemDatabase.userWeapons[weaponId].getAllAmmo());
                }
            }
        }

        private void getWeaponComponents(int player, CallbackDelegate function, int weaponId)
        {
            PlayerList pl = new PlayerList();
            Player p = pl[player];
            string identifier = "steam:" + p.Identifiers["steam"];

            if (ItemDatabase.userWeapons.ContainsKey(weaponId))
            {
                if (ItemDatabase.userWeapons[weaponId].getPropietary() == identifier)
                {
                    function.Invoke(ItemDatabase.userWeapons[weaponId].getAllComponents());
                }
            }
        }

        private void addBullets(int player, int weaponId, string bulletType, int cuantity)
        {
            PlayerList pl = new PlayerList();
            Player p = pl[player];
            string identifier = "steam:" + p.Identifiers["steam"];

            if (ItemDatabase.userWeapons.ContainsKey(weaponId))
            {
                if (ItemDatabase.userWeapons[weaponId].getPropietary() == identifier)
                {
                    ItemDatabase.userWeapons[weaponId].addAmmo(cuantity, bulletType);
                    p.TriggerEvent("vorpCoreClient:addBullets", weaponId, bulletType, cuantity);
                }
            }
            else
            {
                Debug.WriteLine("Error al meter balas en un arma identificador no valido o arma no encontrada");
            }
        }

        private void subBullets(int player, int weaponId, string bulletType, int cuantity)
        {
            PlayerList pl = new PlayerList();
            Player p = pl[player];
            string identifier = "steam:" + p.Identifiers["steam"];

            if (ItemDatabase.userWeapons.ContainsKey(weaponId))
            {
                if (ItemDatabase.userWeapons[weaponId].getPropietary() == identifier)
                {
                    ItemDatabase.userWeapons[weaponId].subAmmo(cuantity, bulletType);
                    p.TriggerEvent("vorpCoreClient:subBullets", weaponId, bulletType, cuantity);
                }
            }
            else
            {
                Debug.WriteLine("Error al meter balas en un arma identificador no valido o arma no encontrada");
            }
        }

        private void getItems(int source, CallbackDelegate funcion, string item)
        {
            PlayerList pl = new PlayerList();
            Player p = pl[source];
            string identifier = "steam:" + p.Identifiers["steam"];
            if (ItemDatabase.usersInventory.ContainsKey(identifier))
            {
                if (ItemDatabase.usersInventory[identifier].ContainsKey(item))
                {
                    funcion.Invoke(ItemDatabase.usersInventory[identifier][item].getCount());
                }
                else
                {
                    funcion.Invoke(0);
                }
            }
        }
        private async void addItem(int player, string name, int cuantity)
        {
            PlayerList pl = new PlayerList();

            if (pl[player] == null)
            {
                return;
            }

            Player p = pl[player];
            bool added = false;
            string identifier = "steam:" + p.Identifiers["steam"];

            if (!ItemDatabase.usersInventory.ContainsKey(identifier))
            {
                Dictionary<string, ItemClass> userinv = new Dictionary<string, ItemClass>();
                ItemDatabase.usersInventory.Add(identifier, userinv);
            }

            if (ItemDatabase.usersInventory.ContainsKey(identifier))
            {
                if (ItemDatabase.usersInventory[identifier].ContainsKey(name))
                {
                    if (ItemDatabase.usersInventory[identifier][name].getCount() + cuantity <= ItemDatabase.usersInventory[identifier][name].getLimit())
                    {
                        if (cuantity > 0)
                        {
                            added = true;
                            ItemDatabase.usersInventory[identifier][name].addCount(cuantity);
                        }
                    }
                    else if (ItemDatabase.usersInventory[identifier][name].getLimit() == -1)
                    {
                        if (cuantity > 0)
                        {
                            added = true;
                            ItemDatabase.usersInventory[identifier][name].addCount(cuantity);
                        }
                    }
                }
                else
                {
                    if (cuantity <= ItemDatabase.svItems[name].getLimit())
                    {
                        added = true;
                        ItemDatabase.usersInventory[identifier].Add(name, new ItemClass(cuantity, ItemDatabase.svItems[name].getLimit(),
                            ItemDatabase.svItems[name].getLabel(), name, ItemDatabase.svItems[name].getType(), true, ItemDatabase.svItems[name].getCanRemove()));
                    }
                    else if (ItemDatabase.svItems[name].getLimit() == -1)
                    {
                        added = true;
                        ItemDatabase.usersInventory[identifier].Add(name, new ItemClass(cuantity, ItemDatabase.svItems[name].getLimit(),
                            ItemDatabase.svItems[name].getLabel(), name, ItemDatabase.svItems[name].getType(), true, ItemDatabase.svItems[name].getCanRemove()));
                    }

                }
                if (ItemDatabase.usersInventory[identifier].ContainsKey(name) && added)
                {
                    int limit = ItemDatabase.usersInventory[identifier][name].getLimit();
                    string label = ItemDatabase.usersInventory[identifier][name].getLabel();
                    string type = ItemDatabase.usersInventory[identifier][name].getType();
                    bool usable = ItemDatabase.usersInventory[identifier][name].getUsable();
                    bool canRemove = ItemDatabase.usersInventory[identifier][name].getCanRemove();
                    p.TriggerEvent("vorpCoreClient:addItem", cuantity, limit, label, name, type, usable, canRemove);//Pass item to client
                }
                else
                {
                    TriggerClientEvent(p, "vorp:Tip", Config.lang["fullInventory"], 2000);
                }
            }
        }

        private void subItem(int player, string name, int cuantity)
        {
            PlayerList pl = new PlayerList();
            Player p = pl[player];
            string identifier = "steam:" + p.Identifiers["steam"];
            if (ItemDatabase.usersInventory.ContainsKey(identifier))
            {
                if (ItemDatabase.usersInventory[identifier].ContainsKey(name))
                {
                    if (cuantity <= ItemDatabase.usersInventory[identifier][name].getCount())
                    {
                        ItemDatabase.usersInventory[identifier][name].quitCount(cuantity);
                    }
                    p.TriggerEvent("vorpCoreClient:subItem", name, ItemDatabase.usersInventory[identifier][name].getCount());
                    if (ItemDatabase.usersInventory[identifier][name].getCount() == 0)
                    {
                        ItemDatabase.usersInventory[identifier].Remove(name);
                    }

                }
            }
        }

        private void registerWeapon(int target, string name,ExpandoObject ammos,ExpandoObject components)//Needs dirt level
        {
            PlayerList pl = new PlayerList();
            Player p = null;
            bool targetIsPlayer = false;
            foreach (Player pla in pl)
            {
                if (int.Parse(pla.Handle) == target)
                {
                    p = pl[target];
                    targetIsPlayer = true;
                }
            }

            string identifier;
            if (targetIsPlayer)
            {
                identifier = "steam:" + p.Identifiers["steam"];
            }
            else
            {
                identifier = target.ToString();
            }

            Dictionary<string, int> ammoaux = new Dictionary<string, int>();
            if (ammos != null)
            {
                foreach (KeyValuePair<string,object> ammo in ammos)
                {
                    ammoaux.Add(ammo.Key,int.Parse(ammo.Value.ToString()));
                }
            }
            
            List<string> auxcomponents = new List<string>();
            if (components != null)
            {
                foreach (KeyValuePair<string,object> component in components)
                {
                    auxcomponents.Add(component.Key);
                }
            }
            int weaponId = -1;
            Exports["ghmattimysql"].execute("SELECT `AUTO_INCREMENT` FROM  INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'vorp' AND TABLE_NAME   = 'loadout';",
                new Action<dynamic>((id) =>
            {
                weaponId = int.Parse(id[0].AUTO_INCREMENT.ToString());
                Debug.WriteLine(weaponId.ToString());
                Exports["ghmattimysql"]
                    .execute(
                        "INSERT INTO loadout (`identifier`,`name`,`ammo`,`components`) VALUES (?,?,?,?)", new object[] { identifier, name
                            ,Newtonsoft.Json.JsonConvert.SerializeObject(ammoaux), Newtonsoft.Json.JsonConvert.SerializeObject(auxcomponents) });

                WeaponClass auxWeapon = new WeaponClass(weaponId, identifier, name, ammoaux, auxcomponents, false);
                ItemDatabase.userWeapons.Add(weaponId, auxWeapon);
                if (targetIsPlayer)
                {
                    p.TriggerEvent("vorpinventory:receiveWeapon", weaponId, ItemDatabase.userWeapons[weaponId].getPropietary(),
                        ItemDatabase.userWeapons[weaponId].getName(), ItemDatabase.userWeapons[weaponId].getAllAmmo(), ItemDatabase.userWeapons[weaponId].getAllComponents());
                }
            }));
        }
        private void giveWeapon(int player, int weapId, int target)
        {
            PlayerList pl = new PlayerList();
            Player p = pl[player];
            Player ptarget = null;
            bool targetIsPlayer = false;
            foreach (Player pla in pl)
            {
                if (int.Parse(pla.Handle) == target)
                {
                    targetIsPlayer = true;
                }
            }

            if (targetIsPlayer)
            {
                ptarget = pl[target];
            }
            string identifier = "steam:" + p.Identifiers["steam"];

            if (ItemDatabase.userWeapons.ContainsKey(weapId))
            {
                ItemDatabase.userWeapons[weapId].setPropietary(identifier);
                Exports["ghmattimysql"]
                    .execute(
                        $"UPDATE loadout SET identifier = '{ItemDatabase.userWeapons[weapId].getPropietary()}' WHERE id=?",
                        new[] { weapId });
                p.TriggerEvent("vorpinventory:receiveWeapon", weapId, ItemDatabase.userWeapons[weapId].getPropietary(),
                    ItemDatabase.userWeapons[weapId].getName(), ItemDatabase.userWeapons[weapId].getAllAmmo(), ItemDatabase.userWeapons[weapId].getAllComponents());
                if (targetIsPlayer && ptarget != null)
                {
                    ptarget.TriggerEvent("vorpCoreClient:subWeapon", weapId);
                }
            }
        }

        private void subWeapon(int player, int weapId)
        {
            PlayerList pl = new PlayerList();
            Player p = pl[player];
            string identifier = "steam:" + p.Identifiers["steam"];
            Debug.WriteLine($"Me han llamado desde lua {player} {p}");
            if (ItemDatabase.userWeapons.ContainsKey(weapId))
            {
                ItemDatabase.userWeapons[weapId].setPropietary("");
                Exports["ghmattimysql"]
                    .execute(
                        $"UPDATE loadout SET identifier = '{ItemDatabase.userWeapons[weapId].getPropietary()}' WHERE id=?",
                        new[] { weapId });
            }
            p.TriggerEvent("vorpCoreClient:subWeapon", weapId);
        }
    }
}