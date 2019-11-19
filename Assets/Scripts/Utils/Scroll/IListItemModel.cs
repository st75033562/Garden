using System.Collections.Generic;
using System;
using System.Collections;

public class ItemIndexChanges
{
    private readonly Dictionary<int, int> m_changes = new Dictionary<int, int>();

    public void Add(int from, int to)
    {
        m_changes.Add(from, to);
    }

    public int count
    {
        get { return m_changes.Count; }
    }

    // return -1 if no corresponding change
    public int Get(int from)
    {
        int change;
        return m_changes.TryGetValue(from, out change) ? change : -1;
    }
}

public interface IListItemModel : IEnumerable
{
    event Action<Range> onItemInserted;
    event Action<int>   onItemUpdated;
    event Action<int>   onItemRemoved;
    event Action<ItemIndexChanges> onItemIndexChanged;
    event Action<int, int> onItemMoved;
    event Action        onReset;

    object getItem(int index);

    void setItem(int index, object item);

    /// <summary>
    /// add an item to the model, for readonly model, this can be a no-op
    /// </summary>
    void addItem(object item);

    /// <summary>
    /// insert an item at the given index, for readonly model, this can be a no-op
    /// </summary>
    void insertItem(int index, object item);

    void updatedItem(int index);

    int count { get; }

    int indexOf(object item);

    /// <summary>
    /// remove an item from the model, for readonly model, this can be a no-op
    /// </summary>
    void removeItem(int index);

    bool canFetchMore { get; }

    void fetchMore();

    void setSorter(IComparer comparer);
}

public static class ListItemModelExtensions
{
    public static void updatedItem(this IListItemModel model, object item)
    {
        int index = model.indexOf(item);
        if (index != -1)
        {
            model.updatedItem(index);
        }
    }

    public static void removeItem(this IListItemModel model, object item)
    {
        int index = model.indexOf(item);
        if (index != -1)
        {
            model.removeItem(index);
        }
    }
}
