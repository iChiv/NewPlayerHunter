using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NewPlayerHunter.Gameplay
{
    public sealed class PlayerCardDragHandler : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerClickHandler
    {
        private IGameInteractionController _controller;
        private string _playerId;
        private CanvasGroup _canvasGroup;
        private RectTransform _dragGhost;
        private Coroutine _punchCoroutine;

        public string PlayerId => _playerId;

        public void Configure(IGameInteractionController controller, string playerId)
        {
            _controller = controller;
            _playerId = playerId;
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_controller == null || _controller.IsGameComplete)
            {
                return;
            }

            _controller.SelectPlayer(_playerId);
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.alpha = 0.55f;
            }

            _dragGhost = _controller.CanvasTransform.Find("DragGhost") as RectTransform;
            if (_dragGhost == null)
            {
                Debug.LogError(
                    "[NewPlayerHunter] Game scene is missing the authored DragGhost.");
                return;
            }

            var label = _dragGhost.Find("Label").GetComponent<TextMeshProUGUI>();
            label.text = _controller.GetPlayerDisplayName(_playerId);
            _dragGhost.position = eventData.position;
            _dragGhost.SetAsLastSibling();
            _dragGhost.gameObject.SetActive(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_dragGhost != null)
            {
                _dragGhost.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.alpha = 1f;
            }

            if (_dragGhost != null)
            {
                _dragGhost.gameObject.SetActive(false);
                _dragGhost = null;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _controller?.SelectPlayer(_playerId);
            if (_punchCoroutine != null)
            {
                StopCoroutine(_punchCoroutine);
            }

            _punchCoroutine = StartCoroutine(PunchRoutine());
        }

        private IEnumerator PunchRoutine()
        {
            const float duration = 0.2f;
            for (var elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                var progress = elapsed / duration;
                var scale = progress < 0.5f
                    ? Mathf.Lerp(1f, 0.95f, progress * 2f)
                    : Mathf.Lerp(0.95f, 1f, (progress - 0.5f) * 2f);
                transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            transform.localScale = Vector3.one;
            _punchCoroutine = null;
        }
    }

}
