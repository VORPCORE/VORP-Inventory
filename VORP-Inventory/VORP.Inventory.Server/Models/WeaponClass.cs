using CitizenFX.Core;
using System.Collections.Generic;

namespace VORP.Inventory.Server.Models
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
        public int CharId { get; set; }

        public void QuitComponent(string component)
        {
            if (Components.Contains(component))
            {
                Components.Remove(component);
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

            Exports["ghmattimysql"]
                .execute(
                    $"UPDATE loadout SET ammo = '{Newtonsoft.Json.JsonConvert.SerializeObject(Ammo)}' WHERE id=?",
                    new[] { Id });
        }

        public void SetAmmo(int ammo, string type)
        {
            if (Ammo.ContainsKey(type))
            {
                Ammo[type] = ammo;
            }
            else
            {
                Ammo.Add(type, ammo);
            }

            Exports["ghmattimysql"]
                .execute(
                    $"UPDATE loadout SET ammo = '{Newtonsoft.Json.JsonConvert.SerializeObject(Ammo)}' WHERE id=?",
                    new[] { Id });
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

            Exports["ghmattimysql"]
                .execute(
                    $"UPDATE loadout SET ammo = '{Newtonsoft.Json.JsonConvert.SerializeObject(Ammo)}' WHERE id=?",
                    new[] { Id });
        }

    }
}