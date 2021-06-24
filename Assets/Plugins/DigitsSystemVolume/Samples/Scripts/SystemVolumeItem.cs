using UnityEngine;
using UnityEngine.UI;

namespace DigitsSystemVolume.Samples
{
	public class SystemVolumeItem: VolumeItem
	{
		[SerializeField]
		private GameObject fillVlolume = null;


		public AudioStreamType AudioStreamType { get; private set; }

		public void InitUI(AudioStreamType audioStreamType)
		{
			AudioStreamType = audioStreamType;
			slider.value = NativeSystemVolumeManager.GetSystemVolume(AudioStreamType);
			UpdateTitleLabel(audioStreamType.ToString());
			NativeSystemVolumeManager.AddSystemVolumeChangedListener(OnSystemVolumeChanged);
			NativeSystemVolumeManager.AddSystemVolumeMuteChangedListener(OnSystemVolumeMuteChanged);

			muted = NativeSystemVolumeManager.IsSystemVolumeMuted(audioStreamType);
			UpdateMuteSprite();
		}

		public void OnSliderValueChanged(float value)
		{
			if(percentageLabel != null)
			{
				//percentageLabel.text = Mathf.RoundToInt(value * 100) + "%";
				NativeSystemVolumeManager.SetSystemVolume(value, AudioStreamType);
			}

			var img = fillVlolume.GetComponent<Image>();
			Debug.Log("img:" + img);
			if (value >= 0.5)
			{

				var cl = new Color(36f / 255f, 203f / 255f, 143f / 255f);
				fillVlolume.GetComponent<Image>().color = cl;
			}
			else
			{
				var cl = new Color(235f / 255f, 87f / 255f, 87f / 255f);
				fillVlolume.GetComponent<Image>().color = cl;
			}
		}

		public void OnMuteClick()
		{
			muted = !muted;

			if(muted)
			{
				NativeSystemVolumeManager.MuteSystemVolume(AudioStreamType);
			}
			else
			{
				NativeSystemVolumeManager.UnmuteSystemVolume(AudioStreamType);
			}

			UpdateMuteSprite();
		}

		public void OnSystemVolumeChanged(float volume, AudioStreamType audioStreamType)
		{
			if(slider != null && AudioStreamType == audioStreamType)
			{
				slider.value = volume;

				Debug.Log("vol:" + volume);

				var img = fillVlolume.GetComponent<Image>();
				Debug.Log("img:" + img);
				if (volume >= 0.5)
				{

					var cl = new Color(36f / 255f, 203f / 255f, 143f / 255f);
					fillVlolume.GetComponent<Image>().color = cl;
				}
				else
				{
					var cl = new Color(235f / 255f, 87f / 255f, 87f / 255f);
					fillVlolume.GetComponent<Image>().color = cl;
				}
			}
		}

		public void OnSystemVolumeMuteChanged(bool muted, AudioStreamType audioStreamType)
		{
			if(AudioStreamType == audioStreamType)
			{
				this.muted = muted;
				UpdateMuteSprite();
			}
		}
	}
}
