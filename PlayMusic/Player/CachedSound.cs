using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayMusic.Player
{
    class CachedSound
    {
        public float[] AudioData { get; private set; }

        public WaveFormat WaveFormat { get; private set; }

        public CachedSound(string audioFileName)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {

                int outRate = 44100;
                var resampler = new WdlResamplingSampleProvider(audioFileReader, outRate);
                
                WaveFormat = resampler.WaveFormat;
                var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                var readBuffer = new float[WaveFormat.SampleRate * WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = resampler.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }
                AudioData = wholeFile.ToArray();
            }
        }
    }
}
