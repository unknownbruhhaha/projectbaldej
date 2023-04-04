using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using System.Runtime.CompilerServices;

namespace BaldejFramework.Assets
{
    //this class is used to load textures from hard drive and use them
    public class TextureAsset : Asset
    {
        public int Handle;
        public string AssetType { get => "TextureAsset"; }
        public string AssetShortName { get; set; }

        public TextureAsset (string path, string shortName = "", bool saveInAssetsList = false)
        {
            Image<Rgba32> img;
            img = Image.Load<Rgba32>(AssetManager.GetAssetFullPath(path));
            img.Mutate(x => x.Flip(FlipMode.Vertical));

            int handle = GL.GenTexture();

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            // Generate handle
            Handle = GL.GenTexture();

            // Bind the handle
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            byte[] pixels = new byte[img.Width * img.Height * Unsafe.SizeOf<Rgba32>()];
            img.CopyPixelDataTo(pixels);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                img.Width, img.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                pixels.ToArray()
            );

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            Handle = handle;
            
            AssetShortName = shortName;
            
            if (saveInAssetsList)
            {
                Console.WriteLine("Saving asset in assets list! AssetShortName: " + AssetShortName);
                AssetManager.Assets.Add(AssetShortName, this);
            }
        }

        public void Use()
        {
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
