namespace VORP.Inventory.Client.Extensions
{
    class DecoratorExtensions
    {
        public static void Set(int handle, string property, object value)
        {
            if (value is int i)
            {
                if (!DecorExistOn(handle, property))
                {
                    DecorRegister(property, 3);
                }

                DecorSetInt(handle, property, i);
            }
            else if (value is float f)
            {
                if (!DecorExistOn(handle, property))
                {
                    DecorRegister(property, 1);
                }

                DecorSetFloat(handle, property, f);
            }
            else if (value is bool b)
            {
                if (!DecorExistOn(handle, property))
                {
                    DecorRegister(property, 2);
                }

                DecorSetBool(handle, property, b);
            }
            else
            {
                Logger.Info("[Decor] Could not set decor object due to it not being a supported type.");
            }
        }

        public static int GetInteger(int handle, string property)
        {
            return DecorExistOn(handle, property) ? DecorGetInt(handle, property) : 0;
        }

        public static float GetFloat(int handle, string property)
        {
            return DecorExistOn(handle, property) ? DecorGetFloat(handle, property) : 0f;
        }

        public static bool GetBoolean(int handle, string property)
        {
            return DecorExistOn(handle, property) && DecorGetBool(handle, property);
        }
    }
}
