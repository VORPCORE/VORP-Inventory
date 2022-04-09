using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Collections.Generic;
using System.Threading.Tasks;
using VorpInventory.Models;

namespace VorpInventory
{
    public class Utils : BaseScript
    {
        public Utils()
        {

        }

        public static void cleanAmmo(int id)
        {
            if (PluginManager.userWeapons.ContainsKey(id))
            {
                API.SetPedAmmo(API.PlayerPedId(), (uint)API.GetHashKey(PluginManager.userWeapons[id].getName()), 0);
                foreach (KeyValuePair<string, int> ammo in PluginManager.userWeapons[id].getAllAmmo())
                {
                    API.SetPedAmmoByType(API.PlayerPedId(), API.GetHashKey(ammo.Key), 0);
                }
            }
        }

        public static void useWeapon(int id)
        {
            //API.GetCurrentPedWeapon(API.PlayerPedId(), ref weaponHash, false, 0, false);

            if (PluginManager.userWeapons[id].getUsed2())
            {
                API.GiveWeaponToPed_2(API.PlayerPedId(), (uint)API.GetHashKey(PluginManager.userWeapons[id].getName()), 0, true, true, 3, false, 0.5f, 1.0f, 752097756, false, 0, false);
                Function.Call((Hash)0xADF692B254977C0C, API.PlayerPedId(), API.GetHashKey(PluginManager.userWeapons[id].getName()), 0, 0, 0, 0);
                API.SetPedAmmo(API.PlayerPedId(), (uint)API.GetHashKey(PluginManager.userWeapons[id].getName()), 0);
                foreach (KeyValuePair<string, int> ammos in PluginManager.userWeapons[id].getAllAmmo())
                {
                    API.SetPedAmmoByType(API.PlayerPedId(), API.GetHashKey(ammos.Key), ammos.Value);
                    Debug.WriteLine($"{API.GetHashKey(ammos.Key)}: {ammos.Key} {ammos.Value}");
                }
            }
            else
            {
                oldUseWeapon(id);
            }
        }

        public static void oldUseWeapon(int id)
        {
            //API.GiveDelayedWeaponToPed(API.PlayerPedId(), (uint)API.GetHashKey(vorp_inventoryClient.userWeapons[id].getName()), 0, true, 2);
            API.GiveWeaponToPed_2(API.PlayerPedId(), (uint)API.GetHashKey(PluginManager.userWeapons[id].getName()), 0, true, true, 2, false, 0.5f, 1.0f, 752097756, false, 0, false);
            Function.Call((Hash)0xADF692B254977C0C, API.PlayerPedId(), API.GetHashKey(PluginManager.userWeapons[id].getName()), 0, 1, 0, 0);
            API.SetPedAmmo(API.PlayerPedId(), (uint)API.GetHashKey(PluginManager.userWeapons[id].getName()), 0);
            foreach (KeyValuePair<string, int> ammos in PluginManager.userWeapons[id].getAllAmmo())
            {
                API.SetPedAmmoByType(API.PlayerPedId(), API.GetHashKey(ammos.Key), ammos.Value);
                Debug.WriteLine($"{API.GetHashKey(ammos.Key)}: {ammos.Key} {ammos.Value}");
            }

            PluginManager.userWeapons[id].setUsed(true);
            TriggerServerEvent("vorpinventory:setUsedWeapon", id, PluginManager.userWeapons[id].getUsed(), PluginManager.userWeapons[id].getUsed2());
        }

        public static void addItems(string name, int cuantity)
        {
            if (PluginManager.useritems.ContainsKey(name))
            {
                PluginManager.useritems[name].addCount(cuantity);
            }
            else
            {
                PluginManager.useritems.Add(name, new ItemClass(cuantity, PluginManager.citems[name]["limit"],
                    PluginManager.citems[name]["label"], name,
                    "item_standard", true, PluginManager.citems[name]["can_remove"]));
            }
        }

        public static async Task DrawText3D(Vector3 position, string text)
        {
            float _x = 0.0F;
            float _y = 0.0F;
            //Debug.WriteLine(position.X.ToString());
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

        public static Dictionary<string, dynamic> ProcessDynamicObject(dynamic dynObject)
        {
            Dictionary<string, dynamic> aux = new Dictionary<string, dynamic>();
            foreach (var o in dynObject)
            {
                aux.Add(o.Key, o.Value);
            }
            return aux;
        }

        public static List<int> getNearestPlayers(float distance = 5.0f)
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

        public static Dictionary<uint, string> Publicweapons = new Dictionary<uint, string>
        {
            {0xC3662B7D,"WEAPON_KIT_CAMERA"},
            {0xAED4C64C,"WEAPON_MOONSHINEJUG"},
            {0x3155643F,"WEAPON_MELEE_LANTERN_ELECTRIC"},
            {0xDDD2F685,"WEAPON_MELEE_TORCH "},
            {0xF79190B4,"WEAPON_MELEE_BROKEN_SWORD"},
            {0xABA87754,"WEAPON_FISHINGROD"},
            {0x9E12A01,"WEAPON_MELEE_HATCHET"},
            {0xEF32A25D,"WEAPON_MELEE_CLEAVER"},
            {0x21CCCA44,"WEAPON_MELEE_ANCIENT_HATCHET"},
            {0x74DC40ED,"WEAPON_MELEE_HATCHET_VIKING"},
            {0x1C02870C,"WEAPON_MELEE_HATCHET_HEWING"},
            {0xBCC63763,"WEAPON_MELEE_HATCHET_DOUBLE_BIT"},
            {0x8F0FDE0E,"WEAPON_MELEE_HATCHET_DOUBLE_BIT_RUSTED"},
            {0x2A5CF9D6,"WEAPON_MELEE_HATCHET_HUNTER"},
            {0xE470B7AD,"WEAPON_MELEE_HATCHET_HUNTER_RUSTED"},
            {0x1D7D0737,"WEAPON_MELEE_KNIFE_JOHN"},
            {0xDB21AC8C,"WEAPON_MELEE_KNIFE"},
            {0x1086D041,"WEAPON_MELEE_KNIFE_JAWBONE"},
            {0xD2718D48,"WEAPON_THROWN_THROWING_KNIVES"},
            {0xC45B2DE,"WEAPON_MELEE_KNIFE_MINER"},
            {0xDA54DD53,"WEAPON_MELEE_KNIFE_CIVIL_WAR"},
            {0x2BC12CDA,"WEAPON_MELEE_KNIFE_BEAR"},
            {0x14D3F94D,"WEAPON_MELEE_KNIFE_VAMPIRE"},
            {0x7A8A724A,"WEAPON_LASSO"},
            {0x28950C71,"WEAPON_MELEE_MACHETE"},
            {0xA5E972D7,"WEAPON_THROWN_TOMAHAWK"},
            {0x7F23B6C7,"WEAPON_THROWN_TOMAHAWK_ANCIENT"},
            {0x54E120F2,"WEAPON_PISTOL_M1899AMMO_PISTOL"},
            {0x8580C63E,"WEAPON_PISTOL_MAUSER"},
            {0x4AAE5FFA,"WEAPON_PISTOL_MAUSER_DRUNK"},
            {0x657065D6,"WEAPON_PISTOL_SEMIAUTO"},
            {0x20D13FF,"WEAPON_PISTOL_VOLCANIC"},
            {0xF5175BA1,"WEAPON_REPEATER_CARBINE"},
            {0x7194721E,"WEAPON_REPEATER_EVANS"},
            {0x95B24592,"WEAPON_REPEATER_HENRY"},
            {0xDDF7BC1E,"WEAPON_RIFLE_VARMINT"},
            {0xA84762EC,"WEAPON_REPEATER_WINCHESTER"},
            {0x169F59F7,"WEAPON_REVOLVER_CATTLEMAN"},
            {0xC9622757,"WEAPON_REVOLVER_CATTLEMAN_JOHN"},
            {0x16D655F7,"WEAPON_REVOLVER_CATTLEMAN_MEXICAN"},
            {0xF5E4207F,"WEAPON_REVOLVER_CATTLEMAN_PIG"},
            {0x797FBF5,"WEAPON_REVOLVER_DOUBLEACTION"},
            {0x23C706CD,"WEAPON_REVOLVER_DOUBLEACTION_EXOTIC"},
            {0x83DD5617,"WEAPON_REVOLVER_DOUBLEACTION_GAMBLER"},
            {0x2300C65,"WEAPON_REVOLVER_DOUBLEACTION_MICAH"},
            {0x5B2D26B5,"WEAPON_REVOLVER_LEMAT"},
            {0x7BBD1FF6,"WEAPON_REVOLVER_SCHOFIELD"},
            {0xE195D259,"WEAPON_REVOLVER_SCHOFIELD_GOLDEN"},
            {0x247E783,"WEAPON_REVOLVER_SCHOFIELD_CALLOWAY"},
            {0x772C8DD6,"WEAPON_RIFLE_BOLTACTION"},
            {0x53944780,"WEAPON_SNIPERRIFLE_CARCANO"},
            {0xE1D2B317,"WEAPON_SNIPERRIFLE_ROLLINGBLOCK"},
            {0x4E328256,"WEAPON_SNIPERRIFLE_ROLLINGBLOCK_EXOTIC"},
            {0x63F46DE6,"WEAPON_RIFLE_SPRINGFIELD"},
            {0x6DFA071B,"WEAPON_SHOTGUN_DOUBLEBARREL"},
            {0x2250E150,"WEAPON_SHOTGUN_DOUBLEBARREL_EXOTIC"},
            {0x31B7B9FE,"WEAPON_SHOTGUN_PUMP"},
            {0x63CA782A,"WEAPON_SHOTGUN_REPEATING"},
            {0x1765A8F8,"WEAPON_SHOTGUN_SAWEDOFF"},
            {0x6D9BB970,"WEAPON_SHOTGUN_SEMIAUTO"},
            {0x88A8505C,"WEAPON_BOW"},
            {0xA64DAA5E,"WEAPON_THROWN_DYNAMITE"},
            {0x9102371A,"WEAPON_THROWN_MOLOTOV"}
        };

        public static Dictionary<uint, string> Publicammo = new Dictionary<uint, string>
        {
            {0x38854A3B,"AMMO_ARROW"},
            {0x5B6ABDF8,"AMMO_ARROW_DYNAMITE"},
            {0xD19E0045,"AMMO_ARROW_FIRE"},
            {0xD62A5A6C,"AMMO_ARROW_POISON"},
            {0x3250353B,"AMMO_ARROW_IMPROVED"},
            {0x9BD3BBB,"AMMO_ARROW_SMALL_GAME"},
            {0xBB8A699D,"AMMO_DYNAMITE"},
            {0xDC91634B,"AMMO_DYNAMITE_VOLATILE"},
            {0x6AB063DE,"AMMO_MOLOTOV"},
            {0xF8FB3AC1,"AMMO_MOLOTOV_VOLATILE"},
            {0x80766738,"AMMO_PISTOL"},
            {0xF1A91F32,"AMMO_PISTOL_EXPRESS"},
            {0x331B008B,"AMMO_PISTOL_EXPRESS_EXPLOSIVE"},
            {0x46F7AA64,"AMMO_PISTOL_HIGH_VELOCITY"},
            {0xAD60BB5F,"AMMO_PISTOL_SPLIT_POINT"},
            {0x5E490BAA,"AMMO_REPEATER"},
            {0x197A9C10,"AMMO_REPEATER_EXPRESS"},
            {0x2390F9C2,"AMMO_REPEATER_EXPRESS_EXPLOSIVE"},
            {0x4FFBFA8C,"AMMO_REPEATER_HIGH_VELOCITY"},
            {0x4CE87556,"AMMO_REVOLVER"},
            {0x3C932F5C,"AMMO_REVOLVER_EXPRESS"},
            {0xAFD00F7F,"AMMO_REVOLVER_EXPRESS_EXPLOSIVE"},
            {0x129C46F,"AMMO_REVOLVER_HIGH_VELOCITY"},
            {0x9C6310D4,"AMMO_REVOLVER_SPLIT_POINT"},
            {0xBAFF5180,"AMMO_RIFLE"},
            {0x2CE404A4,"AMMO_RIFLE_EXPRESS"},
            {0x9116173,"AMMO_RIFLE_EXPRESS_EXPLOSIVE"},
            {0xF76DC763,"AMMO_RIFLE_HIGH_VELOCITY"},
            {0xC1711828,"AMMO_RIFLE_SPLIT_POINT"},
            {0xB7DB96B8,"AMMO_RIFLE_VARMINT"},
            {0x58B272F9,"AMMO_SHOTGUN"},
            {0xC71EE56D,"AMMO_SHOTGUN_BUCKSHOT_INCENDIARY"},
            {0x3450D03C,"AMMO_SHOTGUN_SLUG"},
            {0x4BB641AD,"AMMO_SHOTGUN_EXPRESS_EXPLOSIVE"},
            {0xCFE15715,"AMMO_THROWING_KNIVES"},
            {0xB846FB5B,"AMMO_THROWING_KNIVES_IMPROVED"},
            {0x9AE0598E,"AMMO_THROWING_KNIVES_POISON"},
            {0xB09A8B19,"AMMO_TOMAHAWK"},
            {0x7B87DF4F,"AMMO_TOMAHAWK_HOMING"},
            {0x4F384312,"AMMO_TOMAHAWK_IMPROVED"}
        };

    }
}