using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Data;

namespace Core.Managers
{
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        public event Action OnProgressionChanged;

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

        public bool IsAreaUnlocked(string areaId)
        {
            if (!TryGetProgression(out ProgressionSaveData p))
                return false;
            return !string.IsNullOrEmpty(areaId) && p.unlockedAreas.Contains(areaId);
        }

        public bool IsStationUnlocked(string stationId)
        {
            if (!TryGetProgression(out ProgressionSaveData p))
                return false;
            return !string.IsNullOrEmpty(stationId) && p.unlockedStations.Contains(stationId);
        }

        public bool IsMiniGameUnlocked(string miniGameId)
        {
            if (!TryGetProgression(out ProgressionSaveData p))
                return false;
            return !string.IsNullOrEmpty(miniGameId) && p.unlockedMiniGames.Contains(miniGameId);
        }

        public bool IsMiniGameCompleted(string miniGameId)
        {
            if (!TryGetProgression(out ProgressionSaveData p))
                return false;
            return !string.IsNullOrEmpty(miniGameId) && p.completedMiniGames.Contains(miniGameId);
        }

        public bool UnlockArea(string areaId)
        {
            if (string.IsNullOrEmpty(areaId))
                return false;
            if (!TryGetProgression(out ProgressionSaveData p))
                return false;
            if (!AddUnique(p.unlockedAreas, areaId))
                return false;
            Persist();
            return true;
        }

        public bool UnlockStation(string stationId)
        {
            if (string.IsNullOrEmpty(stationId))
                return false;
            if (!TryGetProgression(out ProgressionSaveData p))
                return false;
            if (!AddUnique(p.unlockedStations, stationId))
                return false;
            Persist();
            return true;
        }

        public bool UnlockMiniGame(string miniGameId)
        {
            if (string.IsNullOrEmpty(miniGameId))
                return false;
            if (!TryGetProgression(out ProgressionSaveData p))
                return false;
            if (!AddUnique(p.unlockedMiniGames, miniGameId))
                return false;
            Persist();
            return true;
        }

        public bool MarkMiniGameCompleted(string miniGameId)
        {
            if (string.IsNullOrEmpty(miniGameId))
                return false;
            if (!TryGetProgression(out ProgressionSaveData p))
                return false;
            if (!AddUnique(p.completedMiniGames, miniGameId))
                return false;
            Persist();
            return true;
        }

        public IReadOnlyList<string> GetUnlockedAreas()
        {
            if (!TryGetProgression(out ProgressionSaveData p))
                return Array.Empty<string>();
            return p.unlockedAreas;
        }

        public IReadOnlyList<string> GetUnlockedStations()
        {
            if (!TryGetProgression(out ProgressionSaveData p))
                return Array.Empty<string>();
            return p.unlockedStations;
        }

        public IReadOnlyList<string> GetUnlockedMiniGames()
        {
            if (!TryGetProgression(out ProgressionSaveData p))
                return Array.Empty<string>();
            return p.unlockedMiniGames;
        }

        private static bool TryGetProgression(out ProgressionSaveData progression)
        {
            progression = null;
            if (SaveManager.Instance == null)
                return false;

            SaveData save = SaveManager.Instance.CurrentSaveData;
            if (save == null)
                return false;

            SaveData.EnsureIntegrity(save);
            progression = save.progression;
            return progression != null;
        }

        private static bool AddUnique(List<string> list, string id)
        {
            if (list.Contains(id))
                return false;
            list.Add(id);
            return true;
        }

        private void Persist()
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.Save();
            OnProgressionChanged?.Invoke();
        }
    }
}
