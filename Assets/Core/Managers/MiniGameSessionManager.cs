using System;
using UnityEngine;
using Core.Data;

namespace Core.Managers
{
    /// <summary>
    /// Owns mini-game session lifecycle. EndSession applies rewards via RewardManager; subscribe to OnSessionEnded for progression/UI.
    /// </summary>
    public class MiniGameSessionManager : MonoBehaviour
    {
        public static MiniGameSessionManager Instance { get; private set; }

        public event Action<string, int> OnSessionStarted;
        public event Action<MiniGameResultData> OnSessionEnded;

        private bool _sessionActive;
        private string _currentMiniGameId = string.Empty;
        private int _currentLevelIndex;
        private MiniGameResultData _lastResult;

        public bool IsSessionActive => _sessionActive;
        public string CurrentMiniGameId => _currentMiniGameId;
        public int CurrentLevelIndex => _currentLevelIndex;
        public MiniGameResultData LastResult => _lastResult;

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

        public void StartSession(string miniGameId, int levelIndex = 0)
        {
            if (string.IsNullOrEmpty(miniGameId))
            {
                Debug.LogError("[MiniGameSessionManager] StartSession: miniGameId is null or empty.");
                return;
            }

            if (_sessionActive)
            {
                Debug.LogWarning($"[MiniGameSessionManager] StartSession rejected: session already active ({_currentMiniGameId}).");
                return;
            }

            _sessionActive = true;
            _currentMiniGameId = miniGameId;
            _currentLevelIndex = levelIndex;

            OnSessionStarted?.Invoke(_currentMiniGameId, _currentLevelIndex);
        }

        public void EndSession(MiniGameResultData result)
        {
            if (!_sessionActive)
            {
                Debug.LogWarning("[MiniGameSessionManager] EndSession: no active session.");
                return;
            }

            _sessionActive = false;
            _currentMiniGameId = string.Empty;
            _currentLevelIndex = 0;

            _lastResult = CloneResult(result);

            if (_lastResult != null && RewardManager.Instance != null)
                RewardManager.Instance.GrantMiniGameRewards(_lastResult);
            else if (_lastResult != null)
                Debug.LogError("[MiniGameSessionManager] EndSession: RewardManager.Instance is null; rewards not applied.");

            OnSessionEnded?.Invoke(_lastResult);
        }

        private static MiniGameResultData CloneResult(MiniGameResultData src)
        {
            if (src == null)
                return null;

            return new MiniGameResultData
            {
                miniGameId = src.miniGameId,
                success = src.success,
                coinsEarned = src.coinsEarned,
                starsEarned = src.starsEarned,
                score = src.score,
                levelIndex = src.levelIndex
            };
        }
    }
}
