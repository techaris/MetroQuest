using System.Collections.Generic;

namespace Core.Data
{
    [System.Serializable]
    public class SaveData
    {
        public int saveVersion;
        public int softCurrency;
        public int hardCurrency;
        public int stars;

        public ProgressionSaveData progression;
        public HubSaveData hub;
        public CafeSaveData cafe;
        public MaintenanceSaveData maintenance;
        public MapSaveData map;

        /// <summary>
        /// Ensures nested objects and lists are non-null after JsonUtility load or partial data.
        /// </summary>
        public static void EnsureIntegrity(SaveData data)
        {
            if (data == null)
                return;

            data.progression ??= ProgressionSaveData.CreateDefault();
            data.hub ??= HubSaveData.CreateDefault();
            data.cafe ??= CafeSaveData.CreateDefault();
            data.maintenance ??= MaintenanceSaveData.CreateDefault();
            data.map ??= MapSaveData.CreateDefault();

            EnsureList(ref data.progression.unlockedAreas);
            EnsureList(ref data.progression.unlockedStations);
            EnsureList(ref data.progression.unlockedMiniGames);
            EnsureList(ref data.progression.completedMiniGames);

            EnsureList(ref data.hub.unlockedHubZones);
            EnsureList(ref data.hub.constructedAreas);

            EnsureList(ref data.map.unlockedStationIds);
            data.map.currentStationId ??= string.Empty;
        }

        private static void EnsureList(ref List<string> list)
        {
            list ??= new List<string>();
        }
    }
}
