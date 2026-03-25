namespace Core.Data
{
    [System.Serializable]
    public class CafeSaveData
    {
        public int highestUnlockedLevel;
        public int highestCompletedLevel;
        public int milkUpgradeLevel;
        public int fillingUpgradeLevel;
        public int toppingUpgradeLevel;
        public bool tutorialCompleted;

        public static CafeSaveData CreateDefault()
        {
            return new CafeSaveData
            {
                highestUnlockedLevel = 0,
                highestCompletedLevel = 0,
                milkUpgradeLevel = 0,
                fillingUpgradeLevel = 0,
                toppingUpgradeLevel = 0,
                tutorialCompleted = false
            };
        }
    }
}
