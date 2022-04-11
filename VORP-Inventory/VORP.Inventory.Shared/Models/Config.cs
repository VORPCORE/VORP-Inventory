using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VORP.Inventory.Shared.Models
{
    [DataContract]
    public class DropOnRespawn
    {
        [DataMember(Name = "Money")]
        public bool Money { get; set; }

        [DataMember(Name = "Weapons")]
        public bool Weapons { get; set; }

        [DataMember(Name = "Items")]
        public bool Items { get; set; }
    }

    [DataContract]
    public class MaxItemsInInventory
    {
        private int _weapons;
        private int _items;

        [DataMember(Name = "Weapons")]
        public int Weapons
        {
            get => _weapons;
            set
            {
                if (value < 0)
                    value = 0;
                _items = value;
            }
        }

        [DataMember(Name = "Items")]
        public int Items
        {
            get => _items;
            set
            {
                if (value < 0)
                    value = 0;
                _items = value;
            }
        }
    }

    [DataContract]
    public class Weapon
    {
        [DataMember(Name = "Name")]
        public string Name { get; set; }

        [DataMember(Name = "HashName")]
        public string HashName { get; set; }

        [DataMember(Name = "WeaponModel")]
        public string WeaponModel { get; set; }

        [DataMember(Name = "Price")]
        public double? Price { get; set; }

        [DataMember(Name = "AmmoHash")]
        public Dictionary<string, double> AmmoHash { get; set; }

        [DataMember(Name = "CompsHash")]
        public Dictionary<string, int> CompsHash { get; set; }
    }

    [DataContract]
    public class Config
    {
        [DataMember(Name = "defaultlang")]
        public string Defaultlanguage { get; set; }

        [DataMember(Name = "OpenKey")]
        public string OpenKey { get; set; }

        [DataMember(Name = "PickupKey")]
        public string PickupKey { get; set; }

        [DataMember(Name = "DropOnRespawn")]
        public DropOnRespawn DropOnRespawn { get; set; }

        [DataMember(Name = "MaxItemsInInventory")]
        public MaxItemsInInventory MaxItemsInInventory { get; set; }

        [DataMember(Name = "startItems")]
        public Dictionary<string, int> StartItems { get; set; }

        [DataMember(Name = "startWeapons")]
        public Dictionary<string, Dictionary<string, double>> StartWeapons { get; set; }

        [DataMember(Name = "Weapons")]
        public List<Weapon> Weapons { get; set; }
    }
}
