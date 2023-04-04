using System.Text;
using System.Timers;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using BaldejFramework.UI;
using BaldejFramework.Assets;
using Neo.IronLua;

namespace BaldejFramework.Render
{
    public static class Render
    {
        public static GameWindow window;
        public static Camera camera;
        public static float[] SkyColor = { 0.5f, 0.9f, 1f };
        public static Vector3 SunColor = new Vector3(1, 1, 1);
        public static Vector3 SunDirection = new Vector3(0.5f, 0.5f, 0.5f);

        public static float DeltaTime;
        public static List<object> CallNewFrame = new();
        public static object game;
        public static Type gameType;

        public static bool IsPaused = false;

        public static bool RenderDisabled = false;
        
        public static bool WindowMinimized
        {
            get
            {
                if (window.WindowState == WindowState.Minimized) return true;
                else return false;
            }
        }

        private static Lua _gameLua = new Lua();
        private static dynamic _gameLuaEnv = _gameLua.CreateEnvironment<LuaGlobal>();

        private static double _currentTime;
        private static int _fps;
        
        public static void RunNoRenderGameLoop()
        {
            string _luaScriptPath = AssetManager.GetAssetFullPath("Lua\\Game.lua");
            _gameLuaEnv = _gameLua.CreateEnvironment();
            try
            {
                _gameLuaEnv.dochunk(File.ReadAllText(_luaScriptPath));
            }
            catch (LuaException e)
            {
                Console.WriteLine("Got an error from Lua script:\nLine: {0}, Data: {1}", e.Line, e.Message);
                throw;
            }

            Console.Clear();
            RenderDisabled = true;
            
            #region Creating Timer, it's Events and running loop
            System.Timers.Timer timer = new System.Timers.Timer(0.01666666f);
            timer.Elapsed += (Object source, ElapsedEventArgs args) =>
            {
                if (!IsPaused)
                {
                    foreach (object o in CallNewFrame)
                    {
                        o.GetType().GetMethod("NewFrame").Invoke(o, null);
                    }

                    SceneManager.Update();
                    Physics.Update();
                
                    try
                    {
                        _gameLuaEnv.dochunk("update()");
                    }
                    catch (LuaException e)
                    {
                        Console.WriteLine("Got an error from Lua script:\nLine: {0}, Data: {1}", e.Line, e.Message);
                        throw;
                    }
                    gameType.GetMethod("Update").Invoke(game, Array.Empty<object>());
                    Networking.Tick();
                }

                try
                {
                    _gameLuaEnv.dochunk("unpausedUpdate()");
                }
                catch (LuaException e)
                {
                    Console.WriteLine("Got an error from Lua script:\nLine: {0}, Data: {1}", e.Line, e.Message);
                    throw;
                }
                gameType.GetMethod("UnpausedUpdate").Invoke(game, Array.Empty<object>());
            };
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();
            #endregion

            #region Running Start Function in Game Lua Script
            try
            {
                _gameLuaEnv.dochunk("update()");
            }
            catch (LuaException e)
            {
                Console.WriteLine("Got an error from Lua script:\nLine: {0}, Data: {1}", e.Line, e.Message);
                throw;
            }
            #endregion

            Console.ReadLine(); // need this thing to make sure that console won't close by itself
        }
        
        public static void RunRender()
        {
            string _luaScriptPath = AssetManager.GetAssetFullPath("scripts\\game.lua");
            _gameLuaEnv = _gameLua.CreateEnvironment();
            try
            {
                Console.WriteLine("script:\n" + File.ReadAllText(_luaScriptPath) + "\n\n");
                _gameLuaEnv.dochunk(File.ReadAllText(_luaScriptPath));
            }
            catch (LuaException e)
            {
                Console.WriteLine("\nGot an error from Lua script:\nLine: {0}, Data: {1}", e.Line, e.Message);
                throw;
            }

            // creating window
            Console.WriteLine("Creating Window");
            NativeWindowSettings nws = new NativeWindowSettings();
            nws.Title = "Project Baldej";
            nws.Size = new Vector2i(1280, 720);
            nws.Profile = ContextProfile.Core;
            nws.Flags = ContextFlags.ForwardCompatible;
            window = new GameWindow(GameWindowSettings.Default, nws);

            // first we run Load func
            window.Load += Window_Load;

            // small lambda func for Resize
            window.Resize += (ResizeEventArgs obj) =>
            {
                GL.Viewport(0, 0, obj.Width, obj.Height);

                Console.WriteLine("window resize");

                camera.AspectRatio = window.Size.X / (float)window.Size.Y;

                UiManager.Resize();
            };
            // then Update, then we starting Render
            window.UpdateFrame += Window_UpdateFrame;
            window.RenderFrame += Window_RenderFrame;
            window.Closing += Window_Closing;
            window.VSync = VSyncMode.Off;
            // let's start our window!
            window.Run();
        }

        private static void Window_Closing(System.ComponentModel.CancelEventArgs obj)
        {
            Console.WriteLine("Closing game window!");
            System.Environment.Exit(1);
        }

        private static void Window_Load()
        {
						Physics.Init();
            //window.UpdateFrequency = 60;
            Console.WriteLine("Game window loaded!");
            Console.WriteLine("GL version: " + window.APIVersion);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);
            window.MouseDown += UiManager.OnClick;

            camera = new Camera(Vector3.UnitZ * 3, window.Size.X / (float)window.Size.Y);
            camera.Fov = 90;
            Physics.UISpace.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, 0, 0);
            gameType.GetMethod("Start").Invoke(game, null);

            try
            {
                _gameLuaEnv.dochunk("start()");
            }
            catch (LuaException e)
            {
                Console.WriteLine("Got an error from Lua script:\nLine: {0}, Data: {1}", e.Line, e.Message);
                throw;
            }
        }   

        private static void Window_UpdateFrame(FrameEventArgs obj)
        {
            if (!IsPaused)
            {
                foreach (object o in CallNewFrame)
                {
                    o.GetType().GetMethod("NewFrame").Invoke(o, null);
                }
                SceneManager.Update();
                Physics.Update();
                try
                {
                    _gameLuaEnv.dochunk("update()");
                }
                catch (LuaException e)
                {
                    Console.WriteLine("Got an error from Lua script:\nLine: {0}, Data: {1}", e.Line, e.Message);
                    throw;
                }
                gameType.GetMethod("Update").Invoke(game, Array.Empty<object>()); 
                Networking.Tick();
            }

            try
            {
                _gameLuaEnv.dochunk("update()");
            }
            catch (LuaException e)
            {
                Console.WriteLine("Got an error from Lua script:\nLine: {0}, Data: {1}", e.Line, e.Message);
                throw;
            }
            gameType.GetMethod("UnpausedUpdate").Invoke(game, Array.Empty<object>());
        }

        private static void Window_RenderFrame(FrameEventArgs obj)
        {
            _fps++;
            _currentTime += window.RenderTime;
            
            if (_currentTime >= 1)
            {
                window.Title = "project baldej - " + _fps + " fps / " + Render.window.RenderTime;
                _fps = 0;
                _currentTime = 0;
            }
            
            if (!IsPaused)
            {
                if (!UiManager.IsInDebugMode)
                {
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                }
                else
                {
                    GL.Disable(EnableCap.Blend);
                }

                GL.ClearColor(SkyColor[0], SkyColor[1], SkyColor[2], 1);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
                GL.FrontFace(FrontFaceDirection.Ccw);
                
                SceneManager.Render();

                GL.Disable(EnableCap.CullFace);
                
                UiManager.Render();

                window.SwapBuffers();
            }
        }
    }
}
