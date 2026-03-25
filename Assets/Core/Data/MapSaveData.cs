using System.Collections.Generic;

namespace Core.Data
{
    [System.Serializable]
    public class MapSaveData
    {
        public string currentStationId;
        public List<string> unlockedStationIds;

        public static MapSaveData CreateDefault()
        {
            return new MapSaveData
            {
                currentStationId = string.Empty,
                unlockedStationIds = new List<string>()
            };
        }
    }
}
