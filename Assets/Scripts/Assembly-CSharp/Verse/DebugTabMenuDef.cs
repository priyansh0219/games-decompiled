using System;
using System.Collections.Generic;

namespace Verse
{
	public class DebugTabMenuDef : Def
	{
		public Type menuClass;

		public int displayOrder = 99999;

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (!typeof(DebugTabMenu).IsAssignableFrom(menuClass))
			{
				yield return "menuClass does not derive from DebugTabMenu.";
			}
		}
	}
}
