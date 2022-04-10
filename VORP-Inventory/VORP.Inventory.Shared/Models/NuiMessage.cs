using System.Runtime.Serialization;

namespace VORP.Inventory.Shared.Models
{
    /// <summary>
    /// Currently isn't fully implemented due to the crap in the NUI HTML, that requires some documentation.
    /// </summary>
    [DataContract]
    public class NuiMessage
    {
        [DataMember(Name = "action", EmitDefaultValue = false)]
        public string Action { get; set; }

        [DataMember(Name = "player", EmitDefaultValue = false)]
        public int PlayerID { get; set; }

        [DataMember(Name = "item", EmitDefaultValue = false)] // this keeps changing in the NUI
        public string Item { get; set; }

        // Yea... WHAT?!
        [DataMember(Name = "what", EmitDefaultValue = false)]
        public string What { get; set; }

        [DataMember(Name = "type", EmitDefaultValue = false)]
        public string Type { get; set; }

        [DataMember(Name = "title", EmitDefaultValue = false)]
        public string Title { get; set; }

        [DataMember(Name = "hash", EmitDefaultValue = false)]
        public string Hash { get; set; }

        [DataMember(Name = "amount", EmitDefaultValue = false)]
        public double Amount { get; set; }

        [DataMember(Name = "id", EmitDefaultValue = false)]
        public int ID { get; set; }

        [DataMember(Name = "number", EmitDefaultValue = false)]
        public int Number { get; set; }

        [DataMember(Name = "horse", EmitDefaultValue = false)]
        public int HorseID { get; set; }

        [DataMember(Name = "wagon", EmitDefaultValue = false)]
        public int WagonID { get; set; }

        [DataMember(Name = "hideout", EmitDefaultValue = false)]
        public int HideoutID { get; set; }

        [DataMember(Name = "house", EmitDefaultValue = false)]
        public int HouseID { get; set; }

        [DataMember(Name = "clan", EmitDefaultValue = false)]
        public int ClanID { get; set; }

        [DataMember(Name = "steal", EmitDefaultValue = false)]
        public int StealID { get; set; }

        [DataMember(Name = "container", EmitDefaultValue = false)]
        public int ContainerID { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}
