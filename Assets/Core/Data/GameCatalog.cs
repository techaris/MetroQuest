using System.Collections.Generic;
using UnityEngine;

namespace Core.Data
{
    [System.Serializable]
    public class GameCatalog
    {
        public List<SceneEntryData> scenes = new List<SceneEntryData>();
    }
}
