using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using VorpInventory.Diagnostics;
using VorpInventory.Models;

namespace VorpInventory
{
    class GetConfig : BaseScript
    {
        public static JObject Config = new JObject();
        public static Dictionary<string, string> Langs = new Dictionary<string, string>();
        public static uint openKey = 0;
        public static uint pickupKey = 0;
        public static bool loaded = false;
        public static Dictionary<string, Weapon> Weapons = new();

        public GetConfig()
        {
            EventHandlers[$"{API.GetCurrentResourceName()}:SendConfig"] += new Action<string, ExpandoObject>(LoadDefaultConfig);
            TriggerServerEvent($"{API.GetCurrentResourceName()}:getConfig");
        }

        private void LoadDefaultConfig(string dc, ExpandoObject dl)
        {

            Config = JObject.Parse(dc);

            foreach (var l in dl)
            {
                Langs[l.Key] = l.Value.ToString();
            }

            openKey = FromHex(Config["OpenKey"].ToString());

            pickupKey = FromHex(Config["PickupKey"].ToString());

            Pickups.SetupPickPrompt();

            loaded = true;

            try
            {
                foreach (var wpn in Config["Weapons"])
                {
                    Weapon weapon = new Weapon();
                    weapon.Name = $"{wpn["Name"]}";
                    weapon.Hash = $"{wpn["HashName"]}";
                    weapon.WeaponModel = $"{wpn["WeaponModel"]}";
                    Weapons.Add(weapon.Hash, weapon);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when trying to create Weapons Dictionary");
            }
        }

        public static uint FromHex(string value)
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(2);
            }
            return (uint)Int32.Parse(value, NumberStyles.HexNumber);
        }

        public static string GetWeaponLabel(string hash)
        {
            if (Weapons.ContainsKey(hash))
                return Weapons[hash].Name;

            return hash;
        }
    }
}
