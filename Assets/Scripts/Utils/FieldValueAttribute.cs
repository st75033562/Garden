using System;

public class FieldValueAttribute : Attribute
{
    public FieldValueAttribute(object defaultValue)
    {
        this.defaultValue = defaultValue;
    }

    public object defaultValue
    {
        get;
        private set;
    }

    public bool IsDefaultValue(object value)
    {
        if (defaultValue == null && value == null)
        {
            return true;
        }

        if (defaultValue != null && value != null)
        {
            return defaultValue.Equals(value);
        }

        return false;
    }
}
