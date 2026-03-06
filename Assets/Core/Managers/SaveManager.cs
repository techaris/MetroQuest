using System.IO;
using UnityEngine;
using Core.Data;

namespace Core.Managers
{
    public class SaveManager : MonoBehaviour
    {
        private const int CURRENT_SAVE_VERSION = 1;
        private const string SAVE_FILE_NAME = "save.json";

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
            return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
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

                if (debugLogs)
                    Debug.Log($"[SaveManager] Loaded: v{currentSaveData.saveVersion} soft={currentSaveData.softCurrency} hard={currentSaveData.hardCurrency}");

                if (currentSaveData.saveVersion < CURRENT_SAVE_VERSION)
                {
                    MigrateSaveData(currentSaveData);
                    Save();
                }

                if (debugLogs)
                    Debug.Log($"[SaveManager] Loaded OK. v{currentSaveData.saveVersion} soft:{currentSaveData.softCurrency} hard:{currentSaveData.hardCurrency}");
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
            {
                currentSaveData = CreateDefaultSaveData();
            }

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
            return new SaveData
            {
                saveVersion = CURRENT_SAVE_VERSION,
                softCurrency = 0,
                hardCurrency = 0
            };
        }

        private void MigrateSaveData(SaveData data)
        {
            if (debugLogs)
                Debug.Log($"[SaveManager] Migrating save from v{data.saveVersion} to v{CURRENT_SAVE_VERSION}");

            while (data.saveVersion < CURRENT_SAVE_VERSION)
            {
                switch (data.saveVersion)
                {
                    case 0:
                        data.saveVersion = 1;
                        break;
                    default:
                        data.saveVersion = CURRENT_SAVE_VERSION;
                        break;
                }
            }
        }
    }
}
