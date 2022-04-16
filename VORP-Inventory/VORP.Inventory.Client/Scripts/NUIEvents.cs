using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using VORP.Inventory.Shared;
using VORP.Inventory.Shared.Models;

namespace VORP.Inventory.Client.Scripts
{
    public class NUIEvents : Manager
    {
        List<Dictionary<string, dynamic>> _userItemsCache = new();
        List<Dictionary<string, dynamic>> _userWeaponsCache = new();

        public static bool IsInventoryOpen = false;

        public static bool isProcessingPay = false;

        public void Init()
        {

#if DEVELOPMENT
            API.RegisterCommand("kill", new Action(() =>
            {
                API.SetEntityHealth(API.PlayerPedId(), 0, 0);
            }), false);
#endif

            NUI.RegisterCallback("NUIFocusOff", new Action(NUIFocusOff));
            NUI.RegisterCallback("DropItem", new Action<ExpandoObject>(NUIDropItem));
            NUI.RegisterCallback("UseItem", new Action<ExpandoObject>(NUIUseItem));
            NUI.RegisterCallback("sound", new Action(NUISound));
            NUI.RegisterCallback("GiveItem", new Action<NuiMessage>(NUIGiveItem));
            NUI.RegisterCallback("GetNearPlayers", new Action<ExpandoObject>(NUIGetNearPlayers));
            NUI.RegisterCallback("UnequipWeapon", new Action<ExpandoObject>(NUIUnequipWeapon));

            AddEvent("vorp_inventory:ProcessingReady", new Action(setProcessingPayFalse));
            AddEvent("vorp_inventory:CloseInv", new Action(OnCloseInventory));

            //HorseModule
            AddEvent("vorp_inventory:OpenHorseInventory", new Action<string, int>(OpenHorseInventory));
            AddEvent("vorp_inventory:ReloadHorseInventory", new Action<string>(ReloadHorseInventory));

            NUI.RegisterCallback("TakeFromHorse", new Action<ExpandoObject>(NUITakeFromHorse));
            NUI.RegisterCallback("MoveToHorse", new Action<ExpandoObject>(NUIMoveToHorse));

            //Steal
            AddEvent("vorp_inventory:OpenstealInventory", new Action<string, int>(OpenstealInventory));
            AddEvent("vorp_inventory:ReloadstealInventory", new Action<string>(ReloadstealInventory));

            NUI.RegisterCallback("TakeFromsteal", new Action<ExpandoObject>(NUITakeFromsteal));
            NUI.RegisterCallback("MoveTosteal", new Action<ExpandoObject>(NUIMoveTosteal));

            //CartModule
            AddEvent("vorp_inventory:OpenCartInventory", new Action<string, int>(OpenCartInventory));
            AddEvent("vorp_inventory:ReloadCartInventory", new Action<string>(ReloadCartInventory));

            NUI.RegisterCallback("TakeFromCart", new Action<ExpandoObject>(NUITakeFromCart));
            NUI.RegisterCallback("MoveToCart", new Action<ExpandoObject>(NUIMoveToCart));

            //HideoutModule
            AddEvent("vorp_inventory:OpenHideoutInventory", new Action<string, int>(OpenHideoutInventory));
            AddEvent("vorp_inventory:ReloadHideoutInventory", new Action<string>(ReloadHideoutInventory));

            NUI.RegisterCallback("TakeFromHideout", new Action<ExpandoObject>(NUITakeFromHideout));
            NUI.RegisterCallback("MoveToHideout", new Action<ExpandoObject>(NUIMoveToHideout));

            //clan
            AddEvent("vorp_inventory:OpenClanInventory", new Action<string, int>(OpenClanInventory));
            AddEvent("vorp_inventory:ReloadClanInventory", new Action<string>(ReloadClanInventory));

            NUI.RegisterCallback("TakeFromClan", new Action<ExpandoObject>(NUITakeFromClan));
            NUI.RegisterCallback("MoveToClan", new Action<ExpandoObject>(NUIMoveToClan));

            //Container
            AddEvent("vorp_inventory:OpenContainerInventory", new Action<string, int>(OpenContainerInventory));
            AddEvent("vorp_inventory:ReloadContainerInventory", new Action<string>(ReloadContainerInventory));

            NUI.RegisterCallback("TakeFromContainer", new Action<ExpandoObject>(NUITakeFromContainer));
            NUI.RegisterCallback("MoveToContainer", new Action<ExpandoObject>(NUIMoveToContainer));

            // HOUSE
            AddEvent("vorp_inventory:OpenHouseInventory", new Action<string, int>(OpenHouseInventory));
            AddEvent("vorp_inventory:ReloadHouseInventory", new Action<string>(ReloadHouseInventory));

            NUI.RegisterCallback("TakeFromHouse", new Action<ExpandoObject>(NUITakeFromHouse));
            NUI.RegisterCallback("MoveToHouse", new Action<ExpandoObject>(NUIMoveToHouse));

            AttachTickHandler(OnOpenInventoryKeyAsync);
        }

        private void OnGameEventTriggered(string name, List<dynamic> args)
        {
            Logger.Debug($"game event {name} ({String.Join(", ", args.ToArray())})");

            if (name == "CEventNetworkEntityDamage")
            {
                int victim = (int)args[0];
                // int attacker = (int)args[1];
                bool isDamageFatal = Convert.ToBoolean((int)args[5]);
                // uint weaponInfoHash = (uint)args[6];
                // bool isMeleeDamage = Convert.ToBoolean((int)args[11]);
                // int damageTypeFlag = (int)args[12];

                int victimNetId = PedToNet(victim);
                int localPlayerNetId = PedToNet(API.PlayerPedId());

                bool isVictimThisPlayer = victimNetId == localPlayerNetId;

                if (isDamageFatal && isVictimThisPlayer)
                {
                    OnCloseInventory();
                }
            }
        }

        private async void ReloadHorseInventory(string horseInventory)
        {
            NUI.SendMessage(horseInventory);
            await Delay(500);
            LoadInventory();
        }

        private async void ReloadstealInventory(string stealInventory)
        {
            NUI.SendMessage(stealInventory);
            await Delay(500);
            LoadInventory();
        }

        private async void ReloadClanInventory(string cartInventory)
        {
            NUI.SendMessage(cartInventory);
            await Delay(500);
            LoadInventory();
        }

        private void OpenClanInventory(string clanName, int clanId)
        {
            NUI.SetFocus(true);

            NuiMessage nui = new NuiMessage();
            nui.Action = "display";
            nui.Type = "clan";
            nui.Title = clanName;
            nui.ClanId = clanId;

            NUI.SendMessage(nui);

            IsInventoryOpen = true;
        }

        private void NUIMoveToClan(ExpandoObject obj)
        {
            JObject data = JObject.FromObject(obj);
            TriggerServerEvent("syn_clan:MoveToClan", data.ToString());
        }

        private void NUITakeFromClan(ExpandoObject obj)
        {
            JObject data = JObject.FromObject(obj);
            TriggerServerEvent("syn_clan:TakeFromClan", data.ToString());
        }

        // container
        private async void ReloadContainerInventory(string cartInventory)
        {
            NUI.SendMessage(cartInventory);
            await Delay(500);
            LoadInventory();
        }

        private void OpenContainerInventory(string containerName, int containerId)
        {
            NUI.SetFocus(true);

            NuiMessage nui = new NuiMessage();
            nui.Action = "display";
            nui.Type = "Container";
            nui.Title = containerName;
            nui.ContainerId = containerId;

            NUI.SendMessage(nui);

            IsInventoryOpen = true;
        }
        private void NUIMoveToContainer(ExpandoObject obj)
        {
            JObject data = JObject.FromObject(obj);
            TriggerServerEvent("syn_Container:MoveToContainer", data.ToString());
        }

        private void NUITakeFromContainer(ExpandoObject obj)
        {
            JObject data = JObject.FromObject(obj);
            TriggerServerEvent("syn_Container:TakeFromContainer", data.ToString());
        }

        private void OpenHorseInventory(string horseName, int horseId)
        {
            NUI.SetFocus(true);

            NuiMessage nui = new NuiMessage();
            nui.Action = "display";
            nui.Type = "horse";
            nui.Title = horseName;
            nui.HorseId = horseId;

            NUI.SendMessage(nui);

            IsInventoryOpen = true;
            TriggerEvent("vorp_stables:setClosedInv", true);
        }

        private void NUIMoveToHorse(ExpandoObject obj)
        {
            JObject data = JObject.FromObject(obj);
            TriggerServerEvent("vorp_stables:MoveToHorse", data.ToString());
        }

        private void NUITakeFromHorse(ExpandoObject obj)
        {
            JObject data = JObject.FromObject(obj);
            TriggerServerEvent("vorp_stables:TakeFromHorse", data.ToString());
        }

        private void OpenstealInventory(string stealName, int stealId)
        {
            NUI.SetFocus(true);

            NuiMessage nui = new NuiMessage();
            nui.Action = "display";
            nui.Type = "steal";
            nui.Title = stealName;
            nui.StealId = stealId;

            NUI.SendMessage(nui);

            IsInventoryOpen = true;
            TriggerEvent("vorp_stables:setClosedInv", true);
        }

        private void NUIMoveTosteal(ExpandoObject obj)
        {
            JObject data = JObject.FromObject(obj);
            TriggerServerEvent("syn_search:MoveTosteal", data.ToString());
        }

        private void NUITakeFromsteal(ExpandoObject obj)
        {
            JObject data = JObject.FromObject(obj);
            TriggerServerEvent("syn_search:TakeFromsteal", data.ToString());
        }

        private async void ReloadCartInventory(string cartInventory)
        {
            NUI.SendMessage(cartInventory);
            await Delay(500);
            LoadInventory();
        }

        private void OpenCartInventory(string cartName, int wagonId)
        {
            NUI.SetFocus(true);

            NuiMessage nui = new NuiMessage();
            nui.Action = "display";
            nui.Type = "cart";
            nui.Title = cartName;
            nui.WagonId = wagonId;

            NUI.SendMessage(nui);

            IsInventoryOpen = true;
            TriggerEvent("vorp_stables:setClosedInv", true);
        }

        private void NUIMoveToCart(ExpandoObject obj)
        {
            JObject data = JObject.FromObject(obj);
            TriggerServerEvent("vorp_stables:MoveToCart", data.ToString());
        }

        private void NUITakeFromCart(ExpandoObject obj)
        {
            JObject data = JObject.FromObject(obj);
            TriggerServerEvent("vorp_stables:TakeFromCart", data.ToString());
        }

        private async void ReloadHouseInventory(string cartInventory)
        {
            NUI.SendMessage(cartInventory);
            await Delay(500);
            LoadInventory();
        }

        private void OpenHouseInventory(string houseName, int houseId)
        {
            NUI.SetFocus(true);

            NuiMessage nui = new NuiMessage();
            nui.Action = "display";
            nui.Type = "house";
            nui.Title = houseName;
            nui.HouseId = houseId;

            NUI.SendMessage(nui);

            IsInventoryOpen = true;
            //TriggerEvent("vorp_stables:setClosedInv", true);
        }

        private void NUIMoveToHouse(ExpandoObject obj)
        {
            JObject data = JObject.FromObject(obj);
            TriggerServerEvent("vorp_housing:MoveToHouse", data.ToString());
        }

        private void NUITakeFromHouse(ExpandoObject obj)
        {
            JObject data = JObject.FromObject(obj);
            TriggerServerEvent("vorp_housing:TakeFromHouse", data.ToString());
        }

        private async void ReloadHideoutInventory(string cartInventory)
        {
            NUI.SendMessage(cartInventory);
            await Delay(500);
            LoadInventory();
        }

        private void OpenHideoutInventory(string hideoutName, int hideoutId)
        {
            NUI.SetFocus(true);

            NuiMessage nui = new NuiMessage();
            nui.Action = "display";
            nui.Type = "hideout";
            nui.Title = hideoutName;
            nui.HideoutId = hideoutId;

            NUI.SendMessage(nui);

            IsInventoryOpen = true;
        }

        private void NUIMoveToHideout(ExpandoObject obj)
        {
            JObject data = JObject.FromObject(obj);
            TriggerServerEvent("syn_underground:MoveToHideout", data.ToString());
        }

        private void NUITakeFromHideout(ExpandoObject obj)
        {
            JObject data = JObject.FromObject(obj);
            TriggerServerEvent("syn_underground:TakeFromHideout", data.ToString());
        }

        private void setProcessingPayFalse()
        {
            isProcessingPay = false;
        }

        private void NUIUnequipWeapon(ExpandoObject obj)
        {
            Dictionary<string, object> data = Utils.ProcessDynamicObject(obj);
            int ItemId = int.Parse(data["id"].ToString());

            if (InventoryAPI.UsersWeapons.ContainsKey(ItemId))
            {
                InventoryAPI.UsersWeapons[ItemId].UnequipWeapon();
            }
            LoadInventory();
        }

        private void NUIGetNearPlayers(ExpandoObject obj)
        {
            List<int> nearestPlayers = Utils.GetNearestPlayers();
            bool foundPlayers = false;
            List<Dictionary<string, object>> playersNearBy = new List<Dictionary<string, object>>();
            Dictionary<string, object> nuireturn = new Dictionary<string, object>();

            foreach (var player in nearestPlayers)
            {
                foundPlayers = true;
                playersNearBy.Add(new Dictionary<string, object>
                {
                    ["label"] = API.GetPlayerName(player),
                    ["player"] = API.GetPlayerServerId(player)
                });
            }

            if (!foundPlayers)
            {
                Logger.Trace("Not near players");
            }
            else
            {
                Dictionary<string, object> item = new Dictionary<string, object>();
                foreach (var thing in obj)
                {
                    item.Add(thing.Key, thing.Value);
                }
                if (!item.ContainsKey("id"))
                {
                    item.Add("id", 0);
                }
                if (!item.ContainsKey("count"))
                {
                    item.Add("count", 1);
                }

                if (!item.ContainsKey("hash"))
                {
                    item.Add("hash", 1);
                }
                nuireturn.Add("action", "nearPlayers");
                nuireturn.Add("foundAny", foundPlayers);
                nuireturn.Add("players", playersNearBy);
                nuireturn.Add("item", item["item"]);
                nuireturn.Add("hash", item["hash"]);
                nuireturn.Add("count", item["count"]);
                nuireturn.Add("id", item["id"]);
                nuireturn.Add("type", item["type"]);
                nuireturn.Add("what", item["what"]);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(nuireturn);
                NUI.SendMessage(json);
            }
        }

        private void NUIGiveItem(NuiMessage data)
        {
            List<int> players = Utils.GetNearestPlayers();

            foreach (var varia in players)
            {
                if (varia != API.PlayerId())
                {
                    if (API.GetPlayerServerId(varia) == data.PlayerID)
                    {
                        string itemname = data.Data.Item;

                        int target = data.PlayerID;

                        if (data.Data.Type == "item_money")
                        {
                            if (isProcessingPay)
                                return;

                            isProcessingPay = true;
                            TriggerServerEvent("vorp_inventory:giveMoneyToPlayer", target, data.Data.Count);
                        }
                        else if (data.Data.ID == 0)
                        {
                            int amount = (int)data.Data.Count;
                            if (amount > 0 && InventoryAPI.UsersItems[itemname].Count >= amount)
                            {
                                TriggerServerEvent("vorpinventory:serverGiveItem", itemname, amount, target, 1);
                            }
                        }
                        else
                        {
                            TriggerServerEvent("vorpinventory:serverGiveWeapon2", data.Data.ID, target);

                            Dictionary<string, object> weaponLogData = new()
                            {
                                { "action", data.Data.Action },
                                { "count", data.Data.Count },
                                { "foundAny", data.Data.FoundAny },
                                { "hash", data.Data.Hash },
                                { "id", data.Data.ID },
                                { "item", data.Data.Item },
                                { "type", data.Data.Type },
                                { "what", data.Data.What },
                                { "players", data.Data.Players }
                            };

                            TriggerServerEvent("vorpinventory:weaponlog", target, weaponLogData);
                        }

                        LoadInventory();
                    }
                }
            }
        }

        private void NUIUseItem(ExpandoObject obj)
        {
            Dictionary<string, object> data = Utils.ProcessDynamicObject(obj);
            if (data["type"].ToString().Contains("item_standard"))
            {
                TriggerServerEvent("vorp:use", data["item"]);
            }
            else if (data["type"].ToString().Contains("item_weapon"))
            {
                uint weaponHash = 0;
                API.GetCurrentPedWeapon(API.PlayerPedId(), ref weaponHash, false, 0, false);

                int ItemId = int.Parse(data["id"].ToString());
                Weapon weapon = InventoryAPI.UsersWeapons[ItemId];

                bool isWeaponARevolver = Function.Call<bool>((Hash)0xC212F1D05A8232BB, API.GetHashKey(weapon.Name));
                bool isWeaponAPistol = Function.Call<bool>((Hash)0xDDC64F5E31EEDAB6, API.GetHashKey(weapon.Name));
                string weaponName = Function.Call<string>((Hash)0x89CF5FF3D363311E, weaponHash);

                // Check if the weapon used is a pistol or a revolver and ped is not unarmed.
                if ((isWeaponARevolver || isWeaponAPistol) && !weaponName.Contains("UNARMED"))
                {
                    bool isWeaponUsedARevolver = Function.Call<bool>((Hash)0xC212F1D05A8232BB, weaponHash);
                    bool isWeaponUsedAPistol = Function.Call<bool>((Hash)0xDDC64F5E31EEDAB6, weaponHash);
                    if (isWeaponUsedARevolver || isWeaponUsedAPistol)
                    {
                        weapon.SetUsed2(true);
                        weapon.LoadAmmo();
                        weapon.LoadComponents();
                        weapon.SetUsed(true);
                        TriggerServerEvent("syn_weapons:weaponused", data);

                    }
                }
                else if (!weapon.Used &&
                   !Function.Call<bool>((Hash)0x8DECB02F88F428BC, API.PlayerPedId(), API.GetHashKey(weapon.Name), 0, true))
                {
                    weapon.LoadAmmo();
                    weapon.LoadComponents();
                    weapon.SetUsed(true);
                    TriggerServerEvent("syn_weapons:weaponused", data);
                }
                else
                {
                    //TriggerEvent("vorp:Tip", "Ya tienes equipada esa arma", 3000);
                }
                LoadInventory();
            }
        }

        private void NUIDropItem(ExpandoObject obj)
        {
            Dictionary<string, dynamic> aux = Utils.ProcessDynamicObject(obj);
            string itemname = aux["item"];
            string type = aux["type"].ToString();

            if (type == "item_money")
            {
                TriggerServerEvent("vorpinventory:serverDropMoney", double.Parse(aux["number"].ToString()));
            }
            else if (type == "item_standard")
            {
                if (aux["number"].ToString() != null && aux["number"].ToString() != "")
                {
                    if (int.Parse(aux["number"].ToString()) > 0 && InventoryAPI.UsersItems[itemname].Count >= int.Parse(aux["number"].ToString()))
                    {
                        TriggerServerEvent("vorpinventory:serverDropItem", itemname, int.Parse(aux["number"].ToString()), 1);
                        InventoryAPI.UsersItems[itemname].Count -= int.Parse(aux["number"].ToString());
                        if (InventoryAPI.UsersItems[itemname].Count == 0)
                        {
                            InventoryAPI.UsersItems.Remove(itemname);
                        }
                    }
                }
            }
            else
            {
                TriggerServerEvent("vorpinventory:serverDropWeapon", int.Parse(aux["id"].ToString()));
                if (InventoryAPI.UsersWeapons.ContainsKey(int.Parse(aux["id"].ToString())))
                {
                    Weapon wp = InventoryAPI.UsersWeapons[int.Parse(aux["id"].ToString())];
                    if (wp.Used)
                    {
                        wp.SetUsed(false);
                        API.RemoveWeaponFromPed(API.PlayerPedId(), (uint)API.GetHashKey(wp.Name),
                            true, 0);
                    }
                    InventoryAPI.UsersWeapons.Remove(int.Parse(aux["id"].ToString()));
                }
            }
            LoadInventory();
        }

        private void NUISound()
        {
            API.PlaySoundFrontend("BACK", "RDRO_Character_Creator_Sounds", true, 0);
        }

        private void NUIFocusOff()
        {
            OnCloseInventory();
            TriggerEvent("vorp_stables:setClosedInv", false);
            TriggerEvent("syn:closeinv");
        }

        private async Task OnOpenInventoryKeyAsync()
        {
            bool isDead = API.IsEntityDead(API.PlayerPedId());
            if (isDead && PluginManager.BLOCK_INVENTORY_WHEN_DEAD)
            {
                await Delay(1000);
                return;
            }

            if (API.IsControlJustReleased(1, (uint)Configuration.KEY_OPEN_INVENTORY) && API.IsInputDisabled(0))
            {
                if (IsInventoryOpen)
                {
                    OnCloseInventory();
                    await Delay(1000);
                }
                else
                {
                    OpenInventory();
                    await Delay(1000);
                }
            }

        }

        public void LoadInventory()
        {
            TriggerServerEvent("vorpinventory:check_slots");

            UpdateUserItemCache();
            UpdateUserWeaponCache();

            var allItems = _userItemsCache.Concat(_userWeaponsCache).ToList();

            Dictionary<string, object> items = new();
            items.Add("action", "setItems");
            items.Add("itemList", allItems);

            string json = JsonConvert.SerializeObject(items);

            Logger.Trace($"INVENTORY: {json}");

            NUI.SendMessage(json);
        }

        private void UpdateUserItemCache()
        {
            _userItemsCache.Clear();

            foreach (KeyValuePair<string, Item> itemKvp in InventoryAPI.UsersItems)
            {
                Item item = itemKvp.Value;

                Dictionary<string, dynamic> userItem = new();
                userItem.Add("count", item.Count);
                userItem.Add("limit", item.Limit);
                userItem.Add("label", item.Label);
                userItem.Add("name", item.Name);
                userItem.Add("type", item.Type);
                userItem.Add("usable", item.Usable);
                userItem.Add("canRemove", item.CanRemove);
                _userItemsCache.Add(userItem);
            }
        }

        private void UpdateUserWeaponCache()
        {
            _userWeaponsCache.Clear();

            foreach (KeyValuePair<int, Weapon> weaponKvp in InventoryAPI.UsersWeapons)
            {
                Weapon weapon = weaponKvp.Value;

                Dictionary<string, dynamic> weaponItem = new();
                weaponItem.Add("count", weapon.GetAmmo("Hola"));
                weaponItem.Add("limit", -1);
                weaponItem.Add("label", weapon.WeaponLabel);
                weaponItem.Add("name", weapon.Name);
                weaponItem.Add("hash", API.GetHashKey(weapon.Name));
                weaponItem.Add("type", "item_weapon");
                weaponItem.Add("usable", true);
                weaponItem.Add("canRemove", true);
                weaponItem.Add("id", weapon.Id);
                weaponItem.Add("used", weapon.Used);
                _userWeaponsCache.Add(weaponItem);
            }
        }

        private void OpenInventory()
        {
            if (PluginManager.BLOCK_INVENTORY_WHEN_DEAD)
                AttachTickHandler(OnCheckPlayerDeathAsync);

            LoadInventory();

            NUI.SetFocus(true);

            NuiMessage nui = new NuiMessage();
            nui.Action = "display";
            nui.Type = "main";

            NUI.SendMessage(nui);
            IsInventoryOpen = true;
        }

        public void OnCloseInventory()
        {
            DetachTickHandler(OnCheckPlayerDeathAsync);

            NUI.SetFocus(false, false);

            NuiMessage nui = new NuiMessage();
            nui.Action = "hide";

            NUI.SendMessage(nui);
            IsInventoryOpen = false;
        }

        private async Task OnCheckPlayerDeathAsync()
        {
            bool isDead = API.IsEntityDead(API.PlayerPedId());

            if (isDead)
            {
                DetachTickHandler(OnCheckPlayerDeathAsync);
                OnCloseInventory();
                await BaseScript.Delay(500);
            }
        }
    }
}
