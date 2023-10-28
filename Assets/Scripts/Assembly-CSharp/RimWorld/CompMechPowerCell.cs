using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompMechPowerCell : ThingComp
	{
		private int powerTicksLeft;

		private MechPowerCellGizmo gizmo;

		public CompProperties_MechPowerCell Props => (CompProperties_MechPowerCell)props;

		public float PercentFull => (float)powerTicksLeft / (float)Props.totalPowerTicks;

		public int PowerTicksLeft => powerTicksLeft;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!ModLister.CheckBiotech("Mech power cell"))
			{
				parent.Destroy();
				return;
			}
			base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				powerTicksLeft = Props.totalPowerTicks;
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (parent.Faction != Faction.OfPlayer)
			{
				yield break;
			}
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (Find.Selector.SingleSelectedThing == parent)
			{
				if (gizmo == null)
				{
					gizmo = new MechPowerCellGizmo(this);
				}
				yield return gizmo;
			}
			if (DebugSettings.ShowDevGizmos)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "DEV: Power left 0%";
				command_Action.action = delegate
				{
					powerTicksLeft = 0;
				};
				yield return command_Action;
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "DEV: Power left 100%";
				command_Action2.action = delegate
				{
					powerTicksLeft = Props.totalPowerTicks;
				};
				yield return command_Action2;
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			powerTicksLeft--;
			if (powerTicksLeft <= 0)
			{
				KillPowerProcessor();
			}
		}

		private void KillPowerProcessor()
		{
			Pawn pawn = (Pawn)parent;
			List<BodyPartRecord> allParts = pawn.def.race.body.AllParts;
			for (int i = 0; i < allParts.Count; i++)
			{
				BodyPartRecord bodyPartRecord = allParts[i];
				if (bodyPartRecord.def.tags.Contains(BodyPartTagDefOf.BloodPumpingSource))
				{
					pawn.health.AddHediff(HediffDefOf.MissingBodyPart, bodyPartRecord);
				}
			}
			if (!pawn.Dead)
			{
				pawn.Kill(null, null);
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref powerTicksLeft, "powerTicksLeft", 0);
		}
	}
}
