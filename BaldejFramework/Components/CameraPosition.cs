using CSCore.Utils;

namespace BaldejFramework.Components
{
    public class CameraPosition : Component
    {
        public string componentID { get => ""; }
        public GameObject? owner { get; set; }

        public void OnRender() { } 

        public void OnUpdate()
        {
            Transform3D transform = (Transform3D)owner.GetComponent("Transform");
            Render.Render.camera.Position = transform.Position;
            AudioManager.listener.Position = new Vector3(transform.Position.X, transform.Position.Y, transform.Position.Z);
        }
    }
}
