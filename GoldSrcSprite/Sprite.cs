using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace GoldSrc.Sprite
{
    public class GoldSrcSprite
    {
        /// <summary>
        /// How the sprite orientation should be
        /// </summary>
        public GoldSrcSpriteType Type { get; set; }
        /// <summary>
        /// How the sprite should be rendered
        /// </summary>
        public GoldSrcSpriteTextureFormat TextureFormat { get; set; }
        public float BoundingRadius { get; set; }
        public float BeamLength { get; set; }
        public GoldSrcSpriteSynchronization Synchronization { get; set; }
        public Color[] Palette { get; set; }
        public List<GoldSrcSpriteFrame> Frames { get; private set; } = new List<GoldSrcSpriteFrame>();

        /// <summary>
        /// Gets the width of the largest frame in this sprite
        /// </summary>
        public int MaxWidth
        {
            get
            {
                int maxWidth = 0;
                foreach (var frame in Frames)
                    if (frame.Width > maxWidth)
                        maxWidth = frame.Width;
                return maxWidth;
            }
        }

        /// <summary>
        /// Gets the height of the largest frame in this sprite
        /// </summary>
        public int MaxHeight
        {
            get
            {
                int maxHeight = 0;
                foreach (var frame in Frames)
                    if (frame.Height > maxHeight)
                        maxHeight = frame.Height;
                return maxHeight;

            }
        }

        /// <summary>
        /// Loads a sprite from a file
        /// </summary>
        /// <param name="path">Sprite file path</param>
        /// <returns>Sprite</returns>
        public static GoldSrcSprite FromFile(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return FromStream(fs);
        }

        /// <summary>
        /// Loads a sprite from a stream
        /// </summary>
        /// <param name="stream">Sprite stream</param>
        /// <returns>Sprite</returns>
        public static GoldSrcSprite FromStream(Stream stream)
        {
            var sprite = new GoldSrcSprite();

            using (BinaryReader reader = new BinaryReader(stream))
            {
                var fileID = reader.ReadInt32();
                if (fileID != 0x50534449)
                    throw new InvalidDataException("Not a valid goldsrc sprite");

                var version = reader.ReadInt32();
                if (version != 2)
                    throw new InvalidDataException("Only version 2 sprites are supported");

                sprite.Type = (GoldSrcSpriteType)reader.ReadInt32();
                sprite.TextureFormat = (GoldSrcSpriteTextureFormat)reader.ReadInt32();
                sprite.BoundingRadius = reader.ReadSingle();


                var maxFrameWidth = reader.ReadInt32();
                var maxFrameHeight = reader.ReadInt32();
                var numberOfFrames = reader.ReadInt32();
                sprite.BeamLength = reader.ReadSingle();
                sprite.Synchronization = (GoldSrcSpriteSynchronization)reader.ReadInt32();

                // Color Palette
                var sizeOfPalette = reader.ReadInt16();
                sprite.Palette = new Color[sizeOfPalette];
                for (var i = 0; i < sizeOfPalette; i++)
                    sprite.Palette[i] = Color.FromArgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

                // Frames
                for (var i = 0; i < numberOfFrames; i++)
                {
                    var group = (int)reader.ReadInt32();
                    var originX = reader.ReadInt32();
                    var originY = reader.ReadInt32();
                    var width = (int)reader.ReadInt32();
                    var height = (int)reader.ReadInt32();
                    var data = reader.ReadBytes(width * height);

                    var frame = new GoldSrcSpriteFrame(sprite, width, height)
                    {
                        Group = group,
                        OriginX = originX,
                        OriginY = originY
                    };

                    // Copy frame data
                    data.CopyTo(frame.Data, 0);

                    sprite.Frames.Add(frame);
                }
            }


            return sprite;
        }

        /// <summary>
        /// Recalculates the sprite BoundingRadius property. This should be called before saving a sprite if new frames with different sizes have been added.
        /// </summary>
        public void RecalculateBoundingRadius()
        {
            BoundingRadius = (float)Math.Sqrt((MaxWidth >> 1) * (MaxWidth >> 1) + (MaxHeight >> 1) * (MaxHeight >> 1));
        }

        /// <summary>
        /// Saves the sprite to a .spr file
        /// </summary>
        /// <param name="path">Target path</param>
        public void SaveToFile(string path)
        {
            using (FileStream fs = new FileStream(path, File.Exists(path) ? FileMode.Truncate : FileMode.OpenOrCreate, FileAccess.Write))
                SaveToStream(fs);
        }

        /// <summary>
        /// Saves the sprite to a stream
        /// </summary>
        /// <param name="stream"></param>
        public void SaveToStream(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(new char[] { 'I', 'D', 'S', 'P' });
                writer.Write((int)0x02);
                writer.Write((int)Type);
                writer.Write((int)TextureFormat);
                writer.Write((float)BoundingRadius);
                writer.Write((int)MaxWidth);
                writer.Write((int)MaxHeight);
                writer.Write((int)Frames.Count);
                writer.Write((float)BeamLength);
                writer.Write((int)Synchronization);

                // Write palette
                writer.Write((short)Palette.Length);
                for (var i = 0; i < Palette.Length; i++)
                {
                    var color = Palette[i];
                    writer.Write((byte)color.R);
                    writer.Write((byte)color.G);
                    writer.Write((byte)color.B);
                }

                // Write frames
                for (var i = 0; i < Frames.Count; i++)
                {
                    var frame = Frames[i];
                    writer.Write((int)frame.Group);
                    writer.Write((int)frame.OriginX);
                    writer.Write((int)frame.OriginY);
                    writer.Write((int)frame.Width);
                    writer.Write((int)frame.Height);
                    writer.Write((byte[])frame.Data);
                }
            }
        }
    }

    public class GoldSrcSpriteFrame
    {
        private GoldSrcSprite sprite;

        public GoldSrcSpriteFrame(GoldSrcSprite sprite,int width,int height)
        {
            this.sprite = sprite;

            this.Data = new byte[width * height];
            this.Width = width;
            this.Height = height;
        }

        public int Group { get; set; }
        public int OriginX { get; set; }
        public int OriginY { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        /// Image data of this frame. Each byte represents an index to the sprite palettes.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Gets a 256 color Bitmap from this frame using the sprite palette
        /// </summary>
        /// <returns>256 color Bitmap</returns>
        public Bitmap GetBitmap()
        {
            Bitmap bmp = new Bitmap(this.Width, this.Height, PixelFormat.Format8bppIndexed);

            // Copy the palette
            var palette = bmp.Palette;
            sprite.Palette.CopyTo(palette.Entries, 0);
            bmp.Palette = palette;


            // Copy frame data into the bitmap
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            var ptr = data.Scan0;
            var idx = 0;

            if (data.Stride != bmp.Width)
            {
                // Copy line by line, because of stride padding
                for (var i = 0; i < bmp.Height; i++)
                {
                    Marshal.Copy(this.Data, idx, ptr, bmp.Width);
                    ptr += Math.Abs(data.Stride);
                    idx += bmp.Width;
                }
            }
            else
            {
                // Just do a faster direct copying
                Marshal.Copy(this.Data, 0, ptr, this.Data.Length);
            }

            bmp.UnlockBits(data);

            return bmp;
        }
    }

    public enum GoldSrcSpriteType
    {
        ParallelUpright = 0,
        FacingUpright = 1,
        Parallel = 2,
        Oriented = 3,
        ParallelOriented = 4
    }

    public enum GoldSrcSpriteTextureFormat
    {
        Normal = 0,
        Additive = 1,
        IndexAlpha = 2,
        AlphaTest = 3
    }

    public enum GoldSrcSpriteSynchronization
    {
        Synchronized = 0,
        Random = 1
    }
}
