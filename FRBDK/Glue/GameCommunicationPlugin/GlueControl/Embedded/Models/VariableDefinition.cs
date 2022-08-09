using System;
using System.Collections.Generic;
using System.Text;

namespace GlueControl.Models
{
    class VariableDefinition
    {
        public static object GetCastedValueForType(string typeName, string value)
        {
            var toReturn = value;
            if (typeName == "bool")
            {
                bool boolToReturn = false;

                bool.TryParse(value, out boolToReturn);

                return boolToReturn;
            }
            else if (typeName == "float")
            {
                float floatToReturn = 0.0f;

                float.TryParse(value, out floatToReturn);

                return floatToReturn;
            }
            else if (typeName == "int")
            {
                int intToReturn = 0;

                int.TryParse(value, out intToReturn);

                return intToReturn;
            }
            else if (typeName == "long")
            {
                long longToReturn = 0;

                long.TryParse(value, out longToReturn);

                return longToReturn;
            }
            else if (typeName == "double")
            {
                double doubleToReturn = 0.0;

                double.TryParse(value, out doubleToReturn);

                return doubleToReturn;
            }
            else
            {
                return toReturn;
            }
        }

    }
}
