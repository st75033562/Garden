using System;

namespace Gameboard
{
    public class ObjectActionParameterAttribute : Attribute
    {
        public ObjectActionParameterAttribute(int numArgs)
        {
            this.numArgs = numArgs;
        }

        public int numArgs
        {
            get;
            set;
        }
    }
}
