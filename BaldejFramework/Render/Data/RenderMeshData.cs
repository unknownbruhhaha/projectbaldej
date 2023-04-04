namespace BaldejFramework.Render
{
    public class RenderMeshData
    {
        public float[] vertices;
        public uint[] indices;
        public Shader shader;
        public float[] textureCoordinates;

        public RenderMeshData(float[] verts, uint[] ind, Shader sh, float[] texCoords)
        {
            vertices = verts;
            indices = ind;
            shader = sh;
            textureCoordinates = texCoords;
        }
    }
}
