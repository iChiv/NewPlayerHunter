using UnityEngine;
using UnityEngine.EventSystems;

namespace NewPlayerHunter.Gameplay
{
    public sealed class DemandSlotDropTarget : MonoBehaviour,
        IDropHandler,
        IPointerClickHandler
    {
        private IGameInteractionController _controller;
        private string _slotId;

        public string SlotId => _slotId;

        public void Configure(
            IGameInteractionController controller,
            string slotId)
        {
            _controller = controller;
            _slotId = slotId;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var playerCard = eventData.pointerDrag == null
                ? null
                : eventData.pointerDrag.GetComponent<PlayerCardDragHandler>();
            if (playerCard != null)
            {
                _controller.AssignPlayerToSlot(
                    _slotId,
                    playerCard.PlayerId);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _controller?.HandleSlotClicked(_slotId);
        }
    }
}

