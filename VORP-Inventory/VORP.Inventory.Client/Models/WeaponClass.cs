using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Collections.Generic;
using VORP.Inventory.Shared;

namespace VORP.Inventory.Client.Models
{
    public class WeaponClass : BaseScript
    {
        public int Id { get; set; }
        public string Propietary { get; set; }
        public string Name { get; set; }
        public Dictionary<string, int> Ammo { get; set; }
        public List<string> Components { get; set; }
        public bool Used { get; set; }
        public bool Used2 { get; set; }

        public string WeaponLabel
        {
            get
            {
                int hashKey = API.GetHashKey(Name);
                string rtnName = Function.Call<string>((Hash)0x6D3AC61694A791C5, hashKey);

                if (rtnName == "WNS_INVALID")
                {
                    return Name;
                }

                return Configuration.GetWeaponLabel(rtnName);
            }
        }

        public void UnequipWeapon()
        {
            SetUsed(false);
            SetUsed2(false);
            TriggerServerEvent("vorpinventory:setUsedWeapon", Id, Used, Used2);
            //int hash = API.GetHashKey(Name);
            RemoveWeaponFromPed();
            Utils.CleanAmmo(Id);
        }

        public void RemoveWeaponFromPed()
        {
            API.RemoveWeaponFromPed(API.PlayerPedId(), (uint)API.GetHashKey(Name), true, 0);
        }

        public void LoadAmmo()
        {
            if (Name.StartsWith("WEAPON_MELEE"))
            {
                Function.Call((Hash)0xB282DC6EBD803C75, API.PlayerPedId(), (uint)API.GetHashKey(Name), 500, true, 0);
            }
            else
            {
                if (Used2)
                {
                    // GETTING THE EQUIPED WEAPON
                    uint weaponHash = 0;
                    API.GetCurrentPedWeapon(API.PlayerPedId(), ref weaponHash, false, 0, false);

                    Function.Call((Hash)0x5E3BDDBCB83F3D84, API.PlayerPedId(), weaponHash, 1, 1, 1, 2, false, 0.5, 1.0, 752097756, 0, true, 0.0);
                    Function.Call((Hash)0x5E3BDDBCB83F3D84, API.PlayerPedId(), (uint)API.GetHashKey(Name), 1, 1, 1, 3, false, 0.5, 1.0, 752097756, 0, true, 0.0);
                    Function.Call((Hash)0xADF692B254977C0C, API.PlayerPedId(), weaponHash, 0, 1, 0, 0);
                    Function.Call((Hash)0xADF692B254977C0C, API.PlayerPedId(), (uint)API.GetHashKey(Name), 0, 0, 0, 0);

                }
                else
                {
                    API.GiveDelayedWeaponToPed(API.PlayerPedId(), (uint)API.GetHashKey(Name), 0, true, 0);

                }
                API.SetPedAmmo(API.PlayerPedId(), (uint)API.GetHashKey(Name), 0);
                foreach (KeyValuePair<string, int> ammos in Ammo)
                {
                    API.SetPedAmmoByType(API.PlayerPedId(), API.GetHashKey(ammos.Key), ammos.Value);
                    Debug.WriteLine($"{API.GetHashKey(ammos.Key)}: {ammos.Key} {ammos.Value}");
                }
            }

        }

        public void LoadComponents()
        {
            foreach (string component in Components)
            {
                Function.Call((Hash)0x74C9090FDD1BB48E, API.PlayerPedId(), (uint)API.GetHashKey(component),
                    (uint)API.GetHashKey(Name), true);//Hay que mirar que hace el true
            }
        }

        public void SetUsed(bool used)
        {
            Used = used;
            TriggerServerEvent("vorpinventory:setUsedWeapon", Id, used, Used2);
        }

        public void SetUsed2(bool used2)
        {
            Used2 = used2;
            TriggerServerEvent("vorpinventory:setUsedWeapon", Id, Used, used2); ;
        }

        public void QuitComponent(string component)
        {
            if (Components.Contains(component))
            {
                Components.Remove(component);
            }
        }

        public int GetAmmo(string type)
        {
            if (Ammo.ContainsKey(type))
            {
                return Ammo[type];
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Update ammo on server by client
        /// </summary>
        /// <param name="ammo"></param>
        /// <param name="type"></param>
        public void SetAmmo(int ammo, string type)
        {
            if (Ammo.ContainsKey(type))
            {
                Ammo[type] = ammo;
                TriggerServerEvent("vorpinventory:setWeaponBullets", Id, type, ammo);
            }
            else
            {
                Ammo.Add(type, ammo);
                TriggerServerEvent("vorpinventory:setWeaponBullets", Id, type, ammo);
            }
        }

        public void AddAmmo(int ammo, string type)
        {
            if (Ammo.ContainsKey(type))
            {
                Ammo[type] += ammo;
            }
            else
            {
                Ammo.Add(type, ammo);
            }
        }

        public void SubAmmo(int ammo, string type)
        {
            if (Ammo.ContainsKey(type))
            {
                Ammo[type] -= ammo;
                if (Ammo[type] == 0)
                {
                    Ammo.Remove(type);
                }
            }
        }
    }
}