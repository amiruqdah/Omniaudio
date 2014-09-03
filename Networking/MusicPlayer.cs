using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;

namespace Omniaudio.Networking
{
    delegate void NetworkedEvent(byte[] audioData);

    class MusicPlayer
    {
        public event NetworkedEvent SendCompressedAudioChunk;
        private Mp3FileReader mp3;
        private int averageBytesPerSecond;
        private byte[] audioData;
       
        public MusicPlayer(string path)
        {
            mp3 = new Mp3FileReader(path);
            audioData = new byte[mp3.Length];
            averageBytesPerSecond = mp3.WaveFormat.AverageBytesPerSecond;
            try
            {
                mp3.Read(audioData, 0, averageBytesPerSecond * 4);
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }
        // 1 chunk = 1 sec of music
        public void SendAudioChunks(int chunkCount)
        {
            int count = averageBytesPerSecond * chunkCount;
            if (count > audioData.Length)
            {
                count = count + (audioData.Length - count);
            }
            byte[] audioChunk = new byte[count];
            Array.Copy(audioData, 0, audioChunk, 0, count);
            SendCompressedAudioChunk(audioChunk);


        }
        // Do I even compresss this??? LOLOLOL
        /// </summary>
        private byte[] Compress(byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }
    }
}
