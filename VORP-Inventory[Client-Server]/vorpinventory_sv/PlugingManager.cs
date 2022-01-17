using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VorpInventory.Diagnostics;
using VorpInventory.Models;

namespace VorpInventory
{
    public class PlugingManager : BaseScript
    {
        public static PlugingManager Instance { get; private set; }
        public static PlayerList PlayerList;

        public static Dictionary<int, PlayerInventory> PlayerInventories = new();

        public EventHandlerDictionary EventRegistry => EventHandlers;
        public ExportDictionary ExportRegistry => Exports;

        public PlugingManager()
        {
            Logger.Info($"Init VORP Inventory");
            PlayerList = Players;

            Instance = this;
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
    }
}
