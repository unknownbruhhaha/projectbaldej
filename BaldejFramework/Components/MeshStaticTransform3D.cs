using BaldejFramework.Assets;
using BEPUphysics.BroadPhaseEntries;
using BEPUutilities;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace BaldejFramework.Components
{
    public class MeshStaticTransform3D : Transform, IDisposable
    {
        public Vector3 Position { get => _position; set { _position = value; UpdateBody(); } }

        public Vector3 Rotation { get => _rotation; set { _rotation = value; UpdateBody(); } }
        public Vector3 Scale { get => _scale; set { _scale = value; UpdateBody(); } }
        public string componentID { get => "Transform"; }
        public GameObject? owner { get; set; }

        private StaticMesh _staticMesh;
        private BEPUutilities.Vector3[] _verts;
        private BEPUutilities.Vector3[] _inds;

        private ObjMeshAsset _meshAsset;

        private Vector3 _position;
        private Vector3 _rotation;
        private Vector3 _scale;
        
        private int collisionID;

        public MeshStaticTransform3D(Vector3 position, Vector3 rotation, Vector3 scale, ObjMeshAsset meshAsset)
        {
            _position = position;
            _rotation = rotation;
            _scale = scale;

            _meshAsset = meshAsset;
            _staticMesh = new(meshAsset.GetBepuVerts(), meshAsset.GetBepuIndices(),
                new AffineTransform(new BEPUutilities.Vector3(_scale.X, _scale.Y, _scale.Z),
                new Quaternion(_rotation.X, _rotation.Y, _rotation.Z, 1),
                new BEPUutilities.Vector3(_position.X, _position.Y, _position.Z)));
            Physics.Space.Add(_staticMesh);
            Physics.TotallyCollisionObjectSpawned++;
            collisionID = Physics.TotallyCollisionObjectSpawned;
            _staticMesh.Tag = collisionID;
        }
 
        public MeshStaticTransform3D(ObjMeshAsset meshAsset)
        {
            _position = Vector3.Zero;
            _rotation = Vector3.Zero;
            _scale = new Vector3(1, 1, 1);

            _meshAsset = meshAsset;
            _staticMesh = new(meshAsset.GetBepuVerts(), meshAsset.GetBepuIndices(),
                new AffineTransform(new BEPUutilities.Vector3(_scale.X, _scale.Y, _scale.Z),
                new Quaternion(_rotation.X, _rotation.Y, _rotation.Z, 1),
                new BEPUutilities.Vector3(_position.X, _position.Y, _position.Z)));
            Physics.Space.Add(_staticMesh);
            Physics.TotallyCollisionObjectSpawned++;
            collisionID = Physics.TotallyCollisionObjectSpawned;
            _staticMesh.Tag = collisionID;
        }

        public void Start()
        {
            Physics.CollisionObjectsIDs.Add(collisionID, owner);
        }
        
        public void OnRender() { }

        public void OnUpdate() { }

        private void UpdateBody()
        {
            _staticMesh = new(_meshAsset.GetBepuVerts(), _meshAsset.GetBepuIndices(),
                new AffineTransform(new BEPUutilities.Vector3(_scale.X, _scale.Y, _scale.Z),
                new Quaternion(_rotation.X, _rotation.Y, _rotation.Z, 1),
                new BEPUutilities.Vector3(_position.X, _position.Y, _position.Z)));
            
            Physics.Space.Add(_staticMesh);
        }
        
        public void Dispose()
        {
            Physics.CollisionObjectsIDs.Remove(collisionID);
        }
    }
}
