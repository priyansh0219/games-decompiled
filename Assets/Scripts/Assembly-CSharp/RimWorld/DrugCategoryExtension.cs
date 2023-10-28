using Verse;

namespace RimWorld
{
	public static class DrugCategoryExtension
	{
		public static bool IncludedIn(this DrugCategory lhs, DrugCategory rhs)
		{
			return lhs <= rhs;
		}

		public static string GetLabel(this DrugCategory category)
		{
			switch (category)
			{
			case DrugCategory.None:
				return "DrugCategory_None".Translate();
			case DrugCategory.Medical:
				return "DrugCategory_Medical".Translate();
			case DrugCategory.Social:
				return "DrugCategory_Social".Translate();
			case DrugCategory.Hard:
				return "DrugCategory_Hard".Translate();
			default:
				return "DrugCategory_Any".Translate();
			}
		}
	}
}
