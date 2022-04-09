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

        private async void OnNewCharacter(int playerId)
        {
            await Delay(5000);

            Player player = PlayerList[playerId];
            if (player == null)
            {
                Logger.Error($"Player '{playerId}' was not found.");
                return;
            }

            string identifier = "steam:" + player.Identifiers["steam"];

            // Attempt to add all starter items/weapons from the Config.json
            try
            {
                JObject items = (JObject)_configJObject["startItems"].FirstOrDefault();
                if (items != null)
                {
                    foreach (KeyValuePair<string, JToken> item in items)
                    {
                        TriggerEvent("vorpCore:addItem", playerId, item.Key, int.Parse(item.Value.ToString()));
                    }
                }

                JObject weapons = (JObject)_configJObject["startWeapons"].FirstOrDefault();
                if (weapons != null)
                {
                    foreach (KeyValuePair<string, JToken> weapon in weapons)
                    {
                        List<string> auxbullets = new List<string>();
                        Dictionary<string, int> receivedBullets = new Dictionary<string, int>();

                        JToken wpc = _configJObject["Weapons"].FirstOrDefault(x => x["HashName"].ToString() == weapon.Key);

                        JObject ammoHash = (JObject)wpc["AmmoHash"].FirstOrDefault();
                        if (ammoHash != null)
                        {
                            foreach (KeyValuePair<string, JToken> bullets in ammoHash)
                            {
                                auxbullets.Add(bullets.Key);
                            }
                        }

                        JObject staterAmmo = (JObject)weapon.Value.FirstOrDefault();
                        if (staterAmmo != null)
                        {
                            foreach (KeyValuePair<string, JToken> bullet in staterAmmo)
                            {
                                if (auxbullets.Contains(bullet.Key))
                                {
                                    receivedBullets.Add(bullet.Key, int.Parse(bullet.Value.ToString()));
                                }
                            }
                        }

                        TriggerEvent("vorpCore:registerWeapon", playerId, weapon.Key, receivedBullets);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"OnNewCharacter: {ex.Message}");
            }
        }
        #endregion
    }
}