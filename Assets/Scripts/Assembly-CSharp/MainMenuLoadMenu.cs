using System;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuLoadMenu : MonoBehaviour, uGUI_INavigableIconGrid, uGUI_IButtonReceiver
{
	public Sprite normalSprite;

	public Sprite selectedSprite;

	public Scrollbar scrollbar;

	public Transform content;

	private GameObject selectedItem;

	[AssertNotNull]
	[SerializeField]
	private FMODAsset hoverSound;

	[NonSerialized]
	public bool isDeletingInProgress;

	public bool isLoading { get; set; }

	bool uGUI_INavigableIconGrid.ShowSelector => false;

	bool uGUI_INavigableIconGrid.EmulateRaycast => false;

	public object GetSelectedItem()
	{
		return selectedItem;
	}

	public Graphic GetSelectedIcon()
	{
		return null;
	}

	public void SelectItem(object item)
	{
		if (isDeletingInProgress)
		{
			return;
		}
		DeselectItem();
		selectedItem = item as GameObject;
		selectedItem.transform.GetChild(0).GetComponent<Image>().sprite = selectedSprite;
		mGUI_Change_Legend_On_Select componentInChildren = selectedItem.GetComponentInChildren<mGUI_Change_Legend_On_Select>();
		if ((bool)componentInChildren)
		{
			componentInChildren.SyncLegendBarToGUISelection();
		}
		UIUtils.ScrollToShowItemInCenter(selectedItem.transform);
		TextMeshProUGUI[] componentsInChildren = selectedItem.GetComponentsInChildren<TextMeshProUGUI>();
		foreach (TextMeshProUGUI textMeshProUGUI in componentsInChildren)
		{
			if (textMeshProUGUI.gameObject.name != "SaveGameMode")
			{
				textMeshProUGUI.color = Color.black;
			}
		}
		RuntimeManager.PlayOneShot(hoverSound.path);
	}

	public void DeselectItem()
	{
		if (!(selectedItem == null))
		{
			selectedItem.transform.GetChild(0).GetComponent<Image>().sprite = normalSprite;
			TextMeshProUGUI[] componentsInChildren = selectedItem.GetComponentsInChildren<TextMeshProUGUI>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].color = Color.white;
			}
			selectedItem = null;
		}
	}

	public bool SelectFirstItem()
	{
		for (int i = 0; i < content.childCount; i++)
		{
			GameObject item = content.GetChild(i).gameObject;
			if (IsItemInteractable(item))
			{
				SelectItem(item);
				return true;
			}
		}
		return false;
	}

	private bool IsItemInteractable(GameObject item)
	{
		if (!item.activeInHierarchy)
		{
			return false;
		}
		MainMenuLoadButton component = item.GetComponent<MainMenuLoadButton>();
		if (component != null && component.deleting)
		{
			return false;
		}
		return true;
	}

	public bool SelectItemClosestToPosition(Vector3 worldPos)
	{
		return false;
	}

	public bool SelectItemInDirection(int dirX, int dirY)
	{
		if (selectedItem == null)
		{
			return SelectFirstItem();
		}
		if (dirY == 0)
		{
			return false;
		}
		int selectedIndex = GetSelectedIndex();
		return SelectItemInDirection(selectedIndex, dirX, dirY);
	}

	public uGUI_INavigableIconGrid GetNavigableGridInDirection(int dirX, int dirY)
	{
		return null;
	}

	public int GetSelectedIndex()
	{
		if (selectedItem != null)
		{
			return selectedItem.transform.GetSiblingIndex();
		}
		return -1;
	}

	public bool SelectItemInDirection(int selectedIndex, int dirX, int dirY)
	{
		int num = ((dirY > 0) ? (selectedIndex + 1) : (selectedIndex - 1));
		int num2 = ((dirY > 0) ? 1 : (-1));
		for (int i = num; i >= 0 && i < content.childCount; i += num2)
		{
			if (SelectItemByIndex(i))
			{
				return true;
			}
		}
		return false;
	}

	public bool SelectItemByIndex(int selectedIndex)
	{
		if (selectedIndex >= content.childCount || selectedIndex < 0)
		{
			return false;
		}
		GameObject item = content.GetChild(selectedIndex).gameObject;
		if (!IsItemInteractable(item))
		{
			return false;
		}
		SelectItem(item);
		return true;
	}

	public bool OnButtonDown(GameInput.Button button)
	{
		switch (button)
		{
		case GameInput.Button.UISubmit:
			OnConfirm();
			return true;
		case GameInput.Button.UIClear:
			OnClear();
			return true;
		case GameInput.Button.UICancel:
			OnBack();
			return true;
		default:
			return false;
		}
	}

	public void OnClear()
	{
		MainMenuLoadButton component = selectedItem.GetComponent<MainMenuLoadButton>();
		if (component != null)
		{
			component.RequestDelete();
		}
	}

	public void OnConfirm()
	{
		if (!(selectedItem != null))
		{
			return;
		}
		if (selectedItem.gameObject.name == "NewGame")
		{
			selectedItem.GetComponentInChildren<Button>().onClick.Invoke();
			return;
		}
		MainMenuLoadButton component = selectedItem.GetComponent<MainMenuLoadButton>();
		if (!component.IsEmpty())
		{
			component.Load();
		}
	}

	public void OnBack()
	{
		MainMenuRightSide.main.OpenGroup("Home");
	}
}
