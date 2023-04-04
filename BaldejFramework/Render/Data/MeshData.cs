namespace BaldejFramework.Render
{
    public class MeshData
    {
        public float[] Vertices;
        public float[] Normals;
        public float[] UVs;

        public MeshData(float[] verts, float[] normals, float[] uvs)
        {
            Vertices = verts;
            Normals = normals;
            UVs = uvs;
        }
    }
}
