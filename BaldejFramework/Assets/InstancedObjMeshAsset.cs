using System.Drawing;
using BaldejFramework.Render;
using OpenTK.Mathematics;
using System.Globalization;
using OpenTK.Graphics.OpenGL;

namespace BaldejFramework.Assets
{
    public class InstancedObjMeshAsset : Asset
    {
        public List<MeshData> Frames { get; set; }
        
        private int _currentAnimationFrame = 0;
        private int _vbo = GL.GenBuffer();
        private int _uvbo = GL.GenBuffer();
        private int _nbo = GL.GenBuffer();
        private int _vao = GL.GenVertexArray();
        private bool _shouldDraw = false;
        private Shader _shader;

        public List<Matrix4> Positions = new();
        private float[] _verts { get => Frames[_currentAnimationFrame].Vertices; }
        private float[] _uvs { get => Frames[_currentAnimationFrame].UVs; }
        private float[] _normals { get => Frames[_currentAnimationFrame].Normals; }
        private TextureAsset _texture;

        public string AssetType
        {
            get => "MeshAsset";
        }

        public string AssetShortName { get; set; }

        public InstancedObjMeshAsset(string path, string assetShortName, string objectName, int firstFileIndex, int lastFileIndex, TextureAsset texture, 
            string vertShader = "Shaders\\instancedVert.shader", string fragShader = "Shaders\\instancedFrag.shader", bool saveInAssetsList = true)
        {
            Render.Render.CallNewFrame.Add(this);
            Frames = new List<MeshData>();
            string currentObjectName = "";

            for (int i = firstFileIndex; i <= lastFileIndex; i++)
            {
                string iString = i.ToString();
                string file = File.ReadAllText(AssetManager.GetAssetFullPath(path + iString + ".obj"));
                StringReader reader = new StringReader(file);

                using (reader)
                {
                    string line;

                    List<float> verts = new List<float>();
                    List<float> uvs = new List<float>();
                    List<float> normals = new List<float>();

                    List<Vector3> tempVerts = new List<Vector3>();
                    List<Vector2> tempUvs = new List<Vector2>();
                    List<Vector3> tempNormals = new List<Vector3>();

                    while ((line = reader.ReadLine()) != null)
                    {
                        // getting current object
                        if (line.StartsWith("o "))
                        {
                            currentObjectName = line.Remove(0, 2);
                        }

                        // getting vertex pos
                        else if (line.StartsWith("v ") && currentObjectName == objectName)
                        {
                            tempVerts.Add(new Vector3(
                                float.Parse(line.Remove(0, 2).Split(' ')[0], CultureInfo.InvariantCulture.NumberFormat),
                                float.Parse(line.Remove(0, 2).Split(' ')[1], CultureInfo.InvariantCulture.NumberFormat),
                                float.Parse(line.Remove(0, 2).Split(' ')[2],
                                    CultureInfo.InvariantCulture.NumberFormat)));
                        }
                        // getting texture coords
                        else if (line.StartsWith("vt ") && currentObjectName == objectName)
                        {
                            tempUvs.Add(new Vector2(
                                float.Parse(line.Remove(0, 3).Split(' ')[0], CultureInfo.InvariantCulture.NumberFormat),
                                float.Parse(line.Remove(0, 3).Split(' ')[1],
                                    CultureInfo.InvariantCulture.NumberFormat)));
                        }
                        //getting normals
                        else if (line.StartsWith("vn ") && currentObjectName == objectName)
                        {
                            tempNormals.Add(new Vector3(
                                float.Parse(line.Remove(0, 3).Split(' ')[0], CultureInfo.InvariantCulture.NumberFormat),
                                float.Parse(line.Remove(0, 3).Split(' ')[1], CultureInfo.InvariantCulture.NumberFormat),
                                float.Parse(line.Remove(0, 3).Split(' ')[2],
                                    CultureInfo.InvariantCulture.NumberFormat)));
                        }

                        // getting indices
                        else if (line.StartsWith("f ") && currentObjectName == objectName)
                        {
                            verts.Add(tempVerts[int.Parse(line.Remove(0, 2).Split(" ")[0].Split("/")[0]) - 1].X);
                            verts.Add(tempVerts[int.Parse(line.Remove(0, 2).Split(" ")[0].Split("/")[0]) - 1].Y);
                            verts.Add(tempVerts[int.Parse(line.Remove(0, 2).Split(" ")[0].Split("/")[0]) - 1].Z);
                            verts.Add(tempVerts[int.Parse(line.Remove(0, 2).Split(" ")[1].Split("/")[0]) - 1].X);
                            verts.Add(tempVerts[int.Parse(line.Remove(0, 2).Split(" ")[1].Split("/")[0]) - 1].Y);
                            verts.Add(tempVerts[int.Parse(line.Remove(0, 2).Split(" ")[1].Split("/")[0]) - 1].Z);
                            verts.Add(tempVerts[int.Parse(line.Remove(0, 2).Split(" ")[2].Split("/")[0]) - 1].X);
                            verts.Add(tempVerts[int.Parse(line.Remove(0, 2).Split(" ")[2].Split("/")[0]) - 1].Y);
                            verts.Add(tempVerts[int.Parse(line.Remove(0, 2).Split(" ")[2].Split("/")[0]) - 1].Z);

                            uvs.Add(tempUvs[int.Parse(line.Remove(0, 2).Split(" ")[0].Split("/")[1]) - 1].X);
                            uvs.Add(tempUvs[int.Parse(line.Remove(0, 2).Split(" ")[0].Split("/")[1]) - 1].Y);
                            uvs.Add(tempUvs[int.Parse(line.Remove(0, 2).Split(" ")[1].Split("/")[1]) - 1].X);
                            uvs.Add(tempUvs[int.Parse(line.Remove(0, 2).Split(" ")[1].Split("/")[1]) - 1].Y);
                            uvs.Add(tempUvs[int.Parse(line.Remove(0, 2).Split(" ")[2].Split("/")[1]) - 1].X);
                            uvs.Add(tempUvs[int.Parse(line.Remove(0, 2).Split(" ")[2].Split("/")[1]) - 1].Y);

                            normals.Add(tempNormals[int.Parse(line.Remove(0, 2).Split(" ")[0].Split("/")[2]) - 1].X);
                            normals.Add(tempNormals[int.Parse(line.Remove(0, 2).Split(" ")[0].Split("/")[2]) - 1].Y);
                            normals.Add(tempNormals[int.Parse(line.Remove(0, 2).Split(" ")[0].Split("/")[2]) - 1].Z);
                            normals.Add(tempNormals[int.Parse(line.Remove(0, 2).Split(" ")[1].Split("/")[2]) - 1].X);
                            normals.Add(tempNormals[int.Parse(line.Remove(0, 2).Split(" ")[1].Split("/")[2]) - 1].Y);
                            normals.Add(tempNormals[int.Parse(line.Remove(0, 2).Split(" ")[1].Split("/")[2]) - 1].Z);
                            normals.Add(tempNormals[int.Parse(line.Remove(0, 2).Split(" ")[2].Split("/")[2]) - 1].X);
                            normals.Add(tempNormals[int.Parse(line.Remove(0, 2).Split(" ")[2].Split("/")[2]) - 1].Y);
                            normals.Add(tempNormals[int.Parse(line.Remove(0, 2).Split(" ")[2].Split("/")[2]) - 1].Z);
                        }
                    }

                    Frames.Add(new MeshData(verts.ToArray(), normals.ToArray(), uvs.ToArray()));
                    
                    reader.Close();
                }
            }

            AssetShortName = assetShortName;
            if (saveInAssetsList)
            {
                Console.WriteLine("Saving asset in assets list! AssetShortName: " + AssetShortName);
                AssetManager.Assets.Add(AssetShortName, this);
            }

            _texture = texture;
            _shader = new(vertShader, fragShader);
            Render.Render.window.UpdateFrame += UpdateFrame;
            Render.Render.window.RenderFrame += RenderFrame;
        }

        void UpdateFrame(OpenTK.Windowing.Common.FrameEventArgs args)
        {
        }
        
        void RenderFrame(OpenTK.Windowing.Common.FrameEventArgs args)
        {
            //Console.WriteLine("asset");
            
            for (int i = 0; i < Positions.Count; i++)
            {
                _shader.SetMatrix4("modelMatrix[" + i + "]", Positions[i]);
            }
            
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _verts.Length * sizeof(float), _verts, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _uvbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _uvs.Length * sizeof(float), _uvs, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _nbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _normals.Length * sizeof(float), _normals, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(2);
            
            _shader.SetInt("texture0", 2);
            _shader.SetVector3("lightColor", Render.Render.SunColor);
            _shader.SetVector3("lightDirection", Render.Render.SunDirection);
            _shader.SetMatrix4("view", Render.Render.camera.GetViewMatrix());
            _shader.SetMatrix4("projection", Render.Render.camera.GetProjectionMatrix());
            
            _texture.Use();
            _shader.Use();
            Console.WriteLine(Positions.Count);
            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, _verts.Length / 3, Positions.Count - 1);
        }

        public BEPUutilities.Vector3[] GetBepuVerts()
        {
            float[] verts = Frames[0].Vertices;
            List<BEPUutilities.Vector3> bepuVertices = new();

            for (int i = 0; i < verts.Length; i += 3)
            {
                bepuVertices.Add(new(verts[i], verts[i + 1], verts[i + 2]));
            }

            return bepuVertices.ToArray();
        }

        public int[] GetBepuIndices()
        {
            float[] verts = Frames[0].Vertices;
            List<int> inds = new();

            for (int i = 0; i < verts.Length / 3; i++)
            {
                inds.Add(i);
            }

            return inds.ToArray();
        }

        public void NewFrame()
        {
            Positions = new List<Matrix4>();
        }
    }
}