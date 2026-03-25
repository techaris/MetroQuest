using UnityEngine;
using Core.Services;

namespace Core.Managers
{
    public class AppRoot : MonoBehaviour
    {
        public static AppRoot Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureComponent<GameHandler>();
            EnsureComponent<SceneLoader>();
            EnsureComponent<SaveManager>();
            EnsureComponent<EconomyManager>();
            EnsureComponent<ProgressionManager>();
            EnsureComponent<RewardManager>();
            EnsureComponent<MiniGameSessionManager>();
        }

        private void EnsureComponent<T>() where T : Component
        {
            if (GetComponent<T>() == null)
                gameObject.AddComponent<T>();
        }
    }
}
