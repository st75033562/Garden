using UnityEngine.UI;

public class UISortMenuItemWidget : UIMenuItemWidget
{
    public Text m_sortDirText;

    public enum SortDir
    {
        None,
        Asc,
        Desc
    }

    private SortDir m_sortDir = SortDir.None;

    public SortDir sortDir
    {
        get { return m_sortDir; }
        set
        {
            m_sortDir = value;
            if (m_sortDir == SortDir.None)
            {
                m_sortDirText.enabled = false;
            }
            else
            {
                m_sortDirText.enabled = true;
                m_sortDirText.text = m_sortDir == SortDir.Asc ? "↑" : "↓";
            }
        }
    }

    protected override void OnClick()
    {
        if (m_sortDir == SortDir.None)
        {
            sortDir = SortDir.Asc;
        }
        else if (m_sortDir == SortDir.Asc)
        {
            sortDir = SortDir.Desc;
        }
        else
        {
            sortDir = SortDir.Asc;
        }
        base.OnClick();
    }
}
