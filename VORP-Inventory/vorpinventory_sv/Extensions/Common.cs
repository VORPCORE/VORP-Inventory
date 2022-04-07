using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using VorpInventory.Diagnostics;

namespace VorpInventory.Extensions
{
    static class Common
    {
        public async static Task<dynamic> GetCoreUser(this Player player)
        {
            if (PluginManager.CORE == null)
            {
                Logger.Error($"GetCoreUser: Core API is null");
                return null;
            }

            dynamic core = await PluginManager.CORE();

            return core.getUser(int.Parse(player.Handle));
        }
        public async static Task<dynamic> GetCoreUser(int playerHandle)
        {
            if (PluginManager.CORE == null)
            {
                Logger.Error($"GetCoreUser: Core API is null");
                return null;
            }

            dynamic core = await PluginManager.CORE();

            return core.getUser(playerHandle);
        }

        public async static Task<dynamic> GetCoreUserCharacter(this Player player)
        {
            dynamic coreUser = await player.GetCoreUser();
            if (coreUser == null)
            {
                Logger.Warn($"GetCoreUser: Player '{player.Handle}' does not exist.");
            }
            return coreUser.getUsedCharacter;
        }

        public async static Task<int> GetCoreUserCharacterId(this Player player)
        {
            dynamic character = await player.GetCoreUserCharacter();

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
