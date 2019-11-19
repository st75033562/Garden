/// <summary>
/// <para>wrapper for value type values</para>
/// </summary>
public class ValueWrapper<T>
{
    public ValueWrapper()
    {
    }

    public ValueWrapper(T value)
    {
        this.value = value;
    }

    public T value
    {
        get;
        set;
    }
}
