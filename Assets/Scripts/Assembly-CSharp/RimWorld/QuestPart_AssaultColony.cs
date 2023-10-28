using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class QuestPart_AssaultColony : QuestPart_MakeLord
	{
		public bool canTimeoutOrFlee = true;

		public bool canKidnap = true;

		public bool canSteal = true;

		protected override Lord MakeLord()
		{
			LordJob_AssaultColony lordJob = new LordJob_AssaultColony(faction ?? pawns[0].Faction, canTimeoutOrFlee: canTimeoutOrFlee, canKidnap: canKidnap, sappers: false, useAvoidGridSmart: false, canSteal: canSteal);
			return LordMaker.MakeNewLord(faction ?? pawns[0].Faction, lordJob, mapParent.Map);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref canTimeoutOrFlee, "canTimeoutOrFlee", defaultValue: true);
			Scribe_Values.Look(ref canKidnap, "canKidnap", defaultValue: true);
			Scribe_Values.Look(ref canSteal, "canSteal", defaultValue: true);
		}
	}
}
