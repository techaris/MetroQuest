using System.Collections.Generic;
using UnityEngine;
using Core.Data;

namespace Core.Managers
{
    public class GameHandler : MonoBehaviour
    {
        public static GameHandler Instance { get; private set; }

        [SerializeField] private List<SceneEntryData> availableScenes = new List<SceneEntryData>();
        [SerializeField] private bool oneClickResult = false;

        public IReadOnlyList<SceneEntryData> AvailableScenes => availableScenes;
        public bool OneClickResult => oneClickResult;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetAvailableScenes(List<SceneEntryData> scenes)
        {
            availableScenes = scenes ?? new List<SceneEntryData>();
        }

        public SceneEntryData GetSceneByName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName) || availableScenes == null)
                return null;

            foreach (var scene in availableScenes)
            {
                if (scene != null && scene.sceneName == sceneName)
                    return scene;
            }

            return null;
        }
    }
}
