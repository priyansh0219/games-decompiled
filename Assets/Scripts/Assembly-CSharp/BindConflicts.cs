using System;
using System.Collections.Generic;
using UnityEngine;

public class BindConflicts
{
	private class Data
	{
		public List<GameInput.Button> buttons;

		public MatrixBool collisions;

		public void Set(GameInput.Button a, GameInput.Button b)
		{
			int num = buttons.IndexOf(a);
			if (num < 0)
			{
				Debug.LogErrorFormat("{0} is not defined in BindingData", a);
				return;
			}
			int num2 = buttons.IndexOf(b);
			if (num2 < 0)
			{
				Debug.LogErrorFormat("{0} is not defined in BindingData", b);
			}
			else
			{
				collisions.Set(num, num2, value: true);
			}
		}

		public bool Get(GameInput.Button a, GameInput.Button b)
		{
			int num = buttons.IndexOf(a);
			if (num < 0)
			{
				Debug.LogErrorFormat("{0} is not defined in BindingData", a);
				return false;
			}
			int num2 = buttons.IndexOf(b);
			if (num2 < 0)
			{
				Debug.LogErrorFormat("{0} is not defined in BindingData", b);
				return false;
			}
			return collisions.Get(num, num2);
		}
	}

	private static Dictionary<GameInput.Device, Data> datas;

	private static void Initialize()
	{
		if (datas != null)
		{
			return;
		}
		Array values = Enum.GetValues(typeof(GameInput.Device));
		Array values2 = Enum.GetValues(typeof(GameInput.Button));
		datas = new Dictionary<GameInput.Device, Data>(values.Length);
		foreach (GameInput.Device item in values)
		{
			Data data = new Data();
			datas.Add(item, data);
			List<GameInput.Button> list = new List<GameInput.Button>();
			for (int i = 0; i < values2.Length; i++)
			{
				GameInput.Button button = (GameInput.Button)values2.GetValue(i);
				if (GameInput.IsBindable(item, button))
				{
					list.Add(button);
				}
			}
			MatrixBool collisions = new MatrixBool(list.Count, defaultValue: false);
			data.buttons = list;
			data.collisions = collisions;
			Setup(item, data);
		}
	}

	private static void Setup(GameInput.Device device, Data data)
	{
		switch (device)
		{
		case GameInput.Device.Keyboard:
			data.Set(GameInput.Button.Jump, GameInput.Button.MoveUp);
			break;
		case GameInput.Device.Controller:
			data.Set(GameInput.Button.Jump, GameInput.Button.MoveUp);
			break;
		}
	}

	public static bool IsAllowed(GameInput.Device device, GameInput.Button a, GameInput.Button b)
	{
		Initialize();
		if (a != b)
		{
			return datas[device].Get(a, b);
		}
		return true;
	}

	public static void GetConflicts(GameInput.Device device, string input, GameInput.Button button, List<BindConflict> conflicts)
	{
		if (conflicts == null)
		{
			return;
		}
		conflicts.Clear();
		GameInput.GetAllActions(device, input, conflicts);
		for (int num = conflicts.Count - 1; num >= 0; num--)
		{
			GameInput.Button action = conflicts[num].action;
			if (!GameInput.IsBindable(device, action) || IsAllowed(device, button, action))
			{
				conflicts.RemoveAt(num);
			}
		}
	}
}
