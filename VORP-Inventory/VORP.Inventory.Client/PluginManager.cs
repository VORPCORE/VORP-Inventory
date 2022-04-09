global using VORP.Inventory.Shared.Diagnostics;
global using static CitizenFX.Core.Native.API;
using CitizenFX.Core;
using System;
using System.Threading.Tasks;
using VORP.Inventory.Shared;

namespace VorpInventory
{
    public class PluginManager : BaseScript
    {
        public static PluginManager Instance;

        public static Scripts.Pickups Pickups = new Scripts.Pickups();
        public static Scripts.NUIEvents NUIEvents = new Scripts.NUIEvents();

        public PluginManager()
        {
            Logger.Info($"VORP INVENTORY INIT");

            Instance = this;

            Config config = Configuration.Config;

            NUIEvents.Init();
            Pickups.Init();
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
