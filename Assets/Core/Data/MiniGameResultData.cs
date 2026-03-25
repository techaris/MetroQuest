namespace Core.Data
{
    /// <summary>
    /// Payload from a mini-game run. RewardManager applies coins/stars; progression is handled elsewhere.
    /// </summary>
    [System.Serializable]
    public class MiniGameResultData
    {
        public string miniGameId;
        public bool success;
        public int coinsEarned;
        public int starsEarned;
        public int score;
        public int levelIndex;
    }
}
