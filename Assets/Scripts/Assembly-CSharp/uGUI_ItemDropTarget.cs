using UnityEngine;
using UnityEngine.EventSystems;

public class uGUI_ItemDropTarget : MonoBehaviour, IDropHandler, IEventSystemHandler, IDragHoverHandler
{
	[AssertNotNull]
	public RectTransform dropTarget;

	public void OnDrop(PointerEventData eventData)
	{
		if (!ItemDragManager.isDragging)
		{
			return;
		}
		InventoryItem draggedItem = ItemDragManager.draggedItem;
		if (draggedItem != null)
		{
			Inventory main = Inventory.main;
			if ((main.GetAllItemActions(draggedItem) & ItemAction.Drop) != 0)
			{
				main.ExecuteItemAction(ItemAction.Drop, draggedItem);
			}
		}
		ItemDragManager.DragStop();
	}

	public void OnDragHoverEnter(PointerEventData eventData)
	{
	}

	public void OnDragHoverStay(PointerEventData eventData)
	{
		if (ItemDragManager.isDragging)
		{
			InventoryItem draggedItem = ItemDragManager.draggedItem;
			if (draggedItem != null && (Inventory.main.GetAllItemActions(draggedItem) & ItemAction.Drop) != 0)
			{
				ItemDragManager.SetActionHint(ItemActionHint.Drop);
			}
		}
	}

	public void OnDragHoverExit(PointerEventData eventData)
	{
	}
}
