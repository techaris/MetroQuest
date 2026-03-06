using System;
using UnityEngine;
using Core.Data;

namespace Core.Managers
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        public event Action<int, int> OnCurrencyChanged;

        public int SoftCurrency { get; private set; }
        public int HardCurrency { get; private set; }

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

        private void Start()
        {
            InitializeFromSave();
        }

        public void InitializeFromSave()
        {
            RefreshFromSave();
        }

        public void RefreshFromSave()
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogError("[EconomyManager] SaveManager.Instance is null.");
                return;
            }

            SaveData data = SaveManager.Instance.CurrentSaveData;
            if (data == null)
            {
                SaveManager.Instance.Load();
                data = SaveManager.Instance.CurrentSaveData;
            }

            if (data == null)
            {
                SoftCurrency = 0;
                HardCurrency = 0;
                OnCurrencyChanged?.Invoke(SoftCurrency, HardCurrency);
                return;
            }

            SoftCurrency = data.softCurrency;
            HardCurrency = data.hardCurrency;
            OnCurrencyChanged?.Invoke(SoftCurrency, HardCurrency);
        }

        public void AddSoftCurrency(int amount)
        {
            if (amount < 0) return;

            SoftCurrency += amount;
            PushToSaveAndNotify();
        }

        public void AddHardCurrency(int amount)
        {
            if (amount < 0) return;

            HardCurrency += amount;
            PushToSaveAndNotify();
        }

        public bool SpendSoftCurrency(int amount)
        {
            if (amount < 0) return false;
            if (SoftCurrency < amount) return false;

            SoftCurrency -= amount;
            PushToSaveAndNotify();
            return true;
        }

        public bool SpendHardCurrency(int amount)
        {
            if (amount < 0) return false;
            if (HardCurrency < amount) return false;

            HardCurrency -= amount;
            PushToSaveAndNotify();
            return true;
        }

        private void PushToSaveAndNotify()
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogError("[EconomyManager] SaveManager.Instance is null.");
                return;
            }

            SaveData data = SaveManager.Instance.CurrentSaveData;
            if (data == null)
            {
                Debug.LogError("[EconomyManager] SaveManager.CurrentSaveData is null.");
                return;
            }

            data.softCurrency = SoftCurrency;
            data.hardCurrency = HardCurrency;
            SaveManager.Instance.Save();
            OnCurrencyChanged?.Invoke(SoftCurrency, HardCurrency);
        }
    }
}
