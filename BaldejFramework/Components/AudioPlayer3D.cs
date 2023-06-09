﻿using CSCore;
using CSCore.Codecs;
using CSCore.Utils;
using CSCore.XAudio2;
using CSCore.XAudio2.X3DAudio;

using BaldejFramework.Assets;

namespace BaldejFramework.Components
{
    public class AudioPlayer3D : Component
    {
        public string componentID => "AudioPlayer";
        public GameObject? owner { get; set; }

        private bool _isPlaying;
        private IWaveSource _waveSource;
        private readonly XAudio2 _xaudio2 = XAudio2.CreateXAudio2();
        private XAudio2MasteringVoice _masteringVoice;
        private StreamingSourceVoice _streamingSourceVoice;
        private X3DAudioCore _x3daudio;

        private int _sourceChannels, _destinationChannels;

        private Emitter _emitter;

        public void Open(string filename)
        {
            Stop();

            _waveSource = CodecFactory.Instance.GetCodec(Path.Combine(AssetManager.AssetsPath, filename)).ToMono();
            _masteringVoice = _xaudio2.CreateMasteringVoice(XAudio2.DefaultChannels, XAudio2.DefaultSampleRate);
            _streamingSourceVoice = new StreamingSourceVoice(_xaudio2, _waveSource, 150);

            object defaultDevice = _xaudio2.DefaultDevice;
            ChannelMask channelMask;
            if (_xaudio2.Version == XAudio2Version.XAudio2_7)
            {
                var xaudio27 = (XAudio2_7)_xaudio2;
                var deviceDetails = xaudio27.GetDeviceDetails((int)defaultDevice);
                channelMask = deviceDetails.OutputFormat.ChannelMask;
                _destinationChannels = deviceDetails.OutputFormat.Channels;
            }
            else
            {
                channelMask = _masteringVoice.ChannelMask;
                _destinationChannels = _masteringVoice.VoiceDetails.InputChannels;
            }
            _sourceChannels = _waveSource.WaveFormat.Channels;

            _x3daudio = new X3DAudioCore(channelMask);

            _emitter = new Emitter()
            {
                ChannelCount = _sourceChannels,
                CurveDistanceScaler = float.MinValue,
                OrientFront = new Vector3(0, 0, 1),
                OrientTop = new Vector3(0, 1, 0),
                Position = new Vector3(0, 0, 0),
                Velocity = new Vector3(0, 0, 0)
            };


            StreamingSourceVoiceListener.Default.Add(_streamingSourceVoice);
            _streamingSourceVoice.Start();

            _isPlaying = true;
        }

        public void Stop()
        {
            if (_isPlaying)
            {
                StreamingSourceVoiceListener.Default.Remove(_streamingSourceVoice);
                _streamingSourceVoice.Stop();
                _streamingSourceVoice.Dispose();
                _masteringVoice.Dispose();
                _waveSource.Dispose();

                _isPlaying = false;
            }
        }

        public Vector3 EmitterPosition
        {
            get { return _emitter == null ? new Vector3(0) : _emitter.Position; }
            set { SetEmitterPosition(value); }
        }

        private void SetEmitterPosition(Vector3 vector3)
        {
            DspSettings dspSettings = new DspSettings(_sourceChannels, _destinationChannels);

            _emitter.Position = vector3;

            //just calculate the Matrix4x4... no doppler or anything else.
            _x3daudio.X3DAudioCalculate(AudioManager.listener, _emitter, CalculateFlags.Matrix, dspSettings);

            _streamingSourceVoice.SetOutputMatrix(_masteringVoice, _sourceChannels, _destinationChannels, dspSettings.MatrixCoefficients);
        }

        public void OnRender() { }
        public void OnUpdate() 
        {
            Transform tr = (Transform)owner.GetComponent("Transform");

            SetEmitterPosition(new Vector3(tr.Position.X, tr.Position.Y, tr.Position.Z));
        }
    }
}
