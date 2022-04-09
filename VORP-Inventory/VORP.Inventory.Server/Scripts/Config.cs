using CitizenFX.Core;
using CitizenFX.Core.Native;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VorpInventory.Scripts
{
    public class Config : BaseScript
    {
        private static string _ConfigString;
        private static JObject _configJObject = new JObject();
        private readonly static string _resourcePath = $"{API.GetResourcePath(API.GetCurrentResourceName())}";


        public static Dictionary<string, string> Lang = new Dictionary<string, string>();

        PlayerList PlayerList => PluginManager.PlayerList;

        internal Config()
        {
            
            EventHandlers[$"{API.GetCurrentResourceName()}:getConfig"] += new Action<Player>(OnGetConfig);

            SetupConfig();
        }

        #region Public Methods
        public static string GetTranslation(string key)
        {
            if (!Lang.ContainsKey(key))
            {
                return $"Translation not found for '{key}'.";
            }

            return Lang[key];
        }

        public static bool HasWeaponHashName(string hashName)
        {
            if (_configJObject["Weapons"].Any(weapon => weapon["HashName"].ToString() == hashName))
            {
                return true;
            }

            return false;
        }
        #endregion

        #region Private Methods
        private static void SetupConfig()
        {
            if (File.Exists($"{_resourcePath}/Config.json"))
            {
                _ConfigString = File.ReadAllText($"{_resourcePath}/Config.json", Encoding.UTF8);
                _configJObject = JObject.Parse(_ConfigString);
                if (File.Exists($"{_resourcePath}/languages/{_configJObject["defaultlang"]}.json"))
                {
                    string langstring = File.ReadAllText($"{_resourcePath}/languages/{_configJObject["defaultlang"]}.json",
                        Encoding.UTF8);
                    Lang = JsonConvert.DeserializeObject<Dictionary<string, string>>(langstring);

                    Logger.Info($"{API.GetCurrentResourceName()}: Language {_configJObject["defaultlang"]}.json loaded!");

                }
                else
                {
                    Logger.Error($"{API.GetCurrentResourceName()}: {_configJObject["defaultlang"]}.json Not Found");
                }
            }
        }

        private void OnGetConfig([FromSource] Player source)
        {
            source.TriggerEvent($"{API.GetCurrentResourceName()}:SendConfig", _ConfigString, Lang);
        }
        #endregion
    }
}