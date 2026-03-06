using UnityEngine;

namespace Core.Interfaces
{
    /// <summary>
    /// Contract for modular mini-games in the framework.
    /// </summary>
    public interface IMiniGame
    {
        void Initialize();
        void StartGame();
        void EndGame();
    }
}
