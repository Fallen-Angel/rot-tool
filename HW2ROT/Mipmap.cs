using System.Windows.Media;
using System.Windows.Media.Imaging;
using Homeworld2.IFF;
using System.IO;
using ManagedSquish;

namespace Homeworld2.ROT
{
    public class Mipmap
    {
        private int level;
        private int width;
        private int height;
        private int dataSize;
        private byte[] data;
        private BitmapSource bitmap;

        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public int DataSize
        {
            get { return dataSize; }
        }

        public BitmapSource Bitmap
        {
            get { return bitmap; }
        }

        public byte[] SwapBR(byte[] data)
        {
            byte r, b;
            data = (byte[])data.Clone();
            for (int i = 0; i < data.Length / 4; ++i)
            {
                b = data[i * 4];
                r = data[i * 4 + 2];
                data[i * 4] = r;
                data[i * 4 + 2] = b;
            }
            return data;
        }

        public void SetBitmap(BitmapSource bitmap, Format format)
        {
            width = bitmap.PixelWidth;
            height = bitmap.PixelHeight;
            dataSize = bitmap.PixelHeight * bitmap.PixelWidth * 4;
            byte[] imageData = new byte[dataSize];

            ScaleTransform scale = new ScaleTransform(1, -1);
            TransformedBitmap bmp = new TransformedBitmap(bitmap, scale);
            bmp.CopyPixels(imageData, bitmap.PixelWidth * 4, 0);

            if (format == Format.RGBA32)
            {
            }
            else
            {
                SquishFlags flags = SquishFlags.Dxt5;

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

                imageData = Squish.CompressImage(imageData, width, height, flags);
                dataSize = imageData.Length;
            }

            data = SwapBR(imageData);
        }

        public void Read(IFFReader iff, Format format)
        {
            level = iff.ReadInt32();
            width = iff.ReadInt32();
            height = iff.ReadInt32();
            dataSize = iff.ReadInt32();
            data = iff.ReadBytes(dataSize);

            byte[] imageData;
            if (format == Format.RGBA32)
            {
                imageData = SwapBR(data);
            }
            else
            {
                SquishFlags flags = SquishFlags.Dxt5;

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

                imageData = Squish.DecompressImage(data, width, height, flags);
            }

            bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, imageData, width * 4);

            ScaleTransform scale = new ScaleTransform(1, -1);
            TransformedBitmap bmp = new TransformedBitmap(bitmap, scale);
            bitmap = bmp;
        }

        public void Write(IFFWriter iff)
        {
            iff.Write(level);
            iff.Write(width);
            iff.Write(height);
            iff.Write(dataSize);
            iff.Write(data);
        }
    }
}
