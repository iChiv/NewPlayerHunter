using UnityEngine;

namespace NewPlayerHunter.Gameplay
{
    public interface IGameInteractionController
    {
        bool IsGameComplete { get; }

        RectTransform CanvasTransform { get; }

        void SelectPlayer(string playerId);

        void HandleSlotClicked(string slotId);

        bool AssignPlayerToSlot(string slotId, string playerId);

        string GetPlayerDisplayName(string playerId);
    }
}
