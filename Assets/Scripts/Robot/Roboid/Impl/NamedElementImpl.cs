using System;

namespace Robomation
{
    /**
     * @author akaii@kw.ac.kr (Kwang-Hyun Park)
     * 
     * ported by alex@g3games.cn
     */
    public class NamedElementImpl : NamedElement
    {
        private string mName;

        protected NamedElementImpl()
        {
            mName = "";
        }

        protected NamedElementImpl(string name)
        {
            mName = name;
        }

        public string getName()
        {
            return mName;
        }

        public virtual void setName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException();
            }

            mName = name;
        }
    }
}
