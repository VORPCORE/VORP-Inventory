using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Collections.Generic;
using VORP.Inventory.Shared;

namespace VorpInventory.Models
{
    public class WeaponClass : BaseScript
    {
        private string name;
        private int id;
        private string propietary;
        private Dictionary<string, int> ammo;
        private List<string> components;
        private bool used;
        private bool used2;

        public string weaponLabel
        {
            get
            {
                int hashKey = API.GetHashKey(name);
                string rtnName = Function.Call<string>((Hash)0x6D3AC61694A791C5, hashKey);

                if (rtnName == "WNS_INVALID")
                    return name;

                return Configuration.GetWeaponLabel(rtnName);
            }
        }

        public WeaponClass(int id, string propietary, string name, Dictionary<string, int> ammo, List<string> components, bool used, bool used2)
        {
            this.id = id;
            this.name = name;
            this.ammo = ammo;
            this.components = components;
            this.propietary = propietary;
            this.used = used;
            this.used2 = used2;
        }


        public void UnequipWeapon()
        {
            setUsed(false);
            setUsed2(false);
            TriggerServerEvent("vorpinventory:setUsedWeapon", id, getUsed(), getUsed2());
            int hash = API.GetHashKey(name);
            RemoveWeaponFromPed();
            Utils.CleanAmmo(id);
        }

        public void RemoveWeaponFromPed()
        {
            API.RemoveWeaponFromPed(API.PlayerPedId(), (uint)API.GetHashKey(this.name), true, 0); //Falta revisar que pasa con esto
        }

        public void loadAmmo()
        {
            if (this.name.StartsWith("WEAPON_MELEE"))
            {
                Function.Call((Hash)0xB282DC6EBD803C75, API.PlayerPedId(), (uint)API.GetHashKey(this.name), 500, true, 0);
            }
            else
            {
                if (used2)
                {
                    // GETTING THE EQUIPED WEAPON
                    uint weaponHash = 0;
                    API.GetCurrentPedWeapon(API.PlayerPedId(), ref weaponHash, false, 0, false);
                    Debug.WriteLine($"equiped one : {weaponHash}");
                    Debug.WriteLine($"{(uint)API.GetHashKey(this.name)}");

                    Function.Call((Hash)0x5E3BDDBCB83F3D84, API.PlayerPedId(), weaponHash, 1, 1, 1, 2, false, 0.5, 1.0, 752097756, 0, true, 0.0);
                    Function.Call((Hash)0x5E3BDDBCB83F3D84, API.PlayerPedId(), (uint)API.GetHashKey(this.name), 1, 1, 1, 3, false, 0.5, 1.0, 752097756, 0, true, 0.0);
                    Function.Call((Hash)0xADF692B254977C0C, API.PlayerPedId(), weaponHash, 0, 1, 0, 0);
                    Function.Call((Hash)0xADF692B254977C0C, API.PlayerPedId(), (uint)API.GetHashKey(this.name), 0, 0, 0, 0);

                }
                else
                {
                    API.GiveDelayedWeaponToPed(API.PlayerPedId(), (uint)API.GetHashKey(this.name), 0, true, 0);

                }
                API.SetPedAmmo(API.PlayerPedId(), (uint)API.GetHashKey(this.name), 0);
                foreach (KeyValuePair<string, int> ammos in this.ammo)
                {
                    API.SetPedAmmoByType(API.PlayerPedId(), API.GetHashKey(ammos.Key), ammos.Value);
                    Debug.WriteLine($"{API.GetHashKey(ammos.Key)}: {ammos.Key} {ammos.Value}");
                }
            }

        }

        public void loadComponents()
        {
            foreach (string componente in getAllComponents())
            {
                Function.Call((Hash)0x74C9090FDD1BB48E, API.PlayerPedId(), (uint)API.GetHashKey(componente),
                    (uint)API.GetHashKey(this.name), true);//Hay que mirar que hace el true
            }
        }

        public bool getUsed()
        {
            return this.used;
        }

        public bool getUsed2()
        {
            return this.used2;
        }

        public void setUsed(bool used)
        {
            this.used = used;
            TriggerServerEvent("vorpinventory:setUsedWeapon", id, used, this.used2);
        }

        public void setUsed2(bool used2)
        {
            this.used2 = used2;
            TriggerServerEvent("vorpinventory:setUsedWeapon", id, this.used, used2); ;
        }
        public string getPropietary()
        {
            return this.propietary;
        }

        public void setPropietary(string propietary)
        {
            this.propietary = propietary;
        }
        public int getId()
        {
            return this.id;
        }

        public void setId(int id)
        {
            this.id = id;
        }

        public string getName()
        {
            return this.name;
        }

        public Dictionary<string, int> getAllAmmo()
        {
            return this.ammo;
        }

        public List<string> getAllComponents()
        {
            return this.components;
        }

        public void setComponent(string component)
        {
            this.components.Add(component);
        }

        public void quitComponent(string component)
        {
            if (this.components.Contains(component))
            {
                this.components.Remove(component);
            }
        }

        public int getAmmo(string type)
        {
            if (this.ammo.ContainsKey(type))
            {
                return this.ammo[type];
            }
            else
            {
                return 0;
            }


        }

        //Update ammo on server by client
        public void setAmmo(int ammo, string type)
        {
            if (this.ammo.ContainsKey(type))
            {
                this.ammo[type] = ammo;
                TriggerServerEvent("vorpinventory:setWeaponBullets", id, type, ammo);
            }
            else
            {
                this.ammo.Add(type, ammo);
                TriggerServerEvent("vorpinventory:setWeaponBullets", id, type, ammo);
            }
        }

        public void addAmmo(int ammo, string type)
        {
            if (this.ammo.ContainsKey(type))
            {
                this.ammo[type] += ammo;
            }
            else
            {
                this.ammo.Add(type, ammo);
            }
        }

        public void subAmmo(int ammo, string type)
        {
            if (this.ammo.ContainsKey(type))
            {
                this.ammo[type] -= ammo;
                if (this.ammo[type] == 0)
                {
                    this.ammo.Remove(type);
                }
            }
        }

    }
}