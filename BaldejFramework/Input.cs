using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BaldejFramework
{
    public static class Input
    {
        public static bool IsKeyDown(Keys key)
        {
            return Render.Render.window.IsKeyDown(key);
        }

        public static bool IsKeyPressed(Keys key)
        {
            return Render.Render.window.IsKeyPressed(key);
        }

        public static bool IsMouseButtonPressed(MouseButton mouseButton)
        {
            return Render.Render.window.IsMouseButtonPressed(mouseButton);
        }
        
        public static bool IsMouseButtonDown(MouseButton mouseButton)
        {
            return Render.Render.window.IsMouseButtonDown(mouseButton);
        }
        
        public static bool IsMouseButtonReleased(MouseButton mouseButton)
        {
            return Render.Render.window.IsMouseButtonReleased(mouseButton);
        }
    }
}
