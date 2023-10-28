using Verse;

namespace RimWorld
{
	public class CompProperties_Studiable : CompProperties
	{
		public int cost = 100;

		public bool requiresMechanitor;

		[MustTranslate]
		public string completedLetterTitle;

		[MustTranslate]
		public string completedLetterText;

		public LetterDef completedLetterDef;

		public CompProperties_Studiable()
		{
			compClass = typeof(CompStudiable);
		}
	}
}
