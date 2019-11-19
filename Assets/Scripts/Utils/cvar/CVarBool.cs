using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CVarBool : CVar
{
    private bool m_defaultValue;
    private bool m_value;

    public CVarBool(string name, bool value)
        : base(name)
    {
        m_defaultValue = value;
        boolValue = value;
    }

    public bool boolValue
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

    public override string stringValue
    {
        get
        {
            return m_value ? "true" : "false";
        }
        set
        {
            int intValue;
            if (int.TryParse(value, out intValue))
            {
                boolValue = intValue != 0;
            }
            else
            {
                value = value.ToLower();
                if (value == "true")
                {
                    boolValue = true;
                }
                else if (value == "false")
                {
                    boolValue = false;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public override void Reset()
    {
        boolValue = m_defaultValue;
    }

    public static implicit operator bool(CVarBool variable)
    {
        return variable.boolValue;
    }
}
