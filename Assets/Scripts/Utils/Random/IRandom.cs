public interface IRandom
{
    /// <summary>
    /// generate a non-negative integer
    /// </summary>
    int nextInt { get; }

    /// <summary>
    /// generate a float in the range [0...1]
    /// </summary>
    float nextFloat { get; }

    /// <summary>
    /// the state used to restore the RNG
    /// </summary>
    object state { get; set; }
}
