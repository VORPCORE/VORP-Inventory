using CitizenFX.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VorpInventory.Database;
using VorpInventory.Diagnostics;
using VorpInventory.Models;

namespace VorpInventory.Scripts
{
    public class VorpInventory : BaseScript
    {
        public static dynamic CORE;

        PlayerList PlayerList;

        public VorpInventory()
        {
            PlayerList = Players;

            EventHandlers["vorpinventory:getItemsTable"] += new Action<Player>(getItemsTable);
            EventHandlers["vorpinventory:getInventory"] += new Action<Player>(getInventory);
            EventHandlers["vorpinventory:serverGiveItem"] += new Action<Player, string, int, int>(serverGiveItem);
            EventHandlers["vorpinventory:serverGiveWeapon"] += new Action<Player, int, int>(serverGiveWeapon);
            EventHandlers["vorpinventory:serverDropItem"] += new Action<Player, string, int>(serverDropItem);
            EventHandlers["vorpinventory:serverDropMoney"] += new Action<Player, double>(serverDropMoney);
            EventHandlers["vorpinventory:serverDropAllMoney"] += new Action<Player>(serverDropAllMoney);
            EventHandlers["vorpinventory:serverDropWeapon"] += new Action<Player, int>(serverDropWeapon);
            EventHandlers["vorpinventory:sharePickupServer"] += new Action<string, int, int, Vector3, int>(sharePickupServer);
            EventHandlers["vorpinventory:shareMoneyPickupServer"] += new Action<int, double, Vector3>(shareMoneyPickupServer);
            EventHandlers["vorpinventory:onPickup"] += new Action<Player, int>(onPickup);
            EventHandlers["vorpinventory:onPickupMoney"] += new Action<Player, int>(onPickupMoney);
            EventHandlers["vorpinventory:setUsedWeapon"] += new Action<Player, int, bool, bool>(usedWeapon);
            EventHandlers["vorpinventory:setWeaponBullets"] += new Action<Player, int, string, int>(setWeaponBullets);
            EventHandlers["vorp_inventory:giveMoneyToPlayer"] += new Action<Player, int, double>(giveMoneyToPlayer);

            TriggerEvent("getCore", new Action<dynamic>((dic) =>
            {
                CORE = dic;
            }));
        }

        private void serverDropMoney([FromSource] Player source, double amount)
        {
            int _source = int.Parse(source.Handle);

            dynamic UserCharacter = VorpInventory.CORE.getUser(_source).getUsedCharacter;

            double sourceMoney = UserCharacter.money;

            if (amount <= 0)
            {
                source.TriggerEvent("vorp:TipRight", Config.lang["TryExploits"], 3000);
            }
            else if (sourceMoney < amount)
            {
                source.TriggerEvent("vorp:TipRight", Config.lang["NotEnoughMoney"], 3000);
            }
            else
            {
                UserCharacter.removeCurrency(0, amount);
                source.TriggerEvent("vorpInventory:createMoneyPickup", amount);
            }

        }

        private void serverDropAllMoney([FromSource] Player source)
        {
            int _source = int.Parse(source.Handle);

            dynamic UserCharacter = VorpInventory.CORE.getUser(_source).getUsedCharacter;

            double sourceMoney = UserCharacter.money;

            if (sourceMoney > 0)
            {
                UserCharacter.removeCurrency(0, sourceMoney);
                source.TriggerEvent("vorpInventory:createMoneyPickup", sourceMoney);
            }

        }

        private async void giveMoneyToPlayer([FromSource] Player source, int target, double amount)
        {
            int _source = int.Parse(source.Handle);
            Player _target = PlayerList[target];

            if (_target == null)
            {
                Logger.Error($"Target Player '{_target} does not exist.");
                return;
            }

            dynamic UserCharacter = VorpInventory.CORE.getUser(_source).getUsedCharacter;

            double sourceMoney = UserCharacter.money;
            Debug.WriteLine(sourceMoney.ToString());
            Debug.WriteLine(amount.ToString());
            if (amount <= 0)
            {
                source.TriggerEvent("vorp:TipRight", Config.lang["TryExploits"], 3000);
                await Delay(3000);
                source.TriggerEvent("vorp_inventory:ProcessingReady");
            }
            else if (sourceMoney < amount)
            {
                source.TriggerEvent("vorp:TipRight", Config.lang["NotEnoughMoney"], 3000);
                await Delay(3000);
                source.TriggerEvent("vorp_inventory:ProcessingReady");

            }
            else
            {
                UserCharacter.removeCurrency(0, amount);
                dynamic TargetUserCharacter = VorpInventory.CORE.getUser(target).getUsedCharacter;
                TargetUserCharacter.addCurrency(0, amount);
                source.TriggerEvent("vorp:TipRight", string.Format(Config.lang["YouPaid"], amount.ToString(), _target.Name), 3000);
                _target.TriggerEvent("vorp:TipRight", string.Format(Config.lang["YouReceived"], amount.ToString(), source.Name), 3000);
                await Delay(3000);
                source.TriggerEvent("vorp_inventory:ProcessingReady");
            }
        }

        public static Dictionary<int, Dictionary<string, dynamic>> Pickups = new Dictionary<int, Dictionary<string, dynamic>>();

        public static Dictionary<int, Dictionary<string, dynamic>> PickupsMoney = new Dictionary<int, Dictionary<string, dynamic>>();

        private void setWeaponBullets([FromSource] Player player, int weaponId, string type, int bullet)
        {
            if (ItemDatabase.userWeapons.ContainsKey(weaponId))
            {
                ItemDatabase.userWeapons[weaponId].setAmmo(bullet, type);
            }
        }

        public async Task SaveInventoryItemsSupport(Player source)
        {
            await Delay(1000);
            string identifier = "steam:" + source.Identifiers["steam"];
            dynamic CoreUser = CORE.getUser(int.Parse(source.Handle)).getUsedCharacter;

            int charIdentifier = CoreUser.charIdentifier;

            Dictionary<string, int> items = new Dictionary<string, int>();
            if (ItemDatabase.usersInventory.ContainsKey(identifier))
            {
                foreach (var item in ItemDatabase.usersInventory[identifier])
                {
                    items.Add(item.Key, item.Value.getCount());
                }
                if (items.Count > 0)
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(items);
                    Exports["ghmattimysql"].execute($"UPDATE characters SET inventory = '{json}' WHERE `identifier` = ? AND `charidentifier` = ?;", new object[] { identifier, charIdentifier });
                }
            }
        }

        private void usedWeapon([FromSource] Player source, int id, bool used, bool used2)
        {
            int Used = used ? 1 : 0;
            int Used2 = used2 ? 1 : 0;
            Exports["ghmattimysql"]
                .execute(
                    $"UPDATE loadout SET used = '{Used}' , used2 = {Used2} WHERE id=?",
                    new[] { id });
        }

        //Sub items for other scripts
        private void subItem(int player, string name, int cuantity)
        {
            Player p = PlayerList[player];

            if (p == null)
            {
                Logger.Error($"Player '{player}' does not exist.");
                return;
            }

            string identifier = "steam:" + p.Identifiers["steam"];
            if (ItemDatabase.usersInventory.ContainsKey(identifier))
            {
                if (ItemDatabase.usersInventory[identifier].ContainsKey(name))
                {
                    if (cuantity <= ItemDatabase.usersInventory[identifier][name].getCount())
                    {
                        ItemDatabase.usersInventory[identifier][name].quitCount(cuantity);
                        SaveInventoryItemsSupport(p);
                    }

                    if (ItemDatabase.usersInventory[identifier][name].getCount() == 0)
                    {
                        ItemDatabase.usersInventory[identifier].Remove(name);
                        SaveInventoryItemsSupport(p);
                    }
                }
            }
        }

        //For other scripts add items
        private void addItem(int player, string name, int cuantity)
        {
            Player p = PlayerList[player];

            if (p == null)
            {
                Logger.Error($"Player '{player}' does not exist.");
                return;
            }

            string identifier = "steam:" + p.Identifiers["steam"];
            if (ItemDatabase.usersInventory.ContainsKey(identifier))
            {
                if (ItemDatabase.usersInventory[identifier].ContainsKey(name))
                {
                    if (cuantity > 0)
                    {
                        ItemDatabase.usersInventory[identifier][name].addCount(cuantity);
                        SaveInventoryItemsSupport(p);
                    }
                }
                else
                {
                    if (ItemDatabase.svItems.ContainsKey(name))
                    {
                        ItemDatabase.usersInventory[identifier].Add(name, new ItemClass(cuantity, ItemDatabase.svItems[name].getLimit(),
                            ItemDatabase.svItems[name].getLabel(), name, "item_inventory", true, ItemDatabase.svItems[name].getCanRemove()));
                        SaveInventoryItemsSupport(p);
                    }
                }
            }
            else
            {
                Dictionary<string, ItemClass> userinv = new Dictionary<string, ItemClass>();
                ItemDatabase.usersInventory.Add(identifier, userinv);
                if (ItemDatabase.svItems.ContainsKey(name))
                {
                    ItemDatabase.usersInventory[identifier].Add(name, new ItemClass(cuantity, ItemDatabase.svItems[name].getLimit(),
                        ItemDatabase.svItems[name].getLabel(), name, "item_inventory", true, ItemDatabase.svItems[name].getCanRemove()));
                    SaveInventoryItemsSupport(p);
                }
            }
        }

        private void addWeapon(int player, int weapId)
        {
            Player p = PlayerList[player];

            if (p == null)
            {
                Logger.Error($"Player '{player}' does not exist.");
                return;
            }

            string identifier = "steam:" + p.Identifiers["steam"];
            if (ItemDatabase.userWeapons.ContainsKey(weapId))
            {
                ItemDatabase.userWeapons[weapId].setPropietary(identifier);
                dynamic CoreUser = CORE.getUser(player).getUsedCharacter;
                int charIdentifier = CoreUser.charIdentifier;
                Exports["ghmattimysql"]
                    .execute(
                        $"UPDATE loadout SET identifier = '{ItemDatabase.userWeapons[weapId].getPropietary()}', charidentifier = '{charIdentifier}' WHERE id=?",
                        new[] { weapId });
            }
        }

        private void subWeapon(int player, int weapId)
        {

            Player p = PlayerList[player];

            if (p == null)
            {
                Logger.Error($"Player '{player}' does not exist.");
                return;
            }

            string identifier = "steam:" + p.Identifiers["steam"];
            if (ItemDatabase.userWeapons.ContainsKey(weapId))
            {
                ItemDatabase.userWeapons[weapId].setPropietary("");
                dynamic CoreUser = CORE.getUser(player).getUsedCharacter;
                int charIdentifier = CoreUser.charIdentifier;
                Exports["ghmattimysql"]
                    .execute(
                        $"UPDATE loadout SET identifier = '{ItemDatabase.userWeapons[weapId].getPropietary()}', charidentifier = '{charIdentifier}' WHERE id=?",
                        new[] { weapId });
            }
        }

        private void onPickup([FromSource] Player player, int obj)
        {
            string identifier = "steam:" + player.Identifiers["steam"];
            int source = int.Parse(player.Handle);
            dynamic CoreUser = CORE.getUser(source).getUsedCharacter;
            int charIdentifier = CoreUser.charIdentifier;
            if (Pickups.ContainsKey(obj))
            {
                if (Pickups[obj]["weaponid"] == 1)
                {
                    if (ItemDatabase.usersInventory.ContainsKey(identifier))
                    {

                        if (ItemDatabase.svItems[Pickups[obj]["name"]].getLimit() != -1)
                        {
                            if (ItemDatabase.usersInventory[identifier].ContainsKey(Pickups[obj]["name"]))
                            {
                                int totalcount = Pickups[obj]["amount"] + ItemDatabase.usersInventory[identifier][Pickups[obj]["name"]].getCount();

                                if (ItemDatabase.svItems[Pickups[obj]["name"]].getLimit() < totalcount)
                                {
                                    TriggerClientEvent(player, "vorp:TipRight", Config.lang["fullInventory"], 2000);
                                    return;
                                }
                            }
                            //int totalcount = Pickups[obj]["amount"] ItemDatabase.usersInventory[identifier];
                            //totalcount += Pickups[obj]["amount"];
                            //ItemDatabase.svItems[Pickups[obj]["name"]].getCount();

                        }

                        if (Config.MaxItems != 0)
                        {
                            int totalcount = InventoryAPI.getUserTotalCount(identifier);
                            totalcount += Pickups[obj]["amount"];
                            if (totalcount <= Config.MaxItems)
                            {
                                addItem(source, Pickups[obj]["name"], Pickups[obj]["amount"]);
                                Debug.WriteLine($"añado {Pickups[obj]["amount"]}");
                                TriggerClientEvent("vorpInventory:sharePickupClient", Pickups[obj]["name"], Pickups[obj]["obj"],
                                    Pickups[obj]["amount"], Pickups[obj]["coords"], 2, Pickups[obj]["weaponid"]);
                                TriggerClientEvent("vorpInventory:removePickupClient", Pickups[obj]["obj"]);
                                player.TriggerEvent("vorpinventory:receiveItem", Pickups[obj]["name"], Pickups[obj]["amount"]);
                                player.TriggerEvent("vorpInventory:playerAnim", obj);
                                Pickups.Remove(obj);
                            }
                            else
                            {
                                TriggerClientEvent(player, "vorp:TipRight", Config.lang["fullInventory"], 2000);
                            }
                        }
                        else
                        {
                            addItem(source, Pickups[obj]["name"], Pickups[obj]["amount"]);
                            Debug.WriteLine($"añado {Pickups[obj]["amount"]}");
                            TriggerClientEvent("vorpInventory:sharePickupClient", Pickups[obj]["name"], Pickups[obj]["obj"],
                                Pickups[obj]["amount"], Pickups[obj]["coords"], 2, Pickups[obj]["weaponid"]);
                            TriggerClientEvent("vorpInventory:removePickupClient", Pickups[obj]["obj"]);
                            player.TriggerEvent("vorpinventory:receiveItem", Pickups[obj]["name"], Pickups[obj]["amount"]);
                            player.TriggerEvent("vorpInventory:playerAnim", obj);
                            Pickups.Remove(obj);
                        }

                    }
                }
                else
                {
                    if (Config.MaxWeapons != 0)
                    {
                        int totalcount = InventoryAPI.getUserTotalCountWeapons(identifier, charIdentifier);
                        totalcount += 1;
                        if (totalcount <= Config.MaxWeapons)
                        {
                            int weaponId = Pickups[obj]["weaponid"];
                            addWeapon(source, Pickups[obj]["weaponid"]);
                            TriggerEvent("syn_weapons:onpickup", Pickups[obj]["weaponid"]);
                            //Debug.WriteLine($"añado {ItemDatabase.userWeapons[Pickups[obj]["weaponid"].ToString()].getPropietary()}");
                            TriggerClientEvent("vorpInventory:sharePickupClient", Pickups[obj]["name"], Pickups[obj]["obj"],
                                Pickups[obj]["amount"], Pickups[obj]["coords"], 2, Pickups[obj]["weaponid"]);
                            TriggerClientEvent("vorpInventory:removePickupClient", Pickups[obj]["obj"]);
                            player.TriggerEvent("vorpinventory:receiveWeapon", weaponId, ItemDatabase.userWeapons[weaponId].getPropietary(),
                                ItemDatabase.userWeapons[weaponId].getName(), ItemDatabase.userWeapons[weaponId].getAllAmmo(), ItemDatabase.userWeapons[weaponId].getAllComponents());
                            player.TriggerEvent("vorpInventory:playerAnim", obj);
                            Pickups.Remove(obj);
                        }
                    }

                }
            }

        }

        private void onPickupMoney([FromSource] Player player, int obj)
        {
            string identifier = "steam:" + player.Identifiers["steam"];
            int source = int.Parse(player.Handle);
            if (PickupsMoney.ContainsKey(obj))
            {

                TriggerClientEvent("vorpInventory:shareMoneyPickupClient", PickupsMoney[obj]["obj"],
                PickupsMoney[obj]["amount"], PickupsMoney[obj]["coords"], 2);
                TriggerClientEvent("vorpInventory:removePickupClient", PickupsMoney[obj]["obj"]);
                player.TriggerEvent("vorpInventory:playerAnim", obj);
                TriggerEvent("vorp:addMoney", source, 0, PickupsMoney[obj]["amount"]);
                PickupsMoney.Remove(obj);
            }
        }

        private void sharePickupServer(string name, int obj, int amount, Vector3 position, int weaponId)
        {
            TriggerClientEvent("vorpInventory:sharePickupClient", name, obj, amount, position, 1, weaponId);
            Debug.WriteLine(obj.ToString());
            Pickups.Add(obj, new Dictionary<string, dynamic>
            {
                ["name"] = name,
                ["obj"] = obj,
                ["amount"] = amount,
                ["weaponid"] = weaponId,
                ["inRange"] = false,
                ["coords"] = position
            });
        }

        private void shareMoneyPickupServer(int obj, double amount, Vector3 position)
        {
            TriggerClientEvent("vorpInventory:shareMoneyPickupClient", obj, amount, position, 1);
            Debug.WriteLine(obj.ToString());
            PickupsMoney.Add(obj, new Dictionary<string, dynamic>
            {
                ["name"] = "Dollars",
                ["obj"] = obj,
                ["amount"] = amount,
                ["inRange"] = false,
                ["coords"] = position
            });
        }

        //Weapon methods
        private void serverDropWeapon([FromSource] Player source, int weaponId)
        {
            subWeapon(int.Parse(source.Handle), weaponId);
            source.TriggerEvent("vorpInventory:createPickup", ItemDatabase.userWeapons[weaponId].getName(), 1, weaponId);
        }

        //Items methods
        private void serverDropItem([FromSource] Player source, string itemname, int cuantity)
        {

            subItem(int.Parse(source.Handle), itemname, cuantity);
            source.TriggerEvent("vorpInventory:createPickup", itemname, cuantity, 1);

        }

        private void serverGiveWeapon([FromSource] Player source, int weaponId, int target)
        {
            Player p = PlayerList[target];

            if (p == null)
            {
                Logger.Error($"Target Player '{target}' does not exist.");
                return;
            }

            string identifier = "steam:" + source.Identifiers["steam"];

            if (ItemDatabase.userWeapons.ContainsKey(weaponId))
            {
                subWeapon(int.Parse(source.Handle), weaponId);
                addWeapon(int.Parse(p.Handle), weaponId);
                p.TriggerEvent("vorpinventory:receiveWeapon", weaponId, ItemDatabase.userWeapons[weaponId].getPropietary(),
                    ItemDatabase.userWeapons[weaponId].getName(), ItemDatabase.userWeapons[weaponId].getAllAmmo(), ItemDatabase.userWeapons[weaponId].getAllComponents());
            }
        }
        private void serverGiveItem([FromSource] Player source, string itemname, int amount, int target)
        {
            bool give = true;
            
            Player p = PlayerList[target];

            if (p == null)
            {
                Logger.Error($"Target Player '{target}' does not exist.");
                return;
            }

            string identifier = "steam:" + source.Identifiers["steam"];
            string targetIdentifier = "steam:" + p.Identifiers["steam"];
            Debug.WriteLine("giving an item");
            if (ItemDatabase.usersInventory.ContainsKey(identifier) && ItemDatabase.usersInventory.ContainsKey(targetIdentifier) && ItemDatabase.usersInventory[identifier].ContainsKey(itemname))
            {
                if (ItemDatabase.usersInventory[identifier][itemname].getCount() >= amount)
                {
                    if (ItemDatabase.usersInventory[targetIdentifier].ContainsKey(itemname))
                    {
                        if (ItemDatabase.usersInventory[targetIdentifier][itemname].getCount() + amount
                            >= ItemDatabase.usersInventory[targetIdentifier][itemname].getLimit())
                        {
                            give = false;
                        }

                    }
                    int totalcount = InventoryAPI.getUserTotalCount(targetIdentifier);
                    totalcount += amount;
                    if (totalcount > Config.MaxItems)

                    {
                        give = false;
                    }

                    if (give)
                    {
                        addItem(int.Parse(p.Handle), itemname, amount);
                        subItem(int.Parse(source.Handle), itemname, amount);
                        p.TriggerEvent("vorpinventory:receiveItem", itemname, amount);
                        source.TriggerEvent("vorpinventory:receiveItem2", itemname, amount);
                        TriggerClientEvent(source, "vorp:TipRight", Config.lang["yougaveitem"], 2000);
                        TriggerClientEvent(p, "vorp:TipRight", Config.lang["YouReceiveditem"], 2000);
                    }
                    else
                    {

                        TriggerClientEvent(source, "vorp:TipRight", Config.lang["fullInventoryGive"], 2000);
                        TriggerClientEvent(p, "vorp:TipRight", Config.lang["fullInventory"], 2000);
                    }

                }
                else
                {
                    TriggerClientEvent(source, "vorp:TipRight", Config.lang["itemerror"], 2000);
                    TriggerClientEvent(p, "vorp:TipRight", Config.lang["itemerror"], 2000);
                }
            }
            else
            {
                Debug.WriteLine("Error Occured");
                TriggerClientEvent(source, "vorp:TipRight", Config.lang["itemerror"], 2000);
            }
        }

        private void getItemsTable([FromSource] Player source)
        {
            //Need rework to callback 2.0
            if (ItemDatabase.items.Count != 0)
            {
                source.TriggerEvent("vorpInventory:giveItemsTable", ItemDatabase.items);
            }
        }

        private void getInventory([FromSource] Player source)
        {
            string steamId = "steam:" + source.Identifiers["steam"];

            dynamic CoreUser = CORE.getUser(int.Parse(source.Handle)).getUsedCharacter;

            int charIdentifier = CoreUser.charIdentifier;
            string inventory = CoreUser.inventory;

            Dictionary<string, ItemClass> userinv = new Dictionary<string, ItemClass>();
            List<WeaponClass> userwep = new List<WeaponClass>();
            if (inventory != null)
            {
                dynamic thing = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(inventory);
                foreach (dynamic itemname in ItemDatabase.items)
                {
                    if (thing[itemname.item.ToString()] != null)
                    {
                        ItemClass item = new ItemClass(int.Parse(thing[itemname.item.ToString()].ToString()), int.Parse(itemname.limit.ToString()),
                            itemname.label, itemname.item, itemname.type, itemname.usable, itemname.can_remove);
                        userinv.Add(itemname.item.ToString(), item);
                    }
                }
                ItemDatabase.usersInventory[steamId] = userinv;
            }
            else
            {
                ItemDatabase.usersInventory[steamId] = userinv;
            }

            source.TriggerEvent("vorpInventory:giveInventory", inventory);



            //Exports["ghmattimysql"].execute("SELECT identifier,inventory FROM characters WHERE identifier = ?;", new[] { steamId }, new Action<dynamic>((uinvento) =>
            //{
            //    if (uinvento.Count == 0)
            //    {
            //        Debug.WriteLine("No users inventory");
            //        Dictionary<string, ItemClass> items = new Dictionary<string, ItemClass>();
            //        ItemDatabase.usersInventory.Add(steamId, items); // Si no existe le metemos en la caché para tenerlo preparado para recibir cosas
            //    }
            //    else
            //    {

            //        //Carga del inventario
            //        Dictionary<string, ItemClass> userinv = new Dictionary<string, ItemClass>();
            //        List<WeaponClass> userwep = new List<WeaponClass>();
            //        if (uinvento[0].inventory != null)
            //        {
            //            dynamic thing = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(uinvento[0].inventory);
            //            foreach (dynamic itemname in ItemDatabase.items)
            //            {
            //                if (thing[itemname.item.ToString()] != null)
            //                {
            //                    ItemClass item = new ItemClass(int.Parse(thing[itemname.item.ToString()].ToString()), int.Parse(itemname.limit.ToString()),
            //                        itemname.label, itemname.item, itemname.type, itemname.usable, itemname.can_remove);
            //                    userinv.Add(itemname.item.ToString(), item);
            //                }
            //            }
            //            ItemDatabase.usersInventory[steamId] = userinv;
            //        }
            //        else
            //        {
            //            ItemDatabase.usersInventory[steamId] = userinv;
            //        }

            //        source.TriggerEvent("vorpInventory:giveInventory", uinvento[0].inventory);
            //    }

            //}));

            Exports["ghmattimysql"].execute("SELECT * FROM loadout WHERE `identifier` = ? AND `charidentifier` = ?;", new object[] { steamId, charIdentifier }, new Action<dynamic>((weaponsinvento) =>
           {
               if (weaponsinvento.Count == 0)
               {

               }
               else
               {
                   WeaponClass wp;
                   foreach (var row in weaponsinvento)
                   {
                       JObject ammo = Newtonsoft.Json.JsonConvert.DeserializeObject(row.ammo.ToString());
                       JArray comp = Newtonsoft.Json.JsonConvert.DeserializeObject(row.components.ToString());
                       Dictionary<string, int> amunition = new Dictionary<string, int>();
                       List<string> components = new List<string>();
                       foreach (JProperty ammos in ammo.Properties())
                       {
                           //Debug.WriteLine(ammos.Name);
                           amunition.Add(ammos.Name, int.Parse(ammos.Value.ToString()));
                       }
                       foreach (JToken x in comp)
                       {
                           components.Add(x.ToString());
                       }

                       bool auused = false;
                       if (row.used == 1)
                       {
                           auused = true;
                       }

                       bool auused2 = false;
                       if (row.used2 == 1)
                       {
                           auused2 = true;
                       }
                       wp = new WeaponClass(int.Parse(row.id.ToString()), row.identifier.ToString(), row.name.ToString(), amunition, components, auused, auused2, charIdentifier);
                       ItemDatabase.userWeapons[wp.getId()] = wp;
                   }

                   source.TriggerEvent("vorpInventory:giveLoadout", weaponsinvento);
               }

           }));
        }
    }
}
