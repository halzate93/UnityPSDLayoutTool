﻿namespace PhotoshopFile
{
    using System.IO;

    /// <summary>
    /// The channel data for a layer
    /// </summary>
    public class Channel
    {
        internal Channel(BinaryReverseReader reader, Layer layer)
        {
            ID = reader.ReadInt16();
            Length = reader.ReadInt32();
            Layer = layer;
        }

        /// <summary>
        /// The length of the compressed channel data.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// The layer to which this channel belongs
        /// </summary>
        private Layer Layer { get; set; }

        /// <summary>
        /// 0 = red, 1 = green, etc.
        /// –1 = transparency mask
        /// –2 = user supplied layer mask
        /// </summary>
        public short ID { get; private set; }

        /// <summary>
        /// The compressed raw channel data
        /// </summary>
        public byte[] Data { private get; set; }

        /// <summary>
        /// The raw image data from the channel.
        /// </summary>
        public byte[] ImageData { get; set; }

        /// <summary>
        /// The compression method of the image
        /// </summary>
        public ImageCompression ImageCompression { get; set; }

        /// <summary>
        /// Gets a BinaryReverseReader setup to read the Channel data contained within this channel.
        /// </summary>
        public BinaryReverseReader DataReader
        {
            get
            {
                if (Data == null)
                {
                    return null;
                }

                return new BinaryReverseReader(new MemoryStream(Data));
            }
        }

        internal void LoadPixelData(BinaryReverseReader reader)
        {
            Data = reader.ReadBytes(Length);
            using (BinaryReverseReader dataReader = DataReader)
            {
                ImageCompression = (ImageCompression)dataReader.ReadInt16();
                int columns = 0;
                switch (Layer.PsdFile.Depth)
                {
                    case 1:
                        columns = Layer.Rect.Width;
                        break;
                    case 8:
                        columns = Layer.Rect.Width;
                        break;
                    case 16:
                        columns = Layer.Rect.Width * 2;
                        break;
                }
                ImageData = new byte[Layer.Rect.Height * columns];
                switch (ImageCompression)
                {
                    case ImageCompression.Raw:
                        dataReader.Read(ImageData, 0, ImageData.Length);
                        break;
                    case ImageCompression.Rle:
                        int[] nums = new int[Layer.Rect.Height];

                        for (int i = 0; i < Layer.Rect.Height; i++)
                        {
                            nums[i] = dataReader.ReadInt16();
                        }

                        for (int index = 0; index < Layer.Rect.Height; ++index)
                        {
                            int startIdx = index * Layer.Rect.Width;
                            RleHelper.DecodedRow(dataReader.BaseStream, ImageData, startIdx, columns);
                        }
                        break;
                }
            }
        }
    }
}
