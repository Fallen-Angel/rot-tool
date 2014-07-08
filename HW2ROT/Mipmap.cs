using Homeworld2.IFF;
using ManagedSquish;

namespace Homeworld2.ROT
{
    public class Mipmap
    {
        private readonly Format _format;

        public int Level { get; set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int DataSize { get; private set; }
        public byte[] Data { get; private set; }

        private Mipmap(Format format)
        {
            _format = format;
        }

        /// <summary>
        /// Initializes a new instance of the Mipmap class that has the specified dimensions, format and image data.
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="format">Data format</param>
        /// <param name="data">Source RGBA32 image data</param>
        public Mipmap(int width, int height, Format format, byte[] data) : this(format)
        {
            Width = width;
            Height = height;

            if (format == Format.RGBA32)
            {
                Data = data;
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

                Data = Squish.CompressImage(data, Width, Height, flags);
            }

            DataSize = Data.Length;
        }

        public static Mipmap Read(IFFReader iff, Format format)
        {
            var mipmap = new Mipmap(format)
            {
                Level = iff.ReadInt32(),
                Width = iff.ReadInt32(),
                Height = iff.ReadInt32(),
                DataSize = iff.ReadInt32()
            };

            mipmap.Data = iff.ReadBytes(mipmap.DataSize);

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

        public byte[] Decompress()
        {
            if (_format == Format.RGBA32)
            {
                return Data;
            }

            var flags = SquishFlags.Dxt5;

            switch (_format)
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

            return Squish.DecompressImage(Data, Width, Height, flags);
        }
    }
}
