using System;
using System.Linq;

namespace UnityEngine.AssetBundles
{
    public static class BuildUtils
    {
        public static bool ValidateAsset()
        {
            var validatorTypes = typeof(BuildUtils).Assembly.GetTypes()
                                      .Where(x => x.GetCustomAttributes(typeof(PreBuildAttribute), true) != null);
            foreach (var type in validatorTypes)
            {
                var method = type.GetMethod("Validate", new Type[0]);
                if (method != null)
                {
                    var instance = Activator.CreateInstance(type);
                    if (!(bool)method.Invoke(instance, null))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
