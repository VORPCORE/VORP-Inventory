using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorpinventory_sv
{
    public class vorpinventory_sv : BaseScript
    {
        public static dynamic VorpCore;

        public vorpinventory_sv()
        {
            TriggerEvent("getCore", new Action<dynamic>((core) =>
            {
                VorpCore = core;
            }));
        }


    }
}
