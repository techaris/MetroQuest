namespace Core.Data
{
    [System.Serializable]
    public class MaintenanceSaveData
    {
        public int highestUnlockedLevel;
        public int highestCompletedLevel;
        public bool tutorialCompleted;

        public static MaintenanceSaveData CreateDefault()
        {
            return new MaintenanceSaveData
            {
                highestUnlockedLevel = 0,
                highestCompletedLevel = 0,
                tutorialCompleted = false
            };
        }
    }
}
