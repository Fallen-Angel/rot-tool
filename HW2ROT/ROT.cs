using Homeworld2.IFF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        public ROT()
        {
            Mipmaps = new List<Mipmap>();
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Format Format { get; set; }

        public int MipmapsCount { get; private set; }

        public List<Mipmap> Mipmaps { get; private set; }

        public BitmapSource Bitmap
        {
            get { return Mipmaps[0].Bitmap; }
        }

        public void GenerateMipmaps(BitmapSource bitmap)
        {
            Width = bitmap.PixelWidth;
            Height = bitmap.PixelHeight;

            Mipmaps.Clear();

            if (Format != Format.RGBA32)
            {
                var mipmap = new Mipmap();
                mipmap.SetBitmap(bitmap, Format);
                Mipmaps.Add(mipmap);
            }
            else
            {
                int log = (int)Math.Log(Math.Max(Width, Height), 2);

                for (int i = 0; i <= log; ++i)
                {
                    double factor = 1 / Math.Pow(2, i);

                    var mipmap = new Mipmap();
                    var scale = new ScaleTransform(factor, factor);
                    var bmp = new TransformedBitmap(bitmap, scale);
                    mipmap.SetBitmap(bmp, Format);
                    mipmap.Level = i;
                    Mipmaps.Add(mipmap);
                }
            }

            MipmapsCount = Mipmaps.Count;
        }

        private void ReadHEADChunk(IFFReader iff, ChunkAttributes attr)
        {
            Width = iff.ReadInt32();
            Height = iff.ReadInt32();
            Format = (Format)iff.ReadUInt32();
            MipmapsCount = iff.ReadInt32();
        }

        private void ReadMIPSChunk(IFFReader iff, ChunkAttributes attr)
        {
            iff.AddHandler(Chunks.MipmapLevel, ChunkType.Form, ReadMLVLChunk);

            iff.Parse();
        }

        private void ReadMLVLChunk(IFFReader iff, ChunkAttributes attr)
        {
            Mipmaps.Add(Mipmap.Read(iff, Format));
        }

        public static ROT Read(Stream stream)
        {
            var rot = new ROT();

            var iff = new IFFReader(stream);
            iff.AddHandler(Chunks.Header, ChunkType.Form, rot.ReadHEADChunk);
            iff.AddHandler(Chunks.Mipmaps, ChunkType.Form, rot.ReadMIPSChunk);
            iff.Parse();

            return rot;
        }

        public void Write(Stream stream)
        {
            var iff = new IFFWriter(stream);
            
            iff.Push(Chunks.Header, ChunkType.Form);
            iff.Write(Width);
            iff.Write(Height);
            iff.Write((uint)Format);
            iff.Write(MipmapsCount);
            iff.Pop();

            iff.Push(Chunks.Mipmaps, ChunkType.Form);
            foreach (var mipmap in Mipmaps)
            {
                mipmap.Write(iff);
            }
            iff.Pop();
        }
    }
}
