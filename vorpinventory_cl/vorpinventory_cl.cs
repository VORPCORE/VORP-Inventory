using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorpinventory_cl
{
    public class vorpinventory_cl : BaseScript
    {
        public static bool isCharacterSelected = false;
        public static bool isOpenInv = false;

        public vorpinventory_cl()
        {
            EventHandlers["vorp:SelectedCharacter"] += new Action<int>(GetCharacterInventory);
        }

        private void GetCharacterInventory(int charId)
        {
            TriggerServerEvent("vorpinventory:LoadCharacterInventory");
            isCharacterSelected = true;
        }

        [Tick]
        private async Task OnKey()
        {
            if (!isCharacterSelected)
            {
                await Delay(4000);
                return;
            }

            if (API.IsControlJustReleased(1, GetConfig.InvKey) && API.IsInputDisabled(0))
            {
                if (isOpenInv)
                {
                    //CloseInv();
                    await Delay(1000);
                }
                else
                {
                    //OpenInv();
                    await Delay(1000);
                }
            }

        }

    }
}
