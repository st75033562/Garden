using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// quake-style command variable
// int, float, double and string
public abstract class CVar
{
    private static Dictionary<string, CVar> s_cvars = new Dictionary<string, CVar>();

    public event Action onChanged;

    static CVar()
    {
        CmdServer.Register("list", new CVarCommand_List());
        CmdServer.Register("set", new CVarCommand_Set());
        CmdServer.Register("get", new CVarCommand_Get());
        CmdServer.Register("reset", new CVarCommand_Reset());
    }

    public static CVar Find(string name)
    {
        CVar variable;
        s_cvars.TryGetValue(name, out variable);
        return variable;
    }

    public static IEnumerable<string> VarNames
    {
        get { return s_cvars.Keys; }
    }

    public static IEnumerable<CVar> Vars
    {
        get { return s_cvars.Values; }
    }

    public CVar(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("name must be empty");
        }

        if (s_cvars.ContainsKey(name))
        {
            throw new ArgumentException("already registered: " + name);
        }
        s_cvars.Add(name, this);

        this.name = name;
    }

    public string name
    {
        get;
        private set;
    }

    public abstract string stringValue { get; set; }

    public abstract void Reset();

    protected void FireChanged()
    {
        if (onChanged != null)
        {
            onChanged();
        }
    }
}
