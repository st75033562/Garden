using UnityEngine.Events;

public class UISortMenuWidget : UIMenuWidget
{
    public UnityEvent onSortChanged;

    private bool m_sortValid;

    public override void SetOptions(System.Collections.Generic.IList<string> options)
    {
        base.SetOptions(options);
        m_sortValid = false;
    }

    public void SetCurrentSort(int index, bool asc)
    {
        if (!m_sortValid || index != activeSortOption || asc != sortAsc)
        {
            GetItem<UISortMenuItemWidget>(index).sortDir = 
                asc ? UISortMenuItemWidget.SortDir.Asc : UISortMenuItemWidget.SortDir.Desc;
            UpdateOtherMenuItems(index);

            m_sortValid = true;
            activeSortOption = index;
            sortAsc = asc;

            if (onSortChanged != null)
            {
                onSortChanged.Invoke();
            }
        }
    }

    public int activeSortOption
    {
        get;
        private set;
    }

    public bool sortAsc
    {
        get;
        private set;
    }

    protected override void OnClick(UIMenuItemWidget menuItem)
    {
        UpdateOtherMenuItems(menuItem.itemIndex);
        activeSortOption = menuItem.itemIndex;
        sortAsc = ((UISortMenuItemWidget)menuItem).sortDir == UISortMenuItemWidget.SortDir.Asc;

        base.OnClick(menuItem);

        if (onSortChanged != null)
        {
            onSortChanged.Invoke();
        }
    }

    private void UpdateOtherMenuItems(int index)
    {
        for (int i = 0; i < itemCount; ++i)
        {
            if (i != index)
            {
                GetItem<UISortMenuItemWidget>(i).sortDir = UISortMenuItemWidget.SortDir.None;
            }
        }
    }
}
