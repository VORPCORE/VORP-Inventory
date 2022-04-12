using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
#if CLIENT
using VORP.Inventory.Client.Scripts;
#endif
using VORP.Inventory.Shared.Models;

namespace VORP.Inventory.Shared
{
    class Configuration
    {
        private static Config _config;
        private static Dictionary<string, string> _language = new();
        public static Dictionary<string, Weapon> Weapons = new();

#if CLIENT
        public static long KEY_OPEN_INVENTORY = 0xC1989F95;
        public static long KEY_PICKUP_ITEM = 0xF84FA74F;
#endif

#if SERVER
        public static int INVENTORY_MAX_ITEMS = 0;
        public static int INVENTORY_MAX_WEAPONS = 0;
#endif

        private static Config GetConfig()
        {
            try
            {
                if (_config is not null) return _config;

                string fileContents = LoadResourceFile(GetCurrentResourceName(), "/config.json");

                if (string.IsNullOrEmpty(fileContents))
                {
                    Logger.CriticalError($"config.json was not found.");
                }

                _config = JsonConvert.DeserializeObject<Config>(fileContents);

#if CLIENT
                UpdateControl(true, _config.PickupKey);
                UpdateControl(false, _config.OpenKey);
#endif

#if SERVER
                INVENTORY_MAX_ITEMS = _config.MaxItemsInInventory.Items;
                INVENTORY_MAX_WEAPONS = _config.MaxItemsInInventory.Weapons;

                Logger.Trace($"INVENTORY_MAX_ITEMS: {INVENTORY_MAX_ITEMS}");
                Logger.Trace($"INVENTORY_MAX_WEAPONS: {INVENTORY_MAX_WEAPONS}");
#endif

                LoadWeapons();
                LoadLanguage();

                return _config;
            }
            catch (Exception ex)
            {
                Logger.CriticalError(ex, $"Configuration.GetConfig");
                return null;
            }
        }

        private static void LoadWeapons()
        {
            _config.Weapons.ForEach(weapon =>
            {
                if (!Weapons.ContainsKey(weapon.HashName))
                    Weapons.Add(weapon.HashName, weapon);
            });

            Logger.Trace($"Weapons Loaded: {Weapons.Count}");
        }

        private static void LoadLanguage()
        {
            string language = _config.Defaultlanguage;
            if (string.IsNullOrEmpty(language))
                language = "en_lang";

            string languageFileContents = LoadResourceFile(GetCurrentResourceName(), $"/languages/{language}.json");

            if (!string.IsNullOrEmpty(languageFileContents))
            {
                _language = JsonConvert.DeserializeObject<Dictionary<string, string>>(languageFileContents);
                Logger.Trace($"Language Loaded: {language}");
            }
            else
            {
                languageFileContents = LoadResourceFile(GetCurrentResourceName(), $"/languages/en_lang.json");
                _language = JsonConvert.DeserializeObject<Dictionary<string, string>>(languageFileContents);
                Logger.Trace($"Language '{language}.json' was not found, defaulted to en_lang.json.");
            }
        }

        public static string GetTranslation(string translationKey)
        {
            if (!_language.ContainsKey(translationKey)) return $"Translation '{translationKey}' is missing.";
            return _language[translationKey];
        }

        public static Config Config => GetConfig();

#if CLIENT
        public static string GetHashReadableLabel(string hash)
        {
            try
            {
                if (HasWeaponHashName(hash))
                    return GetWeaponLabel(hash);

                if (InventoryAPI.citems.ContainsKey(hash))
                    return InventoryAPI.citems[hash]["label"];

                return hash;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GetHashReadableLable");
                return hash;
            }
        }
#endif

        public static string GetWeaponLabel(string hash)
        {
            if (Weapons.ContainsKey(hash))
                return Weapons[hash].Name;

            return hash;
        }

        public static bool HasWeaponHashName(string hashName) => Config.Weapons.Any(weapon => weapon.HashName == hashName);

        private static long FromHex(string value)
        {
            try
            {
                return Convert.ToInt64(value, 16);
            }
            catch (FormatException ex)
            {
                Logger.Error(ex, $"Invalid number format.");
                return 0;
            }
        }

#if CLIENT
        private static void UpdateControl(bool isPickupKey, string keyHex)
        {
            long keyValue = FromHex(keyHex);
            if (keyValue > 0)
            {
                if (isPickupKey)
                    KEY_PICKUP_ITEM = keyValue;
                if (!isPickupKey)
                    KEY_OPEN_INVENTORY = keyValue;
            }
        }

#endif
    }
}
