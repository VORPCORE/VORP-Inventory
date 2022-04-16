using System.Runtime.Serialization;

namespace VORP.Inventory.Server.Models
{
    [DataContract]
    public class ItemClass
    {
        [DataMember(Name = "count")]
        public int Count { get; set; }

        [DataMember(Name = "limit")]
        public int Limit { get; set; }

        [DataMember(Name = "label")]
        public string Label { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "usable")]
        public bool Usable { get; set; }

        [DataMember(Name = "canRemove")]
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