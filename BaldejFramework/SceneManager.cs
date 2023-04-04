using BaldejFramework.Components;
using OpenTK.Graphics.OpenGL;

namespace BaldejFramework
{
    public class SceneManager
    {
        public static Scene currentScene = new Scene();

        public static void SetCurrentScene(Scene scn)
        {
            currentScene = scn;
        }

        public static void Update()
        {
            for (int i = 0; i < currentScene.gameObjects.Count; i++)
            {
                GameObject obj = currentScene.gameObjects[i];

                foreach (Component component in obj.components)
                {
                    if (component.GetType().GetMethod("OnUpdate") != null)
                    {
                        component.OnUpdate();
                    }
                }
            }
        }

        public static void Render()
        {
            for (int i = 0; i < currentScene.gameObjects.Count; i++)
            {
                GameObject obj = currentScene.gameObjects[i];

                foreach (Component component in obj.components)
                {
                    if (component.GetType().GetMethod("OnRender") != null)
                    {
                        component.OnRender();
                    }
                }
            }
        }
    }
}
