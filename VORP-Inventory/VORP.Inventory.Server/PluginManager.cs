global using VORP.Inventory.Shared.Diagnostics;
global using static CitizenFX.Core.Native.API;
using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VORP.Inventory.Server.Scripts;

namespace VORP.Inventory.Server
{
    public class PluginManager : BaseScript
    {
        public static PluginManager Instance { get; private set; }
        public static PlayerList PlayerList;

        public ExportDictionary ExportRegistry => Exports;

        // Database
        public static Database.ItemDatabase ItemsDB = new();
        // private scripts
        public static VorpCoreInventoryAPI ScriptVorpCoreInventoryApi = new();
        public static VorpPlayerInventory ScriptVorpPlayerInventory = new();

        public static Dictionary<string, int> ActiveCharacters = new();

        public PluginManager()
        {
            Logger.Info($"Init VORP Inventory");

            Instance = this;
            PlayerList = Players;

            Setup();

            Logger.Info($"VORP Inventory Loaded");
        }

        // This needs to become an Export on Core, as an EVENT its just adding more onto the event queue.
        public async static Task<dynamic> GetVorpCoreAsync()
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

            ItemsDB.Init();
            ScriptVorpCoreInventoryApi.Init();
            ScriptVorpPlayerInventory.Init();

            AddEvents();
        }

        void AddEvents()
        {
            Hook("playerJoined", new Action<Player>(([FromSource] player) =>
            {
                if (!ActiveCharacters.ContainsKey(player.Handle))
                    ActiveCharacters.Add(player.Handle, -1);
            }));

            Hook("playerDropped", new Action<Player, string>(async ([FromSource] player, reason) =>
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

            Hook("onResourceStart", new Action<string>(resourceName =>
            {
                if (resourceName != GetCurrentResourceName()) return;

                Logger.Info($"VORP Inventory Started");
            }));

            Hook("onResourceStop", new Action<string>(resourceName =>
            {
                if (resourceName != GetCurrentResourceName()) return;

                Logger.Info($"Stopping VORP Inventory");

                UnregisterScript(ItemsDB);
                UnregisterScript(ScriptVorpCoreInventoryApi);
                UnregisterScript(ScriptVorpPlayerInventory);
            }));
        }

        public void Hook(string eventName, Delegate @delegate)
        {
            EventHandlers[eventName] += @delegate;
        }
    }
}
