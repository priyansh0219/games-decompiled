using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class PinManager : MonoBehaviour
{
	private static PinManager _main;

	private static int _max = 7;

	private Action<TechType> _onAdd;

	private Action<TechType> _onRemove;

	private Action<int, int> _onMove;

	private List<TechType> pins = new List<TechType>();

	[AssertLocalization]
	private const string maxPinsMessage = "MaxPinsError";

	public static PinManager main
	{
		get
		{
			if (_main == null)
			{
				_main = new GameObject("PinManager").AddComponent<PinManager>();
			}
			return _main;
		}
	}

	public static int max
	{
		get
		{
			return _max;
		}
		set
		{
			if (_max != value)
			{
				_max = Math.Max(0, value);
				while (main.pins.Count > _max)
				{
					SetPin(main.pins[main.pins.Count - 1], value: false);
				}
			}
		}
	}

	public static int Count => main.pins.Count;

	public static event Action<TechType> onAdd
	{
		add
		{
			PinManager pinManager = main;
			pinManager._onAdd = (Action<TechType>)Delegate.Combine(pinManager._onAdd, value);
		}
		remove
		{
			PinManager pinManager = main;
			pinManager._onAdd = (Action<TechType>)Delegate.Remove(pinManager._onAdd, value);
		}
	}

	public static event Action<TechType> onRemove
	{
		add
		{
			PinManager pinManager = main;
			pinManager._onRemove = (Action<TechType>)Delegate.Combine(pinManager._onRemove, value);
		}
		remove
		{
			PinManager pinManager = main;
			pinManager._onRemove = (Action<TechType>)Delegate.Remove(pinManager._onRemove, value);
		}
	}

	public static event Action<int, int> onMove
	{
		add
		{
			PinManager pinManager = main;
			pinManager._onMove = (Action<int, int>)Delegate.Combine(pinManager._onMove, value);
		}
		remove
		{
			PinManager pinManager = main;
			pinManager._onMove = (Action<int, int>)Delegate.Remove(pinManager._onMove, value);
		}
	}

	public static IEnumerator<TechType> GetPins()
	{
		return main.pins.GetEnumerator();
	}

	public static bool GetPin(TechType techType)
	{
		return main.GetPinImpl(techType);
	}

	public static void SetPin(TechType techType, bool value)
	{
		main.SetPinImpl(techType, value);
	}

	public static void TogglePin(TechType techType)
	{
		main.TogglePinImpl(techType);
		AnalyticsController.AddTag("pinned_recipes");
	}

	public static void Move(int oldIndex, int newIndex)
	{
		main.MoveImpl(oldIndex, newIndex);
	}

	public static bool IsPinnedIngredient(TechType techType)
	{
		return main.IsPinnedIngredientImpl(techType);
	}

	public static void Clear()
	{
		main.ClearImpl();
	}

	public List<TechType> Serialize()
	{
		return new List<TechType>(pins);
	}

	public void Deserialize(List<TechType> serialized)
	{
		ClearImpl();
		if (serialized != null)
		{
			for (int i = 0; i < serialized.Count; i++)
			{
				SetPinImpl(serialized[i], state: true);
			}
		}
	}

	private bool GetPinImpl(TechType techType)
	{
		return pins.Contains(techType);
	}

	private void SetPinImpl(TechType techType, bool state)
	{
		if (techType == TechType.None || state == GetPinImpl(techType))
		{
			return;
		}
		if (state)
		{
			if (pins.Count < max)
			{
				pins.Add(techType);
				NotifyAdd(techType);
			}
			else
			{
				ErrorMessage.AddError(Language.main.Get("MaxPinsError"));
			}
		}
		else
		{
			pins.Remove(techType);
			NotifyRemove(techType);
		}
	}

	private void TogglePinImpl(TechType techType)
	{
		if (techType != 0)
		{
			SetPinImpl(techType, !GetPinImpl(techType));
		}
	}

	private void MoveImpl(int oldIndex, int newIndex)
	{
		if (oldIndex >= 0 && oldIndex < pins.Count && newIndex >= 0 && newIndex < pins.Count)
		{
			TechType item = pins[oldIndex];
			pins.RemoveAt(oldIndex);
			pins.Insert(newIndex, item);
			NotifyMove(oldIndex, newIndex);
		}
	}

	private bool IsPinnedIngredientImpl(TechType techType)
	{
		for (int i = 0; i < pins.Count; i++)
		{
			ReadOnlyCollection<Ingredient> ingredients = TechData.GetIngredients(pins[i]);
			if (ingredients == null)
			{
				continue;
			}
			for (int j = 0; j < ingredients.Count; j++)
			{
				if (ingredients[j].techType == techType)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ClearImpl()
	{
		for (int num = pins.Count - 1; num >= 0; num--)
		{
			TechType techType = pins[num];
			pins.RemoveAt(num);
			NotifyRemove(techType);
		}
	}

	private void NotifyAdd(TechType techType)
	{
		if (_onAdd != null)
		{
			_onAdd(techType);
		}
	}

	private void NotifyRemove(TechType techType)
	{
		if (_onRemove != null)
		{
			_onRemove(techType);
		}
	}

	private void NotifyMove(int oldIndex, int newIndex)
	{
		if (_onMove != null)
		{
			_onMove(oldIndex, newIndex);
		}
	}
}
