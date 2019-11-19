using System;
using System.Collections.Generic;

public class UserARObjects
{
    private readonly HashSet<int> m_unlockedIds = new HashSet<int>();

    public void Initialize(IEnumerable<int> unlockedIds)
    {
        if (unlockedIds == null)
        {
            throw new ArgumentNullException("unlockedIds");
        }
        m_unlockedIds.UnionWith(unlockedIds);
    }

    public void Reset()
    {
        m_unlockedIds.Clear();
    }

    public void Unlock(int objectId)
    {
        m_unlockedIds.Add(objectId);
    }

    public bool IsUnlocked(int objectId)
    {
        return m_unlockedIds.Contains(objectId);
    }
}
