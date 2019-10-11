using Homeworld2.IFF;
using System.Collections.Generic;
using System.IO;

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
        public int Width { get; private set; }

        public int Height { get; private set; }

        public Format Format { get; private set; }

        public int MipmapsCount { get; private set; }

        public List<Mipmap> Mipmaps { get; private set; }

        private ROT()
        {
            Mipmaps = new List<Mipmap>();
        }

        public ROT(int width, int height, Format format) : this()
        {
            Width = width;
            Height = height;
            Format = format;
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

            using (var iff = new IFFReader(stream))
            {
                iff.AddHandler(Chunks.Header, ChunkType.Form, rot.ReadHEADChunk);
                iff.AddHandler(Chunks.Mipmaps, ChunkType.Form, rot.ReadMIPSChunk);
                iff.Parse();
            }

            return rot;
        }

        public void Write(Stream stream)
        {
            using var iff = new IFFWriter(stream);

            iff.Push(Chunks.Header, ChunkType.Form);
            iff.Write(Width);
            iff.Write(Height);
            iff.Write((uint)Format);
            iff.Write(Mipmaps.Count);
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
