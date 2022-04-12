using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using VORP.Inventory.Client.RedM;

namespace VORP.Inventory.Client.Models
{
    internal class Pickup
    {
        public string Name { get; set; }

        public int EntityId { get; set; }

        public double Amount { get; set; }

        public bool IsWeapon => WeaponId > 0;

        public bool IsMoney { get; set; } = false;

        public int WeaponId { get; set; }

        public Vector3 Position { get; set; }

        public float Distance
        {
            get
            {
                int playerPed = API.PlayerPedId();
                Vector3 coords = Function.Call<Vector3>((Hash)0xA86D5F069399F44D, playerPed, true, true);

                return Vdist(coords.X, coords.Y, coords.Z, Position.X, Position.Y, Position.Z);
            }
        }

        public bool IsInRange
        {
            get
            {
                return (Distance <= 5.0f);
            }
        }

        public Prompt Prompt { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
