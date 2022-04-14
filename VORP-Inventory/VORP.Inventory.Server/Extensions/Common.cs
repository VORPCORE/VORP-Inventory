using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace VORP.Inventory.Server.Extensions
{
    static class Common
    {
        public async static Task<dynamic> GetCoreUserAsync(this Player player)
        {
            dynamic core = await PluginManager.GetVorpCoreAsync();
            return core.getUser(int.Parse(player.Handle));
        }

        public async static Task<dynamic> GetCoreUserAsync(int playerHandle)
        {
            dynamic core = await PluginManager.GetVorpCoreAsync();
            return core.getUser(playerHandle);
        }

        public async static Task<dynamic> GetCoreUserCharacterAsync(this Player player)
        {
            dynamic coreUser = await player.GetCoreUserAsync();
            if (coreUser == null)
            {
                Logger.Warn($"GetCoreUser: Player '{player.Handle}' does not exist.");
            }
            return coreUser.getUsedCharacter;
        }

        public async static Task<int> GetCoreUserCharacterIdAsync(this Player player)
        {
            dynamic character = await player.GetCoreUserCharacterAsync();

            if (character == null)
            {
                if (!PluginManager.ActiveCharacters.ContainsKey(player.Handle)) return -1;
                return PluginManager.ActiveCharacters[player.Handle];
            }

            if (!Common.HasProperty(character, "charIdentifier"))
            {
                if (!PluginManager.ActiveCharacters.ContainsKey(player.Handle)) return -1;
                return PluginManager.ActiveCharacters[player.Handle];
            }

            return character?.charIdentifier;
        }

        public static bool HasProperty(ExpandoObject obj, string propertyName)
        {
            return obj != null && ((IDictionary<String, object>)obj).ContainsKey(propertyName);
        }
    }
}
