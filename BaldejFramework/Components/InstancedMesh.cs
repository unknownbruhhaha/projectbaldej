using BaldejFramework.Assets;
using OpenTK.Mathematics;

namespace BaldejFramework.Components
{
    public class InstancedMesh : Component
    {
        public string componentID => "Mesh";
        public GameObject? owner { get; set; }
        private string _instancedAssetShortName;
        public int DisableDistance = -1;
        
        public InstancedMesh(string instancedAssetShortName)
        {
            _instancedAssetShortName = instancedAssetShortName;
        }

        public void OnUpdate()
        {
            InstancedObjMeshAsset meshAsset = (InstancedObjMeshAsset)AssetManager.Assets[_instancedAssetShortName];
            Transform transformComponent = (Transform)owner.GetComponent("Transform");
            
            if (DisableDistance == -1)
            {
                meshAsset.Positions.Add(SetupMatrix(transformComponent));
                //Console.WriteLine("Setting Matrix up");
                return;
            }
            
            if (Math.Abs(transformComponent.Position.X - Render.Render.camera.Position.X)
                + Math.Abs(transformComponent.Position.Y - Render.Render.camera.Position.Y)
                + Math.Abs(transformComponent.Position.Z - Render.Render.camera.Position.Z) < DisableDistance)
            {
                meshAsset.Positions.Add(SetupMatrix(transformComponent));
            }
        }

        private Matrix4 SetupMatrix(Transform transformComponent)
        {
            Matrix4 transform = Matrix4.Identity;
            transform = transform * Matrix4.CreateRotationZ(Convert.ToSingle(Math.PI / 180 * transformComponent.Rotation.Z));
            transform = transform * Matrix4.CreateRotationY(Convert.ToSingle(Math.PI / 180 * transformComponent.Rotation.Y));
            transform = transform * Matrix4.CreateRotationX(Convert.ToSingle(Math.PI / 180 * transformComponent.Rotation.X));
            transform = transform * Matrix4.CreateScale(transformComponent.Scale.X, transformComponent.Scale.Y, transformComponent.Scale.Z);
            transform = transform * Matrix4.CreateTranslation(transformComponent.Position.X, transformComponent.Position.Y, transformComponent.Position.Z);
            
            return transform;
        }

        public void OnRender()
        {
            
        }
    }
}