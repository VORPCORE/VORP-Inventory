namespace VorpInventory.Models
{
    public class ItemClass
    {
        int count;
        int limit;
        string label;
        string name;
        string type;
        bool usable;
        bool canRemove;

        public ItemClass(int count, int limit, string label, string name, string type, bool usable, bool canRemove)
        {
            this.count = count;
            this.limit = limit;
            this.label = label;
            this.name = name;
            this.type = type;
            this.usable = usable;
            this.canRemove = canRemove;
        }

        public void setCount(int count)
        {
            this.count = count;
        }

        public int getCount()
        {
            return this.count;
        }

        public void addCount(int count)
        {
            if (this.count + count <= limit)
            {
                this.count += count;
            }
        }

        public int Subtract(int amountToSubtract)
        {
            if (amountToSubtract < 0)
                amountToSubtract = 0;

            count -= amountToSubtract;
            return count;
        }

        public void setLimit(int limit)
        {
            this.limit = limit;
        }

        public int getLimit()
        {
            return this.limit;
        }

        public void setLabel(string label)
        {
            this.label = label;
        }

        public string getLabel()
        {
            return this.label;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public string getName()
        {
            return this.name;
        }

        public void setType(string type)
        {
            this.type = type;
        }

        public string getType()
        {
            return this.type;
        }

        public void setUsable(bool usable)
        {
            this.usable = usable;
        }

        public bool getUsable()
        {
            return this.usable;
        }

        public void setCanRemove(bool canRemove)
        {
            this.canRemove = canRemove;
        }

        public bool getCanRemove()
        {
            return this.canRemove;
        }
    }
}