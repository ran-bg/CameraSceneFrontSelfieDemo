using UnityEngine;
using UnityEngine.UI;

namespace DigitsSystemVolume.Samples
{
	public abstract class VolumeItem: MonoBehaviour
	{
		[SerializeField]
		private Sprite speakerSprite = null;

		[SerializeField]
		private Sprite muteSprite = null;

		protected Text titleLabel;
		protected Image muteIconRenderer;
		protected Slider slider;
		protected Text percentageLabel;
		protected bool muted;

		public RectTransform RectTransform { get; private set; }

		private void Awake()
		{
			RectTransform = GetComponent<RectTransform>();
			titleLabel = transform.Find("TitleLabel").GetComponent<Text>();
			muteIconRenderer = transform.Find("Mute/Icon").GetComponent<Image>();
			slider = transform.Find("Slider").GetComponent<Slider>();
			percentageLabel = transform.Find("PercentageLabel").GetComponent<Text>();
		}

		public void UpdateTitleLabel(string title)
		{
			titleLabel.text = title;
		}

		protected void UpdateMuteSprite()
		{
			if(muted)
			{
				slider.value = 0;
				muteIconRenderer.sprite = muteSprite;
			}
			else
			{
				muteIconRenderer.sprite = speakerSprite;
			}
		}
	}
}