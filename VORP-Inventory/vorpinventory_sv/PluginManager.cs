using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VorpInventory.Diagnostics;
using VorpInventory.Scripts;
using static CitizenFX.Core.Native.API;

namespace VorpInventory
{
    public class PluginManager : BaseScript
    {
        public static PluginManager Instance { get; private set; }
        public static PlayerList PlayerList;
        public async static Task<dynamic> CORE() 
        {
            dynamic _CORE = null;

            TriggerEvent("getCore", new Action<dynamic>((getCoreResult) =>
            {
                _CORE = getCoreResult;
            }));

            while (_CORE == null)
            {
                await BaseScript.Delay(100);
            }

            return _CORE;
        }

        public EventHandlerDictionary EventRegistry => EventHandlers;
        public ExportDictionary ExportRegistry => Exports;

        // Database
        public static Database.ItemDatabase ItemsDB = new();
        // private scripts
        public static Config _scriptConfig = new Config();
        public static VorpCoreInventoryAPI _scriptVorpCoreInventoryApi = new VorpCoreInventoryAPI();
        public static VorpPlayerInventory _scriptVorpPlayerInventory = new VorpPlayerInventory();

        public static Dictionary<string, int> ActiveCharacters = new();

        public PluginManager()
        {
            Logger.Info($"Init VORP Inventory");

            Instance = this;
            PlayerList = Players;

            Setup();

            Logger.Info($"VORP Inventory Loaded");
        }

        public void AttachTickHandler(Func<Task> task)
        {
            Tick += task;
        }

        public void DetachTickHandler(Func<Task> task)
        {
            Tick -= task;
        }

        string _GHMattiMySqlResourceState => GetResourceState("ghmattimysql");

        async Task VendorReady()
        {
            string dbResource = _GHMattiMySqlResourceState;
            if (dbResource == "missing")
            {
                while (true)
                {
                    Logger.Error($"ghmattimysql resource not found! Please make sure you have the resource!");
                    await Delay(1000);
                }
            }

            while (!(dbResource == "started"))
            {
                await Delay(500);
                dbResource = _GHMattiMySqlResourceState;
            }
        }

        async void Setup()
        {
            await VendorReady(); // wait till ghmattimysql resource has started

            RegisterScript(ItemsDB);
            RegisterScript(_scriptConfig);
            RegisterScript(_scriptVorpCoreInventoryApi);
            RegisterScript(_scriptVorpPlayerInventory);

            AddEvents();
        }

        void AddEvents()
        {
            EventRegistry.Add("playerJoined", new Action<Player>(([FromSource] player) =>
            {
                if (!ActiveCharacters.ContainsKey(player.Handle))
                    ActiveCharacters.Add(player.Handle, -1);
            }));

            EventRegistry.Add("playerDropped", new Action<Player, string>(async ([FromSource] player, reason) =>
            {
                try
                {
                    string steamIdent = $"steam:{player.Identifiers["steam"]}";

                    //int coreUserCharacterId = await player?.GetCoreUserCharacterId();
                    //if (coreUserCharacterId != -1)
                    //    await _scriptVorpCoreInventoryApi.SaveInventoryItemsSupport(steamIdent, coreUserCharacterId);

                    if (Database.ItemDatabase.UserInventory.ContainsKey(steamIdent))
                        Database.ItemDatabase.UserInventory.Remove(steamIdent);

                    if (ActiveCharacters.ContainsKey(player.Handle))
                            ActiveCharacters.Remove(player.Handle);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"playerDropped: So, they don't exist?!");
                }
            }));

            EventRegistry.Add("onResourceStart", new Action<string>(resourceName =>
            {
                if (resourceName != GetCurrentResourceName()) return;

                Logger.Info($"VORP Inventory Started");
            }));

            EventRegistry.Add("onResourceStop", new Action<string>(resourceName =>
            {
                if (resourceName != GetCurrentResourceName()) return;
                
                Logger.Info($"Stopping VORP Inventory");

                UnregisterScript(ItemsDB);
                UnregisterScript(_scriptConfig);
                UnregisterScript(_scriptVorpCoreInventoryApi);
                UnregisterScript(_scriptVorpPlayerInventory);
            }));
        }
    }
}
