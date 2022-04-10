using System.Runtime.Serialization;

namespace VORP.Inventory.Shared.Models
{
    [DataContract]
    public class NuiMessage
    {
        [DataMember(Name = "player")]
        public int PlayerID { get; set; }

        [DataMember(Name = "item")]
        public string Item { get; set; }

        [DataMember(Name = "what")]
        public string Action { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "hash")]
        public string Hash { get; set; }

        [DataMember(Name = "amount")]
        public double Amount { get; set; }

        [DataMember(Name = "id")]
        public int ID { get; set; }

        [DataMember(Name = "number")]
        public int Number { get; set; }

        [DataMember(Name = "horse")]
        public int HorseID { get; set; }

        [DataMember(Name = "wagon")]
        public int WagonID { get; set; }

        [DataMember(Name = "hideout")]
        public int HideoutID { get; set; }

        [DataMember(Name = "house")]
        public int HouseID { get; set; }

        [DataMember(Name = "clan")]
        public int ClanID { get; set; }

        [DataMember(Name = "steal")]
        public int StealID { get; set; }

        [DataMember(Name = "container")]
        public int ContainerID { get; set; }
    }
}
