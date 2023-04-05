using BEPUphysics;
using BEPUutilities;
using System.Runtime.InteropServices;

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
		
		private static float[] _bodyMatrix = {
				1.0f, 0.0f, 0.0f, 0.0f,
				0.0f, 1.0f, 0.0f, 0.0f,
				0.0f, 0.0f, 1.0f, 0.0f,
				0.0f, 0.0f, 0.0f, 1.0f
		};

		private static GCHandle _bodyMatrixHandle = GCHandle.Alloc(_bodyMatrix, GCHandleType.Pinned);
		public static IntPtr BodyMatrixPointer = _bodyMatrixHandle.AddrOfPinnedObject();
	
		public static void Init() 
		{
				World = NewtonCreate();
		}

		public static int GenID()
		{
			_id++;
			return _id;
		}

		public static void Update()
		{
				NewtonUpdate(World, Render.Render.DeltaTime);	
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

		// Newton Dynamics p/invoke
		[DllImport("libNewton")]
		private static extern IntPtr NewtonCreate();
		[DllImport("libNewton")]
		private static extern IntPtr NewtonUpdate(IntPtr newtonWorld, float timestep);
	}
}

