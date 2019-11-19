using System;
using System.Collections.Generic;
using System.Text;

public class QueueData : BaseVarCollection
{
    private readonly Queue<string> m_data = new Queue<string>();

    public QueueData()
    {

    }

    public QueueData(string name, NameScope scope)
        : base(name, scope)
    {

    }

    public QueueData(QueueData rhs)
        : base(rhs)
    {
        m_data = new Queue<string>(rhs.m_data);
    }

    public override BlockVarType type
    {
        get { return BlockVarType.Queue; }
    }

    public int size()
    {
        return m_data.Count;
    }

    public void enqueue(string item)
    {
        if (item == null)
        {
            throw new ArgumentNullException();
        }

        m_data.Enqueue(item);
        fireOnChanged();
    }
    
    public string dequeue()
    {
        if (m_data.Count > 0)
        {
            var data = m_data.Dequeue();
            fireOnChanged();
            return data;
        }
        return "";
    }

    public override void reset()
    {
        if (m_data.Count > 0)
        {
            m_data.Clear();
            fireOnChanged();
        }
    }

    public override IEnumerator<string> GetEnumerator()
    {
        return m_data.GetEnumerator();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendFormat("queue: {0}, count: {1}\n", name, size());
        dumpTo(sb);
        return sb.ToString();
    }

    public override BaseVariable clone(string name)
    {
        var copy = new QueueData(this);
        copy.name = name;
        return copy;
    }

    public override void readFrom(BaseVariable o)
    {
        var rhs = (QueueData)o;
        m_data.Clear();
        foreach (var elem in rhs.m_data)
        {
            m_data.Enqueue(elem);
        }
        fireOnChanged();
    }

    #region serialization

    public override Save_Variable serialize()
    {
        var data = base.serialize();
        data.VariableType = Save_VariableType.QueueType;
        data.VariableQueue.AddRange(m_data);
        return data;
    }

    public override void deserialize(Save_Variable data)
    {
        base.deserialize(data);
        m_data.Clear();
        for (int i = 0; i < data.VariableQueue.Count; ++i)
        {
            m_data.Enqueue(data.VariableQueue[i]);
        }
    }

    #endregion
}
