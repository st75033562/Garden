using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public class StackData : BaseVarCollection
{
    private readonly Stack<string> m_data = new Stack<string>();

    public StackData()
    {

    }

    public StackData(string name, NameScope scope)
        : base(name, scope)
    {

    }

    public StackData(StackData rhs)
        : base(rhs)
    {
        m_data = new Stack<string>(rhs.m_data);
    }

    public override BlockVarType type
    {
        get { return BlockVarType.Stack; }
    }

    public int size()
    {
        return m_data.Count;
    }

    public void push(string item)
    {
        if (item == null)
        {
            throw new ArgumentNullException();
        }

        m_data.Push(item);
        fireOnChanged();
    }

    public string pop()
    {
        if (m_data.Count > 0)
        {
            var data = m_data.Pop();
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
        sb.AppendFormat("stack: {0}, count: {1}\n", name, size());
        dumpTo(sb);
        return sb.ToString();
    }

    public override BaseVariable clone(string name)
    {
        var copy = new StackData(this);
        copy.name = name;
        return copy;
    }

    public override void readFrom(BaseVariable o)
    {
        var rhs = (StackData)o;
        m_data.Clear();
        foreach (var elem in rhs.m_data.Reverse())
        {
            m_data.Push(elem);
        }
        fireOnChanged();
    }

    #region serialization

    public override Save_Variable serialize()
    {
        var data = base.serialize();
        data.VariableType = Save_VariableType.StackType;
        // AddRange will cause bug in mono's stack implementation, so Add manually
        foreach (var e in m_data)
        {
            data.VariableStack.Add(e);
        }
        return data;
    }

    public override void deserialize(Save_Variable data)
    {
        base.deserialize(data);
        m_data.Clear();
        for (int i = data.VariableStack.Count - 1; i >= 0; --i)
        {
            m_data.Push(data.VariableStack[i]);
        }
    }

    #endregion
}
