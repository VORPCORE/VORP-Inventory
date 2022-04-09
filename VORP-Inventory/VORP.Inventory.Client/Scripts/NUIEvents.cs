using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using VorpInventory.Models;

namespace VorpInventory.Scripts
{
    public class NUIEvents : BaseScript
    {
        public static bool InInventory = false;

        public static List<Dictionary<string, dynamic>> gg = new List<Dictionary<string, dynamic>>();
        public static Dictionary<string, object> items = new Dictionary<string, object>();
        public static bool isProcessingPay = false;

        public NUIEvents()
        {

            API.RegisterNuiCallbackType("NUIFocusOff");
            EventHandlers["__cfx_nui:NUIFocusOff"] += new Action<ExpandoObject>(NUIFocusOff);

            API.RegisterNuiCallbackType("DropItem");
            EventHandlers["__cfx_nui:DropItem"] += new Action<ExpandoObject>(NUIDropItem);

            API.RegisterNuiCallbackType("UseItem");
            EventHandlers["__cfx_nui:UseItem"] += new Action<ExpandoObject>(NUIUseItem);

            API.RegisterNuiCallbackType("sound");
            EventHandlers["__cfx_nui:sound"] += new Action<ExpandoObject>(NUISound);

            API.RegisterNuiCallbackType("GiveItem");
            EventHandlers["__cfx_nui:GiveItem"] += new Action<ExpandoObject>(NUIGiveItem);

            API.RegisterNuiCallbackType("GetNearPlayers");
            EventHandlers["__cfx_nui:GetNearPlayers"] += new Action<ExpandoObject>(NUIGetNearPlayers);

            API.RegisterNuiCallbackType("UnequipWeapon");
            EventHandlers["__cfx_nui:UnequipWeapon"] += new Action<ExpandoObject>(NUIUnequipWeapon);

            EventHandlers["vorp_inventory:ProcessingReady"] += new Action(setProcessingPayFalse);


            EventHandlers["vorp_inventory:CloseInv"] += new Action(CloseInventory);

            //HorseModule
            EventHandlers["vorp_inventory:OpenHorseInventory"] += new Action<string, int>(OpenHorseInventory);
            EventHandlers["vorp_inventory:ReloadHorseInventory"] += new Action<string>(ReloadHorseInventory);

            API.RegisterNuiCallbackType("TakeFromHorse");
            EventHandlers["__cfx_nui:TakeFromHorse"] += new Action<ExpandoObject>(NUITakeFromHorse);

            API.RegisterNuiCallbackType("MoveToHorse");
            EventHandlers["__cfx_nui:MoveToHorse"] += new Action<ExpandoObject>(NUIMoveToHorse);

            //Steal
            EventHandlers["vorp_inventory:OpenstealInventory"] += new Action<string, int>(OpenstealInventory);
            EventHandlers["vorp_inventory:ReloadstealInventory"] += new Action<string>(ReloadstealInventory);

            API.RegisterNuiCallbackType("TakeFromsteal");
            EventHandlers["__cfx_nui:TakeFromsteal"] += new Action<ExpandoObject>(NUITakeFromsteal);

            API.RegisterNuiCallbackType("MoveTosteal");
            EventHandlers["__cfx_nui:MoveTosteal"] += new Action<ExpandoObject>(NUIMoveTosteal);

            //CartModule
            EventHandlers["vorp_inventory:OpenCartInventory"] += new Action<string, int>(OpenCartInventory);
            EventHandlers["vorp_inventory:ReloadCartInventory"] += new Action<string>(ReloadCartInventory);

            API.RegisterNuiCallbackType("TakeFromCart");
            EventHandlers["__cfx_nui:TakeFromCart"] += new Action<ExpandoObject>(NUITakeFromCart);

            API.RegisterNuiCallbackType("MoveToCart");
            EventHandlers["__cfx_nui:MoveToCart"] += new Action<ExpandoObject>(NUIMoveToCart);

            //HouseModule
            EventHandlers["vorp_inventory:OpenHouseInventory"] += new Action<string, int>(OpenHouseInventory);
            EventHandlers["vorp_inventory:ReloadHouseInventory"] += new Action<string>(ReloadHouseInventory);

            API.RegisterNuiCallbackType("TakeFromHouse");
            EventHandlers["__cfx_nui:TakeFromHouse"] += new Action<ExpandoObject>(NUITakeFromHouse);

            API.RegisterNuiCallbackType("MoveToHouse");
            EventHandlers["__cfx_nui:MoveToHouse"] += new Action<ExpandoObject>(NUIMoveToHouse);

            //HideoutModule
            EventHandlers["vorp_inventory:OpenHideoutInventory"] += new Action<string, int>(OpenHideoutInventory);
            EventHandlers["vorp_inventory:ReloadHideoutInventory"] += new Action<string>(ReloadHideoutInventory);

            API.RegisterNuiCallbackType("TakeFromHideout");
            EventHandlers["__cfx_nui:TakeFromHideout"] += new Action<ExpandoObject>(NUITakeFromHideout);

            API.RegisterNuiCallbackType("MoveToHideout");
            EventHandlers["__cfx_nui:MoveToHideout"] += new Action<ExpandoObject>(NUIMoveToHideout);

            //clan
            EventHandlers["vorp_inventory:OpenClanInventory"] += new Action<string, int>(OpenClanInventory);
            EventHandlers["vorp_inventory:ReloadClanInventory"] += new Action<string>(ReloadClanInventory);

            API.RegisterNuiCallbackType("TakeFromClan");
            EventHandlers["__cfx_nui:TakeFromClan"] += new Action<ExpandoObject>(NUITakeFromClan);

            API.RegisterNuiCallbackType("MoveToClan");
            EventHandlers["__cfx_nui:MoveToClan"] += new Action<ExpandoObject>(NUIMoveToClan);

            //Container
            EventHandlers["vorp_inventory:OpenContainerInventory"] += new Action<string, int>(OpenContainerInventory);
            EventHandlers["vorp_inventory:ReloadContainerInventory"] += new Action<string>(ReloadContainerInventory);

            API.RegisterNuiCallbackType("TakeFromContainer");
            EventHandlers["__cfx_nui:TakeFromContainer"] += new Action<ExpandoObject>(NUITakeFromContainer);

            API.RegisterNuiCallbackType("MoveToContainer");
            EventHandlers["__cfx_nui:MoveToContainer"] += new Action<ExpandoObject>(NUIMoveToContainer);
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

        private void OpenClanInventory(string clanName, int clanid)
        {
            //"action", "setSecondInventoryItems"
            API.SetNuiFocus(true, true);

            API.SendNuiMessage("{\"action\": \"display\", \"type\": \"clan\", \"title\": \"" + clanName + "\", \"clanid\": " + clanid.ToString() + "}");
            InInventory = true;
            //TriggerEvent("vorp_stables:setClosedInv", true);
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

        private void OpenContainerInventory(string ContainerName, int Containerid)
        {
            //"action", "setSecondInventoryItems"
            API.SetNuiFocus(true, true);

            API.SendNuiMessage("{\"action\": \"display\", \"type\": \"Container\", \"title\": \"" + ContainerName + "\", \"Containerid\": " + Containerid.ToString() + "}");
            InInventory = true;
            //TriggerEvent("vorp_stables:setClosedInv", true);
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

        private void CloseInventory()
        {
            API.SetNuiFocus(false, false);
            API.SendNuiMessage("{\"action\": \"hide\"}");
            InInventory = false;
        }

        private void OpenHorseInventory(string horseName, int horseid)
        {
            //"action", "setSecondInventoryItems"
            API.SetNuiFocus(true, true);

            API.SendNuiMessage("{\"action\": \"display\", \"type\": \"horse\", \"title\": \"" + horseName + "\", \"horseid\": " + horseid.ToString() + "}");
            InInventory = true;
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




        private void OpenstealInventory(string stealName, int stealid)
        {
            //"action", "setSecondInventoryItems"
            API.SetNuiFocus(true, true);

            API.SendNuiMessage("{\"action\": \"display\", \"type\": \"steal\", \"title\": \"" + stealName + "\", \"stealId\": " + stealid.ToString() + "}");

            InInventory = true;
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

        private void OpenCartInventory(string cartName, int wagonid)
        {
            //"action", "setSecondInventoryItems"
            API.SetNuiFocus(true, true);

            API.SendNuiMessage("{\"action\": \"display\", \"type\": \"cart\", \"title\": \"" + cartName + "\", \"wagonid\": " + wagonid.ToString() + "}");
            InInventory = true;
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
            //"action", "setSecondInventoryItems"
            API.SetNuiFocus(true, true);

            API.SendNuiMessage("{\"action\": \"display\", \"type\": \"house\", \"title\": \"" + houseName + "\", \"houseId\": " + houseId.ToString() + "}");
            InInventory = true;
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
            //"action", "setSecondInventoryItems"
            API.SetNuiFocus(true, true);

            API.SendNuiMessage("{\"action\": \"display\", \"type\": \"hideout\", \"title\": \"" + hideoutName + "\", \"hideoutId\": " + hideoutId.ToString() + "}");
            InInventory = true;
            //TriggerEvent("vorp_stables:setClosedInv", true);
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
            if (PluginManager.userWeapons.ContainsKey(int.Parse(data["id"].ToString())))
            {
                PluginManager.userWeapons[int.Parse(data["id"].ToString())].UnequipWeapon();
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
            //Debug.WriteLine(data2["id"].ToString());
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
                            if (amount > 0 && PluginManager.useritems[itemname].getCount() >= amount)
                            {
                                TriggerServerEvent("vorpinventory:serverGiveItem", itemname, amount, target, 1);
                                /*vorp_inventoryClient.useritems[itemname].quitCount(amount);
                                if (vorp_inventoryClient.useritems[itemname].getCount() == 0)
                                {
                                    vorp_inventoryClient.useritems.Remove(itemname);
                                }*/
                            }
                        }
                        else
                        {
                            TriggerServerEvent("vorpinventory:serverGiveWeapon2", int.Parse(data2["id"].ToString()), target);
                            TriggerServerEvent("vorpinventory:weaponlog", target, data2);


                            /*  if (vorp_inventoryClient.userWeapons.ContainsKey(int.Parse(data2["id"].ToString())))
                             {
                                 if (vorp_inventoryClient.userWeapons[int.Parse(data2["id"].ToString())].getUsed())
                                 {
                                     vorp_inventoryClient.userWeapons[int.Parse(data2["id"].ToString())].setUsed(false);
                                     vorp_inventoryClient.userWeapons[int.Parse(data2["id"].ToString())].RemoveWeaponFromPed();
                                 }
                                 vorp_inventoryClient.userWeapons.Remove(int.Parse(data2["id"].ToString()));
                             } */
                        }

                        LoadInv();
                    }
                }
            }
        }

        private void NUIUseItem(ExpandoObject obj)
        {
            Dictionary<string, object> data = Utils.ProcessDynamicObject(obj);
            // foreach (var VARIABLE in data)
            // {
            //     Debug.WriteLine($"{VARIABLE.Key}: {VARIABLE.Value}");
            // }
            if (data["type"].ToString().Contains("item_standard"))
            {
                // string eventString = "vorp:use" + data["item"];
                // TriggerServerEvent(eventString); Version antigua
                TriggerServerEvent("vorp:use", data["item"]);
            }
            else if (data["type"].ToString().Contains("item_weapon"))
            {
                uint weaponHash = 0;
                API.GetCurrentPedWeapon(API.PlayerPedId(), ref weaponHash, false, 0, false);

                bool isWeaponARevolver = Function.Call<bool>((Hash)0xC212F1D05A8232BB, API.GetHashKey(PluginManager.userWeapons[int.Parse(data["id"].ToString())].getName()));
                bool isWeaponAPistol = Function.Call<bool>((Hash)0xDDC64F5E31EEDAB6, API.GetHashKey(PluginManager.userWeapons[int.Parse(data["id"].ToString())].getName()));
                string weaponName = Function.Call<string>((Hash)0x89CF5FF3D363311E, weaponHash);

                // Check if the weapon used is a pistol or a revolver and ped is not unarmed.
                if ((isWeaponARevolver || isWeaponAPistol) && !weaponName.Contains("UNARMED"))
                {
                    // OUR PART
                    Debug.WriteLine("OUR PART");
                    bool isWeaponUsedARevolver = Function.Call<bool>((Hash)0xC212F1D05A8232BB, weaponHash);
                    bool isWeaponUsedAPistol = Function.Call<bool>((Hash)0xDDC64F5E31EEDAB6, weaponHash);
                    if (isWeaponUsedARevolver || isWeaponUsedAPistol)
                    {
                        Debug.WriteLine("Equiping offhand");
                        PluginManager.userWeapons[int.Parse(data["id"].ToString())].setUsed2(true);
                        PluginManager.userWeapons[int.Parse(data["id"].ToString())].loadAmmo();
                        PluginManager.userWeapons[int.Parse(data["id"].ToString())].loadComponents();
                        PluginManager.userWeapons[int.Parse(data["id"].ToString())].setUsed(true);
                        TriggerServerEvent("syn_weapons:weaponused", data);
                        Debug.WriteLine($"used 2 : {PluginManager.userWeapons[int.Parse(data["id"].ToString())].getUsed2()}");

                    }
                }
                else if (!PluginManager.userWeapons[int.Parse(data["id"].ToString())].getUsed() &&
                   !Function.Call<bool>((Hash)0x8DECB02F88F428BC, API.PlayerPedId(), API.GetHashKey(PluginManager.userWeapons[int.Parse(data["id"].ToString())].getName()), 0, true))
                {
                    Debug.WriteLine("THEIR PART");
                    PluginManager.userWeapons[int.Parse(data["id"].ToString())].loadAmmo();
                    PluginManager.userWeapons[int.Parse(data["id"].ToString())].loadComponents();
                    PluginManager.userWeapons[int.Parse(data["id"].ToString())].setUsed(true);
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


                    if (int.Parse(aux["number"].ToString()) > 0 && PluginManager.useritems[itemname].getCount() >= int.Parse(aux["number"].ToString()))
                    {
                        TriggerServerEvent("vorpinventory:serverDropItem", itemname, int.Parse(aux["number"].ToString()), 1);
                        PluginManager.useritems[itemname].quitCount(int.Parse(aux["number"].ToString()));
                        //Debug.Write(vorp_inventoryClient.useritems[itemname].getCount().ToString());
                        if (PluginManager.useritems[itemname].getCount() == 0)
                        {
                            PluginManager.useritems.Remove(itemname);
                        }
                    }
                }
            }
            else
            {
                //Function.Call((Hash) 0x4899CB088EDF59B8, API.PlayerPedId(), (uint) int.Parse(aux["hash"]),false,false);
                TriggerServerEvent("vorpinventory:serverDropWeapon", int.Parse(aux["id"].ToString()));
                if (PluginManager.userWeapons.ContainsKey(int.Parse(aux["id"].ToString())))
                {
                    WeaponClass wp = PluginManager.userWeapons[int.Parse(aux["id"].ToString())];
                    if (wp.getUsed())
                    {
                        wp.setUsed(false);
                        API.RemoveWeaponFromPed(API.PlayerPedId(), (uint)API.GetHashKey(wp.getName()),
                            true, 0);
                    }
                    PluginManager.userWeapons.Remove(int.Parse(aux["id"].ToString()));
                }
            }
            LoadInv();
        }

        private void NUISound(ExpandoObject obj)
        {
            API.PlaySoundFrontend("BACK", "RDRO_Character_Creator_Sounds", true, 0);
        }

        private void NUIFocusOff(ExpandoObject obj)
        {
            CloseInv();
            TriggerEvent("vorp_stables:setClosedInv", false);
            TriggerEvent("syn:closeinv");
        }

        [Tick]
        private async Task OnKey()
        {
            if (!GetConfig.loaded)
            {
                return;
            }

            if (API.IsControlJustReleased(1, GetConfig.openKey) && API.IsInputDisabled(0))
            {
                if (InInventory)
                {
                    CloseInv();
                    await Delay(1000);
                }
                else
                {
                    OpenInv();
                    await Delay(1000);
                }
            }

        }

        public static async Task LoadInv()
        {
            Dictionary<string, dynamic> item;
            Dictionary<string, dynamic> weapon;
            items.Clear();
            gg.Clear();
            TriggerServerEvent("vorpinventory:check_slots");
            foreach (KeyValuePair<string, ItemClass> userit in PluginManager.useritems)
            {
                item = new Dictionary<string, dynamic>();
                item.Add("count", userit.Value.getCount());
                item.Add("limit", userit.Value.getLimit());
                item.Add("label", userit.Value.getLabel());
                item.Add("name", userit.Value.getName());
                item.Add("type", userit.Value.getType());
                item.Add("usable", userit.Value.getUsable());
                item.Add("canRemove", userit.Value.getCanRemove());
                gg.Add(item);
            }

            foreach (KeyValuePair<int, WeaponClass> userwp in PluginManager.userWeapons)
            {
                weapon = new Dictionary<string, dynamic>();
                weapon.Add("count", userwp.Value.getAmmo("Hola"));
                weapon.Add("limit", -1);
                weapon.Add("label", userwp.Value.weaponLabel);
                weapon.Add("name", userwp.Value.getName());
                weapon.Add("hash", API.GetHashKey(userwp.Value.getName()));
                weapon.Add("type", "item_weapon");
                weapon.Add("usable", true);
                weapon.Add("canRemove", true);
                weapon.Add("id", userwp.Value.getId());
                weapon.Add("used", userwp.Value.getUsed());
                gg.Add(weapon);
            }
            items.Add("action", "setItems");
            items.Add("itemList", gg);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(items);

            API.SendNuiMessage(json);
        }

        private async Task OpenInv()
        {
            API.SetNuiFocus(true, true);

            API.SendNuiMessage("{\"action\": \"display\", \"type\": \"main\"}");
            InInventory = true;

            LoadInv();
        }

        private async Task CloseInv()
        {
            API.SetNuiFocus(false, false);
            API.SendNuiMessage("{\"action\": \"hide\"}");
            InInventory = false;
        }

    }
}
