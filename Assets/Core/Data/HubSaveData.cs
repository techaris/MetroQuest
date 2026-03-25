using System.Collections.Generic;

namespace Core.Data
{
    [System.Serializable]
    public class HubSaveData
    {
        public List<string> unlockedHubZones;
        public List<string> constructedAreas;
        public int stationVisualLevel;

        public static HubSaveData CreateDefault()
        {
            return new HubSaveData
            {
                unlockedHubZones = new List<string>(),
                constructedAreas = new List<string>(),
                stationVisualLevel = 0
            };
        }
    }
}
