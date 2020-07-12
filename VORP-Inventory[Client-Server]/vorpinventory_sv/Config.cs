using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace vorpinventory_sv
{
    public class Config : BaseScript
    {
        public static JObject config = new JObject();
        public static string ConfigString;
        public static Dictionary<string, string> lang = new Dictionary<string, string>();
        public static string resourcePath = $"{API.GetResourcePath(API.GetCurrentResourceName())}";

        public static int MaxItems = 0;
        public static int MaxWeapons = 0;

        public Config()
        {
            EventHandlers["vorp:firstSpawn"] += new Action<int>(itemsConfig);
            EventHandlers[$"{API.GetCurrentResourceName()}:getConfig"] += new Action<Player>(getConfig);

            if (File.Exists($"{resourcePath}/Config.json"))
            {
                ConfigString = File.ReadAllText($"{resourcePath}/Config.json", Encoding.UTF8);
                config = JObject.Parse(ConfigString);
                if (File.Exists($"{resourcePath}/languages/{config["defaultlang"]}.json"))
                {
                    string langstring = File.ReadAllText($"{resourcePath}/languages/{config["defaultlang"]}.json",
                        Encoding.UTF8);
                    lang = JsonConvert.DeserializeObject<Dictionary<string, string>>(langstring);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{API.GetCurrentResourceName()}: Language {config["defaultlang"]}.json loaded!");
                    Console.ForegroundColor = ConsoleColor.White;
                    // Debug.WriteLine($"{config["startItems"][0]}");
                    // foreach (KeyValuePair<string,JToken> item in (JObject)config["startItems"][1])
                    // {
                    //    Debug.WriteLine(item.Key);
                    //    foreach (KeyValuePair<string,JToken> bullet in (JObject)item.Value[0])
                    //    {
                    //        Debug.WriteLine(bullet.Key);
                    //        Debug.WriteLine(bullet.Value.ToString());
                    //    }
                    // }
                }
                else
                {
                    Debug.WriteLine($"{API.GetCurrentResourceName()}: {config["defaultlang"]}.json Not Found");
                }
            }

            MaxItems = config["MaxItemsInInventory"]["Items"].ToObject<int>();
            MaxWeapons = config["MaxItemsInInventory"]["Items"].ToObject<int>();

            if (MaxItems < 0)
            {
                MaxItems = 0;
            }
            if (MaxWeapons < 0)
            {
                MaxWeapons = 0;
            }

        }

        private void getConfig([FromSource]Player source)
        {
            source.TriggerEvent($"{API.GetCurrentResourceName()}:SendConfig", ConfigString, lang);
        }

        private async void itemsConfig(int player)
        {
            await Delay(3000);
            PlayerList pl = new PlayerList();
            Player p = pl[player];
            string identifier = "steam:" + p.Identifiers["steam"];
            Debug.WriteLine($"El usuario es nuevo añadiendo items {identifier}");
            foreach (KeyValuePair<string,JToken> item in (JObject)config["startItems"][0])
            {
                Debug.WriteLine($"Añadiendo: {item.Key}: {item.Value} ");
                TriggerEvent("vorpCore:addItem", player, item.Key, int.Parse(item.Value.ToString()));
            }
                
            foreach (KeyValuePair<string,JToken> weapon in (JObject)config["startItems"][1])
            {
                JToken wpc = config["Weapons"].FirstOrDefault(x => x["HashName"].ToString().Contains(weapon.Key));
                List<string> auxbullets = new List<string>();
                Dictionary<string,int> givedBullets = new Dictionary<string, int>();
                foreach (KeyValuePair<string,JToken> bullets in (JObject)wpc["AmmoHash"][0])
                {
                    auxbullets.Add(bullets.Key);
                }
                foreach (KeyValuePair<string,JToken> bullet in (JObject)weapon.Value[0])
                {
                    if (auxbullets.Contains(bullet.Key))
                    {
                        givedBullets.Add(bullet.Key,int.Parse(bullet.Value.ToString()));
                    }
                }
                TriggerEvent("vorpCore:registerWeapon", player, weapon.Key,givedBullets);
            }
        }
    }
}