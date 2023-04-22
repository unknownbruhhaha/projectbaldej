using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace BaldejFramework
{
    public static class MathUtils
    {
				public static float Deg2Rad { get => 0.01745329251f; }
				public static float Rad2Deg { get => 57.2957795131f; }
        public static Vector3 RotateVector3(Vector3 inVector3, Vector3 rotation)
        {
            System.Numerics.Vector3 convertedInVector = new(inVector3.X, inVector3.Y, inVector3.Z);
            System.Numerics.Vector3 rotatedVector =
                System.Numerics.Vector3.Transform(convertedInVector, new System.Numerics.Quaternion(rotation.X, rotation.Y, rotation.Z, 1));

            return new Vector3(rotatedVector.X, rotatedVector.Y, rotatedVector.Z);
        }
    }
}
