using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class ParentRelationUtility
	{
		private static List<string> workingParentagePieces = new List<string>();

		public static TaggedString GetParentage(this Pawn pawn)
		{
			workingParentagePieces.Clear();
			Pawn mother = pawn.GetMother();
			if (mother != null)
			{
				workingParentagePieces.Add(FactionDesc(mother));
			}
			Pawn father = pawn.GetFather();
			if (father != null)
			{
				workingParentagePieces.Add(FactionDesc(father));
			}
			string text = workingParentagePieces.ToCommaList(useAnd: true, emptyIfNone: true);
			if (string.IsNullOrEmpty(text))
			{
				text = "ParentsUnknown".Translate();
			}
			return "BornOfParents".Translate(text);
			string FactionDesc(Pawn parent)
			{
				return parent.FactionDesc(parent.NameFullColored, extraFactionsInfo: false, parent.NameFullColored, parent.gender.GetLabel(parent.RaceProps.Animal)).Resolve();
			}
		}

		public static Pawn GetFather(this Pawn pawn)
		{
			if (!pawn.RaceProps.IsFlesh)
			{
				return null;
			}
			if (pawn.relations == null)
			{
				return null;
			}
			List<DirectPawnRelation> directRelations = pawn.relations.DirectRelations;
			for (int i = 0; i < directRelations.Count; i++)
			{
				DirectPawnRelation directPawnRelation = directRelations[i];
				if (directPawnRelation.def == PawnRelationDefOf.Parent && directPawnRelation.otherPawn.gender != Gender.Female)
				{
					return directPawnRelation.otherPawn;
				}
			}
			return null;
		}

		public static Pawn GetMother(this Pawn pawn)
		{
			if (!pawn.RaceProps.IsFlesh)
			{
				return null;
			}
			if (pawn.relations == null)
			{
				return null;
			}
			List<DirectPawnRelation> directRelations = pawn.relations.DirectRelations;
			for (int i = 0; i < directRelations.Count; i++)
			{
				DirectPawnRelation directPawnRelation = directRelations[i];
				if (directPawnRelation.def == PawnRelationDefOf.Parent && directPawnRelation.otherPawn.gender == Gender.Female)
				{
					return directPawnRelation.otherPawn;
				}
			}
			return null;
		}

		public static Pawn GetBirthParent(this Pawn pawn)
		{
			if (!ModsConfig.BiotechActive)
			{
				return null;
			}
			if (!pawn.RaceProps.IsFlesh)
			{
				return null;
			}
			if (pawn.relations == null)
			{
				return null;
			}
			List<DirectPawnRelation> directRelations = pawn.relations.DirectRelations;
			for (int i = 0; i < directRelations.Count; i++)
			{
				DirectPawnRelation directPawnRelation = directRelations[i];
				if (directPawnRelation.def == PawnRelationDefOf.ParentBirth)
				{
					return directPawnRelation.otherPawn;
				}
			}
			return null;
		}

		public static void SetFather(this Pawn pawn, Pawn newFather)
		{
			if (newFather != null && newFather.gender == Gender.Female)
			{
				Log.Warning(string.Concat("Tried to set ", newFather, " with gender ", newFather.gender, " as ", pawn, "'s father."));
				return;
			}
			Pawn father = pawn.GetFather();
			if (father != newFather)
			{
				if (father != null)
				{
					pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Parent, father);
				}
				if (newFather != null)
				{
					pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, newFather);
				}
			}
		}

		public static void SetMother(this Pawn pawn, Pawn newMother)
		{
			if (newMother != null && newMother.gender != Gender.Female)
			{
				Log.Warning(string.Concat("Tried to set ", newMother, " with gender ", newMother.gender, " as ", pawn, "'s mother."));
				return;
			}
			Pawn mother = pawn.GetMother();
			if (mother != newMother)
			{
				if (mother != null)
				{
					pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Parent, mother);
				}
				if (newMother != null)
				{
					pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, newMother);
				}
			}
		}
	}
}
