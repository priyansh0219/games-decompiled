using System;
using UnityEngine;

namespace UWE
{
	public class EnumToolbar<E>
	{
		private string[] strings;

		public int selectedId { get; private set; }

		public EnumToolbar()
		{
			Array values = Enum.GetValues(typeof(E));
			strings = new string[values.Length];
			int num = 0;
			foreach (object item in values)
			{
				strings[num++] = item.ToString();
			}
		}

		public int Layout()
		{
			selectedId = GUILayout.Toolbar(selectedId, strings);
			return selectedId;
		}
	}
}
