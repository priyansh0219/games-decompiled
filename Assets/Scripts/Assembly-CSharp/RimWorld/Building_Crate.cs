using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Building_Crate : Building_Casket
	{
		private static IntVec3 ejectPositionOffset = new IntVec3(1, 0, 0);

		public const string CrateContentsChanged = "CrateContentsChanged";

		private static List<Pawn> tmpAllowedPawns = new List<Pawn>();

		public override int OpenTicks => 100;

		public override bool CanOpen
		{
			get
			{
				CompHackable comp = GetComp<CompHackable>();
				if (comp != null && !comp.IsHacked)
				{
					return false;
				}
				return base.CanOpen;
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			if (ModLister.CheckIdeology("Ancient security crate"))
			{
				base.SpawnSetup(map, respawningAfterLoad);
			}
		}

		public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (base.TryAcceptThing(thing, allowSpecialEffects))
			{
				BroadcastCompSignal("CrateContentsChanged");
				return true;
			}
			return false;
		}

		public override void EjectContents()
		{
			this.OccupiedRect();
			innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near, null, (IntVec3 c) => c.GetEdifice(base.Map) == null);
			contentsKnown = true;
			if (def.building.openingEffect != null)
			{
				Effecter effecter = def.building.openingEffect.Spawn();
				effecter.Trigger(new TargetInfo(base.Position, base.Map), null);
				effecter.Cleanup();
			}
			BroadcastCompSignal("CrateContentsChanged");
		}

		protected override void ReceiveCompSignal(string signal)
		{
			base.ReceiveCompSignal(signal);
			if (signal == "Hackend")
			{
				Open();
			}
		}

		public override void Open()
		{
			if (CanOpen)
			{
				base.Open();
			}
		}

		public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(List<Pawn> selPawns)
		{
			foreach (FloatMenuOption multiSelectFloatMenuOption in base.GetMultiSelectFloatMenuOptions(selPawns))
			{
				yield return multiSelectFloatMenuOption;
			}
			if (!CanOpen)
			{
				yield break;
			}
			tmpAllowedPawns.Clear();
			for (int i = 0; i < selPawns.Count; i++)
			{
				if (selPawns[i].CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
				{
					tmpAllowedPawns.Add(selPawns[i]);
				}
			}
			if (tmpAllowedPawns.Count <= 0)
			{
				yield return new FloatMenuOption("CannotOpen".Translate(this) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
				yield break;
			}
			tmpAllowedPawns.Clear();
			for (int j = 0; j < selPawns.Count; j++)
			{
				if (HackUtility.IsCapableOfHacking(selPawns[j]))
				{
					tmpAllowedPawns.Add(selPawns[j]);
				}
			}
			if (tmpAllowedPawns.Count <= 0)
			{
				yield return new FloatMenuOption("CannotOpen".Translate(Label) + ": " + "IncapableOfHacking".Translate(), null);
				yield break;
			}
			tmpAllowedPawns.Clear();
			for (int k = 0; k < selPawns.Count; k++)
			{
				if (HackUtility.IsCapableOfHacking(selPawns[k]) && selPawns[k].CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
				{
					tmpAllowedPawns.Add(selPawns[k]);
				}
			}
			if (tmpAllowedPawns.Count <= 0)
			{
				yield break;
			}
			yield return new FloatMenuOption("Open".Translate(this), delegate
			{
				tmpAllowedPawns[0].jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Open, this), JobTag.Misc);
				for (int l = 1; l < tmpAllowedPawns.Count; l++)
				{
					FloatMenuMakerMap.PawnGotoAction(base.Position, tmpAllowedPawns[l], RCellFinder.BestOrderedGotoDestNear(base.Position, tmpAllowedPawns[l]));
				}
			});
		}
	}
}
