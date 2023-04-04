using OpenTK.Mathematics;

namespace BaldejFramework.Render.Data
{
    public class MeshPosition
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Matrix4 Matrix
        {
            get
            {
                return Matrix4.Identity * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateScale(Scale.X, Scale.Y, Scale.Z) * Matrix4.CreateTranslation(Position.X, Position.Y, Position.Z);
            }
        }

        public MeshPosition(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }
}
