using System.Collections.Generic;
using UnityEngine;
using Core.Data;
using Core.Managers;
using Core.Services;

namespace UI.Hub
{
    public class HubMenuController : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private GameButton gameButtonPrefab;
        [SerializeField, Tooltip("If true, uses pre-placed GameButtons in content instead of spawning.")]
        private bool usePrespawned = false;
        [SerializeField] private int bgMusicIndex = 0;

        private void Start()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMusic(bgMusicIndex);

            Populate();
        }

        private void Populate()
        {
            if (contentRoot == null)
            {
                Debug.LogError("[HubMenuController] contentRoot is not assigned.");
                return;
            }

            if (GameHandler.Instance == null)
            {
                Debug.LogError("[HubMenuController] GameHandler.Instance is missing.");
                return;
            }

            IReadOnlyList<SceneEntryData> scenes = GameHandler.Instance.AvailableScenes;
            if (scenes == null) return;

            if (usePrespawned)
            {
                var buttons = contentRoot.GetComponentsInChildren<GameButton>(true);
                int index = 0;
                foreach (var data in scenes)
                {
                    if (data == null || string.IsNullOrEmpty(data.sceneName))
                        continue;
                    if (index >= buttons.Length) break;
                    buttons[index].Bind(data, HandleGameSelected);
                    buttons[index].gameObject.SetActive(true);
                    index++;
                }
                for (int i = index; i < buttons.Length; i++)
                    buttons[i].gameObject.SetActive(false);
            }
            else
            {
                ClearChildren();
                foreach (var data in scenes)
                {
                    if (data == null || string.IsNullOrEmpty(data.sceneName))
                        continue;
                    var button = Instantiate(gameButtonPrefab, contentRoot);
                    button.Bind(data, HandleGameSelected);
                }
            }
        }

        private void ClearChildren()
        {
            if (contentRoot == null) return;

            for (int i = contentRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(contentRoot.GetChild(i).gameObject);
            }
        }

        private void HandleGameSelected(SceneEntryData data)
        {
            if (data == null || string.IsNullOrEmpty(data.sceneName))
                return;

            if (SceneLoader.Instance == null)
            {
                Debug.LogError("[HubMenuController] SceneLoader.Instance is null.");
                return;
            }

            SceneLoader.Instance.LoadSceneByName(data.sceneName);
        }
    }
}
