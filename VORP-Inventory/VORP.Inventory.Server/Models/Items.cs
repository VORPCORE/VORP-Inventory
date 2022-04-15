using System.Collections.Generic;

namespace VORP.Inventory.Server.Models
{
    public class Items
    {
        public string Item { get; set; }
        public string Label { get; set; }
        public int Limit { get; set; }
        public bool CanRemove { get; set; }
        public string Type { get; set; }
        public bool Usable { get; set; }

        //public Items(string item, string label, int limit, bool can_remove, string type, bool usable)

        public Dictionary<string, object> GetItemDictionary()
        {
            return new Dictionary<string, object>()
            {
                {"name", Item},
                {"label", Label},
                {"limit", Limit},
                {"can_remove", CanRemove},
                {"type", Type},
                {"usabel", Usable}
            };
        }
    }
}