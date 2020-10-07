using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vorpinventory_sv.Class;

namespace vorpinventory_sv
{
    public class Database : BaseScript
    {
        public static Dictionary<string, Item> ItemTemplate = new Dictionary<string, Item>();

        public static Dictionary<string, Dictionary<string, int>> CharactersInventory = new Dictionary<string, Dictionary<string, int>>();

        public Database()
        {
            EventHandlers["vorpinventory:LoadCharacterInventory"] += new Action<Player>(LoadCharacterInventory);

            //Gives Inventory On Press Key
            vorpinventory_sv.VorpCore.addRpcCallback("GetMyInventory", new Action<int, CallbackDelegate, dynamic>((source, cb, args) => {

                //cb(inventory);
            }));

        }

        private void LoadCharacterInventory(Player player)
        {
            int _source = int.Parse(player.Handle);
            dynamic UserCharacter = vorpinventory_sv.VorpCore.getUser(_source).getUsedCharacter;
            string identifier = UserCharacter.identifier;
            string inventory_json = UserCharacter.inventory;
            Dictionary<string, int> inventory = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(inventory_json);
            CharactersInventory[identifier] = inventory;
        }

        public static void LoadItemTemplate()
        {
            foreach (var item in LoadConfig.Config["Items"])
            {
                ItemTemplate.Add(item["Name"].ToString(), new Item(item["Name"].ToString(), item["Label"].ToString(), item["Type"].ToString(), item["Model"].ToString(), 0, item["Limit"].ToObject<int>(), item["Weight"].ToObject<double>(), item["CanUse"].ToObject<bool>(), item["canRemove"].ToObject<bool>(), item["DropOnDeath"].ToObject<bool>()));
            }
        }
        
        
    }
}
