using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VORP.Inventory.Server.Models
{
    [DataContract]
    public class Items
    {
        [DataMember(Name = "item")]
        public string Item { get; set; }

        [DataMember(Name = "label")]
        public string Label { get; set; }

        [DataMember(Name = "limit")]
        public int Limit { get; set; }

        [DataMember(Name = "can_remove")]
        public bool CanRemove { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "usable")]
        public bool Usable { get; set; }

        public Dictionary<string, object> GetItemDictionary()
        {
            return new Dictionary<string, object>()
            {
                {"name", Item},
                {"label", Label},
                {"limit", Limit},
                {"can_remove", CanRemove},
                {"type", Type},
                {"usable", Usable}
            };
        }
    }
}