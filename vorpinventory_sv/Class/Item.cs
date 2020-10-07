using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorpinventory_sv.Class
{
    public class Item
    {
        string name;
        string label;
        string type;
        string model;

        int count;
        int limit;

        double weight;

        bool canUse;
        bool canRemove;
        bool dropOnDeath;

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
    }
}
