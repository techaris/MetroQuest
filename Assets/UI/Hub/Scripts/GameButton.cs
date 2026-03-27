using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Data;

namespace UI.Hub
{
    public class GameButton : MonoBehaviour
    {
        [SerializeField] private Image thumbnailImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button button;
        [SerializeField] private int buttonClickSfxIndex = 1;

        private SceneEntryData _boundData;

        public void Bind(SceneEntryData data, Action<SceneEntryData> onClick)
        {
            _boundData = data;

            if (data == null)
            {
                if (titleText != null) titleText.text = string.Empty;
                if (descriptionText != null) descriptionText.text = string.Empty;
                if (thumbnailImage != null) thumbnailImage.sprite = null;
                if (button != null) button.interactable = false;
                return;
            }

            if (titleText != null)
                titleText.text = data.title ?? string.Empty;

            if (descriptionText != null)
                descriptionText.text = data.description ?? string.Empty;

            if (thumbnailImage != null)
                thumbnailImage.sprite = data.sceneDisplay;

            {
                button.interactable = true;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(PlayButtonClickSfx);
                button.onClick.AddListener(() => onClick?.Invoke(_boundData));
            }
        }

        private void PlayButtonClickSfx()
        {
            if (Core.Managers.AudioManager.Instance != null)
                Core.Managers.AudioManager.Instance.PlaySFX(buttonClickSfxIndex);
        }
    }
}
