using System.Collections;
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

        // ── Session stats ──────────────────────────────────────────────────────
        private int   _totalClicks;
        private float _sessionTime;      // seconds elapsed since Start
        private bool  _sessionRunning;

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

            // Initialise score display.
            RefreshScoreText();

            // Start session timer.
            _totalClicks    = 0;
            _sessionTime    = 0f;
            _sessionRunning = true;

            // Play background music.
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMusic(bgMusicIndex);

            // Register button click sounds.
            returnToHubButton?.onClick.AddListener(PlayButtonClickSfx);
            showResult?.onClick.AddListener(PlayButtonClickSfx);
            closeResultButton?.onClick.AddListener(PlayButtonClickSfx);

            // Register close result listener.
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
            if (!_sessionRunning) return;

            Vector2 pointerPosition = (data as PointerEventData)?.position ?? Vector2.zero;

            if (EconomyManager.Instance == null)
            {
                Debug.LogError("[QuickClicker] EconomyManager.Instance is null.");
                return;
            }

            currentGained += rewardPerClick;
            EconomyManager.Instance.AddSoftCurrency(rewardPerClick);
            _totalClicks++;

            SpawnRewardPopup(pointerPosition);

            if (effectAnim != null)
                effectAnim.SetTrigger("Jiggle");

            HandleCoinPress();
            UpdateScore();

            if (GameHandler.Instance != null && GameHandler.Instance.OneClickResult)
            {
                ShowResultDisplay();
            }
        }

        private void HandleCoinPress()
        {
            // Play sound via AudioManager using index — supports overlapping sounds.
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(coinPressSfxIndex);

            // Swap visuals: show Pressed, hide UnPressed.
            if (CoinPressed != null)   CoinPressed.SetActive(true);
            if (CoinUnPressed != null) CoinUnPressed.SetActive(false);

            // Restart the reset timer so rapid clicks extend the pressed state correctly.
            if (_coinResetCoroutine != null)
                StopCoroutine(_coinResetCoroutine);
            _coinResetCoroutine = StartCoroutine(ResetCoinAfterDelay(0.5f));
        }

        private IEnumerator ResetCoinAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (CoinPressed != null)   CoinPressed.SetActive(false);
            if (CoinUnPressed != null) CoinUnPressed.SetActive(true);
            _coinResetCoroutine = null;
        }

        // ── Score display ─────────────────────────────────────────────────────

        private void UpdateScore()
        {
            RefreshScoreText();

            if (scoreText == null) return;

            // Restart punch so rapid clicks never leave the text at a mid-scale.
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

            // Scale UP
            float elapsed = 0f;
            while (elapsed < scorePunchDuration)
            {
                float progress = elapsed / scorePunchDuration;
                float scale = Mathf.Lerp(1f, scorePunchScale, progress);
                t.localScale = new Vector3(scale, scale, 1f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Scale back DOWN
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

            // If we are in "one click" mode, reset the stats so the player can click again for a new result.
            if (GameHandler.Instance != null && GameHandler.Instance.OneClickResult)
            {
                currentGained = 0;
                _totalClicks = 0;
                _sessionTime = 0f;
                _sessionRunning = true;
                RefreshScoreText();
            }
        }

        private void SpawnRewardPopup(Vector2 screenPosition)
        {
            if (popupPool == null)
                return;

            string message = "+" + rewardPerClick;
            Color color = Color.white;

            // Positioning is handled by the pool — respects its useFixedPosition / fixedSpawnPos settings.
            RewardPopup popup = popupPool.GetPopupAtPosition(screenPosition);
            if (popup == null) return;

            popup.Initialize(message, color);
        }

        void ShowResultDisplay()
        {
            // Stop the session timer.
            _sessionRunning = false;

            float cps = _sessionTime > 0f ? _totalClicks / _sessionTime : 0f;

            // Format elapsed time as m:ss
            int minutes = Mathf.FloorToInt(_sessionTime / 60f);
            int seconds = Mathf.FloorToInt(_sessionTime % 60f);
            string timeFormatted = string.Format("{0}:{1:00}", minutes, seconds);

            // Legacy single resultText (kept for compatibility).
            if (resultText != null)
                resultText.text = "" + currentGained;

            if (resultCoinsText  != null) resultCoinsText.text  = "Coins Earned: "    + currentGained;
            if (resultClicksText != null) resultClicksText.text = "Total Clicks: "    + _totalClicks;
            if (resultTimeText   != null) resultTimeText.text   = "Time: "            + timeFormatted;
            if (resultCpsText    != null) resultCpsText.text    = "Clicks / Second: " + cps.ToString("F2");

            if (resultPanel != null)
                resultPanel.SetActive(true);
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
