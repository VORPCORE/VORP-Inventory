using System.Runtime.Serialization;

namespace VORP.Inventory.Client.Models
{
    [DataContract]
    public class ItemClass
    {
        /// <summary>
        /// Ammo in case of weapon, cuantity in case of item
        /// </summary>
        [DataMember(Name = "count")]
        public int Count { get; set; }

        [DataMember(Name = "limit")]
        public int Limit { get; set; }

        [DataMember(Name = "label")]
        public string Label { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Weapon or item
        /// </summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "usable")]
        public bool Usable { get; set; }

        [DataMember(Name = "canRemove")]
        public bool CanRemove { get; set; }
    }
}
