using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.EntityStateManagement;
using BEPUutilities;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace BaldejFramework.Components
{
    public class BoxStaticTransform3D : Transform
    {
        public Vector3 Position { get => _position; set { _position = value; UpdateBody(); } }

        public Vector3 Rotation { get => _rotation; set { _rotation = value; UpdateBody(); } }
        public Vector3 Scale { get => _scale; set { _scale = value; UpdateBody(); } }
        public string componentID { get => "Transform"; }
        public GameObject? owner { get; set; }

        private Box _box;

        private Vector3 _position;
        private Vector3 _rotation;
        private Vector3 _scale;
        private float _mass;
        
        private int collisionID = Physics.TotallyCollisionObjectSpawned + 1;

        public BoxStaticTransform3D(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
            Physics.TotallyCollisionObjectSpawned++;
            _box.CollisionInformation.Tag = collisionID;
        }

        public BoxStaticTransform3D()
        {
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
            Scale = Vector3.One;
            Physics.TotallyCollisionObjectSpawned++;
            _box.CollisionInformation.Tag = collisionID;
        }
        
        public void Start()
        {
            Physics.CollisionObjectsIDs.Add(collisionID, owner);
        }

        public void OnRender()
        {

        }

        public void OnUpdate()
        {
            _position = new(_box.Position.X, _box.Position.Y, _box.Position.Z);
            _rotation = new(_box.Orientation.X, _box.Orientation.Y, _box.Orientation.Z);
        }

        private void UpdateBody()
        {
            _box = new Box(new BEPUutilities.Vector3(Position.X, Position.Y, Position.Z), _scale.X * 2f, _scale.Y * 2f, _scale.Z * 2f);
            _box.OrientationMatrix = Matrix3x3.CreateFromQuaternion(new Quaternion(Convert.ToSingle(Math.PI / 180) *  _rotation.X, Convert.ToSingle(Math.PI / 180) * _rotation.Y, Convert.ToSingle(Math.PI / 180) * _rotation.Z, 1));

            Physics.Space.Add(_box);
        }
        
        public void Dispose()
        {
            Physics.CollisionObjectsIDs.Remove(collisionID);
        }
    }
}
