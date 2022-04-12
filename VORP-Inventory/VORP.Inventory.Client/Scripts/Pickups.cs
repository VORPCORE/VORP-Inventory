using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VORP.Inventory.Shared;
using VORP.Inventory.Client.Models;
using VORP.Inventory.Client.RedM;
using VORP.Inventory.Client.RedM.Enums;

namespace VORP.Inventory.Client.Scripts
{
    public class Pickups : Manager
    {
        Dictionary<int, Pickup> _worldPickups = new();
        int _promptGroup = GetRandomIntInRange(0, 0xffffff);


        private static bool dropAll = false;
        private static Vector3 lastCoords = new Vector3();



        public void Init()
        {
            AddEvent("vorpInventory:createPickup", new Action<string, int, int>(OnCreatePickupAsync));
            AddEvent("vorpInventory:createMoneyPickup", new Action<double>(OnCreateMoneyPickupAsync));
            AddEvent("vorpInventory:sharePickupClient", new Action<string, int, int, Vector3, int, int>(OnSharePickupClient));
            AddEvent("vorpInventory:shareMoneyPickupClient", new Action<int, double, Vector3, int>(OnShareMoneyPickupClient));
            AddEvent("vorpInventory:removePickupClient", new Action<int>(OnRemovePickupClientAsync));
            AddEvent("vorpInventory:playerAnim", new Action(OnPlayerExitAnimationAsync));
            AddEvent("vorp:PlayerForceRespawn", new Action(OnDeadActionsAsync));

            AttachTickHandler(OnWorldPickupAsync);
        }

        private async void OnDeadActionsAsync()
        {
            lastCoords = Function.Call<Vector3>((Hash)0xA86D5F069399F44D, API.PlayerPedId(), true, true);
            dropAll = true;

            if (Configuration.Config.DropOnRespawn.Money)
            {
                TriggerServerEvent("vorpinventory:serverDropAllMoney");
            }

            await DropInventoryAsync();
        }

        public async Task DropInventoryAsync()
        {
            await Delay(200);
            if (Configuration.Config.DropOnRespawn.Items)
            {
                Dictionary<string, ItemClass> items = InventoryAPI.UsersItems.ToDictionary(p => p.Key, p => p.Value);
                foreach (var item in items.Values)
                {
                    TriggerServerEvent("vorpinventory:serverDropItem", item.getName(), item.getCount(), 1);
                    InventoryAPI.UsersItems[item.getName()].quitCount(item.getCount());
                    //Debug.Write(vorp_inventoryClient.useritems[itemname].getCount().ToString());
                    if (InventoryAPI.UsersItems[item.getName()].getCount() == 0)
                    {
                        InventoryAPI.UsersItems.Remove(item.getName());
                    }
                    await Delay(200);
                }
            }

            if (Configuration.Config.DropOnRespawn.Weapons)
            {
                Dictionary<int, WeaponClass> weapons = InventoryAPI.UsersWeapons.ToDictionary(p => p.Key, p => p.Value);
                foreach (var weapon in weapons)
                {
                    TriggerServerEvent("vorpinventory:serverDropWeapon", weapon.Key);
                    if (InventoryAPI.UsersWeapons.ContainsKey(weapon.Key))
                    {
                        WeaponClass wp = InventoryAPI.UsersWeapons[weapon.Key];
                        if (wp.getUsed())
                        {
                            wp.setUsed(false);
                            API.RemoveWeaponFromPed(API.PlayerPedId(), (uint)API.GetHashKey(wp.getName()),
                                true, 0);
                        }
                        InventoryAPI.UsersWeapons.Remove(weapon.Key);
                    }
                    await Delay(200);
                }
            }
            await Delay(800);
            dropAll = false;
        }

        private async Task OnWorldPickupAsync()
        {
            if (_worldPickups.Count == 0)
            {
                await BaseScript.Delay(1000);
                return;
            }

            int playerPedId = API.PlayerPedId();

            List<Pickup> pickupsInRange = _worldPickups.Select(x => x.Value).Where(x => x.IsInRange).OrderBy(x => x.Distance).ToList();

            pickupsInRange.ForEach(x =>
            {
                // Utils.DrawText3D(x.Position, x.Name);

                if (x.Distance <= 1.2)
                {
                    Function.Call((Hash)0x69F4BE8C8CC4796C, playerPedId, x.EntityId, 3000, 2048, 3); // TaskLookAtEntity

                    bool isDead = API.IsEntityDead(playerPedId);

                    x.Prompt.Visible = !isDead;

                    long promptSubLabel = Function.Call<long>(Hash._CREATE_VAR_STRING, 10, "LITERAL_STRING", x.Name);
                    Function.Call((Hash)0xC65A45D4453C2627, _promptGroup, promptSubLabel, 1); // UiPromptSetActiveGroupThisFrame

                    if (x.Prompt.HasHoldModeCompleted)
                    {
                        if (x.IsMoney)
                        {
                            TriggerServerEvent("vorpinventory:onPickupMoney", x.EntityId);
                        }
                        else
                        {
                            TriggerServerEvent("vorpinventory:onPickup", x.EntityId);
                        }

                        x.Prompt.Delete();
                    }
                }
                else
                {
                    if (x.Prompt.Enabled)
                    {
                        x.Prompt.Visible = false;
                    }
                }
            });
        }

        private async void OnPlayerExitAnimationAsync()
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

        private async void OnRemovePickupClientAsync(int entityHandle)
        {
            Function.Call((Hash)0xDC19C288082E586E, entityHandle, false, true);
            API.NetworkRequestControlOfEntity(entityHandle);
            int timeout = 0;
            while (!API.NetworkHasControlOfEntity(entityHandle) && timeout < 5000)
            {
                timeout += 100;
                if (timeout == 5000)
                {
                    Logger.Error("Control of the entity has not been obtained");
                }

                await Delay(100);
            }
            Function.Call((Hash)0x7D9EFB7AD6B19754, entityHandle, false);
            Function.Call((Hash)0x7DFB49BCDB73089A, entityHandle, false);
            API.DeleteObject(ref entityHandle);
        }

        private void OnSharePickupClient(string name, int entityHandle, int amount, Vector3 position, int value, int weaponId)
        {
            if (value == 1)
            {
                if (!_worldPickups.ContainsKey(entityHandle))
                {
                    string label = Configuration.GetHashReadableLabel(name);
                    Pickup pickup = new Pickup()
                    {
                        Name = amount > 1 ? $"{label} x {amount}" : label,
                        EntityId = entityHandle,
                        Amount = amount,
                        WeaponId = weaponId,
                        Position = position,
                        Prompt = Prompt.Create((eControl)Configuration.KEY_PICKUP_ITEM, Configuration.GetTranslation("TakeFromFloor"), promptType: ePromptType.StandardHold, group: (uint)_promptGroup)
                    };

                    pickup.Prompt.Enabled = false;
                    pickup.Prompt.Visible = false;

                    _worldPickups.Add(entityHandle, pickup);
                }

                Logger.Trace($"Item Pickup Added: {_worldPickups[entityHandle]}");
            }
            else
            {
                if (_worldPickups.ContainsKey(entityHandle))
                {
                    Pickup pickup = _worldPickups[entityHandle];
                    pickup.Prompt.Delete();

                    _worldPickups.Remove(entityHandle);
                }
            }
        }

        private void OnShareMoneyPickupClient(int entityHandle, double amount, Vector3 position, int value)
        {
            if (value == 1)
            {
                if (!_worldPickups.ContainsKey(entityHandle))
                {
                    Pickup pickup = new Pickup()
                    {
                        Name = $"Money (${amount:N2})",
                        EntityId = entityHandle,
                        Amount = amount,
                        Position = position,
                        IsMoney = true,
                        Prompt = Prompt.Create((eControl)Configuration.KEY_PICKUP_ITEM, Configuration.GetTranslation("TakeFromFloor"), promptType: ePromptType.StandardHold, group: (uint)_promptGroup)
                    };

                    pickup.Prompt.Enabled = false;
                    pickup.Prompt.Visible = false;

                    _worldPickups.Add(entityHandle, pickup);
                }

                Logger.Trace($"Money Pickup Added: {_worldPickups[entityHandle]}");
            }
            else
            {
                if (_worldPickups.ContainsKey(entityHandle))
                {
                    Pickup pickup = _worldPickups[entityHandle];
                    pickup.Prompt.Delete();

                    _worldPickups.Remove(entityHandle);
                }
            }
        }

        private async void OnCreatePickupAsync(string name, int amount, int weaponId)
        {
            int ped = API.PlayerPedId();
            Vector3 coords = Function.Call<Vector3>((Hash)0xA86D5F069399F44D, ped, true, true);
            Vector3 forward = Function.Call<Vector3>((Hash)0x2412D9C05BB09B97, ped);
            Vector3 position = (coords + forward * 1.6F);

            if (dropAll)
            {
                Random rnd = new Random();
                float rn1 = (float)rnd.Next(-35, 35);
                float rn2 = (float)rnd.Next(-35, 35);
                position = new Vector3((lastCoords.X + (rn1 / 10.0f)), (lastCoords.Y + (rn2 / 10.0f)), lastCoords.Z);
            }

            int entityHandle = await CreateObjectAsync("P_COTTONBOX01X", position);
            TriggerServerEvent("vorpinventory:sharePickupServer", name, entityHandle, amount, position, weaponId);
            Function.Call((Hash)0x67C540AA08E4A6F5, "show_info", "Study_Sounds", true, 0);
        }

        private async void OnCreateMoneyPickupAsync(double amount)
        {
            int ped = API.PlayerPedId();
            Vector3 coords = Function.Call<Vector3>((Hash)0xA86D5F069399F44D, ped, true, true);
            Vector3 forward = Function.Call<Vector3>((Hash)0x2412D9C05BB09B97, ped);
            Vector3 position = (coords + forward * 1.6F);

            if (dropAll)
            {
                Random rnd = new Random();

                position = new Vector3((lastCoords.X + (float)rnd.Next(-3, 3)), (lastCoords.Y + (float)rnd.Next(-3, 3)), lastCoords.Z);
            }

            int entityHandle = await CreateObjectAsync("p_moneybag02x", position);
            TriggerServerEvent("vorpinventory:shareMoneyPickupServer", entityHandle, amount, position);
            Function.Call((Hash)0x67C540AA08E4A6F5, "show_info", "Study_Sounds", true, 0);
        }

        private async Task<int> CreateObjectAsync(string hash, Vector3 position)
        {
            uint objectHash = (uint)API.GetHashKey(hash);

            if (!Function.Call<bool>((Hash)0x1283B8B89DD5D1B6, objectHash))
            {
                Function.Call((Hash)0xFA28FE3A6246FC30, objectHash);
            }

            while (!Function.Call<bool>((Hash)0x1283B8B89DD5D1B6, objectHash))
            {
                await Delay(1);
            }

            int entityHandle = Function.Call<int>((Hash)0x509D5878EB39E842, objectHash, position.X
                , position.Y, position.Z, true, true, true);

            Function.Call((Hash)0x58A850EAEE20FAA3, entityHandle);
            Function.Call((Hash)0xDC19C288082E586E, entityHandle, true, false);
            Function.Call((Hash)0x7D9EFB7AD6B19754, entityHandle, true);
            Function.Call((Hash)0x7DFB49BCDB73089A, entityHandle, true);

            Function.Call((Hash)0xF66F820909453B8C, entityHandle, false, true);

            SetModelAsNoLongerNeeded(objectHash);

            return entityHandle;
        }
    }
}