using UnityEngine;

namespace DigitsSystemVolume.Samples
{
	public class DeviceVolumeItem: VolumeItem
	{
		public AudioOutputDevice AudioOutputDevice { get; private set; }

		public void InitUI(AudioOutputDevice audioOutputDevice)
		{
			AudioOutputDevice = audioOutputDevice;
			slider.value = NativeSystemVolumeManager.GetDeviceVolume(audioOutputDevice);
			UpdateTitleLabel(audioOutputDevice.name);
			NativeSystemVolumeManager.AddDeviceVolumeChangedListener(OnDeviceVolumeChanged);
			NativeSystemVolumeManager.AddDeviceVolumeMuteChangedListener(OnDeviceVolumeMuteChanged);

			muted = NativeSystemVolumeManager.IsDeviceVolumeMuted(audioOutputDevice);
			UpdateMuteSprite();
		}

		public void OnSliderValueChanged(float value)
		{
			if(percentageLabel != null)
			{
				//percentageLabel.text = Mathf.RoundToInt(value * 100) + "%";
				NativeSystemVolumeManager.SetDeviceVolume(value, AudioOutputDevice);
			}
		}

		public void OnMuteClick()
		{
			muted = !muted;

			if(muted)
			{
				NativeSystemVolumeManager.MuteDeviceVolume(AudioOutputDevice);
			}
			else
			{
				NativeSystemVolumeManager.UnmuteDeviceVolume(AudioOutputDevice);
			}

			UpdateMuteSprite();
		}

		public void OnDeviceVolumeChanged(float volume, AudioOutputDevice audioOutputDevice)
		{
			if(slider != null && AudioOutputDevice.id == audioOutputDevice.id)
			{
				slider.value = volume;
			}
		}

		public void OnDeviceVolumeMuteChanged(bool muted, AudioOutputDevice audioOutputDevice)
		{
			if(AudioOutputDevice.id == audioOutputDevice.id)
			{
				this.muted = muted;
				UpdateMuteSprite();
			}
		}
	}
}
