using System.Collections.Generic;
using UnityEngine;

namespace UI.Shared
{
    public class RewardPopupPool : MonoBehaviour
    {
        [SerializeField] private RewardPopup popupPrefab;
        [SerializeField] private Transform popupParent;
        [SerializeField] private int initialPoolSize = 5;

        [Header("Spawn Position")]
        [Tooltip("When enabled the popup always spawns at Fixed Spawn Pos instead of the click position.")]
        [SerializeField] private bool useFixedPosition = false;
        [Tooltip("World/UI Transform whose position is used when Use Fixed Position is enabled.")]
        [SerializeField] private Transform fixedSpawnPos;

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

        /// <summary>Returns a pooled popup without positioning it.</summary>
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

        /// <summary>
        /// Returns a pooled popup and positions it automatically.
        /// If <see cref="useFixedPosition"/> is true and <see cref="fixedSpawnPos"/> is assigned,
        /// the popup is placed at that Transform's position.
        /// Otherwise it is placed at <paramref name="screenClickPos"/>.
        /// </summary>
        /// <param name="screenClickPos">Screen-space position of the click (e.g. from PointerEventData.position).</param>
        /// <param name="cam">Camera used for screen-to-canvas conversion. Pass null for Screen Space – Overlay canvases.</param>
        public RewardPopup GetPopupAtPosition(Vector2 screenClickPos, Camera cam = null)
        {
            RewardPopup popup = GetPopup();
            if (popup == null) return null;

            RectTransform popupRect = popup.transform as RectTransform;
            RectTransform parentRect = popup.transform.parent as RectTransform;

            if (useFixedPosition && fixedSpawnPos != null)
            {
                // Place popup at the fixed Transform's position.
                if (popupRect != null && parentRect != null)
                {
                    // Convert the fixed world/UI position to screen-space then to local canvas space.
                    Vector2 fixedScreen = RectTransformUtility.WorldToScreenPoint(cam, fixedSpawnPos.position);
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, fixedScreen, cam, out Vector2 localPt))
                        popupRect.anchoredPosition = localPt;
                }
                else
                {
                    popup.transform.position = fixedSpawnPos.position;
                }
            }
            else
            {
                // Place popup at the click position.
                if (popupRect != null && parentRect != null)
                {
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenClickPos, cam, out Vector2 localPt))
                        popupRect.anchoredPosition = localPt;
                }
                else
                {
                    popup.transform.position = new Vector3(screenClickPos.x, screenClickPos.y, 0f);
                }
            }

            return popup;
        }
    }
}
