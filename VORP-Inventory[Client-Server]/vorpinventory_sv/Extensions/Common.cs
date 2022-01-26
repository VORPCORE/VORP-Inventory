using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Dynamic;
using VorpInventory.Diagnostics;

namespace VorpInventory.Extensions
{
    static class Common
    {
        public static dynamic GetCoreUser(this Player player)
        {
            if (PluginManager.CORE == null)
            {
                Logger.Error($"GetCoreUser: Core API is null");
                return null;
            }

            return PluginManager.CORE.getUser(int.Parse(player.Handle));
        }

        public static dynamic GetCoreUserCharacter(this Player player)
        {
            dynamic coreUser = player.GetCoreUser();
            if (coreUser == null)
            {
                Logger.Error($"GetCoreUser: Player '{player.Handle}' does not exist.");
                return null;
            }
            return coreUser.getUsedCharacter;
        }

        public static int GetCoreUserCharacterId(this Player player)
        {
            dynamic coreUser = player.GetCoreUser();
            if (coreUser == null)
            {
                Logger.Error($"GetCoreUser: Player '{player.Handle}' does not exist.");
                return -1;
            }

            dynamic character = coreUser.getUsedCharacter;

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
