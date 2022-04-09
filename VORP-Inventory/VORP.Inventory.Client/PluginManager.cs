global using VORP.Inventory.Shared.Diagnostics;
global using static CitizenFX.Core.Native.API;
using CitizenFX.Core;
using System;
using System.Threading.Tasks;
using VORP.Inventory.Shared;
using VORP.Inventory.Shared.Models;

namespace VORP.Inventory.Client
{
    public class PluginManager : BaseScript
    {
        public static PluginManager Instance;

        public static Scripts.Pickups Pickups = new Scripts.Pickups();
        public static Scripts.NUIEvents NUIEvents = new Scripts.NUIEvents();
        public static Scripts.InventoryAPI InventoryAPI = new Scripts.InventoryAPI();

        public PluginManager()
        {
            Logger.Info($"VORP INVENTORY INIT");

            Instance = this;

            Config config = Configuration.Config;

            // Control the start up order of each script
            NUIEvents.Init();
            Pickups.Init();
            InventoryAPI.Init();

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
