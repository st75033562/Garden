public struct Pair<T1, T2>
{
    public T1 first;
    public T2 second;

    public Pair(T1 first, T2 second)
    {
        this.first = first;
        this.second = second;
    }

    // TODO: equals
}

public struct Pair
{
    public static Pair<T1, T2> Of<T1, T2>(T1 first, T2 second)
    {
        return new Pair<T1, T2>(first, second);
    }
}