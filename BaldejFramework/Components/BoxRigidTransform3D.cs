using System.Runtime.InteropServices;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace BaldejFramework.Components
{
    public class BoxRigidTransform3D : Transform, IDisposable
    {
        public Vector3 Position { get => _position; set { _position = value; UpdateBody(); } }

        public Vector3 Rotation { get => _rotation; set { _rotation = value; UpdateBody(); } }
        public Vector3 Scale { get => _scale; set { _scale = value; UpdateBody(); } }
        public float Mass { get => _mass; set { _mass = value; UpdateBody(); } }
        public bool IsKinematic { get => _isKinematic; set { _isKinematic = value; UpdateBody(); } }
        public string componentID { get => "Transform"; }
        public GameObject? owner { get; set; }

        private Box _box;

        private Vector3 _position;
        private Vector3 _rotation;
        private Vector3 _scale;
        private Vector3 _collisionSize;
        private float _mass;
        private bool _isKinematic = false; 

        private int collisionID = Physics.TotallyCollisionObjectSpawned + 1;
				IntPtr collision;
				IntPtr body;
        
				public BoxRigidTransform3D(Vector3 position, Vector3 rotation, Vector3 scale, Vector3 collisionSize, float mass = 1, bool isKinematic = false)
        {
            //_box = new Box(new BEPUutilities.Vector3(position.X, position.Y, position.Z), collisionSize.X * 2, collisionSize.Y * 2, collisionSize.Z * 2, mass);
            //Physics.Space.Add(_box);
	
						collision = NewtonCreateBox(Physics.World, collisionSize.X, collisionSize.Y, collisionSize.Z, 0, new IntPtr());
						body = NewtonCreateDynamicBody(Physics.World, collision, Physics.BodyMatrixPointer);
						NewtonBodySetMassMatrix(body, 1.0f, 1, 1, 1);
						NewtonBodySetForceAndTorqueCallback(body, cb_applyForce);

            _collisionSize = collisionSize;
            _position = position;
            _rotation = rotation;
            _scale = scale;
            _mass = mass;
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
            _rotation = new(Convert.ToSingle(Math.PI / 180) * _box.Orientation.X, Convert.ToSingle(Math.PI / 180) *  _box.Orientation.Y, Convert.ToSingle(Math.PI / 180) *  _box.Orientation.Z);
        }

        private void UpdateBody()
        {
            _box = new Box(new BEPUutilities.Vector3(_position.X, _position.Y, _position.Z), _collisionSize.X * 2f, _collisionSize.Y * 2f, _collisionSize.Z * 2f, _mass);
            _box.OrientationMatrix = Matrix3x3.CreateFromQuaternion(new Quaternion(_rotation.X / Convert.ToSingle(Math.PI / 180), _rotation.Y / Convert.ToSingle(Math.PI / 180), _rotation.Z / Convert.ToSingle(Math.PI / 180), 1));
            
            if (IsKinematic)
                _box.BecomeKinematic();
            
            Physics.Space.Add(_box);
        }

        public void AddForce(Vector3 force)
        {
            _box.ApplyImpulse(BEPUutilities.Vector3.Zero, new BEPUutilities.Vector3(force.X, force.Y, force.Z));
        }

        public void Dispose()
        {
            Physics.CollisionObjectsIDs.Remove(collisionID);
        }

				[DllImport("libNewton")]
				private static extern IntPtr NewtonCreateBox(IntPtr newtonWorld, float dx, float dy, float dz, int shapeID, IntPtr offsetMatrix);

				[DllImport("libNewton")]
				private static extern IntPtr NewtonCreateDynamicBody(IntPtr newtonWorld, IntPtr collision, IntPtr offsetMatrix);

				/*
				 float	foo[16] = {
    1.0f, 0.0f, 0.0f, 0.0f,
    0.0f, 1.0f, 0.0f, 0.0f,
    0.0f, 0.0f, 1.0f, 0.0f,
    0.0f, 0.0f, 0.0f, 1.0f
  };
	
  // Create the sphere. Is this radius or diameter?
  NewtonCollision* const collision = NewtonCreateSphere(world, 1, 0, NULL);

  // fixme: what is this for?
  NewtonBody* const body = NewtonCreateDynamicBody(world, collision, foo);

  // fixme: has no doc string. Is this mass, I_xx, I_yy, and I_zz?
  NewtonBodySetMassMatrix(body, 1.0f, 1, 1, 1);

  // Install callback. Newton will call it whenever the object moves.
  NewtonBodySetForceAndTorqueCallback(body, cb_applyForce);
	*/
    }
}
