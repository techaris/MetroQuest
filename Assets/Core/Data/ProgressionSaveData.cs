using System.Collections.Generic;

namespace Core.Data
{
    [System.Serializable]
    public class ProgressionSaveData
    {
        public List<string> unlockedAreas;
        public List<string> unlockedStations;
        public List<string> unlockedMiniGames;
        public List<string> completedMiniGames;

        public static ProgressionSaveData CreateDefault()
        {
            return new ProgressionSaveData
            {
                unlockedAreas = new List<string>(),
                unlockedStations = new List<string>(),
                unlockedMiniGames = new List<string>(),
                completedMiniGames = new List<string>()
            };
        }
    }
}
