using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	internal static class SelectorUtility
	{
		private static List<Thing> tmp_thingsToSort = new List<Thing>();

		public static void SortInColonistBarOrder(List<Thing> things)
		{
			tmp_thingsToSort.Clear();
			tmp_thingsToSort.AddRange(things);
			things.Clear();
			foreach (Pawn item in Find.ColonistBar.GetColonistsInOrder())
			{
				int num = tmp_thingsToSort.IndexOf(item);
				if (num != -1)
				{
					things.Add(item);
					tmp_thingsToSort.RemoveAt(num);
				}
			}
			things.AddRange(tmp_thingsToSort);
			tmp_thingsToSort.Clear();
		}
	}
}
