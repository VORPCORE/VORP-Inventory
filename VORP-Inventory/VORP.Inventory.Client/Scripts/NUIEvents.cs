using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using VORP.Inventory.Client.Models;
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
            NUI.RegisterCallback("NUIFocusOff", new Action(NUIFocusOff));
            NUI.RegisterCallback("DropItem", new Action<ExpandoObject>(NUIDropItem));
            NUI.RegisterCallback("UseItem", new Action<ExpandoObject>(NUIUseItem));
            NUI.RegisterCallback("sound", new Action(NUISound));
            NUI.RegisterCallback("GiveItem", new Action<ExpandoObject>(NUIGiveItem));
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

            //HouseModule
            AddEvent("vorp_inventory:OpenHouseInventory", new Action<string, int>(OpenHouseInventory));
            AddEvent("vorp_inventory:ReloadHouseInventory", new Action<string>(ReloadHouseInventory));

            NUI.RegisterCallback("TakeFromHouse", new Action<ExpandoObject>(NUITakeFromHouse));
            NUI.RegisterCallback("MoveToHouse", new Action<ExpandoObject>(NUIMoveToHouse));

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

            AttachTickHandler(OnOpenInventoryKeyAsync);
        }

        private async void ReloadHorseInventory(string horseInventory)
        {
            API.SendNuiMessage(horseInventory);
            await Delay(500);
            LoadInv();
        }

        private async void ReloadstealInventory(string stealInventory)
        {
            API.SendNuiMessage(stealInventory);
            await Delay(500);
            LoadInv();
        }

        private async void ReloadClanInventory(string cartInventory)
        {
            API.SendNuiMessage(cartInventory);
            await Delay(500);
            LoadInv();
        }

        private void OpenClanInventory(string clanName, int clanId)
        {
            NUI.SetFocus(true);

            NuiMessage nui = new NuiMessage();
            nui.Action = "display";
            nui.Type = "clan";
            nui.Title = clanName;
            nui.HideoutID = clanId;

            API.SendNuiMessage(nui.ToJson());

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
            API.SendNuiMessage(cartInventory);
            await Delay(500);
            LoadInv();
        }

        private void OpenContainerInventory(string containerName, int containerId)
        {
            NUI.SetFocus(true);

            NuiMessage nui = new NuiMessage();
            nui.Action = "display";
            nui.Type = "Container";
            nui.Title = containerName;
            nui.HideoutID = containerId;

            API.SendNuiMessage(nui.ToJson());

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
            nui.HideoutID = horseId;

            API.SendNuiMessage(nui.ToJson());

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
            nui.HideoutID = stealId;

            API.SendNuiMessage(nui.ToJson());

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
            API.SendNuiMessage(cartInventory);
            await Delay(500);
            LoadInv();
        }

        private void OpenCartInventory(string cartName, int wagonId)
        {
            NUI.SetFocus(true);

            NuiMessage nui = new NuiMessage();
            nui.Action = "display";
            nui.Type = "cart";
            nui.Title = cartName;
            nui.HideoutID = wagonId;

            API.SendNuiMessage(nui.ToJson());

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
            API.SendNuiMessage(cartInventory);
            await Delay(500);
            LoadInv();
        }

        private void OpenHouseInventory(string houseName, int houseId)
        {
            NUI.SetFocus(true);

            NuiMessage nui = new NuiMessage();
            nui.Action = "display";
            nui.Type = "house";
            nui.Title = houseName;
            nui.HideoutID = houseId;

            API.SendNuiMessage(nui.ToJson());
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
            API.SendNuiMessage(cartInventory);
            await Delay(500);
            LoadInv();
        }

        private void OpenHideoutInventory(string hideoutName, int hideoutId)
        {
            NUI.SetFocus(true);

            NuiMessage nui = new NuiMessage();
            nui.Action = "display";
            nui.Type = "hideout";
            nui.Title = hideoutName;
            nui.HideoutID = hideoutId;

            API.SendNuiMessage(nui.ToJson());
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
            LoadInv();
        }

        private void NUIGetNearPlayers(ExpandoObject obj)
        {
            int playerPed = API.PlayerPedId();
            List<int> players = Utils.GetNearestPlayers();
            bool foundPlayers = false;
            List<Dictionary<string, object>> elements = new List<Dictionary<string, object>>();
            Dictionary<string, object> nuireturn = new Dictionary<string, object>();
            foreach (var player in players)
            {
                foundPlayers = true;
                elements.Add(new Dictionary<string, object>
                {
                    ["label"] = API.GetPlayerName(player),
                    ["player"] = API.GetPlayerServerId(player)
                });
            }

            if (!foundPlayers)
            {
                Debug.WriteLine("No near players");
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
                nuireturn.Add("players", elements);
                nuireturn.Add("item", item["item"]);
                nuireturn.Add("hash", item["hash"]);
                nuireturn.Add("count", item["count"]);
                nuireturn.Add("id", item["id"]);
                nuireturn.Add("type", item["type"]);
                nuireturn.Add("what", item["what"]);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(nuireturn);
                API.SendNuiMessage(json);
            }
        }

        private void NUIGiveItem(ExpandoObject obj)
        {
            int playerPed = API.PlayerPedId();
            List<int> players = Utils.GetNearestPlayers();
            Dictionary<string, object> data = Utils.ProcessDynamicObject(obj);
            Dictionary<string, object> data2 = Utils.ProcessDynamicObject(data["data"]);
            foreach (var varia in players)
            {
                if (varia != API.PlayerId())
                {
                    if (API.GetPlayerServerId(varia) == int.Parse(data["player"].ToString()))
                    {
                        string itemname = data2["item"].ToString();

                        int target = int.Parse(data["player"].ToString());

                        if (data2["type"].ToString().Equals("item_money"))
                        {
                            if (isProcessingPay)
                                return;

                            isProcessingPay = true;
                            TriggerServerEvent("vorp_inventory:giveMoneyToPlayer", target, double.Parse(data2["count"].ToString()));
                        }
                        else if (int.Parse(data2["id"].ToString()) == 0)
                        {
                            int amount = int.Parse(data2["count"].ToString());
                            if (amount > 0 && InventoryAPI.UsersItems[itemname].getCount() >= amount)
                            {
                                TriggerServerEvent("vorpinventory:serverGiveItem", itemname, amount, target, 1);
                            }
                        }
                        else
                        {
                            TriggerServerEvent("vorpinventory:serverGiveWeapon2", int.Parse(data2["id"].ToString()), target);
                            TriggerServerEvent("vorpinventory:weaponlog", target, data2);
                        }

                        LoadInv();
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
                WeaponClass weapon = InventoryAPI.UsersWeapons[ItemId];

                bool isWeaponARevolver = Function.Call<bool>((Hash)0xC212F1D05A8232BB, API.GetHashKey(weapon.getName()));
                bool isWeaponAPistol = Function.Call<bool>((Hash)0xDDC64F5E31EEDAB6, API.GetHashKey(weapon.getName()));
                string weaponName = Function.Call<string>((Hash)0x89CF5FF3D363311E, weaponHash);

                // Check if the weapon used is a pistol or a revolver and ped is not unarmed.
                if ((isWeaponARevolver || isWeaponAPistol) && !weaponName.Contains("UNARMED"))
                {
                    bool isWeaponUsedARevolver = Function.Call<bool>((Hash)0xC212F1D05A8232BB, weaponHash);
                    bool isWeaponUsedAPistol = Function.Call<bool>((Hash)0xDDC64F5E31EEDAB6, weaponHash);
                    if (isWeaponUsedARevolver || isWeaponUsedAPistol)
                    {
                        Debug.WriteLine("Equiping offhand");
                        weapon.setUsed2(true);
                        weapon.loadAmmo();
                        weapon.loadComponents();
                        weapon.setUsed(true);
                        TriggerServerEvent("syn_weapons:weaponused", data);
                        Debug.WriteLine($"used 2 : {weapon.getUsed2()}");

                    }
                }
                else if (!weapon.getUsed() &&
                   !Function.Call<bool>((Hash)0x8DECB02F88F428BC, API.PlayerPedId(), API.GetHashKey(weapon.getName()), 0, true))
                {
                    Debug.WriteLine("THEIR PART");
                    weapon.loadAmmo();
                    weapon.loadComponents();
                    weapon.setUsed(true);
                    TriggerServerEvent("syn_weapons:weaponused", data);
                }
                else
                {
                    //TriggerEvent("vorp:Tip", "Ya tienes equipada esa arma", 3000);
                }
                LoadInv();
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
                Debug.Write(aux["number"].ToString());
                if (aux["number"].ToString() != null && aux["number"].ToString() != "")
                {


                    if (int.Parse(aux["number"].ToString()) > 0 && InventoryAPI.UsersItems[itemname].getCount() >= int.Parse(aux["number"].ToString()))
                    {
                        TriggerServerEvent("vorpinventory:serverDropItem", itemname, int.Parse(aux["number"].ToString()), 1);
                        InventoryAPI.UsersItems[itemname].quitCount(int.Parse(aux["number"].ToString()));
                        if (InventoryAPI.UsersItems[itemname].getCount() == 0)
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
                    WeaponClass wp = InventoryAPI.UsersWeapons[int.Parse(aux["id"].ToString())];
                    if (wp.getUsed())
                    {
                        wp.setUsed(false);
                        API.RemoveWeaponFromPed(API.PlayerPedId(), (uint)API.GetHashKey(wp.getName()),
                            true, 0);
                    }
                    InventoryAPI.UsersWeapons.Remove(int.Parse(aux["id"].ToString()));
                }
            }
            LoadInv();
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

        public void LoadInv()
        {
            TriggerServerEvent("vorpinventory:check_slots");

            UpdateUserItemCache();
            UpdateUserWeaponCache();

            var allItems = _userItemsCache.Concat(_userWeaponsCache).ToList();

            Dictionary<string, object> items = new();
            items.Add("action", "setItems");
            items.Add("itemList", allItems);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(items);

            API.SendNuiMessage(json);
        }

        private void UpdateUserItemCache()
        {
            _userItemsCache.Clear();

            foreach (KeyValuePair<string, ItemClass> itemKvp in InventoryAPI.UsersItems)
            {
                ItemClass item = itemKvp.Value;

                Dictionary<string, dynamic> userItem = new();
                userItem.Add("count", item.getCount());
                userItem.Add("limit", item.getLimit());
                userItem.Add("label", item.getLabel());
                userItem.Add("name", item.getName());
                userItem.Add("type", item.getType());
                userItem.Add("usable", item.getUsable());
                userItem.Add("canRemove", item.getCanRemove());
                _userItemsCache.Add(userItem);
            }
        }

        private void UpdateUserWeaponCache()
        {
            _userWeaponsCache.Clear();

            foreach (KeyValuePair<int, WeaponClass> weaponKvp in InventoryAPI.UsersWeapons)
            {
                WeaponClass weapon = weaponKvp.Value;

                Dictionary<string, dynamic> weaponItem = new();
                weaponItem.Add("count", weapon.getAmmo("Hola"));
                weaponItem.Add("limit", -1);
                weaponItem.Add("label", weapon.weaponLabel);
                weaponItem.Add("name", weapon.getName());
                weaponItem.Add("hash", API.GetHashKey(weapon.getName()));
                weaponItem.Add("type", "item_weapon");
                weaponItem.Add("usable", true);
                weaponItem.Add("canRemove", true);
                weaponItem.Add("id", weapon.getId());
                weaponItem.Add("used", weapon.getUsed());
                _userWeaponsCache.Add(weaponItem);
            }
        }

        private void OpenInventory()
        {
            NUI.SetFocus(true);

            NuiMessage nui = new NuiMessage();
            nui.Action = "display";
            nui.Type = "main";

            API.SendNuiMessage(nui.ToJson());
            IsInventoryOpen = true;
            LoadInv();
        }

        private void OnCloseInventory()
        {
            NUI.SetFocus(false, false);

            NuiMessage nui = new NuiMessage();
            nui.Action = "hide";

            API.SendNuiMessage(nui.ToJson());
            IsInventoryOpen = false;
        }

    }
}
