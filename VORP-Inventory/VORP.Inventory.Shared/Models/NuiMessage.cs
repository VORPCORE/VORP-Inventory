using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VORP.Inventory.Shared.Models
{
    [DataContract]
    public class NuiPlayer
    {
        [DataMember(Name = "player", EmitDefaultValue = false)]
        public int PlayerID { get; set; }

        [DataMember(Name = "label", EmitDefaultValue = false)]
        public string PlayerName { get; set; }
    }

    /// <summary>
    /// Currently isn't fully implemented due to the crap in the NUI HTML, that requires some documentation.
    /// </summary>
    [DataContract]
    public class NuiMessage
    {
        [DataMember(Name = "data", EmitDefaultValue = false)]
        public NuiMessage Data { get; set; }

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

        [DataMember(Name = "count", EmitDefaultValue = false)]
        public double Count { get; set; }

        [DataMember(Name = "id", EmitDefaultValue = false)]
        public int ID { get; set; }

        [DataMember(Name = "number", EmitDefaultValue = false)]
        public int Number { get; set; }

        [DataMember(Name = "horse", EmitDefaultValue = false)]
        public int Horse { get; set; }

        [DataMember(Name = "horseid", EmitDefaultValue = false)]
        public int HorseId { get; set; }

        [DataMember(Name = "wagon", EmitDefaultValue = false)]
        public int Wagon { get; set; }

        [DataMember(Name = "wagonid", EmitDefaultValue = false)]
        public int WagonId { get; set; }

        [DataMember(Name = "hideout", EmitDefaultValue = false)]
        public int Hideout { get; set; }

        [DataMember(Name = "hideoutId", EmitDefaultValue = false)]
        public int HideoutId { get; set; }

        [DataMember(Name = "house", EmitDefaultValue = false)]
        public int House { get; set; }

        [DataMember(Name = "houseId", EmitDefaultValue = false)]
        public int HouseId { get; set; }

        [DataMember(Name = "clanid", EmitDefaultValue = false)]
        public int ClanId { get; set; }

        [DataMember(Name = "clan", EmitDefaultValue = false)]
        public int Clan { get; set; }

        [DataMember(Name = "steal", EmitDefaultValue = false)]
        public int Steal { get; set; }

        [DataMember(Name = "stealId", EmitDefaultValue = false)]
        public int StealId { get; set; }

        // HTML NUI File and external resources all use this base casing, heck this whole thing isn't even good JSON.... its a horror show.
        [DataMember(Name = "Container", EmitDefaultValue = false)]
        public int Container { get; set; }

        [DataMember(Name = "Containerid", EmitDefaultValue = false)]
        public int ContainerId { get; set; }

        [DataMember(Name = "players", EmitDefaultValue = false)]
        public List<NuiPlayer> Players { get; set; }

        [DataMember(Name = "foundAny", EmitDefaultValue = false)]
        public bool FoundAny { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}
