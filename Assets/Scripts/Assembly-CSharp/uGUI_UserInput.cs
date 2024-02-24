using TMPro;
using UnityEngine;

public class uGUI_UserInput : uGUI_InputGroup, uGUI_IButtonReceiver
{
	public delegate void UserInputCallback(string text);

	public uGUI_NavigableControlGrid grid;

	public TextMeshProUGUI title;

	public TMP_InputField inputField;

	public TextMeshProUGUI buttonText;

	private UserInputCallback callback;

	private CanvasGroup canvasGroup;

	private bool submit;

	protected override void Awake()
	{
		base.Awake();
		canvasGroup = GetComponent<CanvasGroup>();
		if (canvasGroup == null)
		{
			canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
		}
		SetState(newState: false);
	}

	private void SetState(bool newState)
	{
		if (newState)
		{
			canvasGroup.alpha = 1f;
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
			GamepadInputModule current = GamepadInputModule.current;
			if (current != null && !current.UsingController)
			{
				inputField.Select();
			}
		}
		else
		{
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}
	}

	public void RequestString(string titleText, string buttonLabel, string currentValue, int maxChars, UserInputCallback callback)
	{
		if (callback != null)
		{
			title.text = titleText;
			inputField.characterLimit = maxChars;
			inputField.MoveTextStart(shift: false);
			inputField.text = currentValue;
			inputField.MoveTextEnd(shift: true);
			buttonText.text = buttonLabel;
			this.callback = callback;
			Select();
		}
	}

	public void OnEndEdit()
	{
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			Close(_submit: true);
		}
	}

	public void OnSubmit()
	{
		Close(_submit: true);
	}

	public override void OnSelect(bool lockMovement)
	{
		base.OnSelect(lockMovement: true);
		SetState(newState: true);
		GamepadInputModule.current.SetCurrentGrid(grid);
	}

	public override void OnReselect(bool lockMovement)
	{
		base.OnReselect(lockMovement: true);
	}

	public override void OnDeselect()
	{
		base.OnDeselect();
		if (submit)
		{
			if (callback != null)
			{
				callback(inputField.text);
				callback = null;
			}
			submit = false;
		}
		SetState(newState: false);
	}

	public bool OnButtonDown(GameInput.Button button)
	{
		if (button == GameInput.Button.UICancel)
		{
			Close(_submit: false);
			return true;
		}
		return false;
	}

	private void Close(bool _submit)
	{
		submit = _submit;
		Deselect();
	}
}
