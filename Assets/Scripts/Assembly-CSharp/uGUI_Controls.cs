using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class uGUI_Controls : MonoBehaviour, ICompileTimeCheckable
{
	[AssertNotNull]
	public GameObject prefabHeading;

	[AssertNotNull]
	public GameObject prefabToggle;

	[AssertNotNull]
	public GameObject prefabSlider;

	[AssertNotNull]
	public GameObject prefabDropdown;

	[AssertNotNull]
	public GameObject prefabBinding;

	[AssertNotNull]
	public GameObject prefabChoice;

	[AssertNotNull]
	public GameObject prefabButton;

	[AssertNotNull]
	public GameObject prefabColor;

	[AssertNotNull]
	public GameObject prefabBindingsHeader;

	public GameObject AddItem(RectTransform parent, GameObject prefab)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab, parent, worldPositionStays: false);
		SetupNavigation(gameObject);
		return gameObject;
	}

	public static void AssignLabel(GameObject instance, string value)
	{
		TranslationLiveUpdate componentInChildren = instance.GetComponentInChildren<TranslationLiveUpdate>();
		if (componentInChildren != null)
		{
			componentInChildren.translationKey = value;
			TextMeshProUGUI textComponent = componentInChildren.textComponent;
			textComponent.SetText(Language.main.Get(value));
			if (uGUI.overrideColor.HasValue)
			{
				textComponent.color = uGUI.overrideColor.Value;
			}
		}
	}

	public static void AssignTooltip(GameObject instance, string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			MenuTooltip componentInChildren = instance.GetComponentInChildren<MenuTooltip>();
			if (componentInChildren != null)
			{
				componentInChildren.key = value;
			}
		}
	}

	private static string[] GetNames<T>(T[] values)
	{
		string[] array = new string[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			array[i] = values[i].ToString();
		}
		return array;
	}

	public static Transform FindSelectable(Transform parent, int fromIndex, bool forward)
	{
		int childCount = parent.childCount;
		if (childCount > 0)
		{
			using (ListPool<Selectable> listPool = Pool<ListPool<Selectable>>.Get())
			{
				List<Selectable> list = listPool.list;
				for (int i = fromIndex; i >= 0 && i < childCount; i += (forward ? 1 : (-1)))
				{
					Transform child = parent.GetChild(i);
					child.GetComponentsInChildren(includeInactive: false, list);
					if (list.Count > 0)
					{
						return child;
					}
				}
			}
		}
		return null;
	}

	public void SetupNavigation(GameObject current)
	{
		Transform transform = current.transform;
		Transform transform2 = FindSelectable(transform.parent, transform.GetSiblingIndex() - 1, forward: false);
		using (ListPool<Selectable> listPool = Pool<ListPool<Selectable>>.Get())
		{
			List<Selectable> list = listPool.list;
			current.GetComponentsInChildren(list);
			Selectable selectOnUp = ((transform2 != null) ? transform2.GetComponentInChildren<Selectable>() : null);
			uGUI_Bindings uGUI_Bindings2 = ((transform2 != null) ? transform2.GetComponentInChildren<uGUI_Bindings>() : null);
			uGUI_Bindings componentInChildren = current.GetComponentInChildren<uGUI_Bindings>();
			if (componentInChildren == null || uGUI_Bindings2 == null)
			{
				foreach (Selectable item in list)
				{
					Navigation navigation = item.navigation;
					if (navigation.mode == Navigation.Mode.Explicit)
					{
						navigation.selectOnUp = selectOnUp;
						item.navigation = navigation;
					}
				}
			}
			else
			{
				int i = 0;
				for (int num = Math.Min(componentInChildren.bindings.Length, uGUI_Bindings2.bindings.Length); i < num; i++)
				{
					uGUI_Binding uGUI_Binding2 = componentInChildren.bindings[i];
					uGUI_Binding selectOnUp2 = uGUI_Bindings2.bindings[i];
					Navigation navigation2 = uGUI_Binding2.navigation;
					if (navigation2.mode == Navigation.Mode.Explicit)
					{
						navigation2.selectOnUp = selectOnUp2;
						uGUI_Binding2.navigation = navigation2;
					}
				}
			}
			if (list.Count <= 0 || !(transform2 != null))
			{
				return;
			}
			if (uGUI_Bindings2 == null || componentInChildren == null)
			{
				Selectable selectOnDown = list[0];
				transform2.GetComponentsInChildren(list);
				{
					foreach (Selectable item2 in list)
					{
						Navigation navigation3 = item2.navigation;
						if (navigation3.mode == Navigation.Mode.Explicit)
						{
							navigation3.selectOnDown = selectOnDown;
							item2.navigation = navigation3;
						}
					}
					return;
				}
			}
			int j = 0;
			for (int num2 = Math.Min(componentInChildren.bindings.Length, uGUI_Bindings2.bindings.Length); j < num2; j++)
			{
				uGUI_Binding uGUI_Binding3 = uGUI_Bindings2.bindings[j];
				uGUI_Binding selectOnDown2 = componentInChildren.bindings[j];
				Navigation navigation4 = uGUI_Binding3.navigation;
				if (navigation4.mode == Navigation.Mode.Explicit)
				{
					navigation4.selectOnDown = selectOnDown2;
					uGUI_Binding3.navigation = navigation4;
				}
			}
		}
	}

	public GameObject AddHeading(RectTransform parent, string label)
	{
		GameObject obj = AddItem(parent, prefabHeading);
		AssignLabel(obj, label);
		return obj;
	}

	public GameObject AddToggle(RectTransform parent, string label, bool isOn, UnityAction<bool> callback)
	{
		GameObject obj = AddItem(parent, prefabToggle);
		AssignLabel(obj, label);
		Toggle componentInChildren = obj.GetComponentInChildren<Toggle>();
		componentInChildren.isOn = isOn;
		if (callback != null)
		{
			componentInChildren.onValueChanged.AddListener(callback);
		}
		return obj;
	}

	public GameObject AddSlider(RectTransform parent, string label, float value, float minValue, float maxValue, float defaultValue, float step, UnityAction<float> callback, SliderLabelMode labelMode, string floatFormat)
	{
		GameObject gameObject = AddItem(parent, prefabSlider);
		AssignLabel(gameObject, label);
		uGUI_SnappingSlider slider = gameObject.GetComponentInChildren<uGUI_SnappingSlider>();
		slider.interactable = uGUI.interactable;
		uGUI_SliderWithLabel componentInChildren = gameObject.GetComponentInChildren<uGUI_SliderWithLabel>();
		componentInChildren.mode = labelMode;
		componentInChildren.floatFormat = floatFormat;
		if (uGUI.overrideColor.HasValue)
		{
			componentInChildren.label.color = uGUI.overrideColor.Value;
		}
		slider.minValue = minValue;
		slider.maxValue = maxValue;
		slider.value = value;
		slider.defaultValue = defaultValue;
		slider.step = step;
		if (callback != null)
		{
			UnityAction<float> call = delegate
			{
				callback(slider.value);
			};
			slider.onValueChanged.AddListener(call);
		}
		return gameObject;
	}

	public GameObject AddDropdown(RectTransform parent, string label, string[] items, int value, UnityAction<int> callback)
	{
		GameObject gameObject = AddItem(parent, prefabDropdown);
		AssignLabel(gameObject, label);
		TMP_Dropdown componentInChildren = gameObject.GetComponentInChildren<TMP_Dropdown>();
		componentInChildren.options.Clear();
		foreach (string text in items)
		{
			TMP_Dropdown.OptionData item = new TMP_Dropdown.OptionData
			{
				text = text
			};
			componentInChildren.options.Add(item);
		}
		componentInChildren.value = value;
		if (callback != null)
		{
			componentInChildren.onValueChanged.AddListener(callback);
		}
		return gameObject;
	}

	public GameObject AddBinding(RectTransform parent, string label, GameInput.Device device, GameInput.Button button)
	{
		GameObject obj = AddItem(parent, prefabBinding);
		AssignLabel(obj, label);
		obj.GetComponentInChildren<uGUI_Bindings>().Initialize(device, button);
		return obj;
	}

	public GameObject AddChoice<T>(RectTransform parent, string label, T[] options, T value, UnityAction<T> callback)
	{
		string[] names = GetNames(options);
		int value2 = Array.IndexOf(options, value);
		UnityAction<int> callback2 = null;
		if (callback != null)
		{
			callback2 = delegate(int index)
			{
				callback(options[index]);
			};
		}
		return AddChoice(parent, label, names, value2, callback2);
	}

	public GameObject AddChoice(RectTransform parent, string label, string[] options, int value, UnityAction<int> callback)
	{
		GameObject obj = AddItem(parent, prefabChoice);
		AssignLabel(obj, label);
		uGUI_Choice componentInChildren = obj.GetComponentInChildren<uGUI_Choice>();
		componentInChildren.SetOptions(options);
		componentInChildren.value = value;
		if (callback != null)
		{
			componentInChildren.onValueChanged.AddListener(callback);
		}
		return obj;
	}

	public GameObject AddButton(RectTransform parent, string label, UnityAction callback)
	{
		GameObject obj = AddItem(parent, prefabButton);
		AssignLabel(obj, label);
		Button componentInChildren = obj.GetComponentInChildren<Button>();
		if (callback != null)
		{
			componentInChildren.onClick.AddListener(callback);
		}
		return obj;
	}

	public GameObject AddColor(RectTransform parent, string label, Color color, UnityAction<Color> callback)
	{
		GameObject obj = AddItem(parent, prefabColor);
		AssignLabel(obj, label);
		uGUI_ColorChoice componentInChildren = obj.GetComponentInChildren<uGUI_ColorChoice>();
		componentInChildren.value = color;
		if (callback != null)
		{
			componentInChildren.onValueChanged.AddListener(callback);
		}
		return obj;
	}

	public GameObject AddBindingsHeader(RectTransform parent)
	{
		return AddItem(parent, prefabBindingsHeader);
	}

	public string CompileTimeCheck()
	{
		StringBuilder sb = new StringBuilder();
		Action<GameObject, Type[]> action = delegate(GameObject prefab, Type[] componentTypes)
		{
			foreach (Type type in componentTypes)
			{
				if (prefab.GetComponentInChildren(type) == null)
				{
					if (sb.Length > 0)
					{
						sb.Append('\n');
					}
					sb.AppendFormat("{0} component is not found on prefab \"{1}\"!", type, prefab.name);
				}
			}
		};
		action(prefabHeading, new Type[1] { typeof(TranslationLiveUpdate) });
		action(prefabToggle, new Type[3]
		{
			typeof(TranslationLiveUpdate),
			typeof(Toggle),
			typeof(MenuTooltip)
		});
		action(prefabSlider, new Type[4]
		{
			typeof(TranslationLiveUpdate),
			typeof(uGUI_SnappingSlider),
			typeof(uGUI_SliderWithLabel),
			typeof(MenuTooltip)
		});
		action(prefabDropdown, new Type[2]
		{
			typeof(TranslationLiveUpdate),
			typeof(TMP_Dropdown)
		});
		action(prefabBinding, new Type[2]
		{
			typeof(TranslationLiveUpdate),
			typeof(uGUI_Bindings)
		});
		action(prefabChoice, new Type[3]
		{
			typeof(TranslationLiveUpdate),
			typeof(uGUI_Choice),
			typeof(MenuTooltip)
		});
		action(prefabButton, new Type[2]
		{
			typeof(TranslationLiveUpdate),
			typeof(Button)
		});
		action(prefabColor, new Type[2]
		{
			typeof(TranslationLiveUpdate),
			typeof(uGUI_ColorChoice)
		});
		action(prefabBindingsHeader, new Type[1] { typeof(TranslationLiveUpdate) });
		if (sb.Length <= 0)
		{
			return null;
		}
		return sb.ToString();
	}
}
