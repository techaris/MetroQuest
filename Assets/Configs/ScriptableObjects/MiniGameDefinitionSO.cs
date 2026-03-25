using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "MiniGameDefinition", menuName = "MetroQuest/Mini Game Definition", order = 0)]
    public class MiniGameDefinitionSO : ScriptableObject
    {
        public string miniGameId;
        public string displayName;
        public string sceneName;
        public Sprite icon;
        public bool unlockedByDefault;
    }
}
