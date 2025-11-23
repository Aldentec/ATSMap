namespace ATSLiveMap.Core.Models
{
    // struct is like a lightweight class for simple data (similar to a plain object in JS)
    public struct Vector3
    {
        // Properties are like object properties in JS, but with explicit types
        public float X { get; set; }  // East-West position
        public float Y { get; set; }  // Vertical (altitude)
        public float Z { get; set; }  // North-South position

        // Constructor - like a class constructor in JS
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        // Static property - like a static class property
        public static Vector3 Zero => new Vector3(0, 0, 0);

        // Method to calculate distance between two points
        public float Distance(Vector3 other)
        {
            float dx = X - other.X;
            float dy = Y - other.Y;
            float dz = Z - other.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
