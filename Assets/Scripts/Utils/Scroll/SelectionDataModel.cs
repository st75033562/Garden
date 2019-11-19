using System;
using System.Collections.Generic;
using System.Linq;

public class SelectionDataModel
{
    public event Action<int, bool> onItemSelectionChanged;

    private IListItemModel m_model;
    private readonly List<int> m_selections = new List<int>();
    private bool m_allowMultipleSelection;

    public bool allowMultipleSelection
    {
        get { return m_allowMultipleSelection; }
        set
        {
            m_allowMultipleSelection = value;
            if (!m_allowMultipleSelection && m_selections.Count > 1)
            {
                foreach (var index in m_selections.Skip(1).ToArray())
                {
                    Select(index, false);
                }
            }
        }
    }

    public int count
    {
        get { return m_model.count; }
    }

    public object getItem(int index)
    {
        return m_model.getItem(index);
    }

    public void Select(int index, bool selected)
    {
        if (index < 0 || index >= count)
        {
            throw new ArgumentOutOfRangeException();
        }

        if (!allowMultipleSelection)
        {
            ClearSelections();
        }

        InternalSelect(index, selected);
    }

    private void InternalSelect(int index, bool selected)
    {
        if (selected && !m_selections.Contains(index))
        {
            m_selections.Add(index);
            m_selections.Sort();
            if (onItemSelectionChanged != null)
            {
                onItemSelectionChanged(index, true);
            }
        }
        else if (!selected && m_selections.Contains(index))
        {
            m_selections.Remove(index);
            if (onItemSelectionChanged != null)
            {
                onItemSelectionChanged(index, false);
            }
        }
    }

    public bool IsSelected(int index)
    {
        return m_selections.Contains(index);
    }

    public void ClearSelections()
    {
        if (m_selections.Count > 0)
        {
            foreach (var index in m_selections.ToArray())
            {
                InternalSelect(index, false);
            }
        }
    }

    internal void ResetSelections()
    {
        m_selections.Clear();
    }

	public IEnumerable<int> selectionIndices
    {
        get { return m_selections; }
    }

	public int firstSelectionIndex
    {
        get { return m_selections.Count != 0 ? m_selections[0] : -1; }
    }

    public IEnumerable<object> selections
    {
        get
        {
            foreach (var index in m_selections)
            {
                yield return getItem(index);
            }
        }
    }

    public object firstSelection
    {
        get
        {
            int index = firstSelectionIndex;
            return index != -1 ? getItem(index) : null;
        }
    }

    internal void Reset(IListItemModel model)
    {
        if (m_model != null)
        {
            m_model.onItemRemoved -= OnItemRemoved;
            m_model.onItemInserted -= OnItemInserted;
            m_model.onItemIndexChanged -= OnItemIndexChanged;
            m_model.onItemMoved -= OnItemMoved;
        }
        m_model = model;
        m_model.onItemRemoved += OnItemRemoved;
        m_model.onItemInserted += OnItemInserted;
        m_model.onItemIndexChanged += OnItemIndexChanged;
        m_model.onItemMoved += OnItemMoved;
        m_selections.Clear();
    }

    internal void OnItemRemoved(int index)
    {
        // update indices greater than index
        for (int i = 0; i < m_selections.Count; ++i)
        {
            if (m_selections[i] == index)
            {
                m_selections.RemoveAt(i);
                --i;
            }
            else if (m_selections[i] > index)
            {
                --m_selections[i];
            }
        }
    }

    internal void OnItemInserted(Range range)
    {
        // update indices >= range.start
        for (int i = 0; i < m_selections.Count; ++i)
        {
            if (m_selections[i] >= range.start)
            {
                while (i < m_selections.Count)
                {
                    m_selections[i++] += range.count;
                }
                break;
            }
        }
    }

    private void OnItemIndexChanged(ItemIndexChanges changes)
    {
        // update indices accordingly
        for (int i = 0; i < m_selections.Count; ++i)
        {
            int to = changes.Get(m_selections[i]);
            if (to != -1)
            {
                m_selections[i] = to;
            }
        }
        m_selections.Sort();
    }

    private void OnItemMoved(int from, int to)
    {
        for (int i = 0; i < m_selections.Count; ++i)
        {
            // update indices in (from, to]
            if (from < m_selections[i] && m_selections[i] <= to)
            {
                --m_selections[i];
            }
            else if (m_selections[i] == from)
            {
                m_selections[i] = to;
            }
        }
    }
}