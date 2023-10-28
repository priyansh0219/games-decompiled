using Verse;

namespace RimWorld
{
	[DefOf]
	public static class PawnKindDefOf
	{
		public static PawnKindDef Colonist;

		public static PawnKindDef Slave;

		public static PawnKindDef Villager;

		public static PawnKindDef Drifter;

		public static PawnKindDef SpaceRefugee;

		public static PawnKindDef AncientSoldier;

		public static PawnKindDef WildMan;

		public static PawnKindDef Pirate;

		public static PawnKindDef Tribal_Warrior;

		[MayRequireBiotech]
		public static PawnKindDef Mechanitor;

		[MayRequireBiotech]
		public static PawnKindDef Sanguophage;

		[MayRequireBiotech]
		public static PawnKindDef SanguophageThrall;

		[MayRequireBiotech]
		public static PawnKindDef Mechanitor_Basic;

		public static PawnKindDef Thrumbo;

		public static PawnKindDef Alphabeaver;

		public static PawnKindDef Muffalo;

		public static PawnKindDef Boomalope;

		public static PawnKindDef Megascarab;

		public static PawnKindDef Spelopede;

		public static PawnKindDef Megaspider;

		[MayRequireRoyalty]
		public static PawnKindDef Empire_Royal_Bestower;

		[MayRequireRoyalty]
		public static PawnKindDef Empire_Royal_NobleWimp;

		[MayRequireRoyalty]
		public static PawnKindDef Empire_Fighter_Janissary;

		[MayRequireRoyalty]
		public static PawnKindDef Empire_Fighter_Trooper;

		[MayRequireRoyalty]
		public static PawnKindDef Empire_Fighter_Cataphract;

		[MayRequireRoyalty]
		public static PawnKindDef Empire_Common_Lodger;

		[MayRequireRoyalty]
		public static PawnKindDef Refugee;

		[MayRequireIdeology]
		public static PawnKindDef Beggar;

		[MayRequireIdeology]
		public static PawnKindDef PovertyPilgrim;

		[MayRequireIdeology]
		public static PawnKindDef WellEquippedTraveler;

		[MayRequireIdeology]
		public static PawnKindDef Dryad_Basic;

		[MayRequireIdeology]
		public static PawnKindDef Dryad_Gaumaker;

		[MayRequireBiotech]
		public static PawnKindDef Mech_Warqueen;

		[MayRequireBiotech]
		public static PawnKindDef Mech_Diabolus;

		[MayRequireBiotech]
		public static PawnKindDef Mech_Apocriton;

		static PawnKindDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf));
		}
	}
}
