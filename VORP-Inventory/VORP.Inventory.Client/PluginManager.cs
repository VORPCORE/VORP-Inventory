﻿global using CitizenFX.Core;
global using CitizenFX.Core.Native;
global using VORP.Inventory.Shared.Diagnostics;
global using static CitizenFX.Core.Native.API;
using System;
using System.Threading.Tasks;
using VORP.Inventory.Client.Extensions;
using VORP.Inventory.Shared;
using VORP.Inventory.Shared.Models;

namespace VORP.Inventory.Client
{
    public class PluginManager : BaseScript
    {
        public const string DECOR_SELECTED_CHARACTER_ID = "SELECTED_CHARACTER_ID";
        public static PluginManager Instance;

        public Scripts.Pickups Pickups = new();
        public Scripts.NUIEvents NUIEvents = new();
        public Scripts.InventoryAPI InventoryAPI = new();

        public static bool BLOCK_INVENTORY_WHEN_DEAD = false;

        public PluginManager()
        {
            Logger.Info($"VORP INVENTORY INIT");

            Instance = this;

            Config config = Configuration.Config;

            // Control the start up order of each script
            NUIEvents.Init();
            Pickups.Init();
            InventoryAPI.Init();

            BLOCK_INVENTORY_WHEN_DEAD = GetResourceMetadata(GetCurrentResourceName(), "block_inventory_on_death", 0) == "true";
            Logger.Trace($"BLOCK_INVENTORY_WHEN_DEAD {BLOCK_INVENTORY_WHEN_DEAD}");

            Hook("onResourceStart", new Action<string>(async resourceName =>
            {
                if (GetCurrentResourceName() != resourceName) return;

                await BaseScript.Delay(3000);

                int selectedCharacter = DecoratorExtensions.GetInteger(PlayerPedId(), DECOR_SELECTED_CHARACTER_ID);
                if (selectedCharacter > 0)
                {
                    Logger.Trace($"onResourceStart : Selected Character: {selectedCharacter}");

                    Instance.NUIEvents.OnCloseInventory();

                    TriggerServerEvent("vorpinventory:getItemsTable");
                    Logger.Trace($"OnSelectedCharacterAsync: vorpinventory:getItemsTable");
                    await Delay(1000);
                    TriggerServerEvent("vorpinventory:getInventory");
                    Logger.Trace($"OnSelectedCharacterAsync: vorpinventory:getInventory");
                }
            }));

            Logger.Info($"VORP INVENTORY LOADED");
        }

        public void AttachTickHandler(Func<Task> task)
        {
            Tick += task;
        }

        public void DetachTickHandler(Func<Task> task)
        {
            Tick -= task;
        }

        public void Hook(string eventName, Delegate @delegate)
        {
            EventHandlers[eventName] += @delegate;
        }
    }
}
