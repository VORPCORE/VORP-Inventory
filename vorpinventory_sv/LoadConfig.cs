using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorpinventory_sv
{
    class LoadConfig : BaseScript
    {
        public static JObject Config = new JObject();
        public static string ConfigString;
        public static Dictionary<string, string> Langs = new Dictionary<string, string>();
        public static string resourcePath = $"{API.GetResourcePath(API.GetCurrentResourceName())}";

        public static bool isConfigLoaded = false;

        public LoadConfig()
        {
            EventHandlers[$"{API.GetCurrentResourceName()}:getConfig"] += new Action<Player>(getConfig);

            LoadConfigAndLang();
        }

        private void LoadConfigAndLang()
        {
            if (File.Exists($"{resourcePath}/Config.json"))
            {
                ConfigString = File.ReadAllText($"{resourcePath}/Config.json", Encoding.UTF8);
                Config = JObject.Parse(ConfigString);
                if (File.Exists($"{resourcePath}/{Config["Language"]}.json"))
                {
                    string langstring = File.ReadAllText($"{resourcePath}/{Config["Language"]}.json", Encoding.UTF8);
                    Langs = JsonConvert.DeserializeObject<Dictionary<string, string>>(langstring);
                    Debug.WriteLine($"{API.GetCurrentResourceName()}: Language {Config["defaultlang"]}.json loaded!");
                }
                else
                {
                    Debug.WriteLine($"{API.GetCurrentResourceName()}: {Config["Language"]}.json Not Found");
                }
            }
            else
            {
                Debug.WriteLine($"{API.GetCurrentResourceName()}: Config.json Not Found");
            }

            isConfigLoaded = true;

            Database.LoadItemTemplate();
        }

        private async void getConfig([FromSource] Player source)
        {
            source.TriggerEvent($"{API.GetCurrentResourceName()}:SendConfig", ConfigString, Langs);
        }
    }
}
