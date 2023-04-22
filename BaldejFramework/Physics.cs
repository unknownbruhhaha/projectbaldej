using BEPUphysics;
using BEPUutilities;
using System.Runtime.InteropServices;
using UnmanageUtility;

namespace BaldejFramework
{
	public static class Physics
	{
		public static Space UISpace = new Space();
		public static Space Space = new Space();
		public static IntPtr World;

		static int _id;
		public static Dictionary<int, GameObject> CollisionObjectsIDs = new Dictionary<int, GameObject>();
		public static int TotallyCollisionObjectSpawned = 0;
		
		public static IntPtr BodyMatrixPointer;
		public static IntPtr GravityForcePointer;
		public static IntPtr AngularDampingPtr;
		
		public static NewtonBodyEventHandler ApplyGravity = ApplyGravityCallback;
		private static UnmanagedArray<float> _state = new(16);
		public static UnmanagedArray<float> BodyTransformationMatrix = new(16);
		public static UnmanagedArray<float> BodyRotation = new(3);
		
		public static void Init() 
		{
				World = NewtonCreate();
				// setting some vars
						// body matrix
				float[] _bodyMatrix = {
						1.0f, 0.0f, 0.0f, 0.0f,
						0.0f, 1.0f, 0.0f, 0.0f,
						0.0f, 0.0f, 1.0f, 0.0f,
						0.0f, 0.0f, 0.0f, 1.0f
				};
				GCHandle _bodyMatrixHandle = GCHandle.Alloc(_bodyMatrix, GCHandleType.Pinned);
				BodyMatrixPointer = _bodyMatrixHandle.AddrOfPinnedObject();
						// gravity force
				float[] _gravityForce = {0, -9.8f, 0};
				GCHandle _gravityForceHandle = GCHandle.Alloc(_gravityForce, GCHandleType.Pinned);
				GravityForcePointer = _gravityForceHandle.AddrOfPinnedObject();
				AngularDampingPtr = GCHandle.Alloc(new float[] {0.7f, 0.7f, 0.7f}, GCHandleType.Pinned).AddrOfPinnedObject();
		}

		public static int GenID()
		{
			_id++;
			return _id;
		}

		public static void Update()
		{
				NewtonUpdate(World, Convert.ToSingle(Render.Render.window.RenderTime));	
				Space.Update();
				Space.ForceUpdater.Gravity = new Vector3(0, -9.8f, 0);
		}
		
		public static Dictionary<string, object> Raycast(Vector3 rayPosition, Vector3 rayDirection)
		{
			BEPUutilities.Vector3 rayFrom = new BEPUutilities.Vector3(rayPosition.X, rayPosition.Y, rayPosition.Z);
			BEPUutilities.Vector3 rayTo = new BEPUutilities.Vector3(rayDirection.X, rayDirection.Y, rayDirection.Z);
			Console.WriteLine("rayto is " + rayDirection);
			Physics.Space.RayCast(new Ray(rayFrom, rayTo), out var result);
			GameObject hitObject = null;
            
			if (result.HitObject != null)
			{
				Console.WriteLine("hitobj != null");
				Console.WriteLine("Physics " + Physics.CollisionObjectsIDs.Count);
				hitObject = Physics.CollisionObjectsIDs[(int)result.HitObject.Tag];
			}

			return new Dictionary<string, object>()
			{
				{"HitObject", hitObject},
				{"Location", new Vector3(result.HitData.Location.X, result.HitData.Location.Y, result.HitData.Location.Z)},
				{"Normal", new Vector3(result.HitData.Normal.X, result.HitData.Normal.Y, result.HitData.Normal.Z)},
				{"RaycastResult", result}
			};
		}
				
		private static void ApplyGravityCallback(IntPtr bodyID)
		{
				NewtonBodySetForce(bodyID, Physics.GravityForcePointer);

				NewtonBodyGetMatrix(bodyID, _state.Ptr);
				NewtonGetEulerAngle(_state.Ptr, BodyRotation.Ptr);
				Console.WriteLine("x: {0}; y: {1}; z: {2}", BodyRotation[0], BodyRotation[1], BodyRotation[2]);
				//Console.WriteLine("Time {0}: x={1} y={2} z={3}\n",
				//		Render.Render.DeltaTime, _state[12], _state[13], _state[14]);
				

				//for (int i = 0; i < 16; i++) 
				//{
						//Console.WriteLine ("Body Matrix[{0}] = {1}", i, _state[i]);
				//}
		}

		public static void SetPositionInMatrix(OpenTK.Mathematics.Vector3 position) 
		{
				BodyTransformationMatrix[12] = position.X; 
				BodyTransformationMatrix[13] = position.Y; 
				BodyTransformationMatrix[14] = position.Z; 
		}

		public static void SetRotationInMatrix(OpenTK.Mathematics.Vector3 rotation) 
		{
				Physics.BodyRotation[0] = Convert.ToSingle(MathUtils.Deg2Rad * rotation.X);
				Physics.BodyRotation[1] = Convert.ToSingle(MathUtils.Deg2Rad * rotation.Y);
				Physics.BodyRotation[2] = Convert.ToSingle(MathUtils.Deg2Rad * rotation.Z);
						
				NewtonSetEulerAngle(Physics.BodyRotation.Ptr, Physics.BodyTransformationMatrix.Ptr);	
		}

		// Newton Dynamics p/invoke
		[DllImport("libNewton")]
		private static extern IntPtr NewtonCreate();
		[DllImport("libNewton")]
		private static extern IntPtr NewtonUpdate(IntPtr newtonWorld, float timestep);
		[DllImport("libNewton")]
		private static extern void NewtonBodySetForce(IntPtr body, IntPtr force);
		[DllImport("libNewton")]
		private static extern void NewtonBodyGetMatrix(IntPtr body, IntPtr matrix);
		[DllImport("libNewton")]
		private static extern void NewtonSetEulerAngle(IntPtr eulerAngles, IntPtr matrix);
		[DllImport("libNewton")]
		private static extern void NewtonGetEulerAngle(IntPtr matrix, IntPtr eulerAngles);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void NewtonBodyEventHandler(IntPtr body);
	}
}

