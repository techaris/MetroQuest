using UnityEngine;
using UnityEngine.UI;
using Core.Services;

namespace UI.Hub
{
    public class HubTopBarController : MonoBehaviour
    {
        [SerializeField] private Button playDummyMiniGameButton;
        [SerializeField] private string dummyMiniGameSceneName = "DummyMiniGame";
        [SerializeField] private int buttonClickSfxIndex = 1;

        private void OnEnable()
        {
            if (playDummyMiniGameButton != null)
            {
                playDummyMiniGameButton.onClick.AddListener(PlayButtonClickSfx);
                playDummyMiniGameButton.onClick.AddListener(HandlePlayDummyMiniGame);
            }
        }

        private void PlayButtonClickSfx()
        {
            if (Core.Managers.AudioManager.Instance != null)
                Core.Managers.AudioManager.Instance.PlaySFX(buttonClickSfxIndex);
        }

        private void OnDisable()
        {
            if (playDummyMiniGameButton != null)
            {
                playDummyMiniGameButton.onClick.RemoveListener(PlayButtonClickSfx);
                playDummyMiniGameButton.onClick.RemoveListener(HandlePlayDummyMiniGame);
            }
        }

        private void HandlePlayDummyMiniGame()
        {
            if (SceneLoader.Instance == null)
            {
                Debug.LogError("[HubTopBarController] SceneLoader.Instance is null.");
                return;
            }

            if (string.IsNullOrEmpty(dummyMiniGameSceneName))
            {
                Debug.LogError("[HubTopBarController] dummyMiniGameSceneName is empty.");
                return;
            }

            SceneLoader.Instance.LoadSceneByName(dummyMiniGameSceneName);
        }
    }
}
