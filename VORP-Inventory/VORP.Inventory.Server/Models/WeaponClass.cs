using CitizenFX.Core;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VORP.Inventory.Server.Models
{
    [DataContract]
    public class WeaponClass : BaseScript
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "propietary")]
        public string Propietary { get; set; }
        
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "ammo")]
        public Dictionary<string, int> Ammo { get; set; }

        [DataMember(Name = "components")]
        public List<string> Components { get; set; }

        [DataMember(Name = "used")]
        public bool Used { get; set; }

        [DataMember(Name = "used2")]
        public bool Used2 { get; set; }

        [DataMember(Name = "charid")]
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