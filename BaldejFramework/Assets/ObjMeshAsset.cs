using BaldejFramework.Render;
using OpenTK.Mathematics;
using System.Globalization;

namespace BaldejFramework.Assets
{
    public class ObjMeshAsset : Asset
    {
        public List<MeshData> Frames { get; set; }
        public string AssetType { get => "MeshAsset"; }
        public string AssetShortName { get; set; }

        public ObjMeshAsset(string path, string assetShortName, string objectName, int firstFileIndex, int lastFileIndex, bool saveInAssetsList = false)
        {
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
                            tempVerts.Add(new Vector3(float.Parse(line.Remove(0, 2).Split(' ')[0], CultureInfo.InvariantCulture.NumberFormat), 
                                float.Parse(line.Remove(0, 2).Split(' ')[1], CultureInfo.InvariantCulture.NumberFormat),
                                float.Parse(line.Remove(0, 2).Split(' ')[2], CultureInfo.InvariantCulture.NumberFormat)));
                        }
                        // getting texture coords
                        else if (line.StartsWith("vt ") && currentObjectName == objectName)
                        {
                            tempUvs.Add(new Vector2(float.Parse(line.Remove(0, 3).Split(' ')[0], CultureInfo.InvariantCulture.NumberFormat),
                                float.Parse(line.Remove(0, 3).Split(' ')[1], CultureInfo.InvariantCulture.NumberFormat)));
                        }
                        //getting normals
                        else if (line.StartsWith("vn ") && currentObjectName == objectName)
                        {
                            tempNormals.Add(new Vector3(float.Parse(line.Remove(0, 3).Split(' ')[0], CultureInfo.InvariantCulture.NumberFormat),
                                float.Parse(line.Remove(0, 3).Split(' ')[1], CultureInfo.InvariantCulture.NumberFormat),
                                float.Parse(line.Remove(0, 3).Split(' ')[2], CultureInfo.InvariantCulture.NumberFormat)));
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

                    AssetShortName = assetShortName;
                    if (saveInAssetsList)
                    {
                        Console.WriteLine("Saving asset in assets list! AssetShortName: " + AssetShortName);
                        AssetManager.Assets.Add(AssetShortName, this);
                    }
                    reader.Close();
                }
            }
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
    }
}
/* 
  * BOGDD#3542: 
  * я здесь был
  * 17.08.2022 
*/