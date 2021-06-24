using UnityEngine;
using UnityEngine.UI;

namespace DigitsSystemVolume.Samples
{
    public class ScreenView : MonoBehaviour
    {
        private const float Y_SPACING = 64;

        [SerializeField]
        private SystemVolumeItem systemVolumeItemPrefab = null;

        [SerializeField]
        private DeviceVolumeItem deviceVolumeItemPrefab = null;

        private Text errorLabel;
        private Text playToggleLabel;
        private ScrollRect scrollRect;
        private bool initialized;

        private void Awake()
        {
            if (!initialized) { Initialize(); }
        }

        private void Initialize()
        {
            errorLabel = transform.Find("ErrorLabel").GetComponent<Text>();
            playToggleLabel = transform.Find("PlayToggle/Label").GetComponent<Text>();
            scrollRect = GetComponentInChildren<ScrollRect>();
            initialized = true;
        }

        public void InitializeUI(AudioStreamType[] audioStreamTypes, AudioOutputDevice[] audioOutputDevices)
        {
            if (!initialized) { Initialize(); }
            float y = 0;


            int length = audioStreamTypes.Length;
            for (int i = 0; i < length; i++)
            {
                AudioStreamType audioStreamType = audioStreamTypes[i];

#if UNITY_IOS
                if (audioStreamType == AudioStreamType.SYSTEM)
                {
                    SystemVolumeItem volumeItem = CreateSystemVolumeItem();
                    volumeItem.InitUI(audioStreamType);
                    volumeItem.RectTransform.anchoredPosition = new Vector2(0, y);
                    y -= volumeItem.RectTransform.rect.height;
                    y -= Y_SPACING;
                }
#elif UNITY_ANDROID
                if (audioStreamType == AudioStreamType.MUSIC)
                {
                    SystemVolumeItem volumeItem = CreateSystemVolumeItem();
                    volumeItem.InitUI(audioStreamType);
                    volumeItem.RectTransform.anchoredPosition = new Vector2(0, y);
                    y -= volumeItem.RectTransform.rect.height;
                    y -= Y_SPACING;
                }
#endif
            }

            /*
			int length = audioOutputDevices.Length;
			Debug.LogError("[2]audioStreamTypes:" + length);
			for (int i = 0; i < length; i++)
			{
				AudioOutputDevice audioOutputDevice = audioOutputDevices[i];
				DeviceVolumeItem volumeItem = CreateDeviceVolumeItem();
				volumeItem.InitUI(audioOutputDevice);
				volumeItem.RectTransform.anchoredPosition = new Vector2(0, y);
				y -= volumeItem.RectTransform.rect.height;
				y -= Y_SPACING;
			}
			*/


            scrollRect.content.sizeDelta = new Vector2(0, Mathf.Abs(y));
        }

        public SystemVolumeItem CreateSystemVolumeItem()
        {
            return (SystemVolumeItem)CreateVolumeItem(systemVolumeItemPrefab);
        }

        public DeviceVolumeItem CreateDeviceVolumeItem()
        {
            return (DeviceVolumeItem)CreateVolumeItem(deviceVolumeItemPrefab);
        }

        public VolumeItem CreateVolumeItem(VolumeItem prefab)
        {
            VolumeItem volumeItem = GameObject.Instantiate(prefab);
            Vector2 sizeDelta = volumeItem.RectTransform.sizeDelta;
            volumeItem.RectTransform.SetParent(scrollRect.content);
            volumeItem.RectTransform.localScale = Vector3.one;
            volumeItem.RectTransform.localPosition = Vector3.zero;
            volumeItem.RectTransform.localRotation = Quaternion.identity;
            volumeItem.RectTransform.sizeDelta = sizeDelta;

            return volumeItem;
        }

        public void ChangePlayToggleLabel(string text)
        {
            //playToggleLabel.text = text;
        }

        public void ShowErrorLabel()
        {
            errorLabel.gameObject.SetActive(true);
        }

        public void HideErrorLabel()
        {
            errorLabel.gameObject.SetActive(false);
        }
    }
}