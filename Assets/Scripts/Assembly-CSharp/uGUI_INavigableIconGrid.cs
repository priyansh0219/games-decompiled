using UnityEngine;
using UnityEngine.UI;

public interface uGUI_INavigableIconGrid
{
	bool ShowSelector { get; }

	bool EmulateRaycast { get; }

	object GetSelectedItem();

	Graphic GetSelectedIcon();

	void SelectItem(object item);

	void DeselectItem();

	bool SelectFirstItem();

	bool SelectItemClosestToPosition(Vector3 worldPos);

	bool SelectItemInDirection(int dirX, int dirY);

	uGUI_INavigableIconGrid GetNavigableGridInDirection(int dirX, int dirY);
}
