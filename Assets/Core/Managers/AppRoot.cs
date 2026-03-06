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

            if (GetComponent<GameHandler>() == null)
                gameObject.AddComponent<GameHandler>();

            if (GetComponent<SceneLoader>() == null)
                gameObject.AddComponent<SceneLoader>();

            if (GetComponent<SaveManager>() == null)
                gameObject.AddComponent<SaveManager>();

            if (GetComponent<EconomyManager>() == null)
                gameObject.AddComponent<EconomyManager>();
        }
    }
}
