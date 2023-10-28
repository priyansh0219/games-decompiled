using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Building_GibbetCage : Building_CorpseCasket, IObservedThoughtGiver
	{
		[Unsaved(false)]
		private Graphic cageTopGraphic;

		private float corpseRotation;

		private static List<IntVec3> tmpRadialPositions = new List<IntVec3>();

		private float RandomCorpseRotation => Rand.Range(-45f, 45f);

		public Building_GibbetCage()
		{
			if (corpseRotation == 0f)
			{
				corpseRotation = Rand.Range(-45f, 45f);
			}
		}

		public override bool Accepts(Thing thing)
		{
			if (!base.Accepts(thing))
			{
				return false;
			}
			if (base.HasCorpse)
			{
				return false;
			}
			if (!storageSettings.AllowedToAccept(thing))
			{
				return false;
			}
			return true;
		}

		public void Notify_CorpseAdded()
		{
			tmpRadialPositions.Clear();
			corpseRotation = RandomCorpseRotation;
			if (base.Corpse.GetRotStage() != RotStage.Dessicated)
			{
				int num = GenRadial.NumCellsInRadius(1.5f);
				for (int i = 0; i < num; i++)
				{
					IntVec3 item = base.Position + GenRadial.RadialPattern[i];
					tmpRadialPositions.Add(item);
				}
				int num2 = Mathf.Min(tmpRadialPositions.Count, Rand.Range(2, 4));
				for (int j = 0; j < num2; j++)
				{
					IntVec3 intVec = tmpRadialPositions.RandomElement();
					tmpRadialPositions.Remove(intVec);
					FilthMaker.TryMakeFilth(intVec, base.Map, ThingDefOf.Filth_Blood);
				}
			}
			if (def.building.gibbetCagePlaceCorpseEffecter != null)
			{
				def.building.gibbetCagePlaceCorpseEffecter.Spawn(this, base.Map);
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			if (base.HasCorpse)
			{
				EjectContents();
			}
			base.DeSpawn(mode);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref corpseRotation, "corpseRotation", 0f);
		}

		public static Building_GibbetCage FindGibbetCageFor(Corpse c, Pawn traveler, bool ignoreOtherReservations = false)
		{
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef.IsGibbetCage)
				{
					Building_GibbetCage building_GibbetCage = (Building_GibbetCage)GenClosest.ClosestThingReachable(c.Position, c.Map, ThingRequest.ForDef(allDef), PathEndMode.InteractionCell, TraverseParms.For(traveler), 9999f, (Thing x) => !((Building_GibbetCage)x).HasCorpse && ((Building_GibbetCage)x).Accepts(x) && traveler.CanReserve(x, 1, -1, null, ignoreOtherReservations));
					if (building_GibbetCage != null)
					{
						return building_GibbetCage;
					}
				}
			}
			return null;
		}

		public Thought_Memory GiveObservedThought(Pawn observer)
		{
			if (observer.Ideo != null && observer.Ideo.IdeoApprovesOfSlavery())
			{
				return null;
			}
			Thought_MemoryObservation obj = (Thought_MemoryObservation)ThoughtMaker.MakeThought(ThoughtDefOf.ObservedGibbetCage);
			obj.Target = this;
			return obj;
		}

		public HistoryEventDef GiveObservedHistoryEvent(Pawn observer)
		{
			return null;
		}

		public override void Draw()
		{
			base.Draw();
			if (base.HasCorpse)
			{
				base.Corpse.InnerPawn.Drawer.renderer.wiggler.SetToCustomRotation(corpseRotation);
				base.Corpse.DrawAt(base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingOnTop) + def.building.gibbetCorposeDrawOffset);
			}
			if (cageTopGraphic == null)
			{
				cageTopGraphic = def.building.gibbetCageTopGraphicData.GraphicColoredFor(this);
			}
			cageTopGraphic.Draw(base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Item), Rot4.North, this);
		}

		public override void Notify_ColorChanged()
		{
			cageTopGraphic = null;
			base.Notify_ColorChanged();
		}
	}
}
