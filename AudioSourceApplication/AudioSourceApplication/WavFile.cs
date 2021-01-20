using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSourceApplication
{
    public struct WaveFormat
    {
        public int RIFF;
        public int fileSize;
        public int waveChunk;
        public int formatChunk;
        public int formatSize;
        public short formatID;
        public short numberOfChannels;
        public int samplingRate;
        public int bytePerSec;
        public short bytePerSample;
        public short bitRate;
        public int dataChunk;
        public int dataSize;
    }

    public class WavFile : IDisposable
    {
        List<float[]> channels;

        WaveFormat format;

        /// <summary>
        /// トータルのサンプル数
        /// </summary>
        int howlong;

        public WavFile(BinaryReader br)
        {
            byte[] buffer = new byte[4];
            format = new WaveFormat();

            format.RIFF = br.ReadInt32(); // RIFF
            format.fileSize = br.ReadInt32(); // ファイルサイズ
            format.waveChunk = br.ReadInt32(); // WAVE
            format.formatChunk = br.ReadInt32(); // fmt
            format.formatSize = br.ReadInt32(); // フォーマット定義サイズ
            format.formatID = br.ReadInt16();
            format.numberOfChannels = br.ReadInt16();
            format.samplingRate = br.ReadInt32();
            format.bytePerSec = br.ReadInt32();
            format.bytePerSample = br.ReadInt16();
            format.bitRate = br.ReadInt16();
            format.dataChunk = br.ReadInt32();
            format.dataSize = br.ReadInt32();

            // チャンネルの簡単な初期化
            channels = new List<float[]>();
            howlong = format.dataSize / format.numberOfChannels / (format.bitRate / 8);
            for (short i = 0; i < format.numberOfChannels; ++i)
            {
                channels.Add(new float[howlong]);
            }

            if (format.formatID != 0x1)
                throw new NotImplementedException("PCM Only");

            switch (format.bitRate)
            {
                case 8:
                    for (int i = 0; i < howlong; ++i)
                        for (short ID = 0; ID < format.numberOfChannels; ++ID)
                            channels[ID][i] = (float)br.ReadByte() - 128f;
                    break;
                case 16:
                    for (int i = 0; i < howlong; ++i)
                        for (short ID = 0; ID < format.numberOfChannels; ++ID)
                            channels[ID][i] = (float)br.ReadInt16();
                    break;
                case 24:
                    for (int i = 0; i < howlong; ++i)
                        for (short ID = 0; ID < format.numberOfChannels; ++ID)
                        {
                            var bit = br.ReadBytes(3);
                            channels[ID][i] = (float)BitConverter.ToInt32(bit, 0);
                        }
                    break;
                case 32:
                    for (int i = 0; i < howlong; ++i)
                        for (short ID = 0; ID < format.numberOfChannels; ++ID)
                            channels[ID][i] = br.ReadSingle();
                    break;
            }
        }

        public void Dispose()
        {
            
        }
    }
}
