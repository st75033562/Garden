using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public interface IService
{
    void init();

    Coroutine shutdown();

    void reset();
}

public class ServiceLocator : MonoBehaviour
{
    private static readonly Dictionary<Type, IService> s_services = new Dictionary<Type, IService>();
    private static readonly List<Type> s_initializingTypes = new List<Type>();
    private static ServiceLocator s_instance;
    private static bool s_created;

    private void Awake()
    {
        s_instance = this;
    }

    public static void register<T>() where T : IService
    {
        Type type = typeof(T);
        if (s_services.ContainsKey(type))
        {
            throw new InvalidOperationException("already registered: " + typeof(T).Name);
        }

        if (type.GetConstructor(new Type[0]) == null)
        {
            throw new ArgumentException("type has no default ctor");
        }

        s_services.Add(type, null);
    }

    public static T get<T>() where T : IService
    {
        return (T)getImpl(typeof(T));
    }

    private static IService getImpl(Type type)
    {
        IService service;
        if (!s_services.TryGetValue(type, out service))
        {
            return null;
        }

        if (service == null)
        {
            checkCyclicDependency(type);
            s_initializingTypes.Add(type);

            try
            {
                // add the service to an GameObject if it's a MonoBehaviour
                if (type.IsSubclassOf(typeof(MonoBehaviour)))
                {
                    service = (IService)instance.gameObject.AddComponent(type);
                }
                else
                {
                    // construct with default ctor
                    service = (IService)type.GetConstructor(new Type[0]).Invoke(new object[0]);
                }
                service.init();
                s_services[type] = service;

                //Debug.Log("initialized service: " + type.Name);
            }
            finally
            {
                s_initializingTypes.Remove(type);
            }
        }

        return service;
    }

    private static ServiceLocator instance
    {
        get
        {
            if (!s_created)
            {
                var go = new GameObject("ServiceLocator");
                go.hideFlags = HideFlags.HideAndDontSave;
                go.AddComponent<ServiceLocator>();
                s_created = true;
            }
            return s_instance;
        }
    }

    // eagerly initialize all services
    public static void init()
    {
        List<Type> types = new List<Type>();
        types.AddRange(s_services.Keys);

        foreach (var type in types)
        {
            getImpl(type);
        }
    }

    private static void checkCyclicDependency(Type curType)
    {
        if (s_initializingTypes.Contains(curType))
        {
            string types = string.Empty;
            foreach (var type in s_initializingTypes)
            {
                if (types.Length > 0)
                {
                    types += " ";
                }
                types += type.Name;
            }
            types += " " + curType.Name;
            Debug.LogError("initialization chain: " + types);
            throw new InvalidOperationException("cyclic dependency detected");
        }
    }

    // not safe to be called in OnDestroy
    public static Coroutine shutdown()
    {
        List<Coroutine> shutdownOps = new List<Coroutine>();
        foreach (var service in s_services.Values)
        {
            if (!service.Equals(null))
            {
                var operation = service.shutdown();
                if (operation != null)
                {
                    shutdownOps.Add(operation);
                }
            }
        }

		s_services.Clear();
		s_initializingTypes.Clear();

		// instance might have been destroyed
		if (s_instance)
        {
            return s_instance.StartCoroutine(waitUntilDone(shutdownOps));
        }
        return null;
    }

    private static IEnumerator waitUntilDone(List<Coroutine> ops)
    {
        foreach (var op in ops)
        {
            yield return op;
        }
    }
}
