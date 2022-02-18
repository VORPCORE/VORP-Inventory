using System.Collections.Generic;

namespace VorpInventory.Models
{
    public class Items
    {
        private string item;
        private string label;
        private int limit;
        private bool can_remove;
        private string type;
        private bool usable;

        public Items(string item, string label, int limit, bool can_remove, string type, bool usable)
        {
            this.item = item;
            this.limit = limit;
            this.can_remove = can_remove;
            this.label = label;
            this.type = type;
            this.usable = usable;
        }

        public bool getUsable()
        {
            return this.usable;
        }

        public void setUsable(bool usable)
        {
            this.usable = usable;
        }

        public string getType()
        {
            return this.type;
        }

        public void setType(string type)
        {
            this.type = type;
        }

        public string getName()
        {
            return this.item;
        }

        public string getLabel()
        {
            return this.label;
        }

        public int getLimit()
        {
            return this.limit;
        }

        public bool getCanRemove()
        {
            return this.can_remove;
        }
        public Dictionary<string, object> getItemDictionary()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>()
            {
                {"name", item},
                {"label", label},
                {"limit", limit},
                {"can_remove", can_remove},
                {"type", type},
                {"usabel", usable}
            };
            return dictionary;
        }
    }
}