using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;

namespace VorpInventory
{
    public class InventoryAPI : BaseScript
    {
        public InventoryAPI()
        {
            EventHandlers["vorpCoreClient:addItem"] += new Action<int, int, string, string, string, bool, bool>(addItem);
            EventHandlers["vorpCoreClient:subItem"] += new Action<string, int>(subItem);
            EventHandlers["vorpCoreClient:subWeapon"] += new Action<int>(subWeapon);
            EventHandlers["vorpCoreClient:addBullets"] += new Action<int, string, int>(addWeaponBullets);
            EventHandlers["vorpCoreClient:subBullets"] += new Action<int, string, int>(subWeaponBullets);
            EventHandlers["vorpCoreClient:addComponent"] += new Action<int, string>(addComponent);
            EventHandlers["vorpCoreClient:subComponent"] += new Action<int, string>(subComponent);
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