using CitizenFX.Core;
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
    }
}
