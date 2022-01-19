using System;
using System.Globalization;

namespace VorpInventory.Extensions
{
    public static class StringExt
    {
        public static uint FromHex(this string val)
        {
            if (val.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                val = val.Substring(2);
            }
            return (uint)Int32.Parse(val, NumberStyles.HexNumber);
        }
    }
}
