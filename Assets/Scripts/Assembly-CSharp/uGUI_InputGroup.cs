using System.Collections.Generic;
using UWE;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class uGUI_InputGroup : MonoBehaviour
{
	private GameObject _inputDummy;

	private bool cursorLockCached = true;

	[InputButtonBoolArray]
	[SerializeField]
	private bool[] acceptedMouseButtonsForActivation = new bool[3] { true, true, true };

	private GameObject inputDummy
	{
		get
		{
			if (_inputDummy == null)
			{
				_inputDummy = new GameObject("InputDummy");
				_inputDummy.SetActive(value: false);
			}
			return _inputDummy;
		}
	}

	public bool selected { get; private set; }

	public bool focused
	{
		get
		{
			if (selected)
			{
				return inputDummy.activeSelf;
			}
			return false;
		}
	}

	protected virtual void Awake()
	{
	}

	protected virtual void Update()
	{
		if (focused && Input.GetKeyDown(KeyCode.Escape))
		{
			Deselect();
		}
	}

	protected virtual void OnDisable()
	{
		Deselect();
	}

	public uGUI_InputGroup Select(bool lockMovement = false)
	{
		return FPSInputModule.SelectGroup(this, lockMovement);
	}

	public void Deselect()
	{
		Deselect(null);
	}

	public void Deselect(uGUI_InputGroup restoreGroup)
	{
		if (selected)
		{
			FPSInputModule.DeselectGroup(this, restoreGroup);
		}
	}

	public virtual void OnSelect(bool lockMovement)
	{
		selected = true;
		InterceptInput(state: true);
		if (lockMovement)
		{
			LockMovement(state: true);
		}
	}

	public virtual void OnDeselect()
	{
		selected = false;
		InterceptInput(state: false);
		LockMovement(state: false);
	}

	public virtual void OnReselect(bool lockMovement)
	{
		if (!selected)
		{
			OnSelect(lockMovement);
		}
		else
		{
			LockMovement(lockMovement);
		}
	}

	public virtual bool Raycast(PointerEventData eventData, List<RaycastResult> raycastResults)
	{
		BaseRaycaster componentInChildren = GetComponentInChildren<BaseRaycaster>();
		if (componentInChildren != null && componentInChildren.IsActive())
		{
			componentInChildren.Raycast(eventData, raycastResults);
			return true;
		}
		return false;
	}

	private void InterceptInput(bool state)
	{
		if (inputDummy.activeSelf != state)
		{
			if (state)
			{
				InputHandlerStack.main.Push(inputDummy);
				cursorLockCached = UWE.Utils.lockCursor;
				UWE.Utils.lockCursor = false;
			}
			else
			{
				UWE.Utils.lockCursor = cursorLockCached;
				InputHandlerStack.main.Pop(inputDummy);
			}
		}
	}

	private void LockMovement(bool state)
	{
		FPSInputModule.current.lockMovement = state;
	}

	public bool IsAcceptedToOpenWithButton(PointerEventData.InputButton button)
	{
		return acceptedMouseButtonsForActivation[(int)button];
	}
}
