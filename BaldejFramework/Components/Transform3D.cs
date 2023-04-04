using OpenTK.Mathematics;
using BaldejFramework;

namespace BaldejFramework.Components
{
    public class Transform3D : Transform
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public string componentID { get => "Transform"; }

        public GameObject? owner { get; set; }

        public Transform3D(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public Transform3D()
        {
            Position = new Vector3();
            Rotation = new Vector3();
            Scale = new Vector3(1, 1, 1);
        }

        public void OnUpdate() {}

        public void OnRender() { }
    }
}
