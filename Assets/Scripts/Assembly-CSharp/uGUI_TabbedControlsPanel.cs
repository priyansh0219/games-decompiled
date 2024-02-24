using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class uGUI_TabbedControlsPanel : MonoBehaviour
{
	private class Tab
	{
		public GameObject tab;

		public GameObject pane;

		public RectTransform container;

		public Selectable tabButton;

		public Selectable prevSelectable;
	}

	[AssertNotNull]
	public uGUI_Dialog dialog;

	public RectTransform tabsContainer;

	public RectTransform panesContainer;

	public GameObject tabPrefab;

	public GameObject panePrefab;

	public Button backButton;

	public Button applyButton;

	[AssertNotNull]
	public IngameMenuPanel ingameMenuPanel;

	public Action onClose;

	[AssertNotNull]
	public uGUI_Controls controls;

	private bool tabOpen;

	private int currentTab;

	private List<Tab> tabs = new List<Tab>();

	protected bool navigationDirty = true;

	protected virtual void OnEnable()
	{
		uGUI_LegendBar.ClearButtons();
		uGUI_LegendBar.ChangeButton(0, GameInput.FormatButton(GameInput.Button.UICancel), Language.main.GetFormat("Back"));
		uGUI_LegendBar.ChangeButton(1, GameInput.FormatButton(GameInput.Button.UISubmit), Language.main.GetFormat("ItemSelectorSelect"));
		Language.OnLanguageChanged += HighlightCurrentTab;
		IngameMenuPanel obj = ingameMenuPanel;
		obj.onBack = (Action)Delegate.Combine(obj.onBack, new Action(OnBackPerformed));
		uGUI.main.dialog = dialog;
	}

	protected virtual void OnDisable()
	{
		if (uGUI.main.dialog == dialog)
		{
			uGUI.main.dialog = null;
		}
		IngameMenuPanel obj = ingameMenuPanel;
		obj.onBack = (Action)Delegate.Remove(obj.onBack, new Action(OnBackPerformed));
		Language.OnLanguageChanged -= HighlightCurrentTab;
		dialog.Close();
		RemoveTabs();
	}

	protected virtual void Update()
	{
		UpdateButtonState(backButton, !GameInput.IsPrimaryDeviceGamepad());
		UpdateButtonsNavigation();
	}

	protected void UpdateButtonState(Button button, bool nowActive)
	{
		if (button != null && button.gameObject.activeSelf != nowActive)
		{
			navigationDirty = true;
			button.gameObject.SetActive(nowActive);
		}
	}

	protected void UpdateButtonsNavigation()
	{
		if (!navigationDirty)
		{
			return;
		}
		navigationDirty = false;
		bool num = backButton != null && backButton.gameObject.activeSelf;
		bool flag = applyButton != null && applyButton.gameObject.activeSelf;
		Selectable selectable = ((tabs.Count > 0) ? tabs[tabs.Count - 1].tabButton : null);
		Navigation navigation = new Navigation
		{
			mode = Navigation.Mode.Explicit
		};
		Navigation navigation2 = new Navigation
		{
			mode = Navigation.Mode.Explicit
		};
		Selectable selectOnDown = null;
		if (num)
		{
			selectOnDown = backButton;
			navigation.selectOnUp = selectable;
			if (flag)
			{
				navigation.selectOnRight = applyButton;
				navigation2.selectOnLeft = backButton;
			}
		}
		else if (flag)
		{
			selectOnDown = applyButton;
			navigation2.selectOnUp = selectable;
		}
		if (selectable != null)
		{
			Navigation navigation3 = selectable.navigation;
			navigation3.selectOnDown = selectOnDown;
			selectable.navigation = navigation3;
		}
		if (backButton != null)
		{
			backButton.navigation = navigation;
		}
		if (applyButton != null)
		{
			applyButton.navigation = navigation2;
		}
	}

	public int AddTab(string label)
	{
		Tab tab = new Tab
		{
			pane = UnityEngine.Object.Instantiate(panePrefab, panesContainer, worldPositionStays: false),
			tab = UnityEngine.Object.Instantiate(tabPrefab, tabsContainer, worldPositionStays: false)
		};
		TextMeshProUGUI componentInChildren = tab.tab.GetComponentInChildren<TextMeshProUGUI>();
		if (componentInChildren != null)
		{
			componentInChildren.text = Language.main.Get(label);
			tab.tab.GetComponentInChildren<TranslationLiveUpdate>().translationKey = label;
		}
		int tabIndex = tabs.Count;
		ToggleButton componentInChildren2 = tab.tab.GetComponentInChildren<ToggleButton>();
		UnityAction<bool> call = delegate(bool value)
		{
			if (value)
			{
				SetVisibleTab(tabIndex);
			}
		};
		componentInChildren2.onValueChanged.AddListener(call);
		UnityAction call2 = delegate
		{
			SelectTab(tabIndex);
		};
		componentInChildren2.onButtonPressed.AddListener(call2);
		bool flag = tabIndex == 0;
		tab.pane.SetActive(flag);
		componentInChildren2.isOn = flag;
		componentInChildren2.group = tabsContainer.GetComponentInChildren<ToggleGroup>();
		GameObject gameObject = Utils.FindChild(tab.pane, "Content");
		if (gameObject == null)
		{
			gameObject = tab.pane;
		}
		Selectable selectable = ((tabs.Count > 0) ? tabs[tabs.Count - 1].tabButton : null);
		Navigation navigation = componentInChildren2.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnUp = selectable;
		navigationDirty = true;
		componentInChildren2.navigation = navigation;
		if (selectable != null)
		{
			navigation = selectable.navigation;
			navigation.selectOnDown = componentInChildren2;
			selectable.navigation = navigation;
		}
		tab.tabButton = componentInChildren2;
		tab.container = gameObject.GetComponent<RectTransform>();
		tabs.Add(tab);
		return tabIndex;
	}

	public GameObject AddItem(int tabIndex, GameObject prefab)
	{
		return controls.AddItem(GetParent(tabIndex), prefab);
	}

	public GameObject AddHeading(int tabIndex, string label)
	{
		return controls.AddHeading(GetParent(tabIndex), label);
	}

	public GameObject AddButton(int tabIndex, string label, UnityAction callback)
	{
		return controls.AddButton(GetParent(tabIndex), label, callback);
	}

	public Toggle AddToggleOption(int tabIndex, string label, bool value, UnityAction<bool> callback = null, string tooltip = null)
	{
		GameObject obj = controls.AddToggle(GetParent(tabIndex), label, value, callback);
		uGUI_Controls.AssignTooltip(obj, tooltip);
		return obj.GetComponentInChildren<Toggle>();
	}

	public GameObject AddSliderOption(int tabIndex, string label, float value, float minValue, float maxValue, float defaultValue, float step, UnityAction<float> callback, SliderLabelMode labelMode, string floatFormat, string tooltip = null)
	{
		GameObject obj = controls.AddSlider(GetParent(tabIndex), label, value, minValue, maxValue, defaultValue, step, callback, labelMode, floatFormat);
		uGUI_Controls.AssignTooltip(obj, tooltip);
		return obj;
	}

	public void AddSliderOption(int tabIndex, string label, float value, float defaultValue, UnityAction<float> callback = null)
	{
		AddSliderOption(tabIndex, label, value, 0f, 1f, defaultValue, 0.05f, callback, SliderLabelMode.Percent, "0");
	}

	public GameObject AddColorOption(int tabIndex, string label, Color color, UnityAction<Color> callback = null)
	{
		return controls.AddColor(GetParent(tabIndex), label, color, callback);
	}

	public uGUI_Choice AddChoiceOption<T>(int tabIndex, string label, T[] items, T currentValue, UnityAction<T> callback = null, string tooltip = null)
	{
		return controls.AddChoice(GetParent(tabIndex), label, items, currentValue, callback).GetComponentInChildren<uGUI_Choice>();
	}

	public uGUI_Choice AddChoiceOption(int tabIndex, string label, string[] items, int currentIndex, UnityAction<int> callback = null, string tooltip = null)
	{
		GameObject obj = controls.AddChoice(GetParent(tabIndex), label, items, currentIndex, callback);
		uGUI_Controls.AssignTooltip(obj, tooltip);
		return obj.GetComponentInChildren<uGUI_Choice>();
	}

	public GameObject AddDropdownOption(int tabIndex, string label, string[] items, int currentIndex, UnityAction<int> callback = null)
	{
		return controls.AddDropdown(GetParent(tabIndex), label, items, currentIndex, callback);
	}

	public uGUI_Bindings AddBindingOption(int tabIndex, string label, GameInput.Device device, GameInput.Button button)
	{
		GameObject bindingObject;
		return AddBindingOption(tabIndex, label, device, button, out bindingObject);
	}

	public uGUI_Bindings AddBindingOption(int tabIndex, string label, GameInput.Device device, GameInput.Button button, out GameObject bindingObject)
	{
		bindingObject = controls.AddBinding(GetParent(tabIndex), label, device, button);
		return bindingObject.GetComponentInChildren<uGUI_Bindings>();
	}

	public GameObject AddBindingsHeader(int tabIndex)
	{
		return controls.AddBindingsHeader(GetParent(tabIndex));
	}

	private RectTransform GetParent(int tabIndex)
	{
		return tabs[tabIndex].container;
	}

	private void SetVisibleTab(int tabIndex)
	{
		for (int i = 0; i < tabs.Count; i++)
		{
			tabs[i].pane.SetActive(tabIndex == i);
		}
		if (tabIndex >= 0 && tabIndex < tabs.Count && currentTab != tabIndex)
		{
			tabs[currentTab].prevSelectable = null;
			currentTab = tabIndex;
			Selectable firstSelectable = GetFirstSelectable(tabs[tabIndex].container);
			if (firstSelectable != null)
			{
				UIUtils.ScrollToShowItemInCenter(firstSelectable.transform);
			}
		}
	}

	public void RemoveTabs()
	{
		foreach (Tab tab in tabs)
		{
			UnityEngine.Object.Destroy(tab.tab);
			UnityEngine.Object.Destroy(tab.pane);
		}
		tabs.Clear();
	}

	private void SelectTab(int tabIndex)
	{
		if (!tabOpen)
		{
			SetVisibleTab(tabIndex);
			Tab tab = tabs[tabIndex];
			GamepadInputModule.current.SelectItem((tab.prevSelectable != null) ? tab.prevSelectable : GetFirstSelectable(tab.container));
			tabOpen = true;
		}
	}

	public void HighlightCurrentTab()
	{
		uGUI_LegendBar.ClearButtons();
		uGUI_LegendBar.ChangeButton(0, GameInput.FormatButton(GameInput.Button.UICancel), Language.main.GetFormat("Back"));
		uGUI_LegendBar.ChangeButton(1, GameInput.FormatButton(GameInput.Button.UISubmit), Language.main.GetFormat("ItemSelectorSelect"));
		StartCoroutine(_InternalHighlightCurrentTab());
	}

	private IEnumerator _InternalHighlightCurrentTab()
	{
		yield return new WaitForEndOfFrame();
		tabs[currentTab].prevSelectable = GamepadInputModule.current.GetCurrentGrid().GetSelectedItem() as Selectable;
		GamepadInputModule.current.SelectItem(tabs[currentTab].tabButton);
		tabOpen = false;
	}

	public void OnBackPerformed()
	{
		OnBack();
	}

	public virtual bool OnBack()
	{
		if (dialog.open)
		{
			dialog.Close();
			return true;
		}
		if (tabOpen)
		{
			HighlightCurrentTab();
			return true;
		}
		return false;
	}

	protected void Close()
	{
		if (onClose != null)
		{
			onClose();
		}
	}

	private static Selectable GetFirstSelectable(RectTransform parent)
	{
		Transform transform = uGUI_Controls.FindSelectable(parent, 0, forward: true);
		if (!(transform != null))
		{
			return null;
		}
		return transform.GetComponentInChildren<Selectable>();
	}
}
