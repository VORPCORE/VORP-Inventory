using CitizenFX.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VorpInventory.Diagnostics;
using VorpInventory.Extensions;
using VorpInventory.Model;
using static CitizenFX.Core.Native.API;

namespace VorpInventory
{
    public class PluginManager : BaseScript
    {
        public static PluginManager Instance { get; private set; }
        public static dynamic CORE;
        public EventHandlerDictionary EventRegistry => EventHandlers;
        public ExportDictionary ExportRegistry => Exports;

        public static Config Config = new Config();
        public static Dictionary<string, string> Langs = new Dictionary<string, string>();
        public static bool IsLoaded = false;

        public static uint CONTROL_PICKUP = 0xC1989F95;
        public static uint CONTROL_OPEN = 0xF84FA74F;

        public PluginManager()
        {
            Logger.Info($"VORP Inventory Init");
            Instance = this;
            Setup();
        }

        private async void Setup()
        {
            await GetCore();
            LoadDefaultConfig();
            await IsReady();

            //RegisterScript(_loadPlayer);
            //RegisterScript(_createCharacter);
            //RegisterScript(_selectCharacter);

            Logger.Info($"VORP Inventory Started");
        }

        public async Task GetCore()
        {
            while (CORE == null)
            {
                TriggerEvent("getCore", new Action<dynamic>((dic) =>
                {
                    CORE = dic;
                }));
                await Delay(100);
            }
        }

        private void LoadDefaultConfig()
        {
            string jsonFile = LoadResourceFile(GetCurrentResourceName(), "config/config.json");

            if (string.IsNullOrEmpty(jsonFile))
            {
                Logger.Error($"config.json file is missing.");
                return;
            }

            Config = JsonConvert.DeserializeObject<Config>(jsonFile);

            CONTROL_OPEN = Config.OpenKey.FromHex();
            CONTROL_PICKUP = Config.PickupKey.FromHex();

            string languageFile = LoadResourceFile(GetCurrentResourceName(), $"config/languages/{Config.Defaultlang}.json");

            if (string.IsNullOrEmpty(languageFile))
            {
                Logger.Error($"{Config.Defaultlang}.json file is missing.");
                return;
            }

            Langs = JsonConvert.DeserializeObject<Dictionary<string, string>>(languageFile);

            Logger.Info($"VORP Inventory: Config Loaded");
            IsLoaded = true;
        }

        public async Task IsReady()
        {
            while (!IsLoaded)
            {
                await Delay(100);
            }
        }

        void AddEvents()
        {
            EventRegistry.Add("onResourceStart", new Action<string>(resourceName =>
            {
                if (resourceName != GetCurrentResourceName()) return;

                Logger.Info($"VORP Inventory Started");
            }));

            EventRegistry.Add("onResourceStop", new Action<string>(resourceName =>
            {
                if (resourceName != GetCurrentResourceName()) return;

                Logger.Info($"Stopping VORP Inventory");

                //UnregisterScript(_loadPlayer);
                //UnregisterScript(_createCharacter);
                //UnregisterScript(_selectCharacter);
            }));
        }
    }
}
