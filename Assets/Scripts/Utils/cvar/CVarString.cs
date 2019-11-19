using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CVarString : CVar
{
    private string m_value;
    private string m_defaultValue;

    public CVarString(string name, string value = "")
        : base(name)
    {
        m_defaultValue = value;
        stringValue = value;
    }

    public override string stringValue
    {
        get { return m_value; }
        set
        {
            if (m_value != value)
            {
                m_value = value;
                FireChanged();
            }
        }
    }

    public override void Reset()
    {
        stringValue = m_defaultValue;
    }

    public static implicit operator string(CVarString variable)
    {
        return variable.stringValue;
    }
}