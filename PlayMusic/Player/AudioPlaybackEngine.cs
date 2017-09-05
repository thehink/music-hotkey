using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;

namespace PlayMusic.Player
{
    class AudioPlaybackEngine : IDisposable
    {
        private WaveOutEvent outputDevice;
        private MixingSampleProvider mixer;

        private Dictionary<string, CachedSound> soundCache = new Dictionary<string, CachedSound>();

        public delegate void PlaybackHandler(object sender, PlaybackEventArgs e);
        public event PlaybackHandler PlaybackStarted;
        public event PlaybackHandler PlaybackEnded;

        public int MixerInputsCount { get; private set; } = 0;

        public bool Playing { get; private set; }

        public int DeviceNumber { get; set; } = 0;

        public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
        {
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            mixer.ReadFully = true;

            mixer.MixerInputEnded += OnMixerInputEnded;

            InitOutputDevice();
        }

        public void InitOutputDevice()
        {
            outputDevice = new WaveOutEvent();
            outputDevice.DesiredLatency = 30;
            outputDevice.NumberOfBuffers = 12;
            outputDevice.DeviceNumber = DeviceNumber;
            outputDevice.Init(mixer);
            outputDevice.Play();
        }

        public void ChangeDeviceId(int id)
        {
            DeviceNumber = id;
            Dispose();
            InitOutputDevice();
        }

        private void OnMixerInputEnded(object sender, SampleProviderEventArgs e)
        {
            MixerInputsCount--;
            CheckPlayingState();
        }

        private void CheckPlayingState()
        {
            if (MixerInputsCount == 0)
            {
                Playing = false;
                PlaybackEnded(this, new PlaybackEventArgs());
            }
        }

        public void StopSounds()
        {
            mixer.RemoveAllMixerInputs();
            MixerInputsCount = 0;
            CheckPlayingState();
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        public void PlayCachedSound(string fileName)
        {
            CachedSound cachedSound;
            if (!soundCache.ContainsKey(fileName))
            {
                cachedSound = new CachedSound(fileName);
                soundCache.Add(fileName, cachedSound);
            }
            else
            {
                cachedSound = soundCache[fileName];
            }

            PlaySound(cachedSound);
        }

        public void PlaySound(CachedSound sound)
        {
            AddMixerInput(new CachedSoundSampleProvider(sound));
        }

        public void PlaySound(string fileName)
        {
            var input = new AudioFileReader(fileName);
            AddMixerInput(new AutoDisposeFileReader(input));
        }

        private void AddMixerInput(ISampleProvider input)
        {
            if (!Playing)
            {
                Playing = true;
                PlaybackStarted(this, new PlaybackEventArgs());
            }

            MixerInputsCount++;
            mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        public void Dispose()
        {
            outputDevice.Dispose();
        }

        public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine(44100, 2);
    }
}