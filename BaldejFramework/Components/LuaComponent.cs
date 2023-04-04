using BaldejFramework.Assets;   
using Neo.IronLua;
using System.IO;

namespace BaldejFramework.Components
{
    public class LuaComponent : Component
    {
        public string componentID { get => (string)_luaEnv.componentID; }
        public GameObject? owner
        {
            get => _owner;
            set
            {
                _owner = value;
                _luaEnv.owner = value;
            }
        }

        private GameObject? _owner;

        public string LuaScriptPath
        {
            set
            {
                _luaScriptPath = AssetManager.GetAssetFullPath(value);
         
                _lua = new Lua();
                _luaEnv = _lua.CreateEnvironment<LuaGlobal>();
                try
                {
                    _luaEnv.dochunk(File.ReadAllText(_luaScriptPath));
                }
                catch (LuaException e)
                {
                    Console.WriteLine("Got an error from Lua script:\nLine: {0}, Data: {1}", e.Line, e.Message);
                    throw;
                }
            }

            get => _luaScriptPath;
        }

        private string _luaScriptPath;
        private Lua _lua;
        dynamic _luaEnv;
        LuaMethod _render;
        LuaMethod _update;

        public LuaComponent(string luaScriptPath)
        {
            LuaScriptPath = luaScriptPath;
        }

        public void Start()
        {
            PrepareLua();
        }
        
        private void PrepareLua()
        {
            _luaEnv.owner = owner;
            _luaEnv.LuaFuncs = new LuaFuncs();

            _render = (LuaMethod)_luaEnv.render;
            _update = (LuaMethod)_luaEnv.update;

            LuaMethod start = (LuaMethod)_luaEnv.start;
            start.Method.Invoke(null, null);
        }

        public void OnUpdate()
        {
            _update.Method.Invoke(null, null);
        }

        public void OnRender() 
        {
            _render.Method.Invoke(null, null);
        }
    
        public void SetVar(string name, object value) { _luaEnv[name] = value; }
        
        public object GetVar(string name) { return _luaEnv[name]; }

        public object? Call(string funcName, object[] args)
        {
            LuaMethod func = (LuaMethod)_luaEnv[funcName];
            return func.Method.Invoke(null, args);
        }
    }
}
