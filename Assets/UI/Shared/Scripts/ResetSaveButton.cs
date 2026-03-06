using UnityEngine;
using UnityEngine.UI;
using Core.Managers;

namespace UI.Shared
{
    public class ResetSaveButton : MonoBehaviour
    {
        [SerializeField] private Button button;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (button != null)
                button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (button != null)
                button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogError("[ResetSaveButton] SaveManager.Instance is null.");
                return;
            }

            if (EconomyManager.Instance == null)
            {
                Debug.LogError("[ResetSaveButton] EconomyManager.Instance is null.");
                return;
            }

            SaveManager.Instance.ResetSave();
            EconomyManager.Instance.RefreshFromSave();
        }
    }
}
