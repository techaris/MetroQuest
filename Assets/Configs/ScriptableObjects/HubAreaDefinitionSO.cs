using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "HubAreaDefinition", menuName = "MetroQuest/Hub Area Definition", order = 1)]
    public class HubAreaDefinitionSO : ScriptableObject
    {
        public string areaId;
        public string displayName;
        public string linkedMiniGameId;
        public bool unlockedByDefault;
        [TextArea(2, 4)]
        public string lockedMessage;
    }
}
