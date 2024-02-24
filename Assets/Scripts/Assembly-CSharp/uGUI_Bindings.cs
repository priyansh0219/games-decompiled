using System;
using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;

public class uGUI_Bindings : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler, ICompileTimeCheckable
{
	[AssertNotNull]
	public GameObject selectionBackground;

	[AssertNotNull]
	public uGUI_Binding[] bindings;

	[SerializeField]
	private FMODAsset hoverSound;

	public void Initialize(GameInput.Device device, GameInput.Button button)
	{
		for (int i = 0; i < bindings.Length; i++)
		{
			bindings[i].Initialize(device, button, (GameInput.BindingSet)i);
		}
	}

	public void OnPrimaryDeviceChanged()
	{
		for (int i = 0; i < bindings.Length; i++)
		{
			bindings[i].OnPrimaryDeviceChanged();
		}
	}

	public void OnBindingsChanged()
	{
		for (int i = 0; i < bindings.Length; i++)
		{
			bindings[i].OnBindingsChanged();
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		eventData.Use();
		SetSelected(selected: true);
		if (hoverSound != null)
		{
			RuntimeManager.PlayOneShot(hoverSound.path);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		eventData.Use();
		SetSelected(selected: false);
	}

	private void SetSelected(bool selected)
	{
		selectionBackground.SetActive(selected && GameInput.PrimaryDevice == GameInput.Device.Controller);
	}

	public string CompileTimeCheck()
	{
		Array values = Enum.GetValues(typeof(GameInput.BindingSet));
		if (bindings.Length != values.Length)
		{
			return "Bindings must match binding sets count";
		}
		return null;
	}
}
