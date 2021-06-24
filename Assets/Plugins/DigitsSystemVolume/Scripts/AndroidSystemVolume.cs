
#if !UNITY_EDITOR && UNITY_ANDROID
using UnityEngine;

namespace DigitsSystemVolume
{
	public class AndroidSystemVolume: NativeSystemVolume
	{
		private readonly AudioStreamType[] SUPPORTED_STREAM_TYPES = new AudioStreamType[]
		{
			AudioStreamType.ALARM,
			AudioStreamType.DTMF,
			AudioStreamType.MUSIC,
			AudioStreamType.NOTIFICATION,
			AudioStreamType.RING,
			AudioStreamType.SYSTEM,
			AudioStreamType.VOICE_CALL
		};

		/// <summary>The main Android class</summary>
		private AndroidJavaClass mainClass;

		public override AudioStreamType[] SupportedAudioStreamTypes { get { return SUPPORTED_STREAM_TYPES; } }

		public override void Initialize()
		{
			mainClass = new AndroidJavaClass("com.apollojourney.nativesystemvolume.NativeSystemVolume");
			mainClass.CallStatic("_initialize", gameObject.name);
			base.Initialize();
		}

		public override void RefreshAudioOutputDevices()
		{
			string json = mainClass.CallStatic<string>("_getAudioOutputDevices");
			AudioOutputDeviceCollection audioOutputDeviceCollection = JsonUtility.FromJson<AudioOutputDeviceCollection>(json);
			audioOutputDevices = audioOutputDeviceCollection.devices;
			lastDeviceVolumes = new float[audioOutputDevices.Length];
			lastDeviceVolumeMutes = new bool[audioOutputDevices.Length];
		}

		public override float GetSystemVolume(AudioStreamType audioStreamType)
		{
			return mainClass.CallStatic<float>("_getSystemVolume", (int)audioStreamType);
		}

		public override float GetDeviceVolume(string deviceID)
		{
			return mainClass.CallStatic<float>("_getDeviceVolume", deviceID);
		}

		public override bool IsSystemVolumeMuted(AudioStreamType audioStreamType)
		{
			return mainClass.CallStatic<bool>("_isSystemVolumeMuted", (int)audioStreamType);
		}

		public override bool IsDeviceVolumeMuted(string deviceID)
		{
			return mainClass.CallStatic<bool>("_isDeviceVolumeMuted", deviceID);
		}

		public override void SetSystemVolume(float volume, AudioStreamType audioStreamType, bool sendCallback)
		{
			mainClass.CallStatic("_setSystemVolume", volume, (int)audioStreamType);
			base.SetSystemVolume(volume, audioStreamType, sendCallback);
		}

		public override void SetDeviceVolume(float volume, string deviceID, bool sendCallback)
		{
			mainClass.CallStatic("_setDeviceVolume", volume, deviceID);
			base.SetDeviceVolume(volume, deviceID, sendCallback);
		}

		public override void SetSystemVolumeMute(bool mute, AudioStreamType audioStreamType, bool sendCallback)
		{
			mainClass.CallStatic("_setSystemVolumeMute", mute, (int)audioStreamType);
			base.SetSystemVolumeMute(mute, audioStreamType, sendCallback);
		}

		public override void SetDeviceVolumeMute(bool mute, string deviceID, bool sendCallback)
		{
			mainClass.CallStatic("_setDeviceVolumeMute", mute, deviceID);
			base.SetDeviceVolumeMute(mute, deviceID, sendCallback);
		}
	}
}
#endif