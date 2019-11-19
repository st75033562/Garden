using System;
using System.Reflection;
using UnityEngine.Events;

[Serializable]
public class IntUnityEvent : UnityEvent<int> { }

[Serializable]
public class BoolUnityEvent : UnityEvent<bool> { }

public static class UnityEventExtensions
{
    private static FieldInfo s_nonPersistentCallField;
    private static PropertyInfo s_invokableListCountProperty;

    public static int GetNonPersistentEventCount(this UnityEventBase baseEvent)
    {
        if (s_nonPersistentCallField == null)
        {
            s_nonPersistentCallField = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        if (s_invokableListCountProperty == null)
        {
            s_invokableListCountProperty = s_nonPersistentCallField.FieldType.GetProperty("Count");
        }
        return (int)s_invokableListCountProperty.GetValue(s_nonPersistentCallField.GetValue(baseEvent), null);
    }
}

public class UnityEvent0 : UnityEvent { }

public class UnityEvent1<T> : UnityEvent<T> { }

public class UnityEvent2<T0, T1> : UnityEvent<T0, T1> { }