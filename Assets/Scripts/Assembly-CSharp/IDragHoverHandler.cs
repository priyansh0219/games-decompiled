using UnityEngine.EventSystems;

public interface IDragHoverHandler : IEventSystemHandler
{
	void OnDragHoverEnter(PointerEventData eventData);

	void OnDragHoverStay(PointerEventData eventData);

	void OnDragHoverExit(PointerEventData eventData);
}
