using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_SceneConfirmation : uGUI_InputGroup, uGUI_IButtonReceiver
{
	public delegate void ConfirmationFinishedDelegate(bool confirmed);

	public TextMeshProUGUI description;

	[Tooltip("Button shown when a confirmation delegate is not provided to Show.")]
	public Button ok;

	[AssertNotNull]
	[Tooltip("Button shown when a confirmation delegate is provided to Show.")]
	public Button yes;

	[AssertNotNull]
	[Tooltip("Button shown when a confirmation delegate is provided to Show.")]
	public Button no;

	private ConfirmationFinishedDelegate OnConfirmationFinished;

	public uGUI_NavigableControlGrid panel;

	private uGUI_InputGroup restoreGroup;

	private bool choice;

	private void Start()
	{
		base.gameObject.SetActive(value: false);
	}

	public override void OnSelect(bool lockMovement)
	{
		base.OnSelect(lockMovement);
		GamepadInputModule.current.SetCurrentGrid(panel);
	}

	public override void OnDeselect()
	{
		base.OnDeselect();
		base.gameObject.SetActive(value: false);
		if (OnConfirmationFinished != null)
		{
			OnConfirmationFinished(choice);
		}
	}

	public void Show(string descriptionText, ConfirmationFinishedDelegate callback = null, uGUI_InputGroup restore = null)
	{
		choice = false;
		OnConfirmationFinished = callback;
		description.text = descriptionText;
		base.gameObject.SetActive(value: true);
		if (ok != null)
		{
			bool flag = callback == null;
			ok.gameObject.SetActive(flag);
			yes.gameObject.SetActive(!flag);
			no.gameObject.SetActive(!flag);
		}
		restoreGroup = null;
		uGUI_InputGroup uGUI_InputGroup2 = Select(lockMovement: true);
		restoreGroup = ((restore != null) ? restore : uGUI_InputGroup2);
	}

	private void Close(bool result)
	{
		choice = result;
		uGUI_InputGroup uGUI_InputGroup2 = restoreGroup;
		restoreGroup = null;
		Deselect(uGUI_InputGroup2);
	}

	public bool OnButtonDown(GameInput.Button button)
	{
		if (button == GameInput.Button.UICancel)
		{
			Close(result: false);
			return true;
		}
		return false;
	}

	public void OnYes()
	{
		Close(result: true);
	}

	public void OnNo()
	{
		Close(result: false);
	}
}
