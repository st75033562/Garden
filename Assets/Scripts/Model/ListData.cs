using System;
using System.Collections.Generic;
using System.Text;

// NOTE: all indices are 1-based
public class ListData : BaseVarCollection
{
    private readonly List<string> mList = new List<string>();

    public ListData()
    {
    }

    public ListData(string name, NameScope scope)
        : base(name, scope)
    {
    }

    public ListData(ListData rhs)
        : base(rhs)
    {
        mList = new List<string>(rhs.mList);
    }

    public override BlockVarType type
    {
        get { return BlockVarType.List; }
    }

    public int size()
    {
        return mList.Count;
    }

    public override void reset()
    {
        if (mList.Count > 0)
        {
            mList.Clear();
            fireOnChanged();
        }
    }

    public void add(string item)
    {
        if (item == null)
        {
            throw new ArgumentNullException();
        }
        mList.Add(item);
        fireOnChanged();
    }

    public void add(IEnumerable<string> values)
    {
        mList.AddRange(values);
        fireOnChanged();
    }

    public void insert(int index, string item)
    {
        if (item == null)
        {
            throw new ArgumentNullException();
        }
        if (index <= 0 || index > mList.Count + 1)
            return;

        mList.Insert(index - 1, item);
        fireOnChanged();
    }

    public string this[int index]
    {
        get
        {
            if (index >= 1 && index <= mList.Count)
            {
                return mList[index - 1];
            }
            return "";
        }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }
            if (index <= 0 || index >= mList.Count + 1) return;
            mList[index - 1] = value;
            fireOnChanged();
        }
    }

    public void removeAt(int index)
    {
        if (index <= 0 || index >= mList.Count + 1) return;
        mList.RemoveAt(index - 1);
        fireOnChanged();
    }

    public bool contains(string item)
    {
        if (item == null) return false;
        return mList.Contains(item);
    }

    public override IEnumerator<string> GetEnumerator()
    {
        return mList.GetEnumerator();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendFormat("list: {0}, count: {1}\n", name, size());
        dumpTo(sb);
        return sb.ToString();
    }

    public override BaseVariable clone(string name)
    {
        var copy = new ListData(this);
        copy.name = name;
        return copy;
    }

    public override void readFrom(BaseVariable o)
    {
        var rhs = (ListData)o;
        mList.Clear();
        mList.AddRange(rhs.mList);
        fireOnChanged();
    }

    #region serialization

    public override Save_Variable serialize()
    {
        var data = base.serialize();
        data.VariableType = Save_VariableType.ListType;
        data.VariableList.AddRange(mList);
        return data;
    }

    public override void deserialize(Save_Variable data)
    {
        base.deserialize(data);
        mList.Clear();
        mList.AddRange(data.VariableList);
    }

    #endregion
}
