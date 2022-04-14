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
                Logger.Trace($"{o.Key}");
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
                if (target == localPed) continue;

                Vector3 targetCoords = API.GetEntityCoords(target, true, true);
                float distanceBetween = API.GetDistanceBetweenCoords(targetCoords.X, targetCoords.Y, targetCoords.Z,
                    coords.X, coords.Y, coords.Z, false);

                if (closestDistance > distanceBetween)
                {
                    closestPlayers.Add(player);
                }
            }

#if DEVELOPMENT
            closestPlayers.Add(API.PlayerId());
#endif

            return closestPlayers;
        }

#if DEVELOPMENT
        // This should be replaced with prompts, solution requires designing.
        public static void Draw3DText(Vector3 position, string text)
        {
            float _x = 0.0F;
            float _y = 0.0F;
            API.GetScreenCoordFromWorldCoord(position.X, position.Y, position.Z, ref _x, ref _y);
            API.SetTextScale(0.35F, 0.35F);
            API.SetTextFontForCurrentCommand(1);
            API.SetTextColor(255, 255, 255, 215);
            long str = Function.Call<long>(Hash._CREATE_VAR_STRING, 10, "LITERAL_STRING", text);
            Function.Call((Hash)0xBE5261939FBECB8C, 1);
            Function.Call((Hash)0xD79334A4BB99BAD1, str, _x, _y);
            float factor = text.Length / 150.0F;
            Function.Call((Hash)0xC9884ECADE94CB34, "generic_textures", "hud_menu_4a", _x, _y + 0.0125F, 0.015F + factor,
                0.03F, 0.1F, 100, 1, 1, 190, 0);
        }
#endif
    }
}