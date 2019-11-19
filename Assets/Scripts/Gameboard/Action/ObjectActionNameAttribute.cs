using System;
using System.Text.RegularExpressions;

namespace Gameboard
{
    /// <summary>
    /// attribute for determine the simple name used by editor
    /// the class name should be named as prefixAaBbCc
    /// the corresponding simple name is namespace - Aa Bb Cc
    /// </summary>
    public class ObjectActionNameAttribute : Attribute
    {
        public ObjectActionNameAttribute(string prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }
            this.prefix = prefix;
        }

        /// <summary>
        /// prefix to strip from the class name
        /// </summary>
        public string prefix
        {
            get;
            private set;
        }

        /// <summary>
        /// get the simple name used by editor
        /// </summary>
        public string GetSimpleName(Type type)
        {
            var name = type.Name;
            if (prefix != "" && type.Name.StartsWith(prefix))
            {
                name = name.Substring(prefix.Length);
            }
            var simpleName = string.Join(" ", Regex.Split(name, "(?=[A-Z])"));
            if (type.Namespace != "")
            {
                return type.Namespace + " - " + simpleName;
            }
            return simpleName;
        }
    }
}
