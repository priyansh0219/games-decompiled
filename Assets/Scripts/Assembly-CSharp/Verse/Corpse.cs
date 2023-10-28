using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Corpse : ThingWithComps, IThingHolder, IStrippable, IBillGiver, IObservedThoughtGiver
	{
		private ThingOwner<Pawn> innerContainer;

		public int timeOfDeath = -1;

		private int vanishAfterTimestamp = -1;

		private BillStack operationsBillStack;

		public bool everBuriedInSarcophagus;

		private const int VanishAfterTicksSinceDessicated = 6000000;

		private const int DontCauseObservedCorpseThoughtAfterRitualExecutionTicks = 60000;

		public Pawn InnerPawn
		{
			get
			{
				if (innerContainer.Count > 0)
				{
					return innerContainer[0];
				}
				return null;
			}
			set
			{
				if (value == null)
				{
					innerContainer.Clear();
					return;
				}
				if (innerContainer.Count > 0)
				{
					Log.Error("Setting InnerPawn in corpse that already has one.");
					innerContainer.Clear();
				}
				innerContainer.TryAdd(value);
			}
		}

		public int Age
		{
			get
			{
				return Find.TickManager.TicksGame - timeOfDeath;
			}
			set
			{
				timeOfDeath = Find.TickManager.TicksGame - value;
			}
		}

		public override string LabelNoCount
		{
			get
			{
				if (Bugged)
				{
					Log.ErrorOnce("Corpse.Label while Bugged", 57361644);
					return "";
				}
				return "DeadLabel".Translate(InnerPawn.Label, InnerPawn);
			}
		}

		public override bool IngestibleNow
		{
			get
			{
				if (Bugged)
				{
					Log.Error("IngestibleNow on Corpse while Bugged.");
					return false;
				}
				if (!base.IngestibleNow)
				{
					return false;
				}
				if (!InnerPawn.RaceProps.IsFlesh)
				{
					return false;
				}
				if (this.GetRotStage() != 0)
				{
					return false;
				}
				return true;
			}
		}

		public RotDrawMode CurRotDrawMode
		{
			get
			{
				CompRottable comp = GetComp<CompRottable>();
				if (comp != null)
				{
					if (comp.Stage == RotStage.Rotting)
					{
						return RotDrawMode.Rotting;
					}
					if (comp.Stage == RotStage.Dessicated)
					{
						return RotDrawMode.Dessicated;
					}
				}
				return RotDrawMode.Fresh;
			}
		}

		private bool ShouldVanish
		{
			get
			{
				if (InnerPawn.RaceProps.Animal && vanishAfterTimestamp > 0 && Age >= vanishAfterTimestamp && base.Spawned && this.GetRoom() != null && this.GetRoom().TouchesMapEdge)
				{
					return !base.Map.roofGrid.Roofed(base.Position);
				}
				return false;
			}
		}

		public BillStack BillStack => operationsBillStack;

		public IEnumerable<IntVec3> IngredientStackCells
		{
			get
			{
				yield return InteractionCell;
			}
		}

		public bool Bugged
		{
			get
			{
				if (innerContainer.Count != 0 && innerContainer[0] != null && innerContainer[0].def != null)
				{
					return innerContainer[0].kindDef == null;
				}
				return true;
			}
		}

		public Corpse()
		{
			operationsBillStack = new BillStack(this);
			innerContainer = new ThingOwner<Pawn>(this, oneStackOnly: true, LookMode.Reference);
		}

		public bool CurrentlyUsableForBills()
		{
			return InteractionCell.IsValid;
		}

		public bool UsableForBillsAfterFueling()
		{
			return CurrentlyUsableForBills();
		}

		public bool AnythingToStrip()
		{
			return InnerPawn.AnythingToStrip();
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public override void PostMake()
		{
			base.PostMake();
			timeOfDeath = Find.TickManager.TicksGame;
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			if (Bugged)
			{
				Log.Error(string.Concat(this, " spawned in bugged state."));
				return;
			}
			base.SpawnSetup(map, respawningAfterLoad);
			InnerPawn.Rotation = Rot4.South;
			NotifyColonistBar();
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.DeSpawn(mode);
			if (!Bugged)
			{
				NotifyColonistBar();
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			Pawn pawn = null;
			if (!Bugged)
			{
				pawn = InnerPawn;
				NotifyColonistBar();
				innerContainer.Clear();
			}
			base.Destroy(mode);
			if (pawn != null)
			{
				PostCorpseDestroy(pawn);
			}
		}

		public static void PostCorpseDestroy(Pawn pawn)
		{
			if (pawn.ownership != null)
			{
				pawn.ownership.UnclaimAll();
			}
			if (pawn.equipment != null)
			{
				pawn.equipment.DestroyAllEquipment();
			}
			pawn.inventory.DestroyAll();
			if (pawn.apparel != null)
			{
				pawn.apparel.DestroyAll();
			}
			if (!PawnGenerator.IsBeingGenerated(pawn))
			{
				pawn.Ideo?.Notify_MemberCorpseDestroyed(pawn);
			}
		}

		public override void TickRare()
		{
			base.TickRare();
			if (base.Destroyed)
			{
				return;
			}
			if (Bugged)
			{
				Log.Error(string.Concat(this, " has null innerPawn. Destroying."));
				Destroy();
				return;
			}
			InnerPawn.TickRare();
			if (vanishAfterTimestamp < 0 || this.GetRotStage() != RotStage.Dessicated)
			{
				vanishAfterTimestamp = Age + 6000000;
			}
			if (base.Spawned && this.GetRotStage() == RotStage.Rotting)
			{
				int num = GasUtility.RotStinkToSpawnForCorpse(this);
				if (num > 0)
				{
					GasUtility.AddGas(base.Position, base.Map, GasType.RotStink, num);
				}
			}
			if (ShouldVanish)
			{
				Destroy();
			}
		}

		protected override void IngestedCalculateAmounts(Pawn ingester, float nutritionWanted, out int numTaken, out float nutritionIngested)
		{
			BodyPartRecord bodyPartRecord = GetBestBodyPartToEat(ingester, nutritionWanted);
			if (bodyPartRecord == null)
			{
				Log.Error(string.Concat(ingester, " ate ", this, " but no body part was found. Replacing with core part."));
				bodyPartRecord = InnerPawn.RaceProps.body.corePart;
			}
			float bodyPartNutrition = FoodUtility.GetBodyPartNutrition(this, bodyPartRecord);
			if (bodyPartRecord == InnerPawn.RaceProps.body.corePart)
			{
				if (PawnUtility.ShouldSendNotificationAbout(InnerPawn) && InnerPawn.RaceProps.Humanlike)
				{
					Messages.Message("MessageEatenByPredator".Translate(InnerPawn.LabelShort, ingester.Named("PREDATOR"), InnerPawn.Named("EATEN")).CapitalizeFirst(), ingester, MessageTypeDefOf.NegativeEvent);
				}
				numTaken = 1;
			}
			else
			{
				Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, InnerPawn, bodyPartRecord);
				hediff_MissingPart.lastInjury = HediffDefOf.Bite;
				hediff_MissingPart.IsFresh = true;
				InnerPawn.health.AddHediff(hediff_MissingPart);
				numTaken = 0;
			}
			nutritionIngested = bodyPartNutrition;
		}

		public override IEnumerable<Thing> ButcherProducts(Pawn butcher, float efficiency)
		{
			foreach (Thing item in InnerPawn.ButcherProducts(butcher, efficiency))
			{
				yield return item;
			}
			if (InnerPawn.RaceProps.BloodDef != null)
			{
				FilthMaker.TryMakeFilth(butcher.Position, butcher.Map, InnerPawn.RaceProps.BloodDef, InnerPawn.LabelIndefinite());
			}
			if (InnerPawn.RaceProps.Humanlike)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.ButcheredHuman, new SignalArgs(butcher.Named(HistoryEventArgsNames.Doer), InnerPawn.Named(HistoryEventArgsNames.Victim))));
				TaleRecorder.RecordTale(TaleDefOf.ButcheredHumanlikeCorpse, butcher);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref timeOfDeath, "timeOfDeath", 0);
			Scribe_Values.Look(ref vanishAfterTimestamp, "vanishAfterTimestamp", 0);
			Scribe_Values.Look(ref everBuriedInSarcophagus, "everBuriedInSarcophagus", defaultValue: false);
			Scribe_Deep.Look(ref operationsBillStack, "operationsBillStack", this);
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		}

		public void Strip()
		{
			InnerPawn.Strip();
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			InnerPawn.Drawer.renderer.RenderPawnAt(drawLoc);
		}

		public Thought_Memory GiveObservedThought(Pawn observer)
		{
			return null;
		}

		public HistoryEventDef GiveObservedHistoryEvent(Pawn observer)
		{
			if (!InnerPawn.RaceProps.Humanlike)
			{
				return null;
			}
			if (InnerPawn.health.killedByRitual && Find.TickManager.TicksGame - timeOfDeath < 60000)
			{
				return null;
			}
			if (this.StoringThing() == null)
			{
				if (this.IsNotFresh())
				{
					return HistoryEventDefOf.ObservedLayingRottingCorpse;
				}
				return HistoryEventDefOf.ObservedLayingCorpse;
			}
			return null;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (InnerPawn.Faction != null)
			{
				stringBuilder.AppendLineTagged("Faction".Translate() + ": " + InnerPawn.Faction.NameColored);
			}
			stringBuilder.AppendLine("DeadTime".Translate(Age.ToStringTicksToPeriodVague(vagueMin: true, vagueMax: false)));
			float num = 1f - InnerPawn.health.hediffSet.GetCoverageOfNotMissingNaturalParts(InnerPawn.RaceProps.body.corePart);
			if (num != 0f)
			{
				stringBuilder.AppendLine("CorpsePercentMissing".Translate() + ": " + num.ToStringPercent());
			}
			stringBuilder.AppendLine(base.GetInspectString());
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			foreach (StatDrawEntry item in base.SpecialDisplayStats())
			{
				yield return item;
			}
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "BodySize".Translate(), InnerPawn.BodySize.ToString("F2"), "Stat_Race_BodySize_Desc".Translate(), 500);
			if (this.GetRotStage() == RotStage.Fresh)
			{
				StatDef meatAmount = StatDefOf.MeatAmount;
				yield return new StatDrawEntry(meatAmount.category, meatAmount, InnerPawn.GetStatValue(meatAmount), StatRequest.For(InnerPawn));
				StatDef leatherAmount = StatDefOf.LeatherAmount;
				yield return new StatDrawEntry(leatherAmount.category, leatherAmount, InnerPawn.GetStatValue(leatherAmount), StatRequest.For(InnerPawn));
			}
			if (ModsConfig.BiotechActive && InnerPawn.RaceProps.IsMechanoid)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Mechanoid, "MechWeightClass".Translate(), InnerPawn.RaceProps.mechWeightClass.ToStringHuman().CapitalizeFirst(), "MechWeightClassExplanation".Translate(), 500);
			}
		}

		public void RotStageChanged()
		{
			InnerPawn.Drawer.renderer.WoundOverlays.ClearCache();
			PortraitsCache.SetDirty(InnerPawn);
			GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(InnerPawn);
			NotifyColonistBar();
		}

		private BodyPartRecord GetBestBodyPartToEat(Pawn ingester, float nutritionWanted)
		{
			IEnumerable<BodyPartRecord> source = from x in InnerPawn.health.hediffSet.GetNotMissingParts()
				where x.depth == BodyPartDepth.Outside && FoodUtility.GetBodyPartNutrition(this, x) > 0.001f
				select x;
			if (!source.Any())
			{
				return null;
			}
			return source.MinBy((BodyPartRecord x) => Mathf.Abs(FoodUtility.GetBodyPartNutrition(this, x) - nutritionWanted));
		}

		private void NotifyColonistBar()
		{
			if (InnerPawn.Faction == Faction.OfPlayer && Current.ProgramState == ProgramState.Playing)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
		}

		public void Notify_BillDeleted(Bill bill)
		{
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (DebugSettings.ShowDevGizmos)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Resurrect",
					action = delegate
					{
						ResurrectionUtility.Resurrect(InnerPawn);
					}
				};
			}
		}
	}
}
