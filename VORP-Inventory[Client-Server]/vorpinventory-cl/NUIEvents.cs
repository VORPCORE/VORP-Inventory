using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using vorpinventory_sv;

namespace vorpinventory_cl
{
    public class NUIEvents : BaseScript
    {
        public static bool InInventory = false;

        public static List<Dictionary<string, dynamic>> gg = new List<Dictionary<string, dynamic>>();
        public static Dictionary<string, object> items = new Dictionary<string, object>();
        public static bool isProcessingPay = false;

        public NUIEvents()
        {
            Tick += OnKey;
            API.RegisterCommand("giveAmmo", new Action<int, List<object>, string>(((source, args, raw) =>
               {
                   // int ammoType = API.GetHashKey("AMMO_ARROW_FIRE");
                   // Debug.WriteLine($"Me ejecuto {ammoType}");
                   // API.SetPedAmmoByType(API.PlayerPedId(), ammoType, 200);
                   //API.SetPedAmmo(API.PlayerPedId(),(uint)API.GetHashKey("WEAPON_RIFLE_VARMINT"),200);
                   int playerPed = API.PlayerPedId();
                   int ammoQuantity = 200;
                   int ammoType = API.GetHashKey("AMMO_ARROW_DYNAMITE");
                   API.SetPedAmmoByType(playerPed, ammoType, ammoQuantity);

               })), false);
            API.RegisterCommand("cleanAmmo", new Action<int, List<object>, string>(((source, args, raw) =>
               {
                   API.SetPedAmmoByType(API.PlayerPedId(), API.GetHashKey("AMMO_RIFLE_VARMINT"), 0);
               })), false);
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
        }

        private void setProcessingPayFalse()
        {
            isProcessingPay = false;
        }

        private void NUIUnequipWeapon(ExpandoObject obj)
        {
            Dictionary<string, object> data = Utils.expandoProcessing(obj);
            if (vorp_inventoryClient.userWeapons.ContainsKey(int.Parse(data["id"].ToString())))
            {
                vorp_inventoryClient.userWeapons[int.Parse(data["id"].ToString())].UnequipWeapon();
            }
            LoadInv();
        }

        private void NUIGetNearPlayers(ExpandoObject obj)
        {
            int playerPed = API.PlayerPedId();
            List<int> players = Utils.getNearestPlayers();
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
                    Debug.WriteLine($"{thing.Key}, {thing.Value}");
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
                Debug.WriteLine(json);
                API.SendNuiMessage(json);
            }
        }

        private void NUIGiveItem(ExpandoObject obj)
        {
            int playerPed = API.PlayerPedId();
            List<int> players = Utils.getNearestPlayers();
            Dictionary<string, object> data = Utils.expandoProcessing(obj);
            Dictionary<string, object> data2 = Utils.expandoProcessing(data["data"]);
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
                            if (amount > 0 && vorp_inventoryClient.useritems[itemname].getCount() >= amount)
                            {
                                TriggerServerEvent("vorpinventory:serverGiveItem", itemname, amount, target, 1);
                                vorp_inventoryClient.useritems[itemname].quitCount(amount);
                                if (vorp_inventoryClient.useritems[itemname].getCount() == 0)
                                {
                                    vorp_inventoryClient.useritems.Remove(itemname);
                                }
                            }
                        }
                        else
                        {
                            TriggerServerEvent("vorpinventory:serverGiveWeapon", int.Parse(data2["id"].ToString()), target);
                            if (vorp_inventoryClient.userWeapons.ContainsKey(int.Parse(data2["id"].ToString())))
                            {
                                if (vorp_inventoryClient.userWeapons[int.Parse(data2["id"].ToString())].getUsed())
                                {
                                    vorp_inventoryClient.userWeapons[int.Parse(data2["id"].ToString())].setUsed(false);
                                    vorp_inventoryClient.userWeapons[int.Parse(data2["id"].ToString())].RemoveWeaponFromPed();
                                }
                                vorp_inventoryClient.userWeapons.Remove(int.Parse(data2["id"].ToString()));
                            }
                        }

                        LoadInv();
                    }
                }
            }
        }

        private void NUIUseItem(ExpandoObject obj)
        {
            Debug.WriteLine("Llego");
            Dictionary<string, object> data = Utils.expandoProcessing(obj);
            // foreach (var VARIABLE in data)
            // {
            //     Debug.WriteLine($"{VARIABLE.Key}: {VARIABLE.Value}");
            // }
            if (data["type"].ToString().Contains("item_standard"))
            {
                // string eventString = "vorp:use" + data["item"];
                // TriggerServerEvent(eventString); Version antigua
                Debug.WriteLine(data["item"] + "Usado");
                TriggerServerEvent("vorp:use",data["item"]);
            }
            else if(data["type"].ToString().Contains("item_weapon"))
            {
                if (!vorp_inventoryClient.userWeapons[int.Parse(data["id"].ToString())].getUsed() &&
                    !Function.Call<bool>((Hash)0x8DECB02F88F428BC, API.PlayerPedId(), API.GetHashKey(vorp_inventoryClient.userWeapons[int.Parse(data["id"].ToString())].getName()), 0, true))
                {
                    vorp_inventoryClient.userWeapons[int.Parse(data["id"].ToString())].loadAmmo();
                    vorp_inventoryClient.userWeapons[int.Parse(data["id"].ToString())].loadComponents();
                    vorp_inventoryClient.userWeapons[int.Parse(data["id"].ToString())].setUsed(true);
                }
                else
                {
                    Debug.WriteLine($"No uso el arma {data["id"]}");
                    //TriggerEvent("vorp:Tip", "Ya tienes equipada esa arma", 3000);
                }
                LoadInv();
            }
        }

        private void NUIDropItem(ExpandoObject obj)
        {
            Dictionary<string, dynamic> aux = Utils.expandoProcessing(obj);
            string itemname = aux["item"];
            string type = aux["type"].ToString();
            Debug.WriteLine(type);
            Debug.WriteLine(itemname);
            if (type == "item_money")
            {
                TriggerServerEvent("vorpinventory:serverDropMoney", double.Parse(aux["number"].ToString()));
                Debug.WriteLine("Soltando dinero");
                Debug.WriteLine("Soltando dinero");
            }
            else if (type == "item_standard")
            {
                if (int.Parse(aux["number"].ToString()) > 0 && vorp_inventoryClient.useritems[itemname].getCount() >= int.Parse(aux["number"].ToString()))
                {
                    TriggerServerEvent("vorpinventory:serverDropItem", itemname, int.Parse(aux["number"].ToString()), 1);
                    vorp_inventoryClient.useritems[itemname].quitCount(int.Parse(aux["number"].ToString()));
                    //Debug.Write(vorp_inventoryClient.useritems[itemname].getCount().ToString());
                    if (vorp_inventoryClient.useritems[itemname].getCount() == 0)
                    {
                        vorp_inventoryClient.useritems.Remove(itemname);
                    }
                }
            }
            else
            {
                //Function.Call((Hash) 0x4899CB088EDF59B8, API.PlayerPedId(), (uint) int.Parse(aux["hash"]),false,false);
                Debug.WriteLine("Tirando Arma");
                Debug.WriteLine(aux["id"].ToString());
                TriggerServerEvent("vorpinventory:serverDropWeapon", int.Parse(aux["id"].ToString()));
                if (vorp_inventoryClient.userWeapons.ContainsKey(int.Parse(aux["id"].ToString())))
                {
                    WeaponClass wp = vorp_inventoryClient.userWeapons[int.Parse(aux["id"].ToString())];
                    if (wp.getUsed())
                    {
                        wp.setUsed(false);
                        API.RemoveWeaponFromPed(API.PlayerPedId(), (uint)API.GetHashKey(wp.getName()),
                            true, 0);
                    }
                    vorp_inventoryClient.userWeapons.Remove(int.Parse(aux["id"].ToString()));
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
            foreach (KeyValuePair<string, ItemClass> userit in vorp_inventoryClient.useritems)
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

            foreach (KeyValuePair<int, WeaponClass> userwp in vorp_inventoryClient.userWeapons)
            {
                weapon = new Dictionary<string, dynamic>();
                weapon.Add("count", userwp.Value.getAmmo("Hola"));
                weapon.Add("limit", -1);
                weapon.Add("label", Function.Call<string>((Hash)0x89CF5FF3D363311E, API.GetHashKey(userwp.Value.getName())));
                weapon.Add("name", userwp.Value.getName());
                weapon.Add("hash", API.GetHashKey(userwp.Value.getName()));
                weapon.Add("type", "item_weapon");
                weapon.Add("usable", true);
                weapon.Add("canRemove", true);
                weapon.Add("id", userwp.Value.getId());
                weapon.Add("used", userwp.Value.getUsed());
                Debug.WriteLine(userwp.Value.getId().ToString());
                gg.Add(weapon);
            }
            items.Add("action", "setItems");
            items.Add("itemList", gg);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(items);

            Debug.WriteLine(json);

            API.SendNuiMessage(json);
        }

        private async Task OpenInv()
        {
            API.SetNuiFocus(true, true);

            API.SendNuiMessage("{\"action\": \"display\"}");
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
