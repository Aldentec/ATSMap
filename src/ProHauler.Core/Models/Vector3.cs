namespace ProHauler.Core.Models
{
    /// <summary>
    /// Represents a 3D vector or position in game world coordinates.
    /// Used for tracking vehicle position and calculating distances.
    /// </summary>
    public struct Vector3
    {
        /// <summary>
        /// Gets or sets the X coordinate (East-West position).
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate (vertical altitude).
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Gets or sets the Z coordinate (North-South position).
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        /// Initializes a new instance of the Vector3 struct with specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Gets a Vector3 with all components set to zero.
        /// </summary>
        public static Vector3 Zero => new Vector3(0, 0, 0);

        /// <summary>
        /// Calculates the Euclidean distance between this vector and another.
        /// </summary>
        /// <param name="other">The other vector to calculate distance to.</param>
        /// <returns>The distance between the two vectors.</returns>
        public float Distance(Vector3 other)
        {
            float dx = X - other.X;
            float dy = Y - other.Y;
            float dz = Z - other.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
