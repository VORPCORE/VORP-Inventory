using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VORP.Inventory.Shared;

namespace VorpInventory
{
    class Configuration
    {
        private static Config _config;
        private static Dictionary<string, string> _language = new();
        public static Dictionary<string, Weapon> Weapons = new();

        public static long KEY_OPEN_INVENTORY = 0xC1989F95;
        public static long KEY_PICKUP_ITEM = 0xF84FA74F;

        private static Config GetConfig()
        {
            if (_config is not null) return _config;

            string fileContents = LoadResourceFile(GetCurrentResourceName(), "/config.json");
            _config = JsonConvert.DeserializeObject<Config>(fileContents);

            UpdateControl(true, _config.PickupKey);
            UpdateControl(false, _config.OpenKey);

            _config.Weapons.ForEach(weapon =>
            {
                if (!Weapons.ContainsKey(weapon.HashName))
                    Weapons.Add(weapon.HashName, weapon);
            });

            string language = _config.Defaultlanguage;
            if (string.IsNullOrEmpty(language))
                language = "en_lang";

            string languageFileContents = LoadResourceFile(GetCurrentResourceName(), $"/languages/{language}.json");

            if (!string.IsNullOrEmpty(languageFileContents))
            {
                _language = JsonConvert.DeserializeObject<Dictionary<string, string>>(languageFileContents);
            }

            return _config;
        }

        public static string GetTranslation(string translationKey)
        {
            if (!_language.ContainsKey(translationKey)) return $"Translation '{translationKey}' is missing.";
            return _language[translationKey];
        }

        public static Config Config => GetConfig();

        public static string GetWeaponLabel(string hash)
        {
            if (Weapons.ContainsKey(hash))
                return Weapons[hash].Name;

            return hash;
        }

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

        //public static JObject Config = new JObject();
        //public static Dictionary<string, string> Langs = new Dictionary<string, string>();
        //public static uint openKey = 0;
        //public static uint pickupKey = 0;
        //public static bool loaded = false;
        //public static Dictionary<string, Weapon> Weapons = new();

        //public GetConfig()
        //{
        //    EventHandlers[$"{API.GetCurrentResourceName()}:SendConfig"] += new Action<string, ExpandoObject>(LoadDefaultConfig);
        //    TriggerServerEvent($"{API.GetCurrentResourceName()}:getConfig");
        //}

        //private void LoadDefaultConfig(string dc, ExpandoObject dl)
        //{

        //    Config = JObject.Parse(dc);

        //    foreach (var l in dl)
        //    {
        //        Langs[l.Key] = l.Value.ToString();
        //    }

        //    openKey = FromHex(Config["OpenKey"].ToString());

        //    pickupKey = FromHex(Config["PickupKey"].ToString());

        //    Pickups.SetupPickPrompt();

        //    loaded = true;

        //    try
        //    {
        //        foreach (var wpn in Config["Weapons"])
        //        {
        //            Weapon weapon = new Weapon();
        //            weapon.Name = $"{wpn["Name"]}";
        //            weapon.Hash = $"{wpn["HashName"]}";
        //            weapon.WeaponModel = $"{wpn["WeaponModel"]}";
        //            Weapons.Add(weapon.Hash, weapon);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex, "Error when trying to create Weapons Dictionary");
        //    }
        //}
    }
}
