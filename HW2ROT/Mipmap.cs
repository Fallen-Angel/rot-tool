using Homeworld2.IFF;
using ManagedSquish;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Homeworld2.ROT
{
    public class Mipmap
    {
        public int Level { get; set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int DataSize { get; private set; }
        public byte[] Data { get; private set; }

        public BitmapSource Bitmap { get; private set; }

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

        public void SetBitmap(BitmapSource bitmap, Format format)
        {
            Width = bitmap.PixelWidth;
            Height = bitmap.PixelHeight;
            var imageData = new byte[bitmap.PixelHeight * bitmap.PixelWidth * 4];

            var scale = new ScaleTransform(1, -1);
            var bmp = new TransformedBitmap(bitmap, scale);
            bmp.CopyPixels(imageData, bitmap.PixelWidth * 4, 0);

            if (format == Format.RGBA32)
            {
            }
            else
            {
                var flags = SquishFlags.Dxt5;

                switch (format)
                {
                    case Format.DXT1:
                        flags = SquishFlags.Dxt1;
                        break;
                    case Format.DXT3:
                        flags = SquishFlags.Dxt3;
                        break;
                    case Format.DXT5:
                        flags = SquishFlags.Dxt5;
                        break;
                }

                imageData = Squish.CompressImage(imageData, Width, Height, flags);
            }

            Data = SwapBR(imageData);
            DataSize = Data.Length;
        }

        public static Mipmap Read(IFFReader iff, Format format)
        {
            var mipmap = new Mipmap
            {
                Level = iff.ReadInt32(),
                Width = iff.ReadInt32(),
                Height = iff.ReadInt32(),
                DataSize = iff.ReadInt32()
            };

            mipmap.Data = iff.ReadBytes(mipmap.DataSize);

            byte[] imageData;
            if (format == Format.RGBA32)
            {
                imageData = mipmap.Data;
            }
            else
            {
                var flags = SquishFlags.Dxt5;

                switch (format)
                {
                    case Format.DXT1:
                        flags = SquishFlags.Dxt1;
                        break;
                    case Format.DXT3:
                        flags = SquishFlags.Dxt3;
                        break;
                    case Format.DXT5:
                        flags = SquishFlags.Dxt5;
                        break;
                }

                imageData = Squish.DecompressImage(mipmap.Data, mipmap.Width, mipmap.Height, flags);
            }

            mipmap.Bitmap = BitmapSource.Create(mipmap.Width, mipmap.Height, 96, 96, PixelFormats.Bgra32, null, SwapBR(imageData), mipmap.Width * 4);

            var scale = new ScaleTransform(1, -1);
            mipmap.Bitmap = new TransformedBitmap(mipmap.Bitmap, scale);

            return mipmap;
        }

        public void Write(IFFWriter iff)
        {
            iff.Push(Chunks.MipmapLevel, ChunkType.Form);

            iff.Write(Level);
            iff.Write(Width);
            iff.Write(Height);
            iff.Write(DataSize);
            iff.Write(Data);

            iff.Pop();
        }
    }
}
