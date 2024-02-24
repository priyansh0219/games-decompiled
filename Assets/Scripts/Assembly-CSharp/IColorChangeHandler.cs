using UnityEngine.EventSystems;

public interface IColorChangeHandler : IEventSystemHandler
{
	void OnColorChange(ColorChangeEventData eventData);
}
