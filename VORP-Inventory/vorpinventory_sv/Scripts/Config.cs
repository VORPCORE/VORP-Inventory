using CitizenFX.Core;
using CitizenFX.Core.Native;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using VorpInventory.Diagnostics;

namespace VorpInventory.Scripts
{
    public class Config : BaseScript
    {
        private static string _ConfigString;
        private static JObject _configJObject = new JObject();
        private readonly static string _resourcePath = $"{API.GetResourcePath(API.GetCurrentResourceName())}";
        
        public static int MaxItems = 0;
        public static int MaxWeapons = 0;
        public static Dictionary<string, string> Lang = new Dictionary<string, string>();

        PlayerList PlayerList => PluginManager.PlayerList;

        internal Config()
        {
            EventHandlers["vorp_NewCharacter"] += new Action<int>(OnNewCharacter);
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

                    Logger.Success($"{API.GetCurrentResourceName()}: Language {_configJObject["defaultlang"]}.json loaded!");

                }
                else
                {
                    Logger.Error($"{API.GetCurrentResourceName()}: {_configJObject["defaultlang"]}.json Not Found");
                }
            }

            MaxItems = _configJObject["MaxItemsInInventory"]["Items"].ToObject<int>();
            MaxWeapons = _configJObject["MaxItemsInInventory"]["Weapons"].ToObject<int>();

            if (MaxItems < 0) MaxItems = 0;
            if (MaxWeapons < 0) MaxWeapons = 0;
        }

        private void OnGetConfig([FromSource] Player source)
        {
            source.TriggerEvent($"{API.GetCurrentResourceName()}:SendConfig", _ConfigString, Lang);
        }

        private async void OnNewCharacter(int player)
        {
            await Delay(5000);
            Player p = PlayerList[player];
            if (p == null)
            {
                Logger.Error($"Player '{player}' was not found.");
                return;
            }

            string identifier = "steam:" + p.Identifiers["steam"];
            try
            {
                foreach (KeyValuePair<string, JToken> item in (JObject)_configJObject["startItems"][0])
                {
                    TriggerEvent("vorpCore:addItem", player, item.Key, int.Parse(item.Value.ToString()));
                }

                foreach (KeyValuePair<string, JToken> weapon in (JObject)_configJObject["startItems"][1])
                {
                    JToken wpc = _configJObject["Weapons"].FirstOrDefault(x => x["HashName"].ToString().Contains(weapon.Key));
                    List<string> auxbullets = new List<string>();
                    Dictionary<string, int> givedBullets = new Dictionary<string, int>();
                    foreach (KeyValuePair<string, JToken> bullets in (JObject)wpc["AmmoHash"][0])
                    {
                        auxbullets.Add(bullets.Key);
                    }
                    foreach (KeyValuePair<string, JToken> bullet in (JObject)weapon.Value[0])
                    {
                        if (auxbullets.Contains(bullet.Key))
                        {
                            givedBullets.Add(bullet.Key, int.Parse(bullet.Value.ToString()));
                        }
                    }
                    TriggerEvent("vorpCore:registerWeapon", player, weapon.Key, givedBullets);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        #endregion
    }
}