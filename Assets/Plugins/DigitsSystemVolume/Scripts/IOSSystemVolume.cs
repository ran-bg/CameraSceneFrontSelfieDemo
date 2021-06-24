#if !UNITY_EDITOR && UNITY_IOS
using System.Runtime.InteropServices;
using UnityEngine;

namespace DigitsSystemVolume
{
	/// <summary>Class that acts as a bridge for the Native iOS System Volume</summary>
	public class IOSSystemVolume: NativeSystemVolume
	{
		private readonly AudioStreamType[] SUPPORTED_STREAM_TYPES = new AudioStreamType[]
		{
			AudioStreamType.SYSTEM, //The global volume of the system
		};

		[DllImport("__Internal")]
		private static extern void _nativeSystemVolume_initialize(string gameObjectName);
		[DllImport("__Internal")]
		private static extern string _nativeSystemVolume_getAudioOutputDevices();
		[DllImport("__Internal")]
		private static extern float _nativeSystemVolume_getSystemVolume(int audioStreamType);
		[DllImport("__Internal")]
		private static extern float _nativeSystemVolume_getDeviceVolume(string deviceID);
		[DllImport("__Internal")]
		private static extern void _nativeSystemVolume_setSystemVolume(float volume, int audioStreamType);
		[DllImport("__Internal")]
		private static extern void _nativeSystemVolume_setDeviceVolume(float volume, string deviceID);
		[DllImport("__Internal")]
		private static extern bool _nativeSystemVolume_isSystemVolumeMuted(int audioStreamType);
		[DllImport("__Internal")]
		private static extern bool _nativeSystemVolume_isDeviceVolumeMuted(string deviceID);
		[DllImport("__Internal")]
		private static extern void _nativeSystemVolume_setSystemVolumeMute(bool mute, int audioStreamType);
		[DllImport("__Internal")]
		private static extern void _nativeSystemVolume_setDeviceVolumeMute(bool mute, string deviceID);

		public override AudioStreamType[] SupportedAudioStreamTypes { get { return SUPPORTED_STREAM_TYPES; } }

		public override void Initialize()
		{
			_nativeSystemVolume_initialize(gameObject.name);
			base.Initialize();
		}

		public override void RefreshAudioOutputDevices()
		{
			string json = _nativeSystemVolume_getAudioOutputDevices();
			AudioOutputDeviceCollection audioOutputDeviceCollection = JsonUtility.FromJson<AudioOutputDeviceCollection>(json);
			audioOutputDevices = audioOutputDeviceCollection.devices;
			lastDeviceVolumes = new float[audioOutputDevices.Length];
			lastDeviceVolumeMutes = new bool[audioOutputDevices.Length];
		}

		public override float GetSystemVolume(AudioStreamType audioStreamType)
		{
			return _nativeSystemVolume_getSystemVolume((int)audioStreamType);
		}

		public override float GetDeviceVolume(string deviceID)
		{
			return _nativeSystemVolume_getDeviceVolume(deviceID);
		}

		public override bool IsSystemVolumeMuted(AudioStreamType audioStreamType)
		{
			return _nativeSystemVolume_isSystemVolumeMuted((int)audioStreamType);
		}

		public override bool IsDeviceVolumeMuted(string deviceID)
		{
			return _nativeSystemVolume_isDeviceVolumeMuted(deviceID);
		}

		public override void SetSystemVolume(float volume, AudioStreamType audioStreamType, bool sendCallback)
		{
			_nativeSystemVolume_setSystemVolume(volume, (int)audioStreamType);
			base.SetSystemVolume(volume, audioStreamType, sendCallback);
		}

		public override void SetDeviceVolume(float volume, string deviceID, bool sendCallback)
		{
			_nativeSystemVolume_setDeviceVolume(volume, deviceID);
			base.SetDeviceVolume(volume, deviceID, sendCallback);
		}

		public override void SetSystemVolumeMute(bool mute, AudioStreamType audioStreamType, bool sendCallback)
		{
			_nativeSystemVolume_setSystemVolumeMute(mute, (int)audioStreamType);
			base.SetSystemVolumeMute(mute, audioStreamType, sendCallback);
		}

		public override void SetDeviceVolumeMute(bool mute, string deviceID, bool sendCallback)
		{
			_nativeSystemVolume_setDeviceVolumeMute(mute, deviceID);
			base.SetDeviceVolumeMute(mute, deviceID, sendCallback);
		}
	}
}
#endif
