using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using VorpInventory.Models;

namespace VorpInventory.Scripts
{
    public class InventoryAPI : BaseScript
    {
        public static Dictionary<string, Dictionary<string, dynamic>> citems =
    new Dictionary<string, Dictionary<string, dynamic>>();

        public static Dictionary<string, ItemClass> useritems = new Dictionary<string, ItemClass>();
        public static Dictionary<int, WeaponClass> userWeapons = new Dictionary<int, WeaponClass>();
        public static Dictionary<int, string> bulletsHash = new Dictionary<int, string>();

        public InventoryAPI()
        {
            EventHandlers["vorpCoreClient:addItem"] += new Action<int, int, string, string, string, bool, bool>(addItem);
            EventHandlers["vorpCoreClient:subItem"] += new Action<string, int>(subItem);
            EventHandlers["vorpCoreClient:subWeapon"] += new Action<int>(subWeapon);
            EventHandlers["vorpCoreClient:addBullets"] += new Action<int, string, int>(addWeaponBullets);
            EventHandlers["vorpCoreClient:subBullets"] += new Action<int, string, int>(subWeaponBullets);
            EventHandlers["vorpCoreClient:addComponent"] += new Action<int, string>(addComponent);
            EventHandlers["vorpCoreClient:subComponent"] += new Action<int, string>(subComponent);

            EventHandlers["vorpInventory:giveItemsTable"] += new Action<dynamic>(processItems);
            EventHandlers["vorpInventory:giveInventory"] += new Action<string>(getInventory);
            EventHandlers["vorpInventory:giveLoadout"] += new Action<dynamic>(getLoadout);
            EventHandlers["vorp:SelectedCharacter"] += new Action<int>(OnSelectedCharacter);
            EventHandlers["vorpinventory:receiveItem"] += new Action<string, int>(receiveItem);
            EventHandlers["vorpinventory:receiveItem2"] += new Action<string, int>(receiveItem2);
            EventHandlers["vorpinventory:receiveWeapon"] +=
                new Action<int, string, string, ExpandoObject, List<dynamic>>(receiveWeapon);

        }

        [Tick]
        private async Task updateAmmoInWeapon()
        {
            await Delay(500);
            uint weaponHash = 0;
            if (API.GetCurrentPedWeapon(API.PlayerPedId(), ref weaponHash, false, 0, false))
            {
                string weaponName = Function.Call<string>((Hash)0x89CF5FF3D363311E, weaponHash);
                //Debug.WriteLine(weaponName);
                if (weaponName.Contains("UNARMED")) { return; }

                Dictionary<string, int> ammoDict = new Dictionary<string, int>();
                WeaponClass usedWeapon = null;
                foreach (KeyValuePair<int, WeaponClass> weap in userWeapons.ToList())
                {
                    if (weaponName.Contains(weap.Value.getName()) && weap.Value.getUsed())
                    {
                        ammoDict = weap.Value.getAllAmmo();
                        usedWeapon = weap.Value;
                    }
                }

                if (usedWeapon == null) return;
                foreach (var ammo in ammoDict.ToList())
                {
                    int ammoQuantity = Function.Call<int>((Hash)0x39D22031557946C1, API.PlayerPedId(), API.GetHashKey(ammo.Key));
                    if (ammoQuantity != ammo.Value)
                    {
                        usedWeapon.setAmmo(ammoQuantity, ammo.Key);
                    }
                }
            }
        }//Update weapon ammo

        private void receiveItem(string name, int count)
        {
            if (useritems.ContainsKey(name))
            {
                useritems[name].addCount(count);
            }
            else
            {
                useritems.Add(name, new ItemClass(count, citems[name]["limit"], citems[name]["label"], name,
                    "item_standard", true, citems[name]["can_remove"]));
            }

            NUIEvents.LoadInv();
        }

        private void receiveItem2(string name, int count)
        {
            PluginManager.useritems[name].quitCount(count);
            if (PluginManager.useritems[name].getCount() == 0)
            {
                PluginManager.useritems.Remove(name);
            }
            NUIEvents.LoadInv();
        }

        private void receiveWeapon(int id, string propietary, string name, ExpandoObject ammo, List<dynamic> components)
        {
            Dictionary<string, int> ammoaux = new Dictionary<string, int>();
            foreach (KeyValuePair<string, object> amo in ammo)
            {
                ammoaux.Add(amo.Key, int.Parse(amo.Value.ToString()));
            }
            List<string> auxcomponents = new List<string>();
            foreach (var comp in components)
            {
                auxcomponents.Add(comp.ToString());
            }
            WeaponClass weapon = new WeaponClass(id, propietary, name, ammoaux, auxcomponents, false, false);
            if (!userWeapons.ContainsKey(weapon.getId()))
            {
                userWeapons.Add(weapon.getId(), weapon);
            }
            NUIEvents.LoadInv();
        }

        private async void OnSelectedCharacter(int charId)
        {
            API.SetNuiFocus(false, false);
            API.SendNuiMessage("{\"action\": \"hide\"}");
            Debug.WriteLine("Loading Inventory");
            TriggerServerEvent("vorpinventory:getItemsTable");
            await Delay(300);
            TriggerServerEvent("vorpinventory:getInventory");
        }
        private void processItems(dynamic items)
        {
            citems.Clear();
            foreach (dynamic item in items)
            {
                citems.Add(item.item, new Dictionary<string, dynamic>
                {
                    ["item"] = item.item,
                    ["label"] = item.label,
                    ["limit"] = item.limit,
                    ["can_remove"] = item.can_remove,
                    ["type"] = item.type,
                    ["usable"] = item.usable
                });
            }
        }

        private void getLoadout(dynamic loadout)
        {
            Debug.WriteLine(API.PlayerPedId().ToString());
            foreach (var row in loadout)
            {
                JArray componentes = Newtonsoft.Json.JsonConvert.DeserializeObject(row.components.ToString());
                JObject amunitions = Newtonsoft.Json.JsonConvert.DeserializeObject(row.ammo.ToString());
                List<string> components = new List<string>();
                Dictionary<string, int> ammos = new Dictionary<string, int>();
                foreach (JToken componente in componentes)
                {
                    components.Add(componente.ToString());
                }

                foreach (JProperty amunition in amunitions.Properties())
                {
                    ammos.Add(amunition.Name, int.Parse(amunition.Value.ToString()));
                }
                Debug.WriteLine(row.used.ToString());
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
                WeaponClass auxweapon = new WeaponClass(int.Parse(row.id.ToString()), row.identifier.ToString(), row.name.ToString(), ammos, components, auused, auused2);

                if (!userWeapons.ContainsKey(auxweapon.getId()))
                {
                    userWeapons.Add(auxweapon.getId(), auxweapon);

                    if (auxweapon.getUsed())
                    {
                        Utils.UseWeapon(auxweapon.getId());
                    }
                }
            }
        }

        private void getInventory(string inventory)
        {
            useritems.Clear();
            if (inventory != null)
            {
                dynamic items = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(inventory);
                Debug.WriteLine(items.ToString());
                foreach (KeyValuePair<string, Dictionary<string, dynamic>> fitems in citems)
                {
                    if (items[fitems.Key] != null)
                    {
                        Debug.WriteLine(fitems.Key);
                        int cuantity = int.Parse(items[fitems.Key].ToString());
                        int limit = int.Parse(fitems.Value["limit"].ToString());
                        string label = fitems.Value["label"].ToString();
                        bool can_remove = bool.Parse(fitems.Value["can_remove"].ToString());
                        string type = fitems.Value["type"].ToString();
                        bool usable = bool.Parse(fitems.Value["usable"].ToString());
                        ItemClass item = new ItemClass(cuantity, limit, label, fitems.Key, type, usable, can_remove);
                        useritems.Add(fitems.Key, item);
                    }
                }
            }
        }

        private void subComponent(int weaponId, string component)
        {
            if (PluginManager.userWeapons.ContainsKey(weaponId))
            {
                if (!PluginManager.userWeapons[weaponId].getAllComponents().Contains(component))
                {
                    PluginManager.userWeapons[weaponId].quitComponent(component);
                    if (PluginManager.userWeapons[weaponId].getUsed())
                    {
                        Function.Call((Hash)0x4899CB088EDF59B8, API.PlayerPedId(),
                            (uint)API.GetHashKey(PluginManager.userWeapons[weaponId].getName()), true, 0);
                        PluginManager.userWeapons[weaponId].loadAmmo();
                        PluginManager.userWeapons[weaponId].loadComponents();
                    }
                }
            }
        }

        private void addComponent(int weaponId, string component)
        {
            if (PluginManager.userWeapons.ContainsKey(weaponId))
            {
                if (!PluginManager.userWeapons[weaponId].getAllComponents().Contains(component))
                {
                    PluginManager.userWeapons[weaponId].setComponent(component);
                    if (PluginManager.userWeapons[weaponId].getUsed())
                    {
                        Function.Call((Hash)0x4899CB088EDF59B8, API.PlayerPedId(),
                            (uint)API.GetHashKey(PluginManager.userWeapons[weaponId].getName()), true, 0);
                        PluginManager.userWeapons[weaponId].loadAmmo();
                        PluginManager.userWeapons[weaponId].loadComponents();
                    }
                }
            }
        }

        private void subWeaponBullets(int weaponId, string bulletType, int cuantity)
        {
            if (PluginManager.userWeapons.ContainsKey(weaponId))
            {
                PluginManager.userWeapons[weaponId].subAmmo(cuantity, bulletType);
                if (PluginManager.userWeapons[weaponId].getUsed())
                {
                    API.SetPedAmmoByType(API.PlayerPedId(), API.GetHashKey(bulletType), PluginManager.userWeapons[weaponId].getAmmo(bulletType));
                }
            }
            NUIEvents.LoadInv();
        }

        private void addWeaponBullets(int weaponId, string bulletType, int cuantity)
        {
            if (PluginManager.userWeapons.ContainsKey(weaponId))
            {
                PluginManager.userWeapons[weaponId].addAmmo(cuantity, bulletType);
                if (PluginManager.userWeapons[weaponId].getUsed())
                {
                    API.SetPedAmmoByType(API.PlayerPedId(), API.GetHashKey(bulletType), PluginManager.userWeapons[weaponId].getAmmo(bulletType));
                }
            }
            NUIEvents.LoadInv();
        }


        private void subWeapon(int weaponId)
        {
            if (PluginManager.userWeapons.ContainsKey(weaponId))
            {
                if (PluginManager.userWeapons[weaponId].getUsed())
                {
                    API.RemoveWeaponFromPed(API.PlayerPedId(),
                        (uint)API.GetHashKey(PluginManager.userWeapons[weaponId].getName()),
                        true, 0); //Falta revisar que pasa con esto
                }
                PluginManager.userWeapons.Remove(weaponId);
            }
            NUIEvents.LoadInv();
        }

        private void subItem(string name, int cuantity)
        {
            Debug.WriteLine($"{name} = {cuantity}");
            if (PluginManager.useritems.ContainsKey(name))
            {
                PluginManager.useritems[name].setCount(cuantity);
                if (PluginManager.useritems[name].getCount() == 0)
                {
                    PluginManager.useritems.Remove(name);
                }
            }
            NUIEvents.LoadInv();
        }

        private void addItem(int count, int limit, string label, string name, string type, bool usable, bool canRemove)
        {
            if (PluginManager.useritems.ContainsKey(name))
            {
                PluginManager.useritems[name].addCount(count);
            }
            else
            {
                ItemClass auxitem = new ItemClass(count, limit, label, name, type, usable, canRemove);
                PluginManager.useritems.Add(name, auxitem);
            }
            NUIEvents.LoadInv();
        }
    }
}