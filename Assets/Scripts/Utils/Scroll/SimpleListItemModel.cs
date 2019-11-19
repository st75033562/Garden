using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A default implementation of IListItemModel
/// </summary>
public class SimpleListItemModel : IListItemModel
{
    public event Action<Range> onItemInserted;
    public event Action<int> onItemUpdated;
    public event Action<int> onItemRemoved;
    public event Action<ItemIndexChanges> onItemIndexChanged;
    public event Action<int, int> onItemMoved;
    public event Action onReset;

    protected IList m_items;
    private IComparer m_comparer;

    public SimpleListItemModel(IList items)
    {
        if (items == null)
        {
            throw new ArgumentNullException();
        }
        m_items = items;
    }

    public object getItem(int index)
    {
        return m_items[index];
    }

    public void setItem(int index, object item)
    {
        m_items[index] = item;
        updatedItem(index);
    }

    public void updatedItem(int index)
    {
        if (index < 0 || index >= m_items.Count)
        {
            throw new ArgumentOutOfRangeException();
        }

        if (m_comparer != null)
        {
            int orderHint = getItemOrderHint(index);
            if (orderHint != 0)
            {
                var item = m_items[index];
                int insertionIndex;
                if (orderHint < 0)
                {
                    // find in the left half
                    insertionIndex = findInsertionIndex(m_items[index], 0, index);
                    // move [insertionIndex, index) to the right
                    for (int i = index; i > insertionIndex; ++i)
                    {
                        m_items[i] = m_items[i - 1];
                    }
                }
                else
                {
                    // find in the right half
                    insertionIndex = findInsertionIndex(m_items[index], index + 1, m_items.Count);
                    // move (index, insertionIndex - 1] to the left
                    for (int i = index; i < insertionIndex - 1; ++i)
                    {
                        m_items[index] = m_items[index + 1];
                    }
                }

                m_items[insertionIndex - 1] = item;
                if (onItemMoved != null)
                {
                    onItemMoved(index, insertionIndex - 1);
                }
            }
            else
            {
                fireItemUpdated(index);
            }
        }
        else
        {
            fireItemUpdated(index);
        }
    }

    private int getItemOrderHint(int index)
    {
        if (index > 0 && m_comparer.Compare(m_items[index - 1], m_items[index]) > 0)
        {
            // item needs to be moved toward head
            return -1;
        }

        if (index < m_items.Count - 1 && m_comparer.Compare(m_items[index], m_items[index + 1]) > 0)
        {
            // item needs to be moved toward tail
            return 1;
        }

        // already ordered
        return 0;
    }

    protected void fireItemUpdated(int index)
    {
        if (onItemUpdated != null)
        {
            onItemUpdated(index);
        }
    }

    public void addItem(object item)
    {
        insertItem(m_items.Count, item);
    }

    public void insertItem(int index, object item)
    {
        if (m_items.IsReadOnly || m_items.IsFixedSize)
        {
            return;
        }

        if (m_comparer != null)
        {
            index = findInsertionIndex(item, 0, m_items.Count);
        }
        m_items.Insert(index, item);
        didInsertItems(new Range(index, 1));
    }

    // find insertion index in the ordered list[start, end)
    // implementation should always return a valid index in [start, end)
    protected virtual int findInsertionIndex(object item, int start, int end)
    {
        throw new NotImplementedException();
    }

    public int count
    {
        get { return m_items.Count; }
    }

    public int indexOf(object item)
    {
        return m_items.IndexOf(item);
    }

    public void removeItem(int index)
    {
        if (!m_items.IsFixedSize)
        {
            beforeRemovingItem(index);
            m_items.RemoveAt(index);
            if (onItemRemoved != null)
            {
                onItemRemoved(index);
            }
        }
    }

    protected virtual void beforeRemovingItem(int index) { }

    public virtual bool canFetchMore
    {
        get { return false; }
    }

    public virtual void fetchMore() { }

    public void setSorter(IComparer comparer)
    {
        m_comparer = comparer;
        if (m_items.Count > 0)
        {
            var oldIndices = new Dictionary<object, int>();
            for (int i = 0; i < m_items.Count; ++i)
            {
                oldIndices.Add(m_items[i], i);
            }

            sort();

            var changes = new ItemIndexChanges();
            for (int i = 0; i < m_items.Count; ++i)
            {
                var oldIndex = oldIndices[m_items[i]];
                if (oldIndex != i)
                {
                    changes.Add(oldIndex, i);
                }
            }

            if (changes.count > 0)
            {
                if (onItemIndexChanged != null)
                {
                    onItemIndexChanged(changes);
                }
            }
        }
    }

    protected virtual void sort()
    {
        throw new NotImplementedException();
    }

    public IComparer comparer
    {
        get { return m_comparer; }
    }

    protected virtual void didInsertItems(Range range)
    {
        if (onItemInserted != null)
        {
            onItemInserted(range);
        }
    }

    protected void fireReset()
    {
        if (onReset != null)
        {
            onReset();
        }
    }

    public IEnumerator GetEnumerator()
    {
        return m_items.GetEnumerator();
    }
}

public class SimpleListItemModel<T> : SimpleListItemModel, IEnumerable<T>
{
    public SimpleListItemModel()
        : this(new List<T>())
    {
    }

    public SimpleListItemModel(List<T> items)
        : base(items)
    {
    }

    protected List<T> items
    {
        get { return (List<T>)m_items; }
    }

    protected override int findInsertionIndex(object item, int start, int end)
    {
        int index = items.BinarySearch(start, end - start, (T)item, (IComparer<T>)comparer);
        return index > 0 ? index : ~index;
    }

    protected override void sort()
    {
        items.Sort((IComparer<T>)comparer);
    }

    public new IEnumerator<T> GetEnumerator()
    {
        return items.GetEnumerator();
    }
}
