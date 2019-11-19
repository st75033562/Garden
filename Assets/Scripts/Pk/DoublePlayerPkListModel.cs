using System;
using System.Collections;

public class DoublePlayerPkListModel : SimpleListItemModel<PK>
{
    private PKSort_Type m_sortKey = PKSort_Type.PkStData;
    private bool m_asc = true;
    private SocketRequest m_request;

    public void setSortCriterion(PKSort_Type key, bool asc)
    {
        if (m_sortKey != key || m_asc != asc)
        {
            m_sortKey = key;
            m_asc = asc;

            items.Clear();
            fireReset();
            setSorter(GetComparer());

            if (m_request != null)
            {
                m_request.Abort();
                m_request = null;
                fetchMore();
            }
        }
    }

    private IComparer GetComparer()
    {
        Comparison<PK> comp = null;

        switch (m_sortKey)
        {
        case PKSort_Type.PkStData:
            comp = (lhs, rhs) => lhs.CreationTime.CompareTo(rhs.CreationTime);
            break;

        case PKSort_Type.PkStName:
            comp = (lhs, rhs) => lhs.PkName.CompareWithUICulture(rhs.PkName);
            break;

        default:
            throw new ArgumentException("sort type not implemented " + m_sortKey);
        }

        return DelegatedComparer.Of<PK>(comp.Invert(!m_asc));
    }

    public override void fetchMore()
    {
        if (m_request != null)
        {
            return;
        }

        // TODO: incremental fetch
        var request = new CMD_Get_PKList_r_Parameters();
        request.SortType = m_sortKey;
        request.Desc = m_asc ? 0u : 1u;
        request.StartPos = 0u;
        request.LimitCount = uint.MaxValue;

        m_request = new SocketRequest(Command_ID.CmdGetPklistR, request);
        m_request.On<CMD_Get_PKList_a_Parameters>((res, content) => {
            m_request = null;

            items.Clear();
            items.AddRange(content.PkList);
            fireReset();
        })
        .Send();
    }
}
