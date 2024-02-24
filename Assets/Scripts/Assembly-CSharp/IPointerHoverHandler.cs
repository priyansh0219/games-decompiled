using UnityEngine.EventSystems;

public interface IPointerHoverHandler : IEventSystemHandler
{
	void OnPointerHover(PointerEventData eventData);
}
