using System;
using System.Collections;

namespace Scheduling
{
    public static class Utils
    {
        public static string GetEnumeratorMethodName(IEnumerator enumerator)
        {
            if (enumerator == null)
            {
                throw new ArgumentNullException("enumerator");
            }

            var className = enumerator.GetType().Name;
            var start = className.IndexOf('<');
            var end = className.IndexOf('>');
            var methodName = className.Substring(start + 1, end - start);

            var enclosingType = enumerator.GetType().DeclaringType;
            if (enclosingType != null)
            {
                methodName = enclosingType.FullName + "." + methodName;
            }
            return methodName;
        }

        public static bool ImplementsGenericInterface(Type type, Type interfaceType)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (interfaceType == null)
            {
                throw new ArgumentNullException("interfaceType");
            }

            foreach (var it in type.GetInterfaces())
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == interfaceType)
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerator AsEnumerator(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            return ActionWrapper(action);
        }

        private static IEnumerator ActionWrapper(Action action)
        {
            action();
            yield break;
        }
    }
}
