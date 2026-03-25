using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "StationDefinition", menuName = "MetroQuest/Station Definition", order = 2)]
    public class StationDefinitionSO : ScriptableObject
    {
        public string stationId;
        public string displayName;
        public bool unlockedByDefault;
    }
}
