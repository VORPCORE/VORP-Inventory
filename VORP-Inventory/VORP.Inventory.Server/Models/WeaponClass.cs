using CitizenFX.Core;
using System.Collections.Generic;

namespace VORP.Inventory.Server.Models
{
    public class WeaponClass : BaseScript
    {
        private string name;
        private int id;
        private string propietary;
        private int charId;
        private bool used;
        private bool used2;
        private Dictionary<string, int> ammo;
        private List<string> components;
        public WeaponClass(int id, string propietary, string name, Dictionary<string, int> ammo, List<string> components, bool used, bool used2, int charid)
        {
            this.id = id;
            this.name = name;
            this.ammo = ammo;
            this.components = components;
            this.propietary = propietary;
            this.used = used;
            this.used2 = used2;
            this.charId = charid;
        }

        public void setUsed(bool used)
        {
            this.used = used;
        }

        public bool getUsed()
        {
            return this.used;
        }

        public void setUsed2(bool used2)
        {
            this.used2 = used2;
        }

        public bool getUsed2()
        {
            return this.used2;
        }
        public string getPropietary()
        {
            return this.propietary;
        }

        public int getCharId()
        {
            return this.charId;
        }

        public void setCharId(int charid)
        {
            this.charId = charid;
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
            return this.ammo[type];
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
            Exports["ghmattimysql"]
                .execute(
                    $"UPDATE loadout SET ammo = '{Newtonsoft.Json.JsonConvert.SerializeObject(getAllAmmo())}' WHERE id=?",
                    new[] { id });
        }

        public void setAmmo(int ammo, string type)
        {
            if (this.ammo.ContainsKey(type))
            {
                this.ammo[type] = ammo;
            }
            else
            {
                this.ammo.Add(type, ammo);
            }
            Exports["ghmattimysql"]
                .execute(
                    $"UPDATE loadout SET ammo = '{Newtonsoft.Json.JsonConvert.SerializeObject(getAllAmmo())}' WHERE id=?",
                    new[] { id });
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
            Exports["ghmattimysql"]
                .execute(
                    $"UPDATE loadout SET ammo = '{Newtonsoft.Json.JsonConvert.SerializeObject(getAllAmmo())}' WHERE id=?",
                    new[] { id });
        }

    }
}