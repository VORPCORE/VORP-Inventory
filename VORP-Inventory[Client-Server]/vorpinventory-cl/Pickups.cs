using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vorpinventory_sv;

namespace vorpinventory_cl
{
    public class Pickups : BaseScript
    {
        public Pickups()
        {
            EventHandlers["vorpInventory:createPickup"] += new Action<string, int, int>(createPickup);
            EventHandlers["vorpInventory:createMoneyPickup"] += new Action<double>(createMoneyPickup);
            EventHandlers["vorpInventory:sharePickupClient"] += new Action<string, int, int, Vector3, int, int>(sharePickupClient);
            EventHandlers["vorpInventory:shareMoneyPickupClient"] += new Action<int, double, Vector3, int>(shareMoneyPickupClient);
            EventHandlers["vorpInventory:removePickupClient"] += new Action<int>(removePickupClient);
            EventHandlers["vorpInventory:playerAnim"] += new Action<int>(playerAnim);
            EventHandlers["vorp:PlayerForceRespawn"] += new Action(DeadActions);
            SetupPickPrompt();
            Tick += principalFunctionPickups;
            Tick += principalFunctionPickupsMoney;
        }

        private int PickPrompt;
        public static Dictionary<int, Dictionary<string, dynamic>> pickups = new Dictionary<int, Dictionary<string, dynamic>>();
        public static Dictionary<int, Dictionary<string, dynamic>> pickupsMoney = new Dictionary<int, Dictionary<string, dynamic>>();
        private static bool active = false;

        private async void DeadActions()
        {
            if (GetConfig.Config["DropOnRespawn"]["Money"].ToObject<bool>())
            {
                TriggerServerEvent("vorpinventory:serverDropAllMoney");
            }
            
            dropallPlease();
        }

        public async Task dropallPlease()
        {
            await Delay(100);
            if (GetConfig.Config["DropOnRespawn"]["Items"].ToObject<bool>())
            {
                Dictionary<string, ItemClass> items = vorp_inventoryClient.useritems.ToDictionary(p => p.Key, p => p.Value);
                foreach (var item in items.Values)
                {
                    TriggerServerEvent("vorpinventory:serverDropItem", item.getName(), item.getCount(), 1);
                    vorp_inventoryClient.useritems[item.getName()].quitCount(item.getCount());
                    //Debug.Write(vorp_inventoryClient.useritems[itemname].getCount().ToString());
                    if (vorp_inventoryClient.useritems[item.getName()].getCount() == 0)
                    {
                        vorp_inventoryClient.useritems.Remove(item.getName());
                    }
                    await Delay(200);
                }
            }

            if (GetConfig.Config["DropOnRespawn"]["Weapons"].ToObject<bool>())
            {
                Dictionary<int, WeaponClass> weapons = vorp_inventoryClient.userWeapons.ToDictionary(p => p.Key, p => p.Value);
                foreach (var weapon in weapons)
                {
                    TriggerServerEvent("vorpinventory:serverDropWeapon", weapon.Key);
                    if (vorp_inventoryClient.userWeapons.ContainsKey(weapon.Key))
                    {
                        WeaponClass wp = vorp_inventoryClient.userWeapons[weapon.Key];
                        if (wp.getUsed())
                        {
                            wp.setUsed(false);
                            API.RemoveWeaponFromPed(API.PlayerPedId(), (uint)API.GetHashKey(wp.getName()),
                                true, 0);
                        }
                        vorp_inventoryClient.userWeapons.Remove(weapon.Key);
                    }
                    await Delay(200);
                }
            }
        }

        [Tick]
        private async Task principalFunctionPickups()
        {
            int playerPed = API.PlayerPedId();
            Vector3 coords = Function.Call<Vector3>((Hash)0xA86D5F069399F44D, playerPed, true, true);

            if (pickups.Count == 0)
            {
                return;
            }

            foreach (var pick in pickups)
            {
                float distance = Function.Call<float>((Hash)0x0BE7F4E3CDBAFB28, coords.X, coords.Y, coords.Z,
                    pick.Value["coords"].X,
                    pick.Value["coords"].Y, pick.Value["coords"].Z, false);

                if (distance <= 5.0F)
                {
                    if (pick.Value["weaponid"] == 1)
                    {
                        string name = pick.Value["name"];
                        if (vorp_inventoryClient.citems.ContainsKey(name))
                        {
                            name = vorp_inventoryClient.citems[name]["label"];
                        }
                        Utils.DrawText3D(pick.Value["coords"], name);
                    }
                    else
                    {
                        string name = Function.Call<string>((Hash)0x89CF5FF3D363311E,
                            (uint)API.GetHashKey(pick.Value["name"]));
                        Utils.DrawText3D(pick.Value["coords"], name);
                    }
                }

                if (distance <= 0.7F)
                {
                    Function.Call((Hash)0x69F4BE8C8CC4796C, playerPed, pick.Value["obj"], 3000, 2048, 3);
                    if (active == false)
                    {
                        //Debug.WriteLine("Entro");
                        Function.Call((Hash)0x8A0FB4D03A630D21, PickPrompt, true);
                        Function.Call((Hash)0x71215ACCFDE075EE, PickPrompt, true);
                        active = true;
                    }

                    if (Function.Call<bool>((Hash)0xE0F65F0640EF0617, PickPrompt))
                    {
                        TriggerServerEvent("vorpinventory:onPickup", pick.Value["obj"]);
                        pick.Value["inRange"] = true;
                        active = true;
                        Function.Call((Hash)0x8A0FB4D03A630D21, PickPrompt, false);
                        Function.Call((Hash)0x71215ACCFDE075EE, PickPrompt, false);
                    }
                }
                else
                {
                    if (active)
                    {
                        Function.Call((Hash)0x8A0FB4D03A630D21, PickPrompt, false);
                        Function.Call((Hash)0x71215ACCFDE075EE, PickPrompt, false);
                        pick.Value["inRange"] = false;
                        active = false;
                    }
                }
            }
        }

        [Tick]
        private async Task principalFunctionPickupsMoney()
        {
            int playerPed = API.PlayerPedId();
            Vector3 coords = Function.Call<Vector3>((Hash)0xA86D5F069399F44D, playerPed, true, true);

            if (pickupsMoney.Count == 0)
            {
                return;
            }

            foreach (var pick in pickupsMoney)
            {
                float distance = Function.Call<float>((Hash)0x0BE7F4E3CDBAFB28, coords.X, coords.Y, coords.Z,
                    pick.Value["coords"].X,
                    pick.Value["coords"].Y, pick.Value["coords"].Z, false);

                if (distance <= 5.0F)
                {
                    string name = pick.Value["name"];
                    Utils.DrawText3D(pick.Value["coords"], name);
                }

                if (distance <= 0.7F && !pick.Value["inRange"])
                {
                    Function.Call((Hash)0x69F4BE8C8CC4796C, playerPed, pick.Value["obj"], 3000, 2048, 3);
                    if (active == false)
                    {
                        //Debug.WriteLine("Entro");
                        Function.Call((Hash)0x8A0FB4D03A630D21, PickPrompt, true);
                        Function.Call((Hash)0x71215ACCFDE075EE, PickPrompt, true);
                    }

                    if (Function.Call<bool>((Hash)0xE0F65F0640EF0617, PickPrompt))
                    {
                        TriggerServerEvent("vorpinventory:onPickupMoney", pick.Value["obj"]);
                        pick.Value["inRange"] = true;
                        active = true;
                        Function.Call((Hash)0x8A0FB4D03A630D21, PickPrompt, false);
                        Function.Call((Hash)0x71215ACCFDE075EE, PickPrompt, false);
                    }
                }
                else
                {
                    if (active)
                    {
                        Function.Call((Hash)0x8A0FB4D03A630D21, PickPrompt, false);
                        Function.Call((Hash)0x71215ACCFDE075EE, PickPrompt, false);
                        active = false;
                    }
                }
            }
        }

        private async void playerAnim(int obj)
        {
            string dict = "amb_work@world_human_box_pickup@1@male_a@stand_exit_withprop";
            Function.Call((Hash)0xA862A2AD321F94B4, dict);

            while (!Function.Call<bool>((Hash)0x27FF6FE8009B40CA, dict))
            {
                await Delay(10);
            }
            Function.Call((Hash)0xEA47FE3719165B94, API.PlayerPedId(), dict, "exit_front", 1.0, 8.0, -1, 1, 0, false, false, false);
            await Delay(1200);
            Function.Call((Hash)0x67C540AA08E4A6F5, "CHECKPOINT_PERFECT", "HUD_MINI_GAME_SOUNDSET", true, 1);
            await Delay(1000);
            Function.Call((Hash)0xE1EF3C1216AFF2CD, API.PlayerPedId());
        }

        private async void removePickupClient(int obj)
        {
            Function.Call((Hash)0xDC19C288082E586E, obj, false, true);
            API.NetworkRequestControlOfEntity(obj);
            int timeout = 0;
            while (!API.NetworkHasControlOfEntity(obj) && timeout < 5000)
            {
                timeout += 100;
                if (timeout == 5000)
                {
                    Debug.WriteLine("No se ha obtenido el control de la entidad");
                }

                await Delay(100);
            }
            Function.Call((Hash)0x7D9EFB7AD6B19754, obj, false);
            API.DeleteObject(ref obj);
        }

        private void sharePickupClient(string name, int obj, int amount, Vector3 position, int value, int weaponId)
        {
            if (value == 1)
            {
                Debug.WriteLine(obj.ToString());
                pickups.Add(obj, new Dictionary<string, dynamic>
                {
                    ["name"] = name,
                    ["obj"] = obj,
                    ["amount"] = amount,
                    ["weaponid"] = weaponId,
                    ["inRange"] = false,
                    ["coords"] = position
                });
                Debug.WriteLine($"name: {pickups[obj]["name"].ToString()} cuantity: {pickups[obj]["amount"].ToString()},id:{pickups[obj]["weaponid"].ToString()}");
            }
            else
            {
                pickups.Remove(obj);
            }
        }

        private void shareMoneyPickupClient(int obj, double amount, Vector3 position, int value)
        {
            if (value == 1)
            {
                Debug.WriteLine(obj.ToString());
                pickupsMoney.Add(obj, new Dictionary<string, dynamic>
                {
                    ["name"] = "money",
                    ["obj"] = obj,
                    ["amount"] = amount,
                    ["inRange"] = false,
                    ["coords"] = position
                });
                Debug.WriteLine($"name: {pickupsMoney[obj]["name"].ToString()} cuantity: {pickupsMoney[obj]["amount"].ToString()},");
            }
            else
            {
                pickupsMoney.Remove(obj);
            }
        }

        private async void createPickup(string name, int amoun, int weaponId)
        {
            int ped = API.PlayerPedId();
            Vector3 coords = Function.Call<Vector3>((Hash)0xA86D5F069399F44D, ped, true, true);
            Vector3 forward = Function.Call<Vector3>((Hash)0x2412D9C05BB09B97, ped);
            Vector3 position = (coords + forward * 1.6F);

            if (API.IsPlayerDead(API.PlayerId()))
            {
                Random rnd = new Random();
                float rn1 = (float)rnd.Next(-35, 35);
                float rn2 = (float)rnd.Next(-35, 35);
                position = new Vector3((coords.X + (rn1 / 10.0f)), (coords.Y + (rn2 / 10.0f)), coords.Z);
            }

            if (!Function.Call<bool>((Hash)0x1283B8B89DD5D1B6, (uint)API.GetHashKey("P_COTTONBOX01X")))
            {
                Function.Call((Hash)0xFA28FE3A6246FC30, (uint)API.GetHashKey("P_COTTONBOX01X"));
            }

            while (!Function.Call<bool>((Hash)0x1283B8B89DD5D1B6, (uint)API.GetHashKey("P_COTTONBOX01X")))
            {
                await Delay(1);
            }

            int obj = Function.Call<int>((Hash)0x509D5878EB39E842, (uint)API.GetHashKey("P_COTTONBOX01X"), position.X
                , position.Y, position.Z, true, true, true);
            Function.Call((Hash)0x58A850EAEE20FAA3, obj);
            Function.Call((Hash)0xDC19C288082E586E, obj, true, false);
            Function.Call((Hash)0x7D9EFB7AD6B19754, obj, true);
            Debug.WriteLine(obj.ToString());
            TriggerServerEvent("vorpinventory:sharePickupServer", name, obj, amoun, position, weaponId);
            Function.Call((Hash)0x67C540AA08E4A6F5, "show_info", "Study_Sounds", true, 0);
        }

        private async void createMoneyPickup(double amoun)
        {
            int ped = API.PlayerPedId();
            Vector3 coords = Function.Call<Vector3>((Hash)0xA86D5F069399F44D, ped, true, true);
            Vector3 forward = Function.Call<Vector3>((Hash)0x2412D9C05BB09B97, ped);
            Vector3 position = (coords + forward * 1.6F);

            if (API.IsPlayerDead(API.PlayerId()))
            {
                Random rnd = new Random();

                position = new Vector3((coords.X + (float)rnd.Next(-3, 3)), (coords.Y + (float)rnd.Next(-3, 3)), coords.Z);
            }

            if (!Function.Call<bool>((Hash)0x1283B8B89DD5D1B6, (uint)API.GetHashKey("p_moneybag02x")))
            {
                Function.Call((Hash)0xFA28FE3A6246FC30, (uint)API.GetHashKey("p_moneybag02x"));
            }

            while (!Function.Call<bool>((Hash)0x1283B8B89DD5D1B6, (uint)API.GetHashKey("p_moneybag02x")))
            {
                await Delay(1);
            }

            int obj = Function.Call<int>((Hash)0x509D5878EB39E842, (uint)API.GetHashKey("p_moneybag02x"), position.X
                , position.Y, position.Z, true, true, true);
            Function.Call((Hash)0x58A850EAEE20FAA3, obj);
            Function.Call((Hash)0xDC19C288082E586E, obj, true, false);
            Function.Call((Hash)0x7D9EFB7AD6B19754, obj, true);
            Debug.WriteLine(obj.ToString());
            TriggerServerEvent("vorpinventory:shareMoneyPickupServer", obj, amoun, position);
            Function.Call((Hash)0x67C540AA08E4A6F5, "show_info", "Study_Sounds", true, 0);
        }

        public void SetupPickPrompt()
        {
            Debug.WriteLine("Prompt creado");
            PickPrompt = Function.Call<int>((Hash)0x04F97DE45A519419);
            long str = Function.Call<long>(Hash._CREATE_VAR_STRING, 10, "LITERAL_STRING", "Coger");
            Function.Call((Hash)0x5DD02A8318420DD7, PickPrompt, str);
            Function.Call((Hash)0xB5352B7494A08258, PickPrompt, 0xF84FA74F);
            Function.Call((Hash)0x8A0FB4D03A630D21, PickPrompt, false);
            Function.Call((Hash)0x71215ACCFDE075EE, PickPrompt, false);
            Function.Call((Hash)0x94073D5CA3F16B7B, PickPrompt, true);
            Function.Call((Hash)0xF7AA2696A22AD8B9, PickPrompt);
        }
    }
}