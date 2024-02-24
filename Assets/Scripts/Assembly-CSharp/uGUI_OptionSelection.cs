using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class uGUI_OptionSelection : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[AssertLocalization]
	private const string labelToggle = "Toggle";

	[AssertLocalization]
	private const string labelModify = "Modify";

	[AssertLocalization]
	private const string labelClear = "Clear";

	public GameObject selectionBackground;

	[SerializeField]
	private FMODAsset hoverSound;

	public void OnSelect(BaseEventData data)
	{
		UpdateLegend();
		if (hoverSound != null)
		{
			RuntimeManager.PlayOneShot(hoverSound.path);
		}
	}

	public void OnDeselect(BaseEventData data)
	{
		selectionBackground.SetActive(value: false);
	}

	public void UpdateLegend()
	{
		if (GameInput.PrimaryDevice == GameInput.Device.Controller)
		{
			selectionBackground.SetActive(value: true);
		}
		uGUI_LegendBar.ClearButtons();
		Language main = Language.main;
		if (GetComponent<Toggle>() != null)
		{
			uGUI_LegendBar.ChangeButton(0, GameInput.FormatButton(GameInput.Button.UICancel), main.Get("Back"));
			uGUI_LegendBar.ChangeButton(1, GameInput.FormatButton(GameInput.Button.UISubmit), main.Get("Toggle"));
			return;
		}
		Slider component = GetComponent<Slider>();
		uGUI_Choice component2 = GetComponent<uGUI_Choice>();
		if (component != null || component2 != null)
		{
			uGUI_LegendBar.ChangeButton(0, GameInput.FormatButton(GameInput.Button.UICancel), main.Get("Back"));
			uGUI_LegendBar.ChangeButton(1, GameInput.FormatButton(GameInput.Button.UIAdjust), main.Get("Modify"));
		}
		else
		{
			uGUI_LegendBar.ChangeButton(0, GameInput.FormatButton(GameInput.Button.UICancel), main.Get("Back"));
			uGUI_LegendBar.ChangeButton(1, GameInput.FormatButton(GameInput.Button.UISubmit), main.Get("ItemSelectorSelect"));
		}
	}
}
