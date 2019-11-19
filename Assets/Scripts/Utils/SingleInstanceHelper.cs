using System;
using System.Collections;
using System.Threading;

public class SingleInstanceHelper : Singleton<SingleInstanceHelper>
{
    private const string UniqueId = "E1240EEE-4E87-43FB-90C8-B21B115D488A";
    private static Mutex s_lock;
    private static bool s_owningLock;
 
    public static IEnumerator EnsureSingleInstance()
    {
        EnsureInstance();

        if (s_lock != null)
        {
            throw new InvalidOperationException();
        }

        s_lock = new Mutex(true, UniqueId, out s_owningLock);
        if (!s_owningLock)
        {
            PopupManager.DuplicateInstance();
            while (true)
            {
                yield return null;
            }
        }
    }

    void OnApplicationQuit()
    {
        if (s_lock != null)
        {
            if (s_owningLock)
            {
                s_lock.ReleaseMutex();
            }
            s_lock.Close();
            s_lock = null;
        }
    }
}
