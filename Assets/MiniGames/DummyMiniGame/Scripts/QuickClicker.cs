using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Core.Managers;
using Core.Services;
using UI.Shared;
using Unity.Collections;

namespace MiniGames.DummyMiniGame
{
    public class QuickClicker : MonoBehaviour
    {
        [SerializeField] private AppRoot appRootPrefab;
        [SerializeField] private EventTrigger clickRewardButton;
        [SerializeField] private RectTransform clickAreaRoot;
        [SerializeField] private Animator effectAnim;
        [SerializeField] private RewardPopupPool popupPool;
        [SerializeField] private int rewardPerClick = 10;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button showResult;
        [SerializeField] private Button returnToHubButton;
        [SerializeField] private string hubSceneName = "Hub";
        [SerializeField] private int currentGained = 0;


        private void Awake()
        {
            if (AppRoot.Instance == null && appRootPrefab != null)
                Instantiate(appRootPrefab);
        }

        private void Start()
        {
            if (resultPanel != null)
                resultPanel.SetActive(false);

            if (clickRewardButton != null)
            {
                returnToHubButton.onClick.AddListener(HandleReturnToHub);
                EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                entry.callback.AddListener(OnClickRewardWithPosition);
                clickRewardButton.triggers.Add(entry);
            }

            if (returnToHubButton != null)
                returnToHubButton.onClick.AddListener(HandleReturnToHub);
            if (showResult != null)
                showResult.onClick.AddListener(ShowResultDisplay);
        }

        private void OnDestroy()
        {
            if (clickRewardButton != null)
            {
                if (clickRewardButton != null)
                {
                    for (int i = clickRewardButton.triggers.Count - 1; i >= 0; i--)
                    {
                        if (clickRewardButton.triggers[i].eventID == EventTriggerType.PointerClick)
                            clickRewardButton.triggers.RemoveAt(i);
                    }
                }
            }

            if (returnToHubButton != null)
                returnToHubButton.onClick.RemoveListener(HandleReturnToHub);
            
        }

        private void OnClickRewardWithPosition(BaseEventData data)
        {
            Vector2 pointerPosition = (data as PointerEventData)?.position ?? Vector2.zero;

            if (EconomyManager.Instance == null)
            {
                Debug.LogError("[QuickClicker] EconomyManager.Instance is null.");
                return;
            }

            currentGained += rewardPerClick;
            EconomyManager.Instance.AddSoftCurrency(rewardPerClick);

            SpawnRewardPopup(pointerPosition);

            if (effectAnim != null)
            {
                effectAnim.SetTrigger("Jiggle");
            }
        }

        public void WaitNClose(float time)
        {
            Invoke(nameof(CloseResult), time);
        }

        void CloseResult()
        {
            if (resultPanel != null) ;
                resultPanel.SetActive(false);
        }

        private void SpawnRewardPopup(Vector2 screenPosition)
        {
            if (popupPool == null)
                return;

            string message = "+" + rewardPerClick;
            Color color = Color.green;

            RewardPopup popup = popupPool.GetPopup();
            if (popup == null) return;

            RectTransform popupRect = popup.transform as RectTransform;
            RectTransform parentRect = popup.transform.parent as RectTransform;
            if (popupRect != null && parentRect != null)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPosition, null, out Vector2 localPoint))
                    popupRect.anchoredPosition = localPoint;
            }
            else
            {
                popup.transform.position = new Vector3(screenPosition.x, screenPosition.y, 0f);
            }

            popup.Initialize(message, color);
        }

        void ShowResultDisplay()
        {
            resultText.text = "Your Score: " + currentGained;
        }
        private void HandleReturnToHub()
        {
            if (string.IsNullOrEmpty(hubSceneName))
            {
                Debug.LogError("[QuickClicker] hubSceneName is empty.");
                return;
            }

            if (SceneLoader.Instance == null)
            {
                Debug.LogError("[QuickClicker] SceneLoader.Instance is null.");
                return;
            }

            SceneLoader.Instance.LoadSceneByName(hubSceneName);
        }
    }
}
