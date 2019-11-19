using System;
using System.Collections;
using System.Collections.Generic;

public class ReadOnlyMap<TKey, TValue> : IDictionary<TKey, TValue>
{
    private readonly IDictionary<TKey, TValue> m_dict;

    public ReadOnlyMap(IDictionary<TKey, TValue> dict)
    {
        if (dict == null)
        {
            throw new ArgumentNullException("dict");
        }

        m_dict = dict;
    }

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
    {
        throw new InvalidOperationException();
    }

    public bool ContainsKey(TKey key)
    {
        return m_dict.ContainsKey(key);
    }

    public ICollection<TKey> Keys
    {
        get { return m_dict.Keys; }
    }

    bool IDictionary<TKey, TValue>.Remove(TKey key)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return m_dict.TryGetValue(key, out value);
    }

    public ICollection<TValue> Values
    {
        get { return m_dict.Values; }
    }

    public TValue this[TKey key]
    {
        get
        {
            return m_dict[key];
        }
        set
        {
            throw new InvalidOperationException();
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return m_dict.GetEnumerator();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return m_dict.Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        m_dict.CopyTo(array, arrayIndex);
    }

    public int Count
    {
        get { return m_dict.Count; }
    }

    public bool IsReadOnly
    {
        get { return true; }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_dict.GetEnumerator();
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        throw new NotImplementedException();
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Clear()
    {
        throw new NotImplementedException();
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        throw new NotImplementedException();
    }
}
