using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Homeworld2.IFF;
using System.IO;
using System.Windows.Media.Imaging;
using ManagedSquish;
using System.Windows.Media;

namespace Homeworld2.ROT
{
    public enum Format
    {
        RGBA32 = 1024,
        DXT1 = 1028, 
        DXT3 = 1029,
        DXT5 = 1030
    }

    public class ROT
    {
        private int width;
        private int height;
        private Format format;
        private int mipmapsCount;

        private List<Mipmap> mipmaps = new List<Mipmap>();
        private BitmapSource tempBitmap;

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public Format Format
        {
            get { return format; }
            set { format = value; }
        }

        public int MipmapsCount
        {
            get { return mipmapsCount; }
        }

        public List<Mipmap> Mipmaps
        {
            get { return mipmaps; }
        }

        public BitmapSource Bitmap
        {
            get { return mipmaps[0].Bitmap; }
        }

        public void GenerateMipmaps(BitmapSource bitmap)
        {
            width = bitmap.PixelWidth;
            height = bitmap.PixelHeight;

            mipmaps.Clear();

            if (format != Format.RGBA32)
            {
                Mipmap mipmap = new Mipmap();
                mipmap.SetBitmap(bitmap, format);
                mipmaps.Add(mipmap);
            }
            else
            {
                int log = (int)Math.Log(Math.Max(width, height), 2);

                TransformedBitmap bmp;
                ScaleTransform scale;
                for (int i = 0; i <= log; ++i)
                {
                    double factor = 1 / Math.Pow(2, i);

                    Mipmap mipmap = new Mipmap();
                    scale = new ScaleTransform(factor, factor);
                    bmp = new TransformedBitmap(bitmap, scale);
                    mipmap.SetBitmap(bmp, format);
                    mipmap.Level = i;
                    mipmaps.Add(mipmap);
                }
            }

            mipmapsCount = mipmaps.Count;
        }

        private void ReadHEADChunk(IFFReader iff, ChunkAttributes attr)
        {
            width = iff.ReadInt32();
            height = iff.ReadInt32();
            format = (Format)iff.ReadUInt32();
            mipmapsCount = iff.ReadInt32();
        }

        private void ReadMIPSChunk(IFFReader iff, ChunkAttributes attr)
        {
            iff.AddHandler("MLVL", ChunkType.Form, ReadMLVLChunk);

            iff.Parse();
        }

        private void ReadMLVLChunk(IFFReader iff, ChunkAttributes attr)
        {
            Mipmap mipmap = new Mipmap();
            mipmaps.Add(mipmap);
            mipmap.Read(iff, format);
        }

        public void Read(Stream stream)
        {
            mipmaps.Clear();

            IFFReader iff = new IFFReader(stream);
            iff.AddHandler("HEAD", ChunkType.Form, ReadHEADChunk);
            iff.AddHandler("MIPS", ChunkType.Form, ReadMIPSChunk);
            iff.Parse();
        }

        public void Write(Stream stream)
        {
            IFFWriter iff = new IFFWriter(stream);
            
            iff.Push("HEAD", ChunkType.Form);
            iff.Write(width);
            iff.Write(height);
            iff.Write((uint)format);
            iff.Write(mipmapsCount);
            iff.Pop();

            iff.Push("MIPS", ChunkType.Form);
            for (int i = 0; i < mipmaps.Count; ++i)
            {
                iff.Push("MLVL", ChunkType.Form);
                mipmaps[i].Write(iff);
                iff.Pop();
            }
            iff.Pop();
        }
    }
}
