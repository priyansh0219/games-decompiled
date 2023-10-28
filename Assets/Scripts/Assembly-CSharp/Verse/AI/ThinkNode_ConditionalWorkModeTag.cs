using RimWorld;

namespace Verse.AI
{
	public class ThinkNode_ConditionalWorkModeTag : ThinkNode_Conditional
	{
		[NoTranslate]
		public string tag;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalWorkModeTag obj = (ThinkNode_ConditionalWorkModeTag)base.DeepCopy(resolve);
			obj.tag = tag;
			return obj;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.RaceProps.IsMechanoid && pawn.Faction == Faction.OfPlayer && pawn.relations != null)
			{
				return pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Overseer)?.mechanitor.GetControlGroup(pawn).GetTag(pawn) == tag;
			}
			return false;
		}
	}
}
