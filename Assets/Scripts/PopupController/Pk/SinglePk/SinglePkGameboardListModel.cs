using System;
using System.Collections;
using System.Collections.Generic;

public class SinglePkGameboardListModel : SimpleListItemModel<GameBoard>
{
    private int m_columnCount;
    private int m_batchRowCount;
    private readonly HashSet<uint> m_gbIds = new HashSet<uint>();
    private SocketRequest m_request;
    private GBSort_Type m_sortKey = GBSort_Type.StData;
    private bool m_sortAsc = true;

    public SinglePkGameboardListModel(int columnCount, int batchRowCount)
        : base(new List<GameBoard>())
    {
        if (columnCount <= 0)
        {
            throw new ArgumentOutOfRangeException("sliceSize");
        }
        if (batchRowCount <= 0)
        {
            throw new ArgumentOutOfRangeException("batchSize");
        }

        m_columnCount = columnCount;
        m_batchRowCount = batchRowCount;
    }

    public override bool canFetchMore
    {
        get
        {
            return m_request == null;
        }
    }

    protected override void didInsertItems(Range range)
    {
        for (int i = range.start; i < range.end; ++i)
        {
            m_gbIds.Add(items[i].GbId);
        }

        base.didInsertItems(range);
    }

    protected override void beforeRemovingItem(int index)
    {
        m_gbIds.Remove(items[index].GbId);
    }

    public void setSortCriterion(GBSort_Type sortKey, bool asc)
    {
        if (m_sortKey != sortKey || m_sortAsc != asc)
        {
            m_sortKey = sortKey;
            m_sortAsc = asc;

            m_gbIds.Clear();
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
        Comparison<GameBoard> comp = null;
        switch (m_sortKey)
        {
        case GBSort_Type.StData:
            comp = (lhs, rhs) => lhs.CreationTime.CompareTo(rhs.CreationTime);
            break;

        case GBSort_Type.StName:
            comp = (lhs, rhs) => lhs.GbName.CompareWithUICulture(rhs.GbName);
            break;

        default:
            throw new ArgumentException("unsupported sort type " + m_sortKey);
        }

        return DelegatedComparer.Of<GameBoard>(comp.Invert(!m_sortAsc));
    }

    public override void fetchMore()
    {
        if (!canFetchMore)
        {
            return;
        }

        var cmd_gameBoardList_r = new CMD_Get_Gameboardlist_r_Parameters();
        cmd_gameBoardList_r.SortType = m_sortKey;
        cmd_gameBoardList_r.StartPos = (uint)count;
        cmd_gameBoardList_r.Desc = m_sortAsc ? 0u : 1u;
        cmd_gameBoardList_r.LimitCount = (uint)GetFetchCount();

        m_request = new SocketRequest(Command_ID.CmdGetGameboardlistR, cmd_gameBoardList_r);
        m_request.On<CMD_Get_Gameboardlist_a_Parameters>((res, content) => {
            m_request = null;
            if (res == Command_Result.CmdNoError)
            {
                int oldCount = count;
                foreach (var gb in content.GbList)
                {
                    if (!m_gbIds.Contains(gb.GbId))
                    {
                        m_items.Add(gb);
                        m_gbIds.Add(gb.GbId);
                    }
                }
                if (oldCount != count)
                {
                    didInsertItems(new Range(oldCount, count - oldCount));
                }
            }
        })
        .Send();
    }

    private int GetFetchCount()
    {
        // calculate the desired fetch count so that we can fill a entire row
        if (count == 0)
        {
            return m_batchRowCount * m_columnCount;
        }
        else
        {
            int extra = m_columnCount - count % m_columnCount;
            return m_batchRowCount * m_columnCount + extra;
        }
    }
}
