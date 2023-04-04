using BaldejFramework.Components;

namespace BaldejFramework
{
    public class GameObject : IDisposable
    {
        public string Name;
        public List<Component> components = new List<Component>();
        public GameObject? parrent;
        private bool _disposed = false;
        public Dictionary<string, object> Data = new();

        public GameObject(string name, string preset = "")
        {
            Name = name;
            SceneManager.currentScene.gameObjects.Add(this);
        }

        public void AddComponent(Component component)
        {
            component.owner = this;
            components.Add(component);
            Type componentType = component.GetType();
            if (componentType.GetMethod("Start") != null)
            {
                componentType.GetMethod("Start").Invoke(component, null);
            }
        }

        public void RemoveComponent(string ID)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].componentID == ID)
                {
                    Type componentType = components[i].GetType();
                    if (componentType.GetMethod("Dispose") != null)
                    {
                        componentType.GetMethod("Dispose").Invoke(components[i], null);
                    }
                    components.RemoveAt(i);
                }
            }
        }

        public Component? GetComponent(string ID)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].componentID == ID)
                {
                    return components[i];
                }
            }

            return null;
        }

        public bool HasComponent(string type)
        {
            foreach (Component comp in components)
            {
                if (comp.componentID == type)
                    return true;
            }
            return false;
        }

        ~GameObject()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                for (int i = 0; i < components.Count; i++)
                {
                    Type componentType = components[i].GetType();
                    if (componentType.GetMethod("Dispose") != null)
                    {
                        componentType.GetMethod("Dispose").Invoke(components[i], null);
                    }

                    components.RemoveAt(i);
                }
            }
        }
    }
}
