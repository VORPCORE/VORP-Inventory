using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorpinventory_sv
{
    public class Item
    {
        private string name;
        private string label;
        private string type;
        private string model;

        private int count;
        private int limit;

        private double weight;

        private bool canUse;
        private bool canRemove;
        private bool dropOnDeath;

        public Item(string name, string label, string type, string model, int count, int limit, double weight, bool canUse, bool canRemove, bool dropOnDeath)
        {
            this.name = name;
            this.label = label;
            this.type = type;
            this.model = model;
            this.count = count;
            this.limit = limit;
            this.weight = weight;
            this.canUse = canUse;
            this.canRemove = canRemove;
            this.dropOnDeath = dropOnDeath;
        }

        public string Name { get => name; set => name = value; }
        public string Label { get => label; set => label = value; }
        public string Type { get => type; set => type = value; }
        public string Model { get => model; set => model = value; }
        public int Count { get => count; set => count = value; }
        public int Limit { get => limit; set => limit = value; }
        public double Weight { get => weight; set => weight = value; }
        public bool CanUse { get => canUse; set => canUse = value; }
        public bool CanRemove { get => canRemove; set => canRemove = value; }
        public bool DropOnDeath { get => dropOnDeath; set => dropOnDeath = value; }

        public object getItemDictionary()
        {
            Dictionary<string, object> itemDic = new Dictionary<string, object>()
            {
                {"label", Label},
                {"name", Name},
                {"model", Model},
                {"type", Type},
                {"count", Count},
                {"limit", Limit},
                {"usable", CanUse},
                {"weight", Weight}
            };
            return itemDic;
        }

        public void addCount(int cuantity)
        {
            this.count += cuantity;
        }

        public void delCount(int cuantity)
        {
            this.count -= cuantity;
        }
    }
}