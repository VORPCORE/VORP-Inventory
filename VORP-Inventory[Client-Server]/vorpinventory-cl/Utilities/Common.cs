namespace VorpInventory.Utilities
{
    public class Common
    {
        public static string GetTranslation(string langKey)
        {
            if (!PluginManager.Langs.ContainsKey(langKey)) return $"'{langKey}' Translation missing";
            return PluginManager.Langs[langKey];
        }
    }
}
