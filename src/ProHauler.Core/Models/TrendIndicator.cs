namespace ProHauler.Core.Models
{
    /// <summary>
    /// Indicates the direction of performance trend compared to session average.
    /// </summary>
    public enum TrendIndicator
    {
        /// <summary>
        /// Performance is improving (above session average).
        /// </summary>
        Up,

        /// <summary>
        /// Performance is declining (below session average).
        /// </summary>
        Down,

        /// <summary>
        /// Performance is stable (at or near session average).
        /// </summary>
        Stable
    }
}
