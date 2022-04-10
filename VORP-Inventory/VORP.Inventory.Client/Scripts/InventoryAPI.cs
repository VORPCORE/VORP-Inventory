using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using VORP.Inventory.Client.Extensions;
using VORP.Inventory.Client.Models;

namespace VORP.Inventory.Client.Scripts
{
    public class InventoryAPI : Manager
    {
        public static Dictionary<string, Dictionary<string, dynamic>> citems =
    new Dictionary<string, Dictionary<string, dynamic>>();

        public static Dictionary<string, ItemClass> UsersItems = new Dictionary<string, ItemClass>();
        public static Dictionary<int, WeaponClass> UsersWeapons = new Dictionary<int, WeaponClass>();
        public static Dictionary<int, string> bulletsHash = new Dictionary<int, string>();

        public void Init()
        {
            AddEvent("vorp:SelectedCharacter", new Action<int>(OnSelectedCharacterAsync));

            AddEvent("vorpCoreClient:addItem", new Action<int, int, string, string, string, bool, bool>(OnAddItem));
            AddEvent("vorpCoreClient:subItem", new Action<string, int>(OnSubtractItem));
            AddEvent("vorpCoreClient:subWeapon", new Action<int>(OnSubtractWeapon));
            AddEvent("vorpCoreClient:addBullets", new Action<int, string, int>(OnAddWeaponBullets));
            AddEvent("vorpCoreClient:subBullets", new Action<int, string, int>(OnSubtractWeaponBullets));
            AddEvent("vorpCoreClient:addComponent", new Action<int, string>(OnAddComponent));
            AddEvent("vorpCoreClient:subComponent", new Action<int, string>(OnSubtractComponent));

            AddEvent("vorpInventory:giveItemsTable", new Action<dynamic>(OnProcessItems));
            AddEvent("vorpInventory:giveInventory", new Action<string>(OnGiveInventory));
            AddEvent("vorpInventory:giveLoadout", new Action<dynamic>(OnGiveUserWeapons));
            AddEvent("vorpinventory:receiveItem", new Action<string, int>(OnReceiveItem));
            AddEvent("vorpinventory:receiveItem2", new Action<string, int>(OnReceiveItemTwo));
            AddEvent("vorpinventory:receiveWeapon",
                new Action<int, string, string, ExpandoObject, List<dynamic>>(OnReceiveWeapon));

            AttachTickHandler(UpdateAmmoInWeaponAsync);

        }

        private async Task UpdateAmmoInWeaponAsync()
        {
            await Delay(500);
            uint weaponHash = 0;
            if (API.GetCurrentPedWeapon(API.PlayerPedId(), ref weaponHash, false, 0, false))
            {
                string weaponName = Function.Call<string>((Hash)0x89CF5FF3D363311E, weaponHash);
                if (weaponName.Contains("UNARMED")) { return; }

                Dictionary<string, int> ammoDict = new Dictionary<string, int>();
                WeaponClass usedWeapon = null;
                foreach (KeyValuePair<int, WeaponClass> weap in UsersWeapons.ToList())
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
        }

        private void OnReceiveItem(string name, int count)
        {
            if (UsersItems.ContainsKey(name))
            {
                UsersItems[name].addCount(count);
            }
            else
            {
                UsersItems.Add(name, new ItemClass(count, citems[name]["limit"], citems[name]["label"], name,
                    "item_standard", true, citems[name]["can_remove"]));
            }

            Instance.NUIEvents.LoadInv();
        }

        private void OnReceiveItemTwo(string name, int count)
        {
            UsersItems[name].quitCount(count);
            if (UsersItems[name].getCount() == 0)
            {
                UsersItems.Remove(name);
            }
            Instance.NUIEvents.LoadInv();
        }

        private void OnReceiveWeapon(int id, string propietary, string name, ExpandoObject ammo, List<dynamic> components)
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
            if (!UsersWeapons.ContainsKey(weapon.getId()))
            {
                UsersWeapons.Add(weapon.getId(), weapon);
            }
            Instance.NUIEvents.LoadInv();
        }

        private async void OnSelectedCharacterAsync(int charId)
        {
            Logger.Trace($"OnSelectedCharacterAsync: {charId}");

            DecoratorExtensions.Set(PlayerPedId(), PluginManager.DECOR_SELECTED_CHARACTER_ID, charId);

            Instance.NUIEvents.OnCloseInventory();

            TriggerServerEvent("vorpinventory:getItemsTable");
            Logger.Trace($"OnSelectedCharacterAsync: vorpinventory:getItemsTable");
            await Delay(300);
            TriggerServerEvent("vorpinventory:getInventory");
            Logger.Trace($"OnSelectedCharacterAsync: vorpinventory:getInventory");
        }

        private void OnProcessItems(dynamic items)
        {
            try
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
            catch (Exception ex)
            {
                Logger.Error(ex, $"OnProcessItems");
            }
        }

        private void OnGiveUserWeapons(dynamic loadout)
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

                if (!UsersWeapons.ContainsKey(auxweapon.getId()))
                {
                    UsersWeapons.Add(auxweapon.getId(), auxweapon);

                    if (auxweapon.getUsed())
                    {
                        Utils.UseWeapon(auxweapon.getId());
                    }
                }
            }
        }

        private void OnGiveInventory(string inventory)
        {
            UsersItems.Clear();
            if (inventory != null)
            {
                Logger.Trace($"OnGetInventory: {inventory}");

                dynamic items = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(inventory);
                
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
                        UsersItems.Add(fitems.Key, item);
                    }
                }
            }
        }

        private void OnSubtractComponent(int weaponId, string component)
        {
            if (UsersWeapons.ContainsKey(weaponId))
            {
                if (!UsersWeapons[weaponId].getAllComponents().Contains(component))
                {
                    UsersWeapons[weaponId].quitComponent(component);
                    if (UsersWeapons[weaponId].getUsed())
                    {
                        Function.Call((Hash)0x4899CB088EDF59B8, API.PlayerPedId(),
                            (uint)API.GetHashKey(UsersWeapons[weaponId].getName()), true, 0);
                        UsersWeapons[weaponId].loadAmmo();
                        UsersWeapons[weaponId].loadComponents();
                    }
                }
            }
        }

        private void OnAddComponent(int weaponId, string component)
        {
            if (UsersWeapons.ContainsKey(weaponId))
            {
                if (!UsersWeapons[weaponId].getAllComponents().Contains(component))
                {
                    UsersWeapons[weaponId].setComponent(component);
                    if (UsersWeapons[weaponId].getUsed())
                    {
                        Function.Call((Hash)0x4899CB088EDF59B8, API.PlayerPedId(),
                            (uint)API.GetHashKey(UsersWeapons[weaponId].getName()), true, 0);
                        UsersWeapons[weaponId].loadAmmo();
                        UsersWeapons[weaponId].loadComponents();
                    }
                }
            }
        }

        private void OnSubtractWeaponBullets(int weaponId, string bulletType, int cuantity)
        {
            if (UsersWeapons.ContainsKey(weaponId))
            {
                UsersWeapons[weaponId].subAmmo(cuantity, bulletType);
                if (UsersWeapons[weaponId].getUsed())
                {
                    API.SetPedAmmoByType(API.PlayerPedId(), API.GetHashKey(bulletType), UsersWeapons[weaponId].getAmmo(bulletType));
                }
            }
            Instance.NUIEvents.LoadInv();
        }

        private void OnAddWeaponBullets(int weaponId, string bulletType, int cuantity)
        {
            if (UsersWeapons.ContainsKey(weaponId))
            {
                UsersWeapons[weaponId].addAmmo(cuantity, bulletType);
                if (UsersWeapons[weaponId].getUsed())
                {
                    API.SetPedAmmoByType(API.PlayerPedId(), API.GetHashKey(bulletType), UsersWeapons[weaponId].getAmmo(bulletType));
                }
            }
            Instance.NUIEvents.LoadInv();
        }


        private void OnSubtractWeapon(int weaponId)
        {
            if (UsersWeapons.ContainsKey(weaponId))
            {
                if (UsersWeapons[weaponId].getUsed())
                {
                    API.RemoveWeaponFromPed(API.PlayerPedId(),
                        (uint)API.GetHashKey(UsersWeapons[weaponId].getName()),
                        true, 0); //Falta revisar que pasa con esto
                }
                UsersWeapons.Remove(weaponId);
            }
            Instance.NUIEvents.LoadInv();
        }

        private void OnSubtractItem(string name, int cuantity)
        {
            Debug.WriteLine($"{name} = {cuantity}");
            if (UsersItems.ContainsKey(name))
            {
                UsersItems[name].setCount(cuantity);
                if (UsersItems[name].getCount() == 0)
                {
                    UsersItems.Remove(name);
                }
            }
            Instance.NUIEvents.LoadInv();
        }

        private void OnAddItem(int count, int limit, string label, string name, string type, bool usable, bool canRemove)
        {
            if (UsersItems.ContainsKey(name))
            {
                UsersItems[name].addCount(count);
            }
            else
            {
                ItemClass auxitem = new ItemClass(count, limit, label, name, type, usable, canRemove);
                UsersItems.Add(name, auxitem);
            }
            Instance.NUIEvents.LoadInv();
        }
    }
}