using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Collections.Generic;
using System.Threading.Tasks;
using VORP.Inventory.Client.Scripts;

namespace VORP.Inventory.Client
{
    public class Utils
    {
        public static void CleanAmmo(int id)
        {
            if (InventoryAPI.UsersWeapons.ContainsKey(id))
            {
                API.SetPedAmmo(API.PlayerPedId(), (uint)API.GetHashKey(InventoryAPI.UsersWeapons[id].getName()), 0);
                foreach (KeyValuePair<string, int> ammo in InventoryAPI.UsersWeapons[id].getAllAmmo())
                {
                    API.SetPedAmmoByType(API.PlayerPedId(), API.GetHashKey(ammo.Key), 0);
                }
            }
        }

        public static void UseWeapon(int id)
        {
            //API.GetCurrentPedWeapon(API.PlayerPedId(), ref weaponHash, false, 0, false);

            if (InventoryAPI.UsersWeapons[id].getUsed2())
            {
                API.GiveWeaponToPed_2(API.PlayerPedId(), (uint)API.GetHashKey(InventoryAPI.UsersWeapons[id].getName()), 0, true, true, 3, false, 0.5f, 1.0f, 752097756, false, 0, false);
                Function.Call((Hash)0xADF692B254977C0C, API.PlayerPedId(), API.GetHashKey(InventoryAPI.UsersWeapons[id].getName()), 0, 0, 0, 0);
                API.SetPedAmmo(API.PlayerPedId(), (uint)API.GetHashKey(InventoryAPI.UsersWeapons[id].getName()), 0);
                foreach (KeyValuePair<string, int> ammos in InventoryAPI.UsersWeapons[id].getAllAmmo())
                {
                    API.SetPedAmmoByType(API.PlayerPedId(), API.GetHashKey(ammos.Key), ammos.Value);
                }
            }
            else
            {
                OldUseWeapon(id);
            }
        }

        public static void OldUseWeapon(int id)
        {
            //API.GiveDelayedWeaponToPed(API.PlayerPedId(), (uint)API.GetHashKey(vorp_inventoryClient.userWeapons[id].getName()), 0, true, 2);
            API.GiveWeaponToPed_2(API.PlayerPedId(), (uint)API.GetHashKey(InventoryAPI.UsersWeapons[id].getName()), 0, true, true, 2, false, 0.5f, 1.0f, 752097756, false, 0, false);
            Function.Call((Hash)0xADF692B254977C0C, API.PlayerPedId(), API.GetHashKey(InventoryAPI.UsersWeapons[id].getName()), 0, 1, 0, 0);
            API.SetPedAmmo(API.PlayerPedId(), (uint)API.GetHashKey(InventoryAPI.UsersWeapons[id].getName()), 0);
            foreach (KeyValuePair<string, int> ammos in InventoryAPI.UsersWeapons[id].getAllAmmo())
            {
                API.SetPedAmmoByType(API.PlayerPedId(), API.GetHashKey(ammos.Key), ammos.Value);
            }

            InventoryAPI.UsersWeapons[id].setUsed(true);
            BaseScript.TriggerServerEvent("vorpinventory:setUsedWeapon", id, InventoryAPI.UsersWeapons[id].getUsed(), InventoryAPI.UsersWeapons[id].getUsed2());
        }

        public static Dictionary<string, dynamic> ProcessDynamicObject(dynamic dynObject)
        {
            Dictionary<string, dynamic> aux = new Dictionary<string, dynamic>();
            foreach (var o in dynObject)
            {
                aux.Add(o.Key, o.Value);
            }
            return aux;
        }

        public static List<int> GetNearestPlayers(float distance = 5.0f)
        {
            float closestDistance = distance;
            int localPed = API.PlayerPedId();
            Vector3 coords = API.GetEntityCoords(localPed, true, true);
            List<int> closestPlayers = new List<int>();
            List<int> players = new List<int>();
            foreach (var player in API.GetActivePlayers())
            {
                players.Add(player);
            }

            foreach (var player in players)
            {
                int target = API.GetPlayerPed(player);
                if (target != localPed)
                {
                    Vector3 targetCoords = API.GetEntityCoords(target, true, true);
                    float distanceBetween = API.GetDistanceBetweenCoords(targetCoords.X, targetCoords.Y, targetCoords.Z,
                        coords.X, coords.Y, coords.Z, false);

                    if (closestDistance > distanceBetween)
                    {
                        closestPlayers.Add(player);
                    }
                }
            }

            return closestPlayers;
        }
    }
}