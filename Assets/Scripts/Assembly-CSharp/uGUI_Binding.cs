using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class uGUI_Binding : Selectable, IPointerClickHandler, IEventSystemHandler, ISubmitHandler, ICancelHandler, IUpdateSelectedHandler
{
	[AssertNotNull]
	public uGUI_BindingData data;

	public TextMeshProUGUI currentText;

	private GameInput.Device device;

	private GameInput.Button action;

	private GameInput.BindingSet bindingSet;

	private string binding;

	private bool isRebinding;

	protected override void OnEnable()
	{
		base.OnEnable();
		RefreshValue();
		UpdateState();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		UpdateState();
		FPSInputModule.BubbleEvent(base.gameObject, eventData, ExecuteEvents.selectHandler);
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		UpdateState(forceDeselect: true);
		FPSInputModule.BubbleEvent(base.gameObject, eventData, ExecuteEvents.deselectHandler);
	}

	public void Initialize(GameInput.Device device, GameInput.Button button, GameInput.BindingSet bindingSet)
	{
		this.device = device;
		action = button;
		this.bindingSet = bindingSet;
		RefreshValue();
		UpdateState();
	}

	public void OnPrimaryDeviceChanged()
	{
		UpdateState();
	}

	public void OnBindingsChanged()
	{
		RefreshValue();
		UpdateState();
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			GameInput.TryBind(device, action, bindingSet, null);
		}
		else
		{
			StartRebind();
		}
	}

	void ISubmitHandler.OnSubmit(BaseEventData eventData)
	{
		StartRebind();
	}

	void ICancelHandler.OnCancel(BaseEventData eventData)
	{
		GameInput.CancelRebind();
	}

	void IUpdateSelectedHandler.OnUpdateSelected(BaseEventData eventData)
	{
		if (GameInput.PrimaryDevice == device && GameInput.GetButtonDown(GameInput.Button.UIClear))
		{
			GameInput.SetBinding(device, action, bindingSet, string.Empty);
		}
	}

	private void StartRebind()
	{
		if (!isRebinding && GameInput.StartRebind(device, action, bindingSet, OnRebindCallback))
		{
			isRebinding = true;
			RefreshValue();
			UpdateState();
		}
	}

	private void OnRebindCallback(int state)
	{
		isRebinding = false;
		RefreshValue();
		UpdateState();
		EventSystem.current.SetSelectedGameObject(null);
	}

	private void RefreshValue()
	{
		binding = GameInput.GetBinding(device, action, bindingSet);
		currentText.text = (isRebinding ? string.Empty : GameInput.GetDisplayText(binding));
	}

	private void UpdateState(bool forceDeselect = false)
	{
		EventSystem current = EventSystem.current;
		bool flag = !forceDeselect && current != null && current.currentSelectedGameObject == base.gameObject;
		if (GameInput.PrimaryDevice == GameInput.Device.Controller)
		{
			if (isRebinding)
			{
				base.spriteState = data.stateActive;
			}
			else if (flag)
			{
				base.spriteState = data.stateSelected;
			}
			else
			{
				base.spriteState = data.stateNormal;
			}
			Language main = Language.main;
			if (isRebinding)
			{
				uGUI_LegendBar.ClearButtons();
				uGUI_LegendBar.ChangeButton(0, string.Empty, main.GetFormat("PressToBindFormat", main.Get("Option" + action)));
			}
			else
			{
				if (!flag)
				{
					return;
				}
				uGUI_LegendBar.ClearButtons();
				int num = -1;
				uGUI_LegendBar.ChangeButton(++num, GameInput.FormatButton(GameInput.Button.UICancel), main.Get("Back"));
				if (GameInput.PrimaryDevice == device)
				{
					uGUI_LegendBar.ChangeButton(++num, GameInput.FormatButton(GameInput.Button.UISubmit), main.Get("InputBind"));
					if (binding != null)
					{
						uGUI_LegendBar.ChangeButton(++num, GameInput.FormatButton(GameInput.Button.UIClear), main.Get("InputClear"));
					}
				}
			}
		}
		else if (isRebinding)
		{
			base.spriteState = data.stateActive;
		}
		else
		{
			base.spriteState = data.stateNormal;
		}
	}
}
