using UnityEngine;

namespace DigitsSystemVolume
{
	public enum AudioStreamType
	{
		ALARM, //The volume of audio streams for alarms
		DTMF, //The volume of audio streams for DTMF Tones
		MUSIC, //The volume of audio streams for music playback
		NOTIFICATION, //The volume of audio streams for notifications 
		RING, //The volume of audio streams for the phone ring 
		SYSTEM, //The volume of audio streams for system sounds
		VOICE_CALL //The volume of audio streams for phone calls 
	}

	public delegate void OnSystemVolumeChanged(float volume, AudioStreamType audioStreamType);
	public delegate void OnDeviceVolumeChanged(float volume, AudioOutputDevice audioOutputDevice);
	public delegate void OnSystemVolumeMuteChanged(bool muted, AudioStreamType audioStreamType);
	public delegate void OnDeviceVolumeMuteChanged(bool muted, AudioOutputDevice audioOutputDevice);

	/// <summary>Base class to control the native system volume of a platform</summary>
	public abstract class NativeSystemVolume: MonoBehaviour
	{
		/// <summary>How often to check for volume/mute changes (in seconds)</summary>
		private const float UPDATE_INTERVAL = 1f;

		/// <summary>The audio output devices of this device</summary>
		protected AudioOutputDevice[] audioOutputDevices;

		/// <summary>Last known system volumes</summary>
		protected float[] lastSystemVolumes;

		/// <summary>Last known device volumes</summary>
		protected float[] lastDeviceVolumes;

		/// <summary>Last known system volume mute states</summary>
		protected bool[] lastSystemVolumeMutes;

		/// <summary>Last known device volume mute states</summary>
		protected bool[] lastDeviceVolumeMutes;

		/// <summary>Indicates whether to check for system volume changes</summary>
		protected bool systemVolumeUpdatesEnabled;

		/// <summary>Indicates whether to check for device volume changes</summary>
		protected bool deviceVolumeUpdatesEnabled;

		/// <summary>Indicates whether to check for system volume mute state changes</summary>
		protected bool systemVolumeMuteUpdatesEnabled;

		/// <summary>Indicates whether to check for device volume mute state changes</summary>
		protected bool deviceVolumeMuteUpdatesEnabled;

		/// <summary>Current update time</summary>
		protected float updateTime;

		/// <summary>Event for system volume changes</summary>
		protected event OnSystemVolumeChanged onSystemVolumeChanged;

		/// <summary>Event for device volume changes</summary>
		protected event OnDeviceVolumeChanged onDeviceVolumeChanged;

		/// <summary>Event for system volume mute changes</summary>
		protected event OnSystemVolumeMuteChanged onSystemVolumeMuteChanged;

		/// <summary>Event for device volume mute changes</summary>
		protected event OnDeviceVolumeMuteChanged onDeviceVolumeMuteChanged;

		/// <summary>Event for system volume changes</summary>
		public event OnSystemVolumeChanged SystemVolumeChanged
		{
			add
			{
				onSystemVolumeChanged += value;
				if(onSystemVolumeChanged.GetInvocationList().Length > 0)
				{
					EnableSystemVolumeUpdates();
				}
			}
			remove
			{
				onSystemVolumeChanged -= value;
				if(onSystemVolumeChanged.GetInvocationList().Length == 0)
				{
					DisableSystemVolumeUpdates();
				}
			}
		}

		/// <summary>Event for device volume changes</summary>
		public event OnDeviceVolumeChanged DeviceVolumeChanged
		{
			add
			{
				onDeviceVolumeChanged += value;
				if(onDeviceVolumeChanged.GetInvocationList().Length > 0)
				{
					EnableDeviceVolumeUpdates();
				}
			}
			remove
			{
				onDeviceVolumeChanged -= value;
				if(onDeviceVolumeChanged.GetInvocationList().Length == 0)
				{
					DisableDeviceVolumeUpdates();
				}
			}
		}

		/// <summary>Event for system volume mute changes</summary>
		public event OnSystemVolumeMuteChanged SystemVolumeMuteChanged
		{
			add
			{
				onSystemVolumeMuteChanged += value;
				if(onSystemVolumeMuteChanged.GetInvocationList().Length > 0)
				{
					EnableSystemVolumeMuteUpdates();
				}
			}
			remove
			{
				onSystemVolumeMuteChanged -= value;
				if(onSystemVolumeMuteChanged.GetInvocationList().Length == 0)
				{
					DisableSystemVolumeMuteUpdates();
				}
			}
		}

		/// <summary>Event for device volume mute changes</summary>
		public event OnDeviceVolumeMuteChanged DeviceVolumeMuteChanged
		{
			add
			{
				onDeviceVolumeMuteChanged += value;
				if(onDeviceVolumeMuteChanged.GetInvocationList().Length > 0)
				{
					EnableDeviceVolumeMuteUpdates();
				}
			}
			remove
			{
				onDeviceVolumeMuteChanged -= value;
				if(onDeviceVolumeMuteChanged.GetInvocationList().Length == 0)
				{
					DisableDeviceVolumeMuteUpdates();
				}
			}
		}

		/// <summary>The supported AudioStreamTypes of this platform</summary>
		public abstract AudioStreamType[] SupportedAudioStreamTypes { get; }

		/// <summary>The audio output devices of this device</summary>
		public AudioOutputDevice[] AudioOutputDevices { get { return audioOutputDevices; } }

		/// <summary>Refreshes the (connected) audio output devices</summary>
		public abstract void RefreshAudioOutputDevices();

		/// <summary>Gets the system volume for given audio stream type</summary>
		public abstract float GetSystemVolume(AudioStreamType audioStreamType);

		/// <summary>Gets the device volume for the device with given ID</summary>
		public abstract float GetDeviceVolume(string deviceID);

		/// <summary>Indicates if the system volume is muted for given audio stream type</summary>
		public abstract bool IsSystemVolumeMuted(AudioStreamType audioStreamType);

		/// <summary>Indicates if the system volume is muted for the device with given ID</summary>
		public abstract bool IsDeviceVolumeMuted(string deviceID);

		#region UNITY
		private void Update()
		{
			if(!systemVolumeUpdatesEnabled && !deviceVolumeUpdatesEnabled && !systemVolumeMuteUpdatesEnabled && !deviceVolumeMuteUpdatesEnabled) { return; }

			updateTime += Time.deltaTime;
			if(updateTime >= UPDATE_INTERVAL)
			{
				if(systemVolumeUpdatesEnabled)
				{
					UpdateSystemVolumes();
				}
				if(deviceVolumeUpdatesEnabled)
				{
					UpdateDeviceVolumes();
				}
				if(systemVolumeMuteUpdatesEnabled)
				{
					UpdateSystemVolumeMutes();
				}
				if(deviceVolumeUpdatesEnabled)
				{
					UpdateDeviceVolumeMutes();
				}

				updateTime = 0;
			}
		}
		#endregion

		/// <summary>Initializes this NativeSystemVolume</summary>
		public virtual void Initialize()
		{
			lastSystemVolumes = new float[SupportedAudioStreamTypes.Length];
			lastSystemVolumeMutes = new bool[SupportedAudioStreamTypes.Length];
			updateTime = float.MaxValue;
			RefreshAudioOutputDevices();
		}

		/// <summary>Sets the system volume for given audio stream type</summary>
		/// <param name="volume">The requested volume value (between 0 - 1)</param>
		/// <param name="audioStreamType">The audio stream type</param>
		/// <param name="sendCallback">Indicates whether to send an event callback</param>
		public virtual void SetSystemVolume(float volume, AudioStreamType audioStreamType, bool sendCallback)
		{
			if(!sendCallback)
			{
				int index = IndexOfSupportedAudioStreamType(audioStreamType);
				if(index == -1) { return; }

				lastSystemVolumes[index] = GetSystemVolume(audioStreamType);
			}
		}

		/// <summary>Sets the device volume for device with given id</summary>
		/// <param name="volume">The requested volume value (between 0 - 1)</param>
		/// <param name="deviceID">The id of the device</param>
		/// <param name="sendCallback">Indicates whether to send an event callback</param>
		public virtual void SetDeviceVolume(float volume, string deviceID, bool sendCallback)
		{
			if(!sendCallback)
			{
				int index = GetIndexOfAudioOutputDevice(deviceID);
				if(index == -1) { return; }

				lastDeviceVolumes[index] = GetDeviceVolume(deviceID);
			}
		}

		/// <summary>Sets the system volume mute state for given audio stream type</summary>
		/// <param name="volume">The requested volume value (between 0 - 1)</param>
		/// <param name="audioStreamType">The audio stream type</param>
		/// <param name="sendCallback">Indicates whether to send an event callback</param>
		public virtual void SetSystemVolumeMute(bool mute, AudioStreamType audioStreamType, bool sendCallback)
		{
			if(!sendCallback)
			{
				int index = IndexOfSupportedAudioStreamType(audioStreamType);
				if(index == -1) { return; }

				lastSystemVolumeMutes[index] = IsSystemVolumeMuted(audioStreamType);
			}
		}

		/// <summary>Sets the device volume mute state for device with given id</summary>
		/// <param name="volume">The requested volume value (between 0 - 1)</param>
		/// <param name="deviceID">The id of the device</param>
		/// <param name="sendCallback">Indicates whether to send an event callback</param>
		public virtual void SetDeviceVolumeMute(bool mute, string deviceID, bool sendCallback)
		{
			if(!sendCallback)
			{
				int index = GetIndexOfAudioOutputDevice(deviceID);
				if(index == -1) { return; }

				lastDeviceVolumeMutes[index] = IsDeviceVolumeMuted(deviceID);
			}
		}

		/// <summary>Gets the index of given audio stream type in the supported audio stream type list</summary>
		protected int IndexOfSupportedAudioStreamType(AudioStreamType audioStreamType)
		{
			int length = SupportedAudioStreamTypes.Length;
			for(int i = 0; i < length; i++)
			{
				if(SupportedAudioStreamTypes[i] == audioStreamType) { return i; }
			}

			return -1;
		}

		/// <summary>Update the system volume states</summary>
		private void UpdateSystemVolumes()
		{
			int length = SupportedAudioStreamTypes.Length;
			for(int i = 0; i < length; i++)
			{
				AudioStreamType audioStreamType = SupportedAudioStreamTypes[i];
				float lastVolume = lastSystemVolumes[i];
				float volume = GetSystemVolume(audioStreamType);
				if(lastVolume != volume)
				{
					InvokeSystemVolumeChanged(volume, audioStreamType);
					lastSystemVolumes[i] = volume;
				}
			}
		}

		/// <summary>Update the device volume states</summary>
		private void UpdateDeviceVolumes()
		{
			int length = AudioOutputDevices.Length;
			for(int i = 0; i < length; i++)
			{
				AudioOutputDevice audioOutputDevice = AudioOutputDevices[i];
				float lastVolume = lastDeviceVolumes[i];
				float volume = GetDeviceVolume(audioOutputDevice.id);
				if(lastVolume != volume)
				{
					InvokeDeviceVolumeChanged(volume, audioOutputDevice);
					lastDeviceVolumes[i] = volume;
				}
			}
		}

		/// <summary>Update the system volume mute states</summary>
		private void UpdateSystemVolumeMutes()
		{
			int length = SupportedAudioStreamTypes.Length;
			for(int i = 0; i < length; i++)
			{
				AudioStreamType audioStreamType = SupportedAudioStreamTypes[i];
				bool lastMute = lastSystemVolumeMutes[i];
				bool mute = IsSystemVolumeMuted(audioStreamType);
				if(lastMute != mute)
				{
					InvokeSystemVolumeMuteChanged(mute, audioStreamType);
					lastSystemVolumeMutes[i] = mute;
				}
			}
		}

		/// <summary>Update the device volume mute states</summary>
		private void UpdateDeviceVolumeMutes()
		{
			int length = AudioOutputDevices.Length;
			for(int i = 0; i < length; i++)
			{
				AudioOutputDevice audioOutputDevice = AudioOutputDevices[i];
				bool lastMute = lastDeviceVolumeMutes[i];
				bool muted = IsDeviceVolumeMuted(audioOutputDevice.id);
				if(lastMute != muted)
				{
					InvokeDeviceVolumeMuteChanged(muted, audioOutputDevice);
					lastDeviceVolumeMutes[i] = muted;
				}
			}
		}

		/// <summary>Validates whether given audio stream type is supported on this platform</summary>
		public bool ValidateAudioStreamType(AudioStreamType audioStreamType)
		{
			if(IsAudioStreamTypeSupported(audioStreamType))
			{
				return true;
			}
			else
			{
				Debug.LogWarningFormat("AudioStreamType {0} is not supported on this platform");
				return false;
			}
		}

		/// <summary>Checks whether given audio stream type is supported on this platform</summary>
		protected bool IsAudioStreamTypeSupported(AudioStreamType audioStreamType)
		{
			int length = SupportedAudioStreamTypes.Length;
			for(int i = 0; i < length; i++)
			{
				if(SupportedAudioStreamTypes[i] == audioStreamType) { return true; }
			}

			return false;
		}

		/// <summary>Validates whether a device with given id exists</summary>
		public bool ValidateDeviceID(string deviceID)
		{
			if(GetIndexOfAudioOutputDevice(deviceID) != -1)
			{
				return true;
			}
			else
			{
				Debug.LogWarningFormat("DeviceID {0} is unknown on this device");
				return false;
			}
		}

		/// <summary>Gets the index of the device with given id in the audio output devices list</summary>
		protected int GetIndexOfAudioOutputDevice(string deviceID)
		{
			int length = AudioOutputDevices.Length;
			for(int i = 0; i < length; i++)
			{
				if(AudioOutputDevices[i].id == deviceID) { return i; }
			}

			return -1;
		}

		/// <summary>Invoke the system volume changed event (if it has any listeners)</summary>
		protected void InvokeSystemVolumeChanged(float volume, AudioStreamType audioStreamType)
		{
			if(onSystemVolumeChanged != null)
			{
				onSystemVolumeChanged(volume, audioStreamType);
			}
		}

		/// <summary>Invoke the device volume changed event (if it has any listeners)</summary>
		protected void InvokeDeviceVolumeChanged(float volume, AudioOutputDevice audioOutputDevice)
		{
			if(onDeviceVolumeChanged != null)
			{
				onDeviceVolumeChanged(volume, audioOutputDevice);
			}
		}

		/// <summary>Invoke the system volume mute changed event (if it has any listeners)</summary>
		protected void InvokeSystemVolumeMuteChanged(bool muted, AudioStreamType audioStreamType)
		{
			if(onSystemVolumeMuteChanged != null)
			{
				onSystemVolumeMuteChanged(muted, audioStreamType);
			}
		}

		/// <summary>Invoke the device volume mute changed event (if it has any listeners)</summary>
		protected void InvokeDeviceVolumeMuteChanged(bool muted, AudioOutputDevice audioOutputDevice)
		{
			if(onDeviceVolumeMuteChanged != null)
			{
				onDeviceVolumeMuteChanged(muted, audioOutputDevice);
			}
		}

		/// <summary>Enables checks for system volume changes</summary>
		public void EnableSystemVolumeUpdates()
		{
			systemVolumeUpdatesEnabled = true;
		}

		/// <summary>Disables checks for system volume changes</summary>
		public void DisableSystemVolumeUpdates()
		{
			systemVolumeUpdatesEnabled = false;
		}

		/// <summary>Enables checks for device volume changes</summary>
		public void EnableDeviceVolumeUpdates()
		{
			deviceVolumeUpdatesEnabled = true;
		}

		/// <summary>Disables checks for device volume changes</summary>
		public void DisableDeviceVolumeUpdates()
		{
			deviceVolumeUpdatesEnabled = false;
		}

		/// <summary>Enables checks for system volume mute state changes</summary>
		public void EnableSystemVolumeMuteUpdates()
		{
			systemVolumeMuteUpdatesEnabled = true;
		}

		/// <summary>Disables checks for system volume mute state changes</summary>
		public void DisableSystemVolumeMuteUpdates()
		{
			systemVolumeMuteUpdatesEnabled = false;
		}

		/// <summary>Enables checks for device volume mute state changes</summary>
		public void EnableDeviceVolumeMuteUpdates()
		{
			deviceVolumeMuteUpdatesEnabled = true;
		}

		/// <summary>Disables checks for device volume mute state changes</summary>
		public void DisableDeviceVolumeMuteUpdates()
		{
			deviceVolumeMuteUpdatesEnabled = false;
		}
	}
}
