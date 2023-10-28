using Verse;

namespace RimWorld
{
	[DefOf]
	public static class LetterDefOf
	{
		public static LetterDef ThreatBig;

		public static LetterDef ThreatSmall;

		public static LetterDef NegativeEvent;

		public static LetterDef NeutralEvent;

		public static LetterDef PositiveEvent;

		public static LetterDef Death;

		public static LetterDef NewQuest;

		public static LetterDef AcceptVisitors;

		public static LetterDef AcceptJoiner;

		public static LetterDef BetrayVisitors;

		public static LetterDef ChoosePawn;

		public static LetterDef RitualOutcomeNegative;

		public static LetterDef RitualOutcomePositive;

		[MayRequireIdeology]
		public static LetterDef RelicHuntInstallationFound;

		[MayRequireBiotech]
		public static LetterDef BabyBirth;

		[MayRequireBiotech]
		public static LetterDef BabyToChild;

		[MayRequireBiotech]
		public static LetterDef ChildToAdult;

		[MayRequireBiotech]
		public static LetterDef ChildBirthday;

		[MayRequireBiotech]
		public static LetterDef Bossgroup;

		public static LetterDef BundleLetter;

		static LetterDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(LetterDefOf));
		}
	}
}
