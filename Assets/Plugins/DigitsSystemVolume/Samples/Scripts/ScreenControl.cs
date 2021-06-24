using UnityEngine;

namespace DigitsSystemVolume.Samples
{
	public class ScreenControl: MonoBehaviour
	{
		[SerializeField]
		private ScreenView view = null;

		[SerializeField]
		private AudioSource audioPlayer = null;

		private void Start()
		{
            if (NativeSystemVolumeManager.IsCurrentPlatformSupported())
            {
                view.HideErrorLabel();

                AudioStreamType[] audioStreamTypes = NativeSystemVolumeManager.GetSupportedAudioStreamTypes();
                AudioOutputDevice[] audioOutputDevices = NativeSystemVolumeManager.GetAudioOutputDevices();
                view.InitializeUI(audioStreamTypes, audioOutputDevices);
            }
            else
            {
                view.ShowErrorLabel();

                Debug.LogWarningFormat("Native System Volume is not supported on this platform.");
            }
		}

		public void OnPlayAudioToggleChanged(bool isON)
		{
			if(isON)
			{
				audioPlayer.Play();
				view.ChangePlayToggleLabel("STOP AUDIO");
			}
			else
			{
				audioPlayer.Stop();
				view.ChangePlayToggleLabel("PLAY AUDIO");
			}
		}
	}
}
