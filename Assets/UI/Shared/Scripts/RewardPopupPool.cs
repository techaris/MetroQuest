using System.Collections.Generic;
using UnityEngine;

namespace UI.Shared
{
    public class RewardPopupPool : MonoBehaviour
    {
        [SerializeField] private RewardPopup popupPrefab;
        [SerializeField] private Transform popupParent;
        [SerializeField] private int initialPoolSize = 5;

        private List<RewardPopup> _pool = new List<RewardPopup>();

        private void Awake()
        {
            if (popupPrefab == null) return;

            Transform parent = popupParent != null ? popupParent : transform;

            for (int i = 0; i < initialPoolSize; i++)
            {
                RewardPopup popup = Instantiate(popupPrefab, parent);
                popup.gameObject.SetActive(false);
                _pool.Add(popup);
            }
        }

        public RewardPopup GetPopup()
        {
            if (popupPrefab == null) return null;

            foreach (var popup in _pool)
            {
                if (popup != null && !popup.gameObject.activeSelf)
                {
                    popup.gameObject.SetActive(true);
                    return popup;
                }
            }

            Transform parent = popupParent != null ? popupParent : transform;
            RewardPopup newPopup = Instantiate(popupPrefab, parent);
            newPopup.gameObject.SetActive(true);
            _pool.Add(newPopup);
            return newPopup;
        }
    }
}
