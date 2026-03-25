using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Core.Constants;
using Core.Data;
using Core.Managers;
using Core.Services;
using UI.Shared;

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
        [SerializeField] private Button closeResultButton;
        [SerializeField] private Button returnToHubButton;
        [SerializeField] private string hubSceneName = "Hub";
        [SerializeField] private int currentGained = 0;

        [SerializeField] private GameObject CoinPressed;
        [SerializeField] private GameObject CoinUnPressed;

        [Header("Audio Settings")]
        [SerializeField] private int coinPressSfxIndex = 0;
        [SerializeField] private int buttonClickSfxIndex = 1;
        [SerializeField] private int bgMusicIndex = 0;

        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private float scorePunchScale = 1.35f;
        [SerializeField] private float scorePunchDuration = 0.12f;

        [Header("Result Screen Labels")]
        [SerializeField] private TextMeshProUGUI resultCoinsText;
        [SerializeField] private TextMeshProUGUI resultClicksText;
        [SerializeField] private TextMeshProUGUI resultTimeText;
        [SerializeField] private TextMeshProUGUI resultCpsText;

        private int _totalClicks;
        private float _sessionTime;
        private bool _sessionRunning;

        private Coroutine _coinResetCoroutine;
        private Coroutine _scorePunchCoroutine;

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

            RefreshScoreText();

            _totalClicks = 0;
            _sessionTime = 0f;
            _sessionRunning = true;

            TryStartPlaySession();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMusic(bgMusicIndex);

            returnToHubButton?.onClick.AddListener(PlayButtonClickSfx);
            showResult?.onClick.AddListener(PlayButtonClickSfx);
            closeResultButton?.onClick.AddListener(PlayButtonClickSfx);

            closeResultButton?.onClick.AddListener(CloseResult);
        }

        private void PlayButtonClickSfx()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(buttonClickSfxIndex);
        }

        private void Update()
        {
            if (_sessionRunning)
                _sessionTime += Time.deltaTime;
        }

        private void OnDestroy()
        {
            if (clickRewardButton != null)
            {
                for (int i = clickRewardButton.triggers.Count - 1; i >= 0; i--)
                {
                    if (clickRewardButton.triggers[i].eventID == EventTriggerType.PointerClick)
                        clickRewardButton.triggers.RemoveAt(i);
                }
            }

            if (returnToHubButton != null)
                returnToHubButton.onClick.RemoveListener(HandleReturnToHub);

            // MiniGameSessionManager is DDOL — clear stuck session if scene unloads without hub.
            TryEndPlaySessionLeavingScene();
        }

        private void TryStartPlaySession()
        {
            if (MiniGameSessionManager.Instance == null)
                return;
            MiniGameSessionManager.Instance.StartSession(MiniGameIds.QuickTap, 0);
        }

        private MiniGameResultData BuildSessionResult(bool success)
        {
            return new MiniGameResultData
            {
                miniGameId = MiniGameIds.QuickTap,
                success = success,
                coinsEarned = 0,
                starsEarned = 0,
                score = currentGained,
                levelIndex = 0
            };
        }

        /// <summary>
        /// Coins are already granted per click via RewardManager; payload uses 0 so EndSession does not double-award.
        /// Subscribe to OnSessionEnded for progression (e.g. ProgressionManager).
        /// </summary>
        private void TryEndPlaySession(bool success)
        {
            if (MiniGameSessionManager.Instance == null || !MiniGameSessionManager.Instance.IsSessionActive)
                return;
            MiniGameSessionManager.Instance.EndSession(BuildSessionResult(success));
        }

        private void TryEndPlaySessionLeavingScene()
        {
            if (MiniGameSessionManager.Instance == null || !MiniGameSessionManager.Instance.IsSessionActive)
                return;
            if (MiniGameSessionManager.Instance.CurrentMiniGameId != MiniGameIds.QuickTap)
                return;
            TryEndPlaySession(true);
        }

        private void OnClickRewardWithPosition(BaseEventData data)
        {
            if (!_sessionRunning) return;

            Vector2 pointerPosition = (data as PointerEventData)?.position ?? Vector2.zero;

            if (RewardManager.Instance == null)
            {
                Debug.LogError("[QuickClicker] RewardManager.Instance is null.");
                return;
            }

            currentGained += rewardPerClick;
            RewardManager.Instance.GrantCoins(rewardPerClick);
            _totalClicks++;

            SpawnRewardPopup(pointerPosition);

            if (effectAnim != null)
                effectAnim.SetTrigger("Jiggle");

            HandleCoinPress();
            UpdateScore();

            if (GameHandler.Instance != null && GameHandler.Instance.OneClickResult)
                ShowResultDisplay();
        }

        private void HandleCoinPress()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(coinPressSfxIndex);

            if (CoinPressed != null) CoinPressed.SetActive(true);
            if (CoinUnPressed != null) CoinUnPressed.SetActive(false);

            if (_coinResetCoroutine != null)
                StopCoroutine(_coinResetCoroutine);
            _coinResetCoroutine = StartCoroutine(ResetCoinAfterDelay(0.5f));
        }

        private IEnumerator ResetCoinAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (CoinPressed != null) CoinPressed.SetActive(false);
            if (CoinUnPressed != null) CoinUnPressed.SetActive(true);
            _coinResetCoroutine = null;
        }

        private void UpdateScore()
        {
            RefreshScoreText();

            if (scoreText == null) return;

            if (_scorePunchCoroutine != null)
                StopCoroutine(_scorePunchCoroutine);
            _scorePunchCoroutine = StartCoroutine(PunchScoreText());
        }

        private void RefreshScoreText()
        {
            if (scoreText != null)
                scoreText.text = "" + currentGained;
        }

        private IEnumerator PunchScoreText()
        {
            Transform t = scoreText.transform;
            Vector3 originalScale = Vector3.one;

            float elapsed = 0f;
            while (elapsed < scorePunchDuration)
            {
                float progress = elapsed / scorePunchDuration;
                float scale = Mathf.Lerp(1f, scorePunchScale, progress);
                t.localScale = new Vector3(scale, scale, 1f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < scorePunchDuration)
            {
                float progress = elapsed / scorePunchDuration;
                float scale = Mathf.Lerp(scorePunchScale, 1f, progress);
                t.localScale = new Vector3(scale, scale, 1f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            t.localScale = originalScale;
            _scorePunchCoroutine = null;
        }

        public void WaitNClose(float time)
        {
            Invoke(nameof(CloseResult), time);
        }

        public void CloseResult()
        {
            if (resultPanel != null)
                resultPanel.SetActive(false);

            if (GameHandler.Instance != null && GameHandler.Instance.OneClickResult)
            {
                currentGained = 0;
                _totalClicks = 0;
                _sessionTime = 0f;
                _sessionRunning = true;
                RefreshScoreText();
                TryStartPlaySession();
            }
        }

        private void SpawnRewardPopup(Vector2 screenPosition)
        {
            if (popupPool == null)
                return;

            string message = "+" + rewardPerClick;
            Color color = Color.white;

            RewardPopup popup = popupPool.GetPopupAtPosition(screenPosition);
            if (popup == null) return;

            popup.Initialize(message, color);
        }

        private void ShowResultDisplay()
        {
            _sessionRunning = false;

            TryEndPlaySession(true);

            float cps = _sessionTime > 0f ? _totalClicks / _sessionTime : 0f;

            int minutes = Mathf.FloorToInt(_sessionTime / 60f);
            int seconds = Mathf.FloorToInt(_sessionTime % 60f);
            string timeFormatted = string.Format("{0}:{1:00}", minutes, seconds);

            if (resultText != null)
                resultText.text = "" + currentGained;

            if (resultCoinsText != null) resultCoinsText.text = "Coins Earned: " + currentGained;
            if (resultClicksText != null) resultClicksText.text = "Total Clicks: " + _totalClicks;
            if (resultTimeText != null) resultTimeText.text = "Time: " + timeFormatted;
            if (resultCpsText != null) resultCpsText.text = "Clicks / Second: " + cps.ToString("F2");

            if (resultPanel != null)
                resultPanel.SetActive(true);
        }

        private void HandleReturnToHub()
        {
            TryEndPlaySession(true);

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
