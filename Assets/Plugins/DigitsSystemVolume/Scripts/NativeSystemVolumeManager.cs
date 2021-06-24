using UnityEngine;

namespace DigitsSystemVolume
{
	/// <summary>Access point for the NativeSystemVolume for current platform</summary>
	public class NativeSystemVolumeManager: MonoBehaviour
	{
		/// <summary>The singleton instance of NativeSystemVolumeManager</summary>
		private static NativeSystemVolumeManager instance;

		/// <summary>The NativeSystemVolume instance of current platform</summary>
		private NativeSystemVolume systemVolume;

		/// <summary>The singleton instance of NativeSystemVolumeManager</summary>
		private static NativeSystemVolumeManager Instance
		{
			get
			{
				if(instance == null)
				{
					instance = GameObject.FindObjectOfType<NativeSystemVolumeManager>();
					if(instance == null)
					{
						GameObject gameObject = new GameObject("NativeSystemVolumeManager");
						instance = gameObject.AddComponent<NativeSystemVolumeManager>();
					}
				}

				return instance;
			}
		}

		/// <summary>The NativeSystemVolume instance of current platform</summary>
		public static NativeSystemVolume SystemVolume
		{
			get { return Instance.systemVolume; }
		}

        #region UNITY
        private void Awake()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			systemVolume = gameObject.AddComponent<WindowsSystemVolume>();
			systemVolume.Initialize();
#elif !UNITY_EDITOR && UNITY_ANDROID
			systemVolume = gameObject.AddComponent<AndroidSystemVolume>();
			systemVolume.Initialize();
#elif !UNITY_EDITOR && UNITY_IOS
			systemVolume = gameObject.AddComponent<IOSSystemVolume>();
			systemVolume.Initialize();
#else
            Debug.LogWarningFormat("Native System Volume is not supported on this platform.");
#endif
        }

		private void OnDestroy()
		{
			instance = null;
		}
		#endregion

		public static void TryDestroy()
		{
			if(instance != null && instance.gameObject != null)
			{
				Destroy(instance.gameObject);
			}
		}

        /// <summary>Indicates if current platform is supported</summary>
        public static bool IsCurrentPlatformSupported()
        {
            /// <summary>Refreshes the (connected) audio output devices</summary>
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return true;
#elif !UNITY_EDITOR && UNITY_ANDROID
            return true;
#elif !UNITY_EDITOR && UNITY_IOS
            return true;
#else
            return false;
#endif
        }

        /// <summary>Refreshes the (connected) audio output devices</summary>
        public static void RefreshAudioOutputDevices()
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			systemVolume.RefreshAudioOutputDevices();
		}

		/// <summary>Gets the system volume for given audio stream type</summary>
		public static float GetSystemVolume(AudioStreamType audioStreamType)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			if(!systemVolume.ValidateAudioStreamType(audioStreamType)) { return -1; }

			return systemVolume.GetSystemVolume(audioStreamType);
		}

		/// <summary>Gets the device volume for the device with given ID</summary>
		public static float GetDeviceVolume(AudioOutputDevice audioOutputDevice)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			if(!systemVolume.ValidateDeviceID(audioOutputDevice.id)) { return -1; }

			return systemVolume.GetDeviceVolume(audioOutputDevice.id);
		}

		/// <summary>Indicates if the system volume is muted for given audio stream type</summary>
		public static bool IsSystemVolumeMuted(AudioStreamType audioStreamType)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			if(!systemVolume.ValidateAudioStreamType(audioStreamType)) { return false; }

			return systemVolume.IsSystemVolumeMuted(audioStreamType);
		}

		/// <summary>Indicates if the system volume is muted for the device with given ID</summary>
		public static bool IsDeviceVolumeMuted(AudioOutputDevice audioOutputDevice)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			if(!systemVolume.ValidateDeviceID(audioOutputDevice.id)) { return false; }

			return systemVolume.IsDeviceVolumeMuted(audioOutputDevice.id);
		}

		/// <summary>Sets the system volume for given audio stream type</summary>
		/// <param name="volume">The requested volume value (between 0 - 1)</param>
		/// <param name="audioStreamType">The audio stream type</param>
		/// <param name="sendCallback">Optional: Indicates whether to send an event callback, default is false</param>
		public static void SetSystemVolume(float volume, AudioStreamType audioStreamType, bool sendCallback = false)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			if(!systemVolume.ValidateAudioStreamType(audioStreamType)) { return; }

			systemVolume.SetSystemVolume(volume, audioStreamType, sendCallback);
		}

		/// <summary>Sets the device volume for device with given id</summary>
		/// <param name="volume">The requested volume value (between 0 - 1)</param>
		/// <param name="deviceID">The id of the device</param>
		/// <param name="sendCallback">Optional: Indicates whether to send an event callback, default is false</param>
		public static void SetDeviceVolume(float volume, AudioOutputDevice audioOutputDevice, bool sendCallback = false)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			if(!systemVolume.ValidateDeviceID(audioOutputDevice.id)) { return; }

			systemVolume.SetDeviceVolume(volume, audioOutputDevice.id, sendCallback);
		}

		/// <summary>Mutes the system volume for given audio stream type</summary>
		/// <param name="audioStreamType">The audio stream type</param>
		/// <param name="sendCallback">Optional: Indicates whether to send an event callback, default is false</param>
		public static void MuteSystemVolume(AudioStreamType audioStreamType, bool sendCallback = false)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			if(!systemVolume.ValidateAudioStreamType(audioStreamType)) { return; }

			systemVolume.SetSystemVolumeMute(true, audioStreamType, sendCallback);
		}

		/// <summary>Unmutes the system volume for given audio stream type</summary>
		/// <param name="audioStreamType">The audio stream type</param>
		/// <param name="sendCallback">Optional: Indicates whether to send an event callback, default is false</param>
		public static void UnmuteSystemVolume(AudioStreamType audioStreamType, bool sendCallback = false)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			if(!systemVolume.ValidateAudioStreamType(audioStreamType)) { return; }

			systemVolume.SetSystemVolumeMute(false, audioStreamType, sendCallback);
		}

		/// <summary>Mutes the device volume mute state for device with given id</summary>
		/// <param name="deviceID">The id of the device</param>
		/// <param name="sendCallback">Optional: Indicates whether to send an event callback, default is false</param>
		public static void MuteDeviceVolume(AudioOutputDevice audioOutputDevice, bool sendCallback = false)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			if(!systemVolume.ValidateDeviceID(audioOutputDevice.id)) { return; }

			systemVolume.SetDeviceVolumeMute(true, audioOutputDevice.id, sendCallback);
		}

		/// <summary>Unmutes the device volume mute state for device with given id</summary>
		/// <param name="deviceID">The id of the device</param>
		/// <param name="sendCallback">Optional: Indicates whether to send an event callback, default is false</param>
		public static void UnmuteDeviceVolume(AudioOutputDevice audioOutputDevice, bool sendCallback = false)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			if(!systemVolume.ValidateDeviceID(audioOutputDevice.id)) { return; }

			systemVolume.SetDeviceVolumeMute(false, audioOutputDevice.id, sendCallback);
		}

		/// <summary>Gets the supported AudioStreamTypes of this platform</summary>
		public static AudioStreamType[] GetSupportedAudioStreamTypes()
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			return systemVolume.SupportedAudioStreamTypes;
		}

		/// <summary>Gets the AudioOutputDevices of this device</summary>
		public static AudioOutputDevice[] GetAudioOutputDevices()
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			return systemVolume.AudioOutputDevices;
		}

		/// <summary>Adds given SystemVolumeChanged listener</summary>
		public static void AddSystemVolumeChangedListener(OnSystemVolumeChanged listener)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			systemVolume.SystemVolumeChanged += listener;
		}

		/// <summary>Removes given SystemVolumeChanged listener</summary>
		public static void RemoveSystemVolumeChangedListener(OnSystemVolumeChanged listener)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			systemVolume.SystemVolumeChanged -= listener;
		}

		/// <summary>Adds given DeviceVolumeChanged listener</summary>
		public static void AddDeviceVolumeChangedListener(OnDeviceVolumeChanged listener)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			systemVolume.DeviceVolumeChanged += listener;
		}

		/// <summary>Removes given DeviceVolumeChanged listener</summary>
		public static void RemoveDeviceVolumeChangedListener(OnDeviceVolumeChanged listener)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			systemVolume.DeviceVolumeChanged -= listener;
		}

		/// <summary>Adds given SystemVolumeMuteChanged listener</summary>
		public static void AddSystemVolumeMuteChangedListener(OnSystemVolumeMuteChanged listener)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			systemVolume.SystemVolumeMuteChanged += listener;
		}

		/// <summary>Removes given SystemVolumeMuteChanged listener</summary>
		public static void RemoveSystemVolumeMuteChangedListener(OnSystemVolumeMuteChanged listener)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			systemVolume.SystemVolumeMuteChanged -= listener;
		}

		/// <summary>Adds given DeviceVolumeMuteChanged listener</summary>
		public static void AddDeviceVolumeMuteChangedListener(OnDeviceVolumeMuteChanged listener)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			systemVolume.DeviceVolumeMuteChanged += listener;
		}

		/// <summary>Removes given DeviceVolumeMuteChanged listener</summary>
		public static void RemoveDeviceVolumeMuteChangedListener(OnDeviceVolumeMuteChanged listener)
		{
			NativeSystemVolume systemVolume = Instance.systemVolume;
			systemVolume.DeviceVolumeMuteChanged -= listener;
		}
	}
}