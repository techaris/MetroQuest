using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Services
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

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

        public void LoadSceneByName(string sceneName)
        {
            if (!CanLoadScene(sceneName))
            {
                Debug.LogError("[SceneLoader] LoadSceneByName failed: sceneName is null or empty.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }

        public bool CanLoadScene(string sceneName)
        {
            return !string.IsNullOrEmpty(sceneName);
        }
    }
}
