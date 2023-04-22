using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace BaldejFramework.Components
{
    public class BoxRigidTransform3D : Transform, IDisposable
    {
        public Vector3 Position { get => GetPosition(); set { _newPosition = value; } }
        public Vector3 Rotation { get => GetRotation(); set { _newRotation = value; } }
        public Vector3 Scale { get => _scale; set { SetScale(value); } }
        
				public float Mass { get => _mass; set { SetMass(value); } }
        public string componentID { get => "Transform"; }
        public GameObject? owner { get; set; }

        private Vector3 _scale;
        private Vector3 _collisionSize;
        private float _mass;

        private int collisionID = Physics.TotallyCollisionObjectSpawned + 1;
				private IntPtr _collision;
				private IntPtr _body;

				private Vector3? _newPosition;
				private Vector3? _newRotation;
        
				public BoxRigidTransform3D(Vector3 position, Vector3 rotation, Vector3 scale, Vector3 collisionSize, float mass = 1)
        {
						_collision = NewtonCreateBox(Physics.World, collisionSize.X, collisionSize.Y, collisionSize.Z, 0, new IntPtr());
						_body = NewtonCreateDynamicBody(Physics.World, _collision, Physics.BodyMatrixPointer);
						NewtonBodySetForceAndTorqueCallback(_body, Physics.ApplyGravity);
						NewtonBodySetLinearDamping(_body, 0.7f);
						NewtonBodySetAngularDamping(_body, Physics.AngularDampingPtr);

            _collisionSize = collisionSize;
						UpdateMatrix(rotation, position);
						_scale = scale; 
						_collisionSize = collisionSize;
						//SetScale(scale);
						SetMass(mass);
						Physics.TotallyCollisionObjectSpawned++;
        }

        public void Start()
        {
            Physics.CollisionObjectsIDs.Add(collisionID, owner);
        }

        public void OnRender() { }

        public void OnUpdate()
				{
						if (_newRotation == null && _newPosition != null) UpdatePosition((Vector3)_newPosition);
						else if (_newRotation != null) UpdateMatrix((Vector3)_newRotation, _newPosition);
						_newPosition = null;
						_newRotation = null;
				}

				private Vector3 GetPosition()
				{
						NewtonBodyGetMatrix(_body, Physics.BodyTransformationMatrix.Ptr);
						Vector3 positionVector = new Vector3(Physics.BodyTransformationMatrix[12], Physics.BodyTransformationMatrix[13], Physics.BodyTransformationMatrix[14]);
						return positionVector;
				}
				
				private Vector3 GetRotation()
				{
						NewtonBodyGetMatrix(_body, Physics.BodyTransformationMatrix.Ptr);

						NewtonGetEulerAngle(Physics.BodyTransformationMatrix.Ptr, Physics.BodyRotation.Ptr);
						Vector3 rotationVector = new (MathUtils.Rad2Deg * Physics.BodyRotation[0], MathUtils.Rad2Deg * Physics.BodyRotation[1], MathUtils.Rad2Deg * Physics.BodyRotation[2]);
						Vector3 positionVector = new Vector3(Physics.BodyTransformationMatrix[12], Physics.BodyTransformationMatrix[13], Physics.BodyTransformationMatrix[14]);
						return rotationVector;
				}
				
				private void UpdateMatrix(Vector3 rotation, Vector3? position)
				{
						NewtonBodyGetMatrix(_body, Physics.BodyTransformationMatrix.Ptr);
						
						Vector3 currentBodyPosition = new (Physics.BodyTransformationMatrix[12], Physics.BodyTransformationMatrix[13], Physics.BodyTransformationMatrix[14]);
						Physics.SetRotationInMatrix(rotation);
						if (position != null) Physics.SetPositionInMatrix((Vector3)position); 
						else Physics.SetPositionInMatrix(currentBodyPosition); 

						NewtonBodySetMatrix(_body, Physics.BodyTransformationMatrix.Ptr);
				}

				private void UpdatePosition(Vector3 position)
				{
						NewtonBodyGetMatrix(_body, Physics.BodyTransformationMatrix.Ptr);
						Physics.SetPositionInMatrix(position);
						NewtonBodySetMatrix(_body, Physics.BodyTransformationMatrix.Ptr);
				}

				private void SetScale(Vector3 scale)
				{
						_scale = scale; 
						NewtonBodySetCollisionScale(_body, _collisionSize.X * scale.X, _collisionSize.Y * scale.Y, 	_collisionSize.Z * scale.Z); 
				}

				private void SetMass(float mass)
				{
						_mass = mass;
						NewtonBodySetMassMatrix(_body, mass, 1, 1, 1);
				}

        // TODO: public void AddForce(Vector3 force) {}

        public void Dispose()
        {
            Physics.CollisionObjectsIDs.Remove(collisionID);
        }

				[DllImport("libNewton")]
				private static extern IntPtr NewtonCreateBox(IntPtr newtonWorld, float dx, float dy, float dz, int shapeID, IntPtr offsetMatrix);
				[DllImport("libNewton")]
				private static extern IntPtr NewtonCreateDynamicBody(IntPtr newtonWorld, IntPtr collision, IntPtr offsetMatrix);
				[DllImport("libNewton")]
				private static extern void NewtonBodySetMassMatrix(IntPtr body, float mass, float Ixx, float Iyy, float Izz);
				[DllImport("libNewton")]
				private static extern void NewtonBodySetForceAndTorqueCallback(IntPtr body, Physics.NewtonBodyEventHandler callback);
				[DllImport("libNewton")]
				private static extern void NewtonBodySetForce(IntPtr body, IntPtr force);
				[DllImport("libNewton")]
				private static extern void NewtonBodyGetRotation(IntPtr body, IntPtr rotation);
				[DllImport("libNewton")]
				private static extern void NewtonBodyGetMatrix(IntPtr body, IntPtr matrix);
				[DllImport("libNewton")]
				private static extern void NewtonBodySetCollisionScale (IntPtr body, float scaleX, float scaleY, float scaleZ);	
				[DllImport("libNewton")]
				private static extern void NewtonBodySetMatrix(IntPtr body, IntPtr matrix);
				[DllImport("libNewton")]
				private static extern void NewtonGetEulerAngle(IntPtr matrix, IntPtr eulerAngles);
				[DllImport("libNewton")]
				private static extern void NewtonSetEulerAngle(IntPtr eulerAngles, IntPtr matrix);
				[DllImport("libNewton")]
				private static extern void NewtonBodySetLinearDamping(IntPtr body, float linearDamp);
				[DllImport("libNewton")]
				private static extern void NewtonBodySetAngularDamping(IntPtr body, IntPtr angularDamp);

				[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
				private delegate void NewtonBodyEventHandler(IntPtr body);
		}
}
