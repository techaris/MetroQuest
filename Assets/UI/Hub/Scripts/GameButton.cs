using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Data;

namespace UI.Hub
{
    public class GameButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image thumbnailImage;
        [SerializeField] private Button button;

        private SceneEntryData _boundData;

        public void Bind(SceneEntryData data, Action<SceneEntryData> onClick)
        {
            _boundData = data;

            if (data == null)
            {
                if (titleText != null) titleText.text = string.Empty;
                if (thumbnailImage != null) thumbnailImage.sprite = null;
                if (button != null) button.interactable = false;
                return;
            }

            if (titleText != null)
                titleText.text = data.displayName ?? string.Empty;

            if (thumbnailImage != null)
                thumbnailImage.sprite = data.sceneDisplay;

            if (button != null)
            {
                button.interactable = true;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClick?.Invoke(_boundData));
            }
        }
    }
}
