using LitJson;
using System;
using System.Linq;

namespace Gameboard
{
    static class VariableJsonUtils
    {
        public static string ValueToJson(BaseVariable variable)
        {
            if (variable == null)
            {
                throw new ArgumentNullException("variable");
            }
            if (variable.type == BlockVarType.Variable)
            {
                return ((VariableData)variable).getString();
            }
            else
            {
                return JsonMapper.ToJson(((BaseVarCollection)variable).ToArray());
            }
        }
    }
}
