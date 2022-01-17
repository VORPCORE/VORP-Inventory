using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VorpInventory.Diagnostics;
using VorpInventory.Models;
using VorpInventory.Scripts;

namespace VorpInventory
{
    public class PluginManager : BaseScript
    {
        public static PluginManager Instance { get; private set; }
        public static PlayerList PlayerList;
        public static dynamic CORE;

        public static Dictionary<int, PlayerInventory> PlayerInventories = new();

        public EventHandlerDictionary EventRegistry => EventHandlers;
        public ExportDictionary ExportRegistry => Exports;

        // Database
        Database.ItemDatabase _itemDatabase = new();
        // private scripts
        Config _scriptConfig = new Config();
        VorpCoreInvenoryAPI _scriptVorpCoreInventoryApi = new VorpCoreInvenoryAPI();
        VorpPlayerInventory _scriptVorpPlayerInventory = new VorpPlayerInventory();

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

            TriggerEvent("getCore", new Action<dynamic>((dic) =>
            {
                Logger.Success($"VORP Core Setup");
                CORE = dic;
            }));

            RegisterScript(_itemDatabase);
            RegisterScript(_scriptConfig);
            RegisterScript(_scriptVorpCoreInventoryApi);
            RegisterScript(_scriptVorpPlayerInventory);

            AddEvents();
        }

        void AddEvents()
        {
            EventRegistry.Add("playerJoined", new Action<Player>(([FromSource] player) =>
            {

            }));

            EventRegistry.Add("playerDropped", new Action<Player, string>(([FromSource] player, reason) =>
            {
                string steamIdent = $"steam:{player.Identifiers["steam"]}";
                if (Database.ItemDatabase.UserInventory.ContainsKey(steamIdent))
                {
                    Database.ItemDatabase.UserInventory.Remove(steamIdent);
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

                UnregisterScript(_itemDatabase);
                UnregisterScript(_scriptConfig);
                UnregisterScript(_scriptVorpCoreInventoryApi);
                UnregisterScript(_scriptVorpPlayerInventory);
            }));
        }
    }
}
