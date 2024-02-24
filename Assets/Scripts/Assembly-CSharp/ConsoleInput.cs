using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConsoleInput : TMP_InputField
{
	public delegate bool Submit(string text);

	public KeyCode[] ignoreKeyCodes = new KeyCode[1];

	private Navigation _navigation = new Navigation
	{
		mode = Navigation.Mode.None
	};

	private const char emptyChar = '\0';

	private const char newLineChar = '\n';

	private int enabledInFrame = -1;

	private GameObject go;

	private List<string> history;

	private int historyIndex;

	private string cachedInput = string.Empty;

	private int cachedPos;

	private int cachedSelectPos;

	private bool activateNextUpdate;

	private bool valueChangeExpected;

	private Event processingEvent = new Event();

	private Queue<Event> eventQueue = new Queue<Event>();

	public new Navigation navigation => _navigation;

	public new Transition transition => Transition.None;

	public new ContentType contentType => ContentType.Custom;

	public new LineType lineType => LineType.SingleLine;

	public new InputType inputType => InputType.Standard;

	public new CharacterValidation characterValidation => CharacterValidation.None;

	public new TouchScreenKeyboardType keyboardType => TouchScreenKeyboardType.Default;

	public new OnValidateInput onValidateInput => null;

	public new OnChangeEvent onValueChanged => null;

	public new SubmitEvent onEndEdit => new SubmitEvent();

	private bool enabledThisFrame
	{
		get
		{
			return enabledInFrame == Time.frameCount;
		}
		set
		{
			if (value)
			{
				enabledInFrame = Time.frameCount;
			}
		}
	}

	public event PlatformServicesUtils.VirtualKeyboardFinished OnTouchScreenKeyboardDone;

	public new event Submit onSubmit;

	protected override void Awake()
	{
		base.Awake();
		base.contentType = ContentType.Custom;
		base.lineType = LineType.SingleLine;
		base.inputType = InputType.Standard;
		base.characterValidation = CharacterValidation.None;
		base.keyboardType = TouchScreenKeyboardType.Default;
		base.onValidateInput = Validate;
		base.onValueChanged.AddListener(ValueChanged);
		go = base.gameObject;
	}

	protected override void Start()
	{
		base.Start();
		base.image = GetComponent<Image>();
	}

	private void Update()
	{
		if (activateNextUpdate)
		{
			ActivateInputField();
			if (!EventSystem.current.alreadySelecting)
			{
				EventSystem.current.SetSelectedGameObject(go);
				activateNextUpdate = false;
			}
		}
	}

	public void SetHistory(List<string> h)
	{
		history = h;
		historyIndex = history.Count;
	}

	public override void OnUpdateSelected(BaseEventData eventData)
	{
		if (enabledThisFrame)
		{
			if (base.isFocused)
			{
				eventData.Use();
			}
			return;
		}
		eventQueue.Clear();
		if (base.isFocused)
		{
			while (Event.PopEvent(processingEvent))
			{
				eventQueue.Enqueue(new Event(processingEvent));
			}
		}
		base.OnUpdateSelected(eventData);
		if (!base.isFocused)
		{
			return;
		}
		bool flag = false;
		while (eventQueue.Count > 0)
		{
			processingEvent = eventQueue.Dequeue();
			if (processingEvent.rawType == EventType.KeyDown)
			{
				flag = true;
				if (KeyPressedOverride(processingEvent) || KeyPressed(processingEvent) == EditState.Finish)
				{
					break;
				}
			}
		}
		if (flag)
		{
			UpdateLabel();
		}
		eventData.Use();
	}

	private bool KeyPressedOverride(Event evt)
	{
		switch (processingEvent.keyCode)
		{
		case KeyCode.Return:
		case KeyCode.KeypadEnter:
			SubmitInput(base.text);
			return false;
		case KeyCode.Backspace:
		case KeyCode.Delete:
			historyIndex = history.Count;
			break;
		case KeyCode.UpArrow:
			return ProcessUpKey();
		case KeyCode.DownArrow:
			return ProcessDownKey();
		}
		return false;
	}

	private bool ProcessUpKey()
	{
		if (historyIndex == history.Count)
		{
			CacheInput();
		}
		if (historyIndex > 0)
		{
			historyIndex--;
			if (historyIndex != history.Count)
			{
				SetText(history[historyIndex]);
				SelectAll();
			}
			return true;
		}
		return false;
	}

	private bool ProcessDownKey()
	{
		if (historyIndex < history.Count - 1)
		{
			historyIndex++;
			SetText(history[historyIndex]);
			SelectAll();
			return true;
		}
		if (historyIndex == history.Count - 1)
		{
			historyIndex++;
			SetText(cachedInput);
			base.caretPositionInternal = cachedPos;
			base.caretSelectPositionInternal = cachedSelectPos;
			return true;
		}
		if (historyIndex == history.Count)
		{
			SetText(string.Empty);
			cachedInput = string.Empty;
			base.caretPosition = 0;
			return true;
		}
		return false;
	}

	protected new char Validate(string text, int pos, char ch)
	{
		if (enabledThisFrame)
		{
			return '\0';
		}
		for (int i = 0; i < ignoreKeyCodes.Length; i++)
		{
			KeyCode key = ignoreKeyCodes[i];
			if (Input.GetKey(key) || Input.GetKeyDown(key) || Input.GetKeyUp(key))
			{
				return '\0';
			}
		}
		switch (ch)
		{
		case '\n':
			return '\0';
		case 'a':
		case 'b':
		case 'c':
		case 'd':
		case 'e':
		case 'f':
		case 'g':
		case 'h':
		case 'i':
		case 'j':
		case 'k':
		case 'l':
		case 'm':
		case 'n':
		case 'o':
		case 'p':
		case 'q':
		case 'r':
		case 's':
		case 't':
		case 'u':
		case 'v':
		case 'w':
		case 'x':
		case 'y':
		case 'z':
			return ch;
		default:
			if (ch >= 'A' && ch <= 'Z')
			{
				return ch;
			}
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
			if (" -_./:()".IndexOf(ch) != -1)
			{
				return ch;
			}
			return '\0';
		}
	}

	private void SetText(string t)
	{
		valueChangeExpected = true;
		base.text = t;
		valueChangeExpected = false;
	}

	protected void ValueChanged(string newValue)
	{
		if (!valueChangeExpected)
		{
			historyIndex = history.Count;
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		this.OnTouchScreenKeyboardDone?.Invoke(m_SoftKeyboard.status == TouchScreenKeyboard.Status.Done, m_SoftKeyboard.status == TouchScreenKeyboard.Status.Canceled, base.text);
		activateNextUpdate = true;
	}

	public void Activate()
	{
		base.enabled = true;
		SetElements(value: true);
		Select();
		base.text = cachedInput;
		ActivateInputField();
		MoveTextEnd(shift: false);
		enabledThisFrame = true;
	}

	public void Deactivate()
	{
		if (historyIndex >= history.Count - 1)
		{
			CacheInput();
		}
		Clear();
		historyIndex = history.Count;
		DeactivateInputField();
		base.OnDeselect((BaseEventData)null);
		base.enabled = false;
		SetElements(value: false);
		Rebuild(CanvasUpdate.LatePreRender);
	}

	private void SetElements(bool value)
	{
		if (base.image != null)
		{
			base.image.enabled = value;
		}
		if (base.textComponent != null)
		{
			base.textComponent.enabled = value;
		}
	}

	private void Clear()
	{
		base.text = string.Empty;
	}

	private void SubmitInput(string value)
	{
		if (!enabledThisFrame)
		{
			ActivateInputField();
			Select();
			bool flag = false;
			if (this.onSubmit != null)
			{
				flag = this.onSubmit(base.text);
			}
			cachedInput = string.Empty;
			Clear();
			if (flag && (history.Count == 0 || !string.Equals(value, history[history.Count - 1])))
			{
				history.Add(value);
			}
			historyIndex = history.Count;
		}
	}

	public override void OnSubmit(BaseEventData eventData)
	{
	}

	protected new void SendOnSubmit()
	{
	}

	private void CacheInput()
	{
		cachedInput = base.text;
		cachedPos = m_CaretPosition;
		cachedSelectPos = m_CaretSelectPosition;
	}

	private string GetDebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < history.Count; i++)
		{
			stringBuilder.Append(history[i]);
			if (i == historyIndex)
			{
				stringBuilder.Append(" <");
				stringBuilder.Append(historyIndex);
			}
			stringBuilder.Append('\n');
		}
		stringBuilder.Append("cachedInput: ");
		stringBuilder.Append(cachedInput);
		if (historyIndex == history.Count)
		{
			stringBuilder.Append(" <");
			stringBuilder.Append(historyIndex);
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("\nisFocused = {0}", base.isFocused.ToString());
		stringBuilder.AppendFormat("\ncachedPos = {0}", cachedPos.ToString());
		stringBuilder.AppendFormat("\ncachedSelectPos = {0}", cachedSelectPos.ToString());
		return stringBuilder.ToString();
	}
}
