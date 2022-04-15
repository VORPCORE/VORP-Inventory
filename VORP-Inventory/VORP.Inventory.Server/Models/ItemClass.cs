namespace VORP.Inventory.Server.Models
{
    public class ItemClass
    {
        public int Count { get; set; }
        public int Limit { get; set; }
        public string Label { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Usable { get; set; }
        public bool CanRemove { get; set; }

        public void AddCount(int count)
        {
            if (Count + count <= Limit)
            {
                Count += count;
            }
        }

        public int Subtract(int amountToSubtract)
        {
            if (amountToSubtract < 0)
            {
                amountToSubtract = 0;
            }

            Count -= amountToSubtract;
            return Count;
        }
    }
}