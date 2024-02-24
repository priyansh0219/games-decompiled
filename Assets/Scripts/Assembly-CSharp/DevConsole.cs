using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UWE;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DevConsole : uGUI_InputGroup
{
	public delegate void Callback(NotificationCenter.Notification n);

	private class CommandData
	{
		public bool caseSensitive;

		public bool combineArgs;

		public HashSet<object> listeners = new HashSet<object>();
	}

	private static DevConsole instance;

	private const string historyKey = "SNConsoleHistory";

	private const int characterLimit = 256;

	private const int maxHistory = 10;

	private const string commandPrefix = "OnConsoleCommand_";

	public TMP_FontAsset font;

	public Sprite background;

	public Vector2 border = new Vector2(4f, 4f);

	private ITouchScreenKeyboard softKeyboard;

	private static readonly Dictionary<string, CommandData> commands = new Dictionary<string, CommandData>(StringComparer.InvariantCultureIgnoreCase);

	private bool hasUsedConsole;

	private bool state;

	private GameObject inputFieldGO;

	private RectTransform inputFieldTr;

	private ConsoleInput inputField;

	private Image inputImage;

	private TextMeshProUGUI textField;

	[SerializeField]
	[AssertNotNull]
	private uGUI_GraphicRaycaster interactionRaycaster;

	private List<string> history = new List<string>();

	protected override void Awake()
	{
		base.Awake();
		if (instance != null)
		{
			Debug.LogError("Multiple DevConsole instances detected!");
			UnityEngine.Object.Destroy(this);
			return;
		}
		instance = this;
		LoadHistory();
		_ = base.gameObject;
		RectTransform component = GetComponent<RectTransform>();
		inputFieldGO = new GameObject("InputField");
		inputImage = inputFieldGO.AddComponent<Image>();
		inputImage.sprite = background;
		inputImage.type = Image.Type.Sliced;
		inputFieldGO.AddComponent<RectMask2D>();
		inputField = inputFieldGO.AddComponent<ConsoleInput>();
		inputField.SetHistory(history);
		inputField.characterLimit = 256;
		inputField.selectionColor = new Color(0f, 0f, 0f, 0.753f);
		inputField.caretBlinkRate = 2f;
		inputField.ignoreKeyCodes = new KeyCode[2]
		{
			KeyCode.Return,
			KeyCode.KeypadEnter
		};
		inputField.onSubmit += OnSubmit;
		inputFieldTr = inputFieldGO.GetComponent<RectTransform>();
		inputFieldTr.SetParent(component, worldPositionStays: false);
		inputFieldTr.anchorMin = new Vector2(0f, 0f);
		inputFieldTr.anchorMax = new Vector2(0f, 0f);
		inputFieldTr.pivot = new Vector2(0f, 0f);
		inputFieldTr.anchoredPosition = new Vector2(10f, 10f);
		inputField.textViewport = inputFieldTr;
		GameObject gameObject = new GameObject("Text");
		textField = gameObject.AddComponent<TextMeshProUGUI>();
		textField.richText = false;
		textField.fontSize = 16f;
		textField.font = font;
		textField.alignment = TextAlignmentOptions.TopLeft;
		textField.overflowMode = TextOverflowModes.Overflow;
		textField.color = new Color(1f, 1f, 1f, 1f);
		textField.maskable = true;
		RectTransform component2 = gameObject.GetComponent<RectTransform>();
		component2.SetParent(inputFieldTr, worldPositionStays: false);
		component2.anchorMin = new Vector2(0f, 0f);
		component2.anchorMax = new Vector2(1f, 1f);
		component2.offsetMin = border;
		component2.offsetMax = -border;
		inputField.textComponent = textField;
		textField.text = "\u200b";
		textField.ForceMeshUpdate();
		inputFieldTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 300f);
		inputFieldTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textField.preferredHeight + 2f * border.y);
		inputFieldTr.SetAsLastSibling();
		inputField.Deactivate();
		interactionRaycaster.updateRaycasterStatusDelegate = SetRaycasterStatus;
	}

	private void Start()
	{
		RegisterConsoleCommand(this, "commands");
		RegisterConsoleCommand(this, "clearhistory");
		RegisterConsoleCommand(this, "developermode");
	}

	private bool ShouldEnableDevTools()
	{
		if ((Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.BackQuote)) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
		{
			return true;
		}
		return false;
	}

	protected override void Update()
	{
		if (FreezeTime.PleaseWait)
		{
			return;
		}
		if (ShouldEnableDevTools())
		{
			PlatformUtils.SetDevToolsEnabled(enabled: true);
		}
		if (!PlatformUtils.GetDevToolsEnabled())
		{
			if (state)
			{
				SetState(value: false);
			}
			return;
		}
		GameObject gameObject = null;
		if (EventSystem.current != null)
		{
			gameObject = EventSystem.current.currentSelectedGameObject;
		}
		if (state || gameObject == null || gameObject.GetComponent<TMP_InputField>() == null)
		{
			if (Input.GetKeyDown(KeyCode.BackQuote))
			{
				ToggleState();
			}
			else if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && !state)
			{
				SetState(value: true);
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			SetState(value: false);
		}
		if (softKeyboard == null)
		{
			return;
		}
		switch (softKeyboard.status)
		{
		case TouchScreenKeyboard.Status.Done:
		{
			string text = softKeyboard.text;
			if (OnSubmit(text))
			{
				history.Add(text);
			}
			break;
		}
		case TouchScreenKeyboard.Status.Canceled:
		case TouchScreenKeyboard.Status.LostFocus:
			SetState(value: false);
			break;
		case TouchScreenKeyboard.Status.Visible:
			break;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		SaveHistory();
	}

	public static void RegisterConsoleCommand(string command, Callback callback, bool caseSensitiveArgs = false, bool combineArgs = false)
	{
		if (!commands.TryGetValue(command, out var value))
		{
			value = new CommandData();
			value.caseSensitive = caseSensitiveArgs;
			value.combineArgs = combineArgs;
			commands.Add(command, value);
		}
		value.listeners.Add(callback);
	}

	[Obsolete("Use overload which accepts DevConsole.Callback instead")]
	public static void RegisterConsoleCommand(Component listener, string command, bool caseSensitiveArgs = false, bool combineArgs = false)
	{
		if (!commands.TryGetValue(command, out var value))
		{
			value = new CommandData();
			value.caseSensitive = caseSensitiveArgs;
			value.combineArgs = combineArgs;
			commands.Add(command, value);
		}
		value.listeners.Add(listener);
	}

	public static void UnregisterConsoleCommand(string command, Callback callback)
	{
		if (commands.TryGetValue(command, out var value))
		{
			value.listeners.Remove(callback);
		}
	}

	public static void SendConsoleCommand(string value)
	{
		instance.Submit(value);
	}

	public static bool HasUsedConsole()
	{
		if (!instance)
		{
			return false;
		}
		return instance.hasUsedConsole;
	}

	public static void SetUsedConsole(bool usedConsole)
	{
		if ((bool)instance)
		{
			instance.hasUsedConsole = usedConsole;
		}
	}

	public static void SubscribeOnKeyboardFinished(PlatformServicesUtils.VirtualKeyboardFinished callback)
	{
		if (instance != null)
		{
			instance.inputField.OnTouchScreenKeyboardDone += callback;
		}
	}

	private bool Submit(string value)
	{
		char[] separator = new char[2] { ' ', '\t' };
		string text = value.Trim();
		string[] array = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 0)
		{
			return false;
		}
		string text2 = array[0];
		if (commands.TryGetValue(text2, out var value2))
		{
			using (ListPool<object> listPool = Pool<ListPool<object>>.Get())
			{
				List<object> list = listPool.list;
				HashSet<object> listeners = value2.listeners;
				foreach (object item in listeners)
				{
					if (!(item as Component != null) && MathExtensions.IsDestroyed(item as Callback))
					{
						list.Add(item);
					}
				}
				for (int i = 0; i < list.Count; i++)
				{
					listeners.Remove(list[i]);
				}
				if (listeners.Count == 0)
				{
					commands.Remove(text2);
					value2 = null;
				}
			}
		}
		if (value2 != null)
		{
			bool caseSensitive = value2.caseSensitive;
			bool combineArgs = value2.combineArgs;
			Hashtable hashtable = null;
			if (combineArgs)
			{
				text = text.Substring(text2.Length).Trim();
				if (!caseSensitive)
				{
					text = text.ToLower();
				}
				if (text.Length > 0)
				{
					hashtable = new Hashtable();
					hashtable.Add(0, text);
				}
			}
			else if (array.Length > 1)
			{
				hashtable = new Hashtable();
				for (int j = 1; j < array.Length; j++)
				{
					hashtable[j - 1] = (caseSensitive ? array[j] : array[j].ToLower());
				}
			}
			string methodName = "OnConsoleCommand_" + text2;
			NotificationCenter.Notification notification = new NotificationCenter.Notification(hashtable);
			foreach (object listener in value2.listeners)
			{
				Component component = listener as Component;
				if (component != null)
				{
					component.SendMessage(methodName, notification, SendMessageOptions.DontRequireReceiver);
				}
				else if (listener is Callback callback)
				{
					callback(notification);
				}
			}
			hasUsedConsole = true;
			if (Player.main != null)
			{
				Player.main.hasUsedConsole = true;
			}
			return true;
		}
		return false;
	}

	private bool OnSubmit(string value)
	{
		if (!state)
		{
			return false;
		}
		bool result = false;
		if (!string.IsNullOrEmpty(value))
		{
			result = Submit(value);
		}
		SetState(value: false);
		return result;
	}

	private void OnConsoleCommand_developermode()
	{
		IngameMenu main = IngameMenu.main;
		if ((bool)main)
		{
			main.ActivateDeveloperMode();
		}
	}

	private void OnConsoleCommand_magic()
	{
		string[] array = new string[3] { "nocost", "item builder", "unlock all" };
		for (int i = 0; i < array.Length; i++)
		{
			Submit(array[i]);
		}
	}

	private void OnConsoleCommand_commands()
	{
		StringBuilder stringBuilder = new StringBuilder();
		Dictionary<string, CommandData>.KeyCollection.Enumerator enumerator = commands.Keys.GetEnumerator();
		int num = 0;
		while (enumerator.MoveNext())
		{
			stringBuilder.Append(enumerator.Current);
			stringBuilder.Append(' ');
			if (num % 4 == 0)
			{
				stringBuilder.AppendLine();
			}
			num++;
		}
		ErrorMessage.AddDebug("Console commands: " + stringBuilder.ToString());
	}

	public void ToggleState()
	{
		SetState(!state);
	}

	public void SetState(bool value)
	{
		if (state == value)
		{
			return;
		}
		state = value;
		if (state)
		{
			Select(lockMovement: true);
			if (TouchScreenKeyboardManager.isSupported)
			{
				string text = ((history.Count > 0) ? history[history.Count - 1] : string.Empty);
				softKeyboard = TouchScreenKeyboardManager.Open(text, TouchScreenKeyboardType.Default, autocorrection: false, multiline: false, secure: false, alert: false, "Enter command", -1);
			}
			else
			{
				inputField.Activate();
			}
		}
		else
		{
			softKeyboard = null;
			if (inputField.enabled)
			{
				inputField.Deactivate();
			}
			Deselect();
		}
	}

	private void SetRaycasterStatus(uGUI_GraphicRaycaster raycaster)
	{
		if (GameInput.IsPrimaryDeviceGamepad() && !VROptions.GetUseGazeBasedCursor())
		{
			raycaster.enabled = false;
		}
		else
		{
			raycaster.enabled = base.focused;
		}
	}

	public override void OnReselect(bool lockMovement)
	{
		base.OnReselect(lockMovement: true);
	}

	public override void OnDeselect()
	{
		SetState(value: false);
		base.OnDeselect();
	}

	private void OnConsoleCommand_clearhistory()
	{
		history.Clear();
		inputField.SetHistory(history);
		MiscSettings.consoleHistory = string.Empty;
	}

	private void SaveHistory()
	{
		if (history == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int i = Mathf.Max(0, history.Count - 10);
		for (int num = history.Count - 1; i <= num; i++)
		{
			string value = history[i];
			if (i == num)
			{
				stringBuilder.Append(value);
			}
			else
			{
				stringBuilder.AppendLine(value);
			}
		}
		string consoleHistory = stringBuilder.ToString();
		MiscSettings.consoleHistory = consoleHistory;
	}

	private void LoadHistory()
	{
		history.Clear();
		string consoleHistory = MiscSettings.consoleHistory;
		if (!string.IsNullOrEmpty(consoleHistory))
		{
			string[] collection = consoleHistory.Split(new string[2] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
			history.AddRange(collection);
		}
	}

	public static bool ParseFloat(NotificationCenter.Notification n, int argIndex, out float value, float defaultValue = 0f)
	{
		if (n != null)
		{
			Hashtable data = n.data;
			if (data != null && data.Count > argIndex && float.TryParse((string)data[argIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				value = result;
				return true;
			}
		}
		value = defaultValue;
		return false;
	}
}
