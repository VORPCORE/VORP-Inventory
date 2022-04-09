﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VORP.Inventory.Shared.Models
{
    [DataContract]
    public class DropOnRespawn
    {
        [DataMember(Name = "money")]
        public bool Money { get; set; }

        [DataMember(Name = "weapons")]
        public bool Weapons { get; set; }

        [DataMember(Name = "items")]
        public bool Items { get; set; }
    }

    [DataContract]
    public class MaxItemsInInventory
    {
        private int _weapons;
        private int _items;

        [DataMember(Name = "weapons")]
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

        [DataMember(Name = "items")]
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
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "hashName")]
        public string HashName { get; set; }

        [DataMember(Name = "weaponModel")]
        public string WeaponModel { get; set; }

        [DataMember(Name = "price")]
        public double? Price { get; set; }

        [DataMember(Name = "ammoHash")]
        public Dictionary<string, int> AmmoHash { get; set; }

        [DataMember(Name = "compsHash")]
        public Dictionary<string, int> CompsHash { get; set; }
    }

    [DataContract]
    public class Config
    {
        [DataMember(Name = "defaultlang")]
        public string Defaultlanguage { get; set; }

        [DataMember(Name = "openKey")]
        public string OpenKey { get; set; }

        [DataMember(Name = "pickupKey")]
        public string PickupKey { get; set; }

        [DataMember(Name = "dropOnRespawn")]
        public DropOnRespawn DropOnRespawn { get; set; }

        [DataMember(Name = "maxItemsInInventory")]
        public MaxItemsInInventory MaxItemsInInventory { get; set; }

        [DataMember(Name = "startItems")]
        public Dictionary<string, int> StartItems { get; set; }

        [DataMember(Name = "startWeapons")]
        public Dictionary<string, Dictionary<string, int>> StartWeapons { get; set; }

        [DataMember(Name = "weapons")]
        public List<Weapon> Weapons { get; set; }
    }
}