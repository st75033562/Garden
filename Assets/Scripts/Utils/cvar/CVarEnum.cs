using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CVarEnum<T> : CVar
{
    private T m_defaultValue;
    private T m_value;

    public CVarEnum(string name, T value)
        : base(name)
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException("T must be enum");
        }
        m_defaultValue = value;
        enumValue = value;
    }

    public T enumValue
    {
        get { return m_value; }
        set
        {
            if (!m_value.Equals(value))
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
            return m_value.ToString();
        }
        set
        {
            int intValue;
            if (int.TryParse(value, out intValue))
            {
                foreach (T e in Enum.GetValues(typeof(T)))
                {
                    if ((int)(object)e == intValue)
                    {
                        enumValue = e;
                        return;
                    }
                }

                throw new ArgumentOutOfRangeException("Invalid enum value for " + typeof(T));
            }
            else
            {
                enumValue = (T)Enum.Parse(typeof(T), value);
            }
        }
    }

    public override void Reset()
    {
        enumValue = m_defaultValue;
    }

    public static implicit operator T(CVarEnum<T> v)
    {
        return v.enumValue;
    }
}

public static class CVarEnum
{
    public static CVarEnum<T> Of<T>(string name, T value)
    {
        return new CVarEnum<T>(name, value);
    }
}