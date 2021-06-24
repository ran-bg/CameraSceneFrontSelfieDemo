using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitsSystemVolume;

public class TutorialSoundController : MonoBehaviour
{
    [SerializeField]
    private DigitsSystemVolume.Samples.ScreenView view = null;

//    [SerializeField]
//    private AudioSource audioPlayer = null;

    // Start is called before the first frame update
    void Start()
    {
        if (NativeSystemVolumeManager.IsCurrentPlatformSupported())
        {
            view.HideErrorLabel();

            // サポートしている
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
}
