using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorpinventory_cl
{
    public class NUIManager : BaseScript
    {
        public NUIManager()
        {
            EventHandlers["vorpinventory:refreshInventory"] += new Action<string>(RefreshInventory);
        }

        private void RefreshInventory(string invjson)
        {
            API.SendNuiMessage(invjson);
        }
    }
}
