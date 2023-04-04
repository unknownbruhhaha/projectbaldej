using BEPUphysics;
using BEPUutilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BaldejFramework.UI
{
    public static class UiManager
    {
        public static List<UiElement> elements = new List<UiElement>();
        public static Matrix4 UiMatrix = Matrix4.CreateOrthographicOffCenter(0, -1, 1, 0, 15, -15);
        public static bool IsInDebugMode = false;
        public static UiElement? SelectedElement;

        public static void Render()
        {
            #region Rendering elements
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].Render();
            }
            #endregion
        }

        public static void Resize()
        {
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].Resize(BaldejFramework.Render.Render.window.Size);
            }
        }

        public static void OnClick(OpenTK.Windowing.Common.MouseButtonEventArgs args)
        {
            if (args.Button == MouseButton.Left)
            {
                Console.WriteLine("lmb pressed");
                #region Setting parameters
                OpenTK.Mathematics.Vector2 mousePos = BaldejFramework.Render.Render.window.MousePosition;
                Vector2i windowSize = BaldejFramework.Render.Render.window.Size;
                OpenTK.Mathematics.Vector2 worldMouse = new OpenTK.Mathematics.Vector2(mousePos.X / windowSize.X, mousePos.Y / windowSize.Y);
                BEPUutilities.Vector3 rayFrom = new BEPUutilities.Vector3(worldMouse.X, worldMouse.Y, 15);
                BEPUutilities.Vector3 rayTo = new BEPUutilities.Vector3(0, 0, -5);
                UiElement? pressed = null;
                #endregion

                #region Updating UI Space
                Physics.UISpace.Update();
                #endregion

                #region Making a raycast
                Physics.UISpace.RayCast(new Ray(rayFrom, rayTo), out var result);
                #endregion

                #region Setting pressed UI element
                if (result.HitObject != null)
                {
                    for (int i = 0; i < elements.Count; i++)
                    {
                        UiElement element = elements[i];
                        if (element.BodyID == (int)result.HitObject.Tag)
                        {
                            pressed = element;
                            break;
                        }
                    }
                }
                #endregion

                #region Running UI element's Clicked method
                if (pressed != null) {
                    Console.WriteLine("Clicked on UiElement(" + pressed + ")");
                    SelectedElement = pressed;
                    pressed.Clicked(new OpenTK.Mathematics.Vector2(result.HitData.Location.X, result.HitData.Location.Y));
                }
                else
                {
                    SelectedElement = null;
                }
                #endregion
            }
        }
    }
}
