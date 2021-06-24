#if UNITY_EDITOR || UNITY_STANDALONE
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace DigitsSystemVolume
{
	/// <summary>Class that acts as a bridge for the Native Windows System Volume</summary>
	public class WindowsSystemVolume: NativeSystemVolume
	{
		private readonly AudioStreamType[] SUPPORTED_STREAM_TYPES = new AudioStreamType[]
		{
			AudioStreamType.SYSTEM, //The volume of the main selected audio output device
		};

		[DllImport("NativeSystemVolume", CharSet = CharSet.Unicode)]
		private static extern int _getAudioOutputDevices(StringBuilder buffer, int bufferLength);
		[DllImport("NativeSystemVolume")]
		private static extern float _getSystemVolume(int audioStreamType);
		[DllImport("NativeSystemVolume")]
		private static extern float _getDeviceVolume([MarshalAs(UnmanagedType.LPWStr)]string deviceID);
		[DllImport("NativeSystemVolume")]
		private static extern void _setSystemVolume(float volume, int audioStreamType);
		[DllImport("NativeSystemVolume")]
		private static extern void _setDeviceVolume(float volume, [MarshalAs(UnmanagedType.LPWStr)]string deviceID);
		[DllImport("NativeSystemVolume")]
		private static extern bool _isSystemVolumeMuted(int audioStreamType);
		[DllImport("NativeSystemVolume")]
		private static extern bool _isDeviceVolumeMuted([MarshalAs(UnmanagedType.LPWStr)]string deviceID);
		[DllImport("NativeSystemVolume")]
		private static extern void _setSystemVolumeMute(bool mute, int audioStreamType);
		[DllImport("NativeSystemVolume")]
		private static extern void _setDeviceVolumeMute(bool mute, [MarshalAs(UnmanagedType.LPWStr)]string deviceID);

		public override AudioStreamType[] SupportedAudioStreamTypes { get { return SUPPORTED_STREAM_TYPES; } }

		public override void RefreshAudioOutputDevices()
		{
			int bufferLength = 1024;
			StringBuilder stringBuilder = new StringBuilder(bufferLength);
			string json = null;
			while(string.IsNullOrEmpty(json))
			{
				int requiredLength = _getAudioOutputDevices(stringBuilder, bufferLength);
				if(bufferLength >= requiredLength)
				{
					json = stringBuilder.ToString();
				}
				else
				{
					bufferLength += 1024;
					stringBuilder.Capacity = bufferLength;
				}
			}

			AudioOutputDeviceCollection audioOutputDeviceCollection = JsonUtility.FromJson<AudioOutputDeviceCollection>(json);
			audioOutputDevices = audioOutputDeviceCollection.devices;
			lastDeviceVolumes = new float[audioOutputDevices.Length];
			lastDeviceVolumeMutes = new bool[audioOutputDevices.Length];
		}

		public override float GetSystemVolume(AudioStreamType audioStreamType)
		{
			return _getSystemVolume((int)audioStreamType);
		}

		public override float GetDeviceVolume(string deviceID)
		{
			return _getDeviceVolume(deviceID);
		}

		public override bool IsSystemVolumeMuted(AudioStreamType audioStreamType)
		{
			return _isSystemVolumeMuted((int)audioStreamType);
		}

		public override bool IsDeviceVolumeMuted(string deviceID)
		{
			return _isDeviceVolumeMuted(deviceID);
		}

		public override void SetSystemVolume(float volume, AudioStreamType audioStreamType, bool sendCallback)
		{
			_setSystemVolume(volume, (int)audioStreamType);
			base.SetSystemVolume(volume, audioStreamType, sendCallback);
		}

		public override void SetDeviceVolume(float volume, string deviceID, bool sendCallback)
		{
			_setDeviceVolume(volume, deviceID);
			base.SetDeviceVolume(volume, deviceID, sendCallback);
		}

		public override void SetSystemVolumeMute(bool mute, AudioStreamType audioStreamType, bool sendCallback)
		{
			_setSystemVolumeMute(mute, (int)audioStreamType);
			base.SetSystemVolumeMute(mute, audioStreamType, sendCallback);
		}

		public override void SetDeviceVolumeMute(bool mute, string deviceID, bool sendCallback)
		{
			_setDeviceVolumeMute(mute, deviceID);
			base.SetDeviceVolumeMute(mute, deviceID, sendCallback);
		}
	}
}
#endif