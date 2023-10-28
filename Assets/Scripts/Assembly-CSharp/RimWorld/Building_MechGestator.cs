using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Building_MechGestator : Building_WorkTable, IThingHolder
	{
		private Mote workingMote;

		private Sustainer workingSound;

		public ThingOwner innerContainer;

		private Graphic cylinderGraphic;

		private Graphic topGraphic;

		private CompPowerTrader power;

		private CompWasteProducer wasteProducer;

		private static Material FormingCycleBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.98f, 0.46f, 0f));

		private static Material FormingCycleUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0f, 0f, 0f, 0f));

		private Bill_Mech activeBill;

		public CompWasteProducer WasteProducer
		{
			get
			{
				if (wasteProducer == null)
				{
					wasteProducer = GetComp<CompWasteProducer>();
				}
				return wasteProducer;
			}
		}

		public Bill_Mech ActiveBill
		{
			get
			{
				return activeBill;
			}
			set
			{
				if (activeBill != value)
				{
					activeBill = value;
				}
			}
		}

		public CompPowerTrader Power
		{
			get
			{
				if (power == null)
				{
					power = this.TryGetComp<CompPowerTrader>();
				}
				return power;
			}
		}

		public bool PoweredOn => Power.PowerOn;

		public bool BoundPawnStateAllowsForming
		{
			get
			{
				if (activeBill.BoundPawn != null && !activeBill.BoundPawn.Dead)
				{
					return !activeBill.BoundPawn.Suspended;
				}
				return false;
			}
		}

		public float CurrentBillFormingCyclePercent
		{
			get
			{
				if (activeBill == null || activeBill.State != FormingCycleState.Forming)
				{
					return 0f;
				}
				return 1f - activeBill.formingTicks / (float)activeBill.recipe.formingTicks;
			}
		}

		public GenDraw.FillableBarRequest BarDrawData => def.building.BarDrawDataFor(base.Rotation);

		public Pawn GestatingMech
		{
			get
			{
				Pawn pawn = (Pawn)innerContainer.FirstOrDefault((Thing t) => t is Pawn);
				if (pawn != null)
				{
					return pawn;
				}
				return ResurrectingMechCorpse?.InnerPawn;
			}
		}

		public Corpse ResurrectingMechCorpse => (Corpse)innerContainer.FirstOrDefault((Thing t) => t is Corpse);

		public Building_MechGestator()
		{
			innerContainer = new ThingOwner<Thing>(this);
		}

		public override void PostPostMake()
		{
			if (!ModLister.CheckBiotech("Mech gestator"))
			{
				Destroy();
			}
			else
			{
				base.PostPostMake();
			}
		}

		public bool CanBeUsedNowBy(Pawn user)
		{
			if (activeBill != null)
			{
				return activeBill.BoundPawn == user;
			}
			return true;
		}

		public void Notify_StartGestation()
		{
			SoundDefOf.MechGestatorCycle_Started.PlayOneShot(this);
		}

		public void Notify_AllGestationCyclesCompleted()
		{
			Pawn pawn = activeBill.ProducePawn();
			Messages.Message("GestationComplete".Translate() + ": " + pawn.kindDef.LabelCap, this, MessageTypeDefOf.PositiveEvent);
			innerContainer.ClearAndDestroyContents();
			innerContainer.TryAdd(pawn);
			WasteProducer.ProduceWaste((int)pawn.GetStatValue(StatDefOf.WastepacksPerRecharge));
			SoundDefOf.MechGestatorBill_Completed.PlayOneShot(this);
		}

		public override void Notify_BillDeleted(Bill bill)
		{
			if (activeBill == bill)
			{
				EjectContentsAndRemovePawns();
				activeBill = null;
			}
		}

		public void Notify_MaterialsAdded()
		{
			SoundDefOf.MechGestator_MaterialInserted.PlayOneShot(this);
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			activeBill?.Reset();
			EjectContentsAndRemovePawns();
			base.DeSpawn(mode);
		}

		public void EjectContentsAndRemovePawns()
		{
			for (int num = innerContainer.Count - 1; num >= 0; num--)
			{
				if (innerContainer[num] is Pawn pawn)
				{
					innerContainer.RemoveAt(num);
					pawn.Destroy();
				}
			}
			innerContainer.TryDropAll(InteractionCell, base.Map, ThingPlaceMode.Near);
		}

		public override void Tick()
		{
			base.Tick();
			innerContainer.ThingOwnerTick();
			if (activeBill != null && PoweredOn && BoundPawnStateAllowsForming)
			{
				activeBill.FormerBillTick();
				ThingDef thingDef = null;
				if (ActiveBill.State == FormingCycleState.Forming)
				{
					thingDef = def.building.gestatorFormingMote.GetForRotation(base.Rotation);
				}
				else if (ActiveBill.State == FormingCycleState.Preparing && ActiveBill.GestationCyclesCompleted > 0)
				{
					thingDef = def.building.gestatorCycleCompleteMote.GetForRotation(base.Rotation);
				}
				else if (ActiveBill.State == FormingCycleState.Formed)
				{
					thingDef = def.building.gestatorFormedMote.GetForRotation(base.Rotation);
				}
				if (thingDef != null)
				{
					if (workingMote == null || workingMote.Destroyed || workingMote.def != thingDef)
					{
						workingMote = MoteMaker.MakeAttachedOverlay(this, thingDef, Vector3.zero);
					}
					workingMote.Maintain();
				}
			}
			if (this.IsHashIntervalTick(250))
			{
				if (activeBill != null && activeBill.State == FormingCycleState.Forming)
				{
					Power.PowerOutput = 0f - Power.Props.PowerConsumption;
				}
				else
				{
					Power.PowerOutput = 0f - Power.Props.idlePowerDraw;
				}
			}
			if (activeBill != null && PoweredOn && activeBill.State != 0)
			{
				if (workingSound == null || workingSound.Ended)
				{
					workingSound = SoundDefOf.MechGestator_Ambience.TrySpawnSustainer(this);
				}
				workingSound.Maintain();
			}
			else if (workingSound != null)
			{
				workingSound.End();
				workingSound = null;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_References.Look(ref activeBill, "activeBill");
		}

		public override void Draw()
		{
			base.Draw();
			if (activeBill != null && activeBill.State != 0 && def.building.formingGraphicData != null)
			{
				Vector3 loc = DrawPos + def.building.formingMechPerRotationOffset[base.Rotation.AsInt];
				loc.y += 3f / 148f;
				loc.z += Mathf.PingPong((float)Find.TickManager.TicksGame * def.building.formingMechBobSpeed, def.building.formingMechYBobDistance);
				if (TryGetMechFormingGraphic(out var graphic))
				{
					graphic.Draw(loc, Rot4.South, this);
				}
				else
				{
					def.building.formingGraphicData.Graphic.Draw(loc, Rot4.North, this);
				}
			}
			GenDraw.FillableBarRequest barDrawData = BarDrawData;
			barDrawData.center = DrawPos + Vector3.up * 0.1f;
			barDrawData.fillPercent = CurrentBillFormingCyclePercent;
			barDrawData.filledMat = FormingCycleBarFilledMat;
			barDrawData.unfilledMat = FormingCycleUnfilledMat;
			barDrawData.rotation = base.Rotation;
			GenDraw.DrawFillableBar(barDrawData);
			if (topGraphic == null)
			{
				topGraphic = def.building.mechGestatorTopGraphic.GraphicColoredFor(this);
			}
			if (cylinderGraphic == null)
			{
				cylinderGraphic = def.building.mechGestatorCylinderGraphic.GraphicColoredFor(this);
			}
			Vector3 loc2 = new Vector3(DrawPos.x, AltitudeLayer.BuildingBelowTop.AltitudeFor(), DrawPos.z);
			cylinderGraphic.Draw(loc2, base.Rotation, this);
			Vector3 loc3 = new Vector3(DrawPos.x, AltitudeLayer.BuildingOnTop.AltitudeFor(), DrawPos.z);
			topGraphic.Draw(loc3, base.Rotation, this);
		}

		private bool TryGetMechFormingGraphic(out Graphic graphic)
		{
			graphic = null;
			if (ResurrectingMechCorpse != null)
			{
				graphic = ResurrectingMechCorpse.InnerPawn.ageTracker.CurKindLifeStage.bodyGraphicData.Graphic;
			}
			else if (GestatingMech != null)
			{
				graphic = GestatingMech.ageTracker.CurKindLifeStage.bodyGraphicData.Graphic;
			}
			if (graphic != null && graphic.drawSize.x <= def.building.maxFormedMechDrawSize.x && graphic.drawSize.y <= def.building.maxFormedMechDrawSize.y)
			{
				return true;
			}
			graphic = null;
			return false;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			string inspectString = base.GetInspectString();
			if (!inspectString.NullOrEmpty())
			{
				stringBuilder.AppendLine(inspectString);
			}
			if (activeBill != null)
			{
				activeBill.AppendInspectionData(stringBuilder);
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (!DebugSettings.ShowDevGizmos)
			{
				yield break;
			}
			Command_Action command_Action = new Command_Action();
			command_Action.action = delegate
			{
				WasteProducer.ProduceWaste(5);
			};
			command_Action.defaultLabel = "DEV: Generate 5 waste";
			yield return command_Action;
			if (ActiveBill == null || ActiveBill.State == FormingCycleState.Gathering || ActiveBill.State == FormingCycleState.Formed)
			{
				yield break;
			}
			Command_Action command_Action2 = new Command_Action();
			command_Action2.action = ActiveBill.ForceCompleteAllCycles;
			command_Action2.defaultLabel = "DEV: Complete all cycles";
			yield return command_Action2;
			if (ActiveBill.State == FormingCycleState.Forming)
			{
				Command_Action command_Action3 = new Command_Action();
				command_Action3.action = delegate
				{
					ActiveBill.formingTicks -= (float)ActiveBill.recipe.formingTicks * 0.25f;
				};
				command_Action3.defaultLabel = "DEV: Forming cycle +25%";
				yield return command_Action3;
				Command_Action command_Action4 = new Command_Action();
				command_Action4.action = delegate
				{
					ActiveBill.formingTicks = 0f;
				};
				command_Action4.defaultLabel = "DEV: Complete cycle";
				yield return command_Action4;
			}
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}
	}
}
