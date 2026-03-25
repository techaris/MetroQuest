using UnityEngine;
using Core.Data;

namespace Core.Managers
{
    public class RewardManager : MonoBehaviour
    {
        public static RewardManager Instance { get; private set; }

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

        public void GrantCoins(int amount)
        {
            if (amount < 0)
            {
                Debug.LogError("[RewardManager] GrantCoins: amount must be non-negative.");
                return;
            }

            if (amount == 0)
                return;

            if (EconomyManager.Instance == null)
            {
                Debug.LogError("[RewardManager] GrantCoins: EconomyManager.Instance is null.");
                return;
            }

            EconomyManager.Instance.AddSoftCurrency(amount);
        }

        public void GrantStars(int amount)
        {
            if (amount < 0)
            {
                Debug.LogError("[RewardManager] GrantStars: amount must be non-negative.");
                return;
            }

            if (amount == 0)
                return;

            if (SaveManager.Instance == null)
            {
                Debug.LogError("[RewardManager] GrantStars: SaveManager.Instance is null.");
                return;
            }

            SaveData save = SaveManager.Instance.CurrentSaveData;
            if (save == null)
            {
                Debug.LogError("[RewardManager] GrantStars: CurrentSaveData is null.");
                return;
            }

            SaveData.EnsureIntegrity(save);
            long sum = (long)save.stars + amount;
            if (sum < 0) sum = 0;
            if (sum > int.MaxValue) sum = int.MaxValue;
            save.stars = (int)sum;
            SaveManager.Instance.Save();
        }

        public void GrantMiniGameRewards(MiniGameResultData result)
        {
            if (result == null)
            {
                Debug.LogError("[RewardManager] GrantMiniGameRewards: result is null.");
                return;
            }

            if (result.coinsEarned != 0)
                GrantCoins(result.coinsEarned);

            if (result.starsEarned != 0)
                GrantStars(result.starsEarned);
        }
    }
}
