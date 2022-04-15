namespace VORP.Inventory.Client.Models
{
    public class ItemClass
    {
        /// <summary>
        /// Ammo in case of weapon, cuantity in case of item
        /// </summary>
        public int Count { get; set; }
        public int Limit { get; set; }
        public string Label { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Weapon or item
        /// </summary>
        public string Type { get; set; }
        public bool Usable { get; set; }
        public bool CanRemove { get; set; }
    }
}
