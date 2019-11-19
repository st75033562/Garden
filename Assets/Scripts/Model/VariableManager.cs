using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.Events;

public enum NameScope
{
    Local,
    Global
}

public enum GlobalVarOwner
{
    Invalid = -1,
    Gameboard = 0,
    Robot,
    All, // no specific ownership, anyone can write to the variable
}

// NOTE: these values correspond to values of Save_VariableType
public enum BlockVarType
{
	Variable,
	List,
	Stack,
	Queue,
}

public abstract class BaseVariable
{
    public event Action<BaseVariable> onChanged;

    // for backward compatibility
    private GlobalVarOwner m_oldGlobalVarOwner = GlobalVarOwner.Gameboard;
    private GlobalVarOwner m_globalVarOwner = GlobalVarOwner.Gameboard;

    // used for serialization
    protected BaseVariable()
    {
    }

    protected BaseVariable(string name, NameScope scope)
    {
        if (name == null)
        {
            throw new ArgumentNullException();
        }
        this.name = name;
        this.scope = scope;
        isReserved = VariableManager.isReserved(name);
        globalVarOwner = GlobalVarOwner.Gameboard;
    }

    protected BaseVariable(BaseVariable rhs)
    {
        name = rhs.name;
        isReserved = rhs.isReserved;
        scope = rhs.scope;
        globalVarOwner = rhs.globalVarOwner;
    }

    public abstract BlockVarType type { get; }

    public string name { get; internal set; }

    public GlobalVarOwner globalVarOwner
    {
        get { return m_globalVarOwner; }
        set
        {
            if (value == GlobalVarOwner.Invalid)
            {
                throw new ArgumentException("invalid value");
            }

            m_globalVarOwner = value;

            if (value != GlobalVarOwner.All)
            {
                m_oldGlobalVarOwner = m_globalVarOwner;
            }
        }
    }

    public abstract void reset();

    protected void fireOnChanged()
    {
        if (onChanged != null)
        {
            onChanged(this);
        }
    }

    public virtual Save_Variable serialize()
    {
        var data = new Save_Variable();
        data.VariableKey = name;
        data.VariableType = (Save_VariableType)type;
        data.GlobalVarOwner = (Save_GlobalVarOwner)globalVarOwner;
        data.OldGlobalVarOwner = (Save_GlobalVarOwner)m_oldGlobalVarOwner;
        return data;
    }

    public virtual void deserialize(Save_Variable data)
    {
        name = data.VariableKey;
        m_oldGlobalVarOwner = (GlobalVarOwner)data.OldGlobalVarOwner;
        var owner = (GlobalVarOwner)data.GlobalVarOwner;
        if (owner == GlobalVarOwner.Invalid)
        {
            owner = m_oldGlobalVarOwner;
        }
        globalVarOwner = owner;
    }

    public static BaseVariable createFrom(Save_Variable data, NameScope scope)
    {
        BaseVariable obj;
        switch (data.VariableType)
        {
        case Save_VariableType.VariabelType:
            obj = new VariableData();
            break;

        case Save_VariableType.ListType:
            obj = new ListData();
            break;

        case Save_VariableType.QueueType:
            obj = new QueueData();
            break;

        case Save_VariableType.StackType:
            obj = new StackData();
            break;

        default:
            return null;
        }

        obj.deserialize(data);
        obj.scope = scope;
        return obj;
    }

    /// <summary>
    /// <para>true if the variable is reserved</para>
    /// <para>By default, the property is initialized to the result of VariableManager.isReserved()</para>
    /// </summary>
    public bool isReserved
    {
        get;
        set;
    }

    public NameScope scope
    {
        get;
        private set;
    }

    /// <summary>
    /// create a copy with another name
    /// </summary>
    public abstract BaseVariable clone(string name);

    /// <summary>
    /// read the data from the given variable, parameter must have same type as the object
    /// </summary>
    public abstract void readFrom(BaseVariable o);
}

public abstract class BaseVarCollection : BaseVariable, IEnumerable<string>
{
    protected BaseVarCollection()
    {
    }

    protected BaseVarCollection(string name, NameScope scope)
        : base(name, scope)
    {
    }

    protected BaseVarCollection(BaseVarCollection rhs)
        : base(rhs)
    {
    }

    public abstract IEnumerator<string> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // helper for dumping
    protected void dumpTo(StringBuilder sb)
    {
        foreach (var item in this)
        {
            sb.Append("\t");
            sb.Append(item);
            sb.AppendLine();
        }
    }
}

public class VariableManager : IEnumerable<BaseVariable>
{
    public readonly UnityEvent<BaseVariable> onVariableAdded = new UnityEvent1<BaseVariable>();
    public readonly UnityEvent<BaseVariable> onVariableRemoved = new UnityEvent1<BaseVariable>();
    public readonly UnityEvent               onVariablesCleared = new UnityEvent0();
    public readonly UnityEvent<BaseVariable> onVariableChanged = new UnityEvent1<BaseVariable>();
    // old name
    public readonly UnityEvent<BaseVariable, string> onVariableRenamed = new UnityEvent2<BaseVariable, string>();

    private static readonly HashSet<string> s_reservedNamePatterns = new HashSet<string>();
	private readonly Dictionary<string, BaseVariable> m_data = new Dictionary<string, BaseVariable>();

    public int count
    {
        get { return m_data.Count; }
    }

    public void add(BaseVariable data)
    {
        m_data.Add(data.name, data);
        data.onChanged += onVariableChanged.Invoke;

        onVariableAdded.Invoke(data);
    }

    public void remove(string name)
    {
        BaseVariable data;
        if (m_data.TryGetValue(name, out data))
        {
    		m_data.Remove(name);

            onVariableRemoved.Invoke(data);
        }
    }

    public void rename(BaseVariable data, string newName)
    {
        if (data == null)
        {
            throw new ArgumentNullException("data");
        }

        if (string.IsNullOrEmpty(newName))
        {
            throw new ArgumentException("newName");
        }

        if (data.name == newName)
        {
            return;
        }

        if (has(newName))
        {
            throw new ArgumentException("duplicate");
        }

        var oldName = data.name;
        data.name = newName;
        m_data.Remove(oldName);
        m_data.Add(newName, data);

        onVariableRenamed.Invoke(data, oldName);
    }

    public void clear()
    {
        m_data.Clear();
        onVariablesCleared.Invoke();
    }

    public bool hasVarOfType(BlockVarType type)
    {
        foreach (var data in m_data.Values)
        {
            if (data.type == type)
            {
                return true;
            }
        }
        return false;
    }

    public IEnumerable<BaseVariable> allVarsOfType(BlockVarType type)
    {
        foreach (var data in m_data.Values)
        {
            if (data.type == type)
            {
                yield return data;
            }
        }
    }

    public BaseVariable get(string name)
    {
        BaseVariable data;
        m_data.TryGetValue(name, out data);
        return data;
    }

    /// <summary>
    /// get data of type with given name
    /// </summary>
    /// <returns>returns null if not found or variable is not of type T</returns>
    public T get<T>(string name) where T : BaseVariable
    {
        return get(name) as T;
    }

    public bool has(string name)
    {
        return m_data.ContainsKey(name);
    }

    public void reset(bool resetGlobalVars = true)
    {
        foreach (var data in m_data.Values)
        {
            if (resetGlobalVars || data.scope == NameScope.Local)
            {
                data.reset();
            }
        }
    }

    /// <summary>
    /// return the count of variables matching the pattern
    /// </summary>
    /// <param name="pattern">a regex</param>
    public int getCount(string pattern)
    {
        return m_data.Keys.Count(x => Regex.IsMatch(x, pattern));
    }

    public IEnumerator<BaseVariable> GetEnumerator()
    {
        return m_data.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static bool isReserved(string name)
    {
        foreach (var reservedPatten in s_reservedNamePatterns)
        {
            if (Regex.IsMatch(name, reservedPatten))
            {
                return true;
            }
        }
        return false;
    }

    public static void addReservedNamePattern(string name)
    {
        s_reservedNamePatterns.Add(name);
    }
}