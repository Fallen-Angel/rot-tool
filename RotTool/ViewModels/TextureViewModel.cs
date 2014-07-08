using Homeworld2.ROT;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RotTool.ViewModels
{
    public class TextureViewModel
    {
        private readonly ROT _texture;
        private BitmapSource _bitmap;
        public BitmapSource Bitmap
        {
            get
            {
                if (_bitmap == null)
                {
                    _bitmap = BitmapSource.Create(
                        _texture.Width,
                        _texture.Height,
                        96,
                        96,
                        PixelFormats.Bgra32,
                        null,
                        SwapBR(_texture.Mipmaps[0].Decompress()),
                        _texture.Width * 4);

                    var scale = new ScaleTransform(1, -1);
                    _bitmap = new TransformedBitmap(_bitmap, scale);
                }
                return _bitmap;
            }
        }

        public TextureViewModel(ROT texture)
        {
            _texture = texture;
        }

        public void GenerateMipmaps(BitmapSource bitmap)
        {
            bitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);

            _texture.Mipmaps.Clear();

            if (_texture.Format != Format.RGBA32)
            {
                AddMipmap(bitmap);
            }
            else
            {
                int mipmapCount = (int)Math.Log(Math.Max(_texture.Width, _texture.Height), 2);

                for (int level = 0; level <= mipmapCount; ++level)
                {
                    double scaleFactor = 1 / Math.Pow(2, level);

                    var scale = new ScaleTransform(scaleFactor, scaleFactor);
                    var bmp = new TransformedBitmap(bitmap, scale);
                    AddMipmap(bmp, level);
                }
            }
        }



        private void AddMipmap(BitmapSource bitmap, int level = 0)
        {
            var imageData = new byte[bitmap.PixelHeight * bitmap.PixelWidth * 4];

            var scale = new ScaleTransform(1, -1);
            bitmap = new TransformedBitmap(bitmap, scale);
            bitmap.CopyPixels(imageData, bitmap.PixelWidth * 4, 0);

            var mipmap = new Mipmap(bitmap.PixelWidth, bitmap.PixelHeight, _texture.Format, SwapBR(imageData));
            _texture.Mipmaps.Add(mipmap);
        }

        public void Write(Stream stream)
        {
            _texture.Write(stream);
        }

        private static byte[] SwapBR(byte[] data)
        {
            data = (byte[])data.Clone();
            for (int i = 0; i < data.Length / 4; ++i)
            {
                var b = data[i * 4];
                var r = data[i * 4 + 2];
                data[i * 4] = r;
                data[i * 4 + 2] = b;
            }
            return data;
        }
    }
}