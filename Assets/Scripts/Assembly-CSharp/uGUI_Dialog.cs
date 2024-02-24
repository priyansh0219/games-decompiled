using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UWE;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_Dialog : MonoBehaviour
{
	[AssertNotNull]
	public TextMeshProUGUI text;

	[AssertNotNull]
	public uGUI_NavigableControlGrid panel;

	[AssertNotNull]
	public RectTransform buttonsRoot;

	[AssertNotNull]
	public uGUI_DialogButton buttonPrefab;

	private bool initialized;

	private Action<int> callback;

	private uGUI_INavigableIconGrid previousGrid;

	private object previousSelection;

	private PrefabPool<uGUI_DialogButton> poolButtons;

	private List<uGUI_DialogButton> buttons = new List<uGUI_DialogButton>();

	private Coroutine routine;

	public bool open { get; private set; }

	private void Awake()
	{
		Initialize();
	}

	private void Update()
	{
		if (open && GameInput.GetButtonDown(GameInput.Button.UICancel))
		{
			Close();
			GameInput.ClearInput();
		}
	}

	public void Show(string text, Action<int> callback, params string[] options)
	{
		Show(text, callback, null, -1, options);
	}

	public void Show(string text, Action<int> callback, IEnumerator routine, int selectOptionIndex, params string[] options)
	{
		if (options == null || options.Length == 0)
		{
			return;
		}
		Initialize();
		Close();
		open = true;
		if (routine != null)
		{
			this.routine = CoroutineHost.StartCoroutine(routine);
		}
		this.text.SetText(text);
		for (int i = 0; i < options.Length; i++)
		{
			string sourceText = options[i];
			uGUI_DialogButton uGUI_DialogButton2;
			if (i < buttons.Count)
			{
				uGUI_DialogButton2 = buttons[i];
			}
			else
			{
				uGUI_DialogButton2 = poolButtons.Get();
				buttons.Add(uGUI_DialogButton2);
			}
			uGUI_DialogButton2.option = i;
			uGUI_DialogButton2.action = Close;
			uGUI_DialogButton2.gameObject.SetActive(value: true);
			uGUI_DialogButton2.text.SetText(sourceText);
			uGUI_DialogButton2.rectTransform.SetAsFirstSibling();
		}
		for (int num = buttons.Count - 1; num >= options.Length; num--)
		{
			uGUI_DialogButton entry = buttons[num];
			buttons.RemoveAt(num);
			poolButtons.Release(entry);
		}
		int count = buttons.Count;
		for (int j = 0; j < count; j++)
		{
			buttons[j].button.navigation = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				selectOnUp = null,
				selectOnDown = null,
				selectOnLeft = buttons[MathExtensions.Mod(j + 1, count)].button,
				selectOnRight = buttons[MathExtensions.Mod(j - 1, count)].button
			};
		}
		this.callback = callback;
		SetVisible(visible: true);
		previousGrid = GamepadInputModule.current.GetCurrentGrid();
		if (previousGrid != null && !previousGrid.Equals(null))
		{
			previousSelection = previousGrid.GetSelectedItem();
		}
		GamepadInputModule.current.SetCurrentGrid(panel);
		if (selectOptionIndex < 0 || selectOptionIndex >= count)
		{
			selectOptionIndex = options.Length - 1;
		}
		panel.SelectItem(buttons[selectOptionIndex].button);
		Language main = Language.main;
		uGUI_LegendBar.ClearButtons();
		uGUI_LegendBar.ChangeButton(0, GameInput.FormatButton(GameInput.Button.UICancel), main.Get("Back"));
	}

	public void Close()
	{
		Close(-1);
	}

	private void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			poolButtons = new PrefabPool<uGUI_DialogButton>(buttonPrefab.gameObject, buttonsRoot, 0, 1, delegate(uGUI_DialogButton button)
			{
				button.gameObject.SetActive(value: false);
			}, delegate(uGUI_DialogButton button)
			{
				button.gameObject.SetActive(value: false);
			});
		}
	}

	private void SetVisible(bool visible)
	{
		base.gameObject.SetActive(visible);
	}

	private void Close(int option)
	{
		if (!open)
		{
			return;
		}
		open = false;
		if (routine != null)
		{
			CoroutineHost.StopCoroutine(routine);
			routine = null;
		}
		SetVisible(visible: false);
		uGUI_INavigableIconGrid uGUI_INavigableIconGrid2 = previousGrid;
		object obj = previousSelection;
		Action<int> action = callback;
		callback = null;
		previousGrid = null;
		previousSelection = null;
		if (uGUI_INavigableIconGrid2 != null && !uGUI_INavigableIconGrid2.Equals(null))
		{
			GamepadInputModule.current.SetCurrentGrid(uGUI_INavigableIconGrid2);
			if (GameInput.PrimaryDevice == GameInput.Device.Controller && obj != null)
			{
				uGUI_INavigableIconGrid2.SelectItem(obj);
			}
		}
		action?.Invoke(option);
	}

	public IEnumerator DialogTimeout(string key, int timeout)
	{
		timeout = System.Math.Max(0, timeout);
		float timeEnd = Time.realtimeSinceStartup + (float)timeout;
		int timeLeft = -1;
		do
		{
			int num = System.Math.Max(0, (int)System.Math.Floor(timeEnd - Time.realtimeSinceStartup));
			if (timeLeft != num)
			{
				timeLeft = num;
				using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
				{
					StringBuilder sb = stringBuilderPool.sb;
					string format = Language.main.Get(key);
					sb.AppendFormat(format, timeLeft);
					text.SetText(sb);
				}
			}
			yield return null;
		}
		while (timeLeft > 0);
		Close();
	}
}
