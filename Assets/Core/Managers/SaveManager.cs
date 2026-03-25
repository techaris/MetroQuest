using System.IO;
using UnityEngine;
using Core.Data;

namespace Core.Managers
{
    /// <summary>
    /// Persists <see cref="SaveData"/> as JSON under Application.persistentDataPath/save.json.
    /// Milestone 0 on-disk format: saveVersion 1 with currency fields only.
    /// Milestone 1 (MetroQuest): saveVersion 2 with stars and nested progression/hub/cafe/maintenance/map sections.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        /// <summary>Legacy saves before explicit versioning.</summary>
        private const int SaveVersionLegacy = 0;
        /// <summary>Milestone 0: softCurrency, hardCurrency (and optional stars defaulting to 0).</summary>
        private const int SaveVersionMilestone0 = 1;
        /// <summary>Milestone 1: full MetroQuest save tree.</summary>
        private const int SaveVersionMilestone1 = 2;

        private const int CurrentSaveVersion = SaveVersionMilestone1;
        private const string SaveFileName = "save.json";

        public static SaveManager Instance { get; private set; }

        [SerializeField] private bool debugLogs = true;
        [SerializeField] private SaveData currentSaveData;

        public SaveData CurrentSaveData => currentSaveData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (debugLogs)
                    Debug.Log("[SaveManager] Duplicate instance destroyed.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (debugLogs)
            {
                string path = GetSaveFilePath();
                Debug.Log($"[SaveManager] Path: {path}. File exists: {File.Exists(path)}");
            }

            Load();
        }

        public string GetSaveFilePath()
        {
            return Path.Combine(Application.persistentDataPath, SaveFileName);
        }

        public void Load()
        {
            string path = GetSaveFilePath();

            if (!File.Exists(path))
            {
                currentSaveData = CreateDefaultSaveData();
                Save();
                if (debugLogs)
                    Debug.Log($"[SaveManager] No save file found. Created default at {path}");
                return;
            }

            try
            {
                if (debugLogs)
                    Debug.Log($"[SaveManager] Loading from {path}");

                string json = File.ReadAllText(path);
                currentSaveData = JsonUtility.FromJson<SaveData>(json);

                if (currentSaveData == null)
                {
                    if (debugLogs)
                        Debug.LogWarning("[SaveManager] Deserialization returned null. Creating default save.");
                    currentSaveData = CreateDefaultSaveData();
                    Save();
                    return;
                }

                // Milestone 1: nested objects/lists are null if absent from JSON — fill before migration or gameplay.
                SaveData.EnsureIntegrity(currentSaveData);

                if (debugLogs)
                    Debug.Log($"[SaveManager] Loaded: v{currentSaveData.saveVersion} soft={currentSaveData.softCurrency} hard={currentSaveData.hardCurrency} stars={currentSaveData.stars}");

                if (currentSaveData.saveVersion < CurrentSaveVersion)
                {
                    MigrateSaveData(currentSaveData);
                    SaveData.EnsureIntegrity(currentSaveData);
                    Save();
                }

                if (debugLogs)
                    Debug.Log($"[SaveManager] Loaded OK. v{currentSaveData.saveVersion} soft:{currentSaveData.softCurrency} hard:{currentSaveData.hardCurrency} stars:{currentSaveData.stars}");
            }
            catch (System.Exception e)
            {
                if (debugLogs)
                    Debug.LogError($"[SaveManager] Failed to load save: {e.Message}");
                currentSaveData = CreateDefaultSaveData();
                Save();
            }
        }

        public void Save()
        {
            if (currentSaveData == null)
                currentSaveData = CreateDefaultSaveData();

            SaveData.EnsureIntegrity(currentSaveData);

            try
            {
                string path = GetSaveFilePath();
                string json = JsonUtility.ToJson(currentSaveData, true);
                File.WriteAllText(path, json);

                if (debugLogs)
                    Debug.Log($"[SaveManager] Saved to {path}");
            }
            catch (System.Exception e)
            {
                if (debugLogs)
                    Debug.LogError($"[SaveManager] Failed to save: {e.Message}");
            }
        }

        public void ResetSave()
        {
            currentSaveData = CreateDefaultSaveData();
            Save();

            if (debugLogs)
                Debug.Log("[SaveManager] Save reset to default.");
        }

        private SaveData CreateDefaultSaveData()
        {
            var data = new SaveData
            {
                saveVersion = CurrentSaveVersion,
                softCurrency = 0,
                hardCurrency = 0,
                stars = 0,
                progression = ProgressionSaveData.CreateDefault(),
                hub = HubSaveData.CreateDefault(),
                cafe = CafeSaveData.CreateDefault(),
                maintenance = MaintenanceSaveData.CreateDefault(),
                map = MapSaveData.CreateDefault()
            };
            SaveData.EnsureIntegrity(data);
            return data;
        }

        private void MigrateSaveData(SaveData data)
        {
            if (debugLogs)
                Debug.Log($"[SaveManager] Migrating save from v{data.saveVersion} to v{CurrentSaveVersion}");

            while (data.saveVersion < CurrentSaveVersion)
            {
                switch (data.saveVersion)
                {
                    case SaveVersionLegacy:
                        data.saveVersion = SaveVersionMilestone0;
                        break;

                    case SaveVersionMilestone0:
                        MigrateMilestone0ToMilestone1(data);
                        data.saveVersion = SaveVersionMilestone1;
                        break;

                    default:
                        if (debugLogs)
                            Debug.LogWarning($"[SaveManager] Unknown saveVersion {data.saveVersion}; clamping to v{CurrentSaveVersion}.");
                        data.saveVersion = CurrentSaveVersion;
                        break;
                }
            }
        }

        /// <summary>
        /// Milestone 0 → Milestone 1: preserve currency (and stars if present); all nested sections come from <see cref="SaveData.EnsureIntegrity"/>.
        /// </summary>
        private static void MigrateMilestone0ToMilestone1(SaveData data)
        {
            if (data.stars < 0)
                data.stars = 0;
            SaveData.EnsureIntegrity(data);
        }
    }
}
