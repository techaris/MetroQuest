using UnityEngine;
using UnityEngine.UI;
using Core.Services;

namespace UI.Hub
{
    public class HubTopBarController : MonoBehaviour
    {
        [SerializeField] private Button playDummyMiniGameButton;
        [SerializeField] private string dummyMiniGameSceneName = "DummyMiniGame";

        private void OnEnable()
        {
            if (playDummyMiniGameButton != null)
                playDummyMiniGameButton.onClick.AddListener(HandlePlayDummyMiniGame);
        }

        private void OnDisable()
        {
            if (playDummyMiniGameButton != null)
                playDummyMiniGameButton.onClick.RemoveListener(HandlePlayDummyMiniGame);
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
