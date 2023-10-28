using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace Verse
{
	public class Pawn : ThingWithComps, IStrippable, IBillGiver, IVerbOwner, ITrader, IAttackTarget, ILoadReferenceable, IAttackTargetSearcher, IThingHolder
	{
		public PawnKindDef kindDef;

		private Name nameInt;

		public Gender gender;

		public Pawn_AgeTracker ageTracker;

		public Pawn_HealthTracker health;

		public Pawn_RecordsTracker records;

		public Pawn_InventoryTracker inventory;

		public Pawn_MeleeVerbs meleeVerbs;

		public VerbTracker verbTracker;

		public Pawn_Ownership ownership;

		public Pawn_CarryTracker carryTracker;

		public Pawn_NeedsTracker needs;

		public Pawn_MindState mindState;

		public Pawn_SurroundingsTracker surroundings;

		public Pawn_Thinker thinker;

		public Pawn_JobTracker jobs;

		public Pawn_StanceTracker stances;

		public Pawn_RotationTracker rotationTracker;

		public Pawn_PathFollower pather;

		public Pawn_NativeVerbs natives;

		public Pawn_FilthTracker filth;

		public Pawn_RopeTracker roping;

		public Pawn_EquipmentTracker equipment;

		public Pawn_ApparelTracker apparel;

		public Pawn_SkillTracker skills;

		public Pawn_StoryTracker story;

		public Pawn_GuestTracker guest;

		public Pawn_GuiltTracker guilt;

		public Pawn_RoyaltyTracker royalty;

		public Pawn_AbilityTracker abilities;

		public Pawn_IdeoTracker ideo;

		public Pawn_GeneTracker genes;

		public Pawn_WorkSettings workSettings;

		public Pawn_TraderTracker trader;

		public Pawn_StyleTracker style;

		public Pawn_StyleObserverTracker styleObserver;

		public Pawn_ConnectionsTracker connections;

		public Pawn_TrainingTracker training;

		public Pawn_CallTracker caller;

		public Pawn_PsychicEntropyTracker psychicEntropy;

		public Pawn_RelationsTracker relations;

		public Pawn_InteractionsTracker interactions;

		public Pawn_PlayerSettings playerSettings;

		public Pawn_OutfitTracker outfits;

		public Pawn_DrugPolicyTracker drugs;

		public Pawn_FoodRestrictionTracker foodRestriction;

		public Pawn_TimetableTracker timetable;

		public Pawn_InventoryStockTracker inventoryStock;

		public Pawn_MechanitorTracker mechanitor;

		public Pawn_LearningTracker learning;

		public Pawn_DraftController drafter;

		private Pawn_DrawTracker drawer;

		public int becameWorldPawnTickAbs = -1;

		public bool teleporting;

		public bool forceNoDeathNotification;

		public int showNamePromptOnTick = -1;

		public int babyNamingDeadline = -1;

		private Sustainer sustainerAmbient;

		private const float HumanSizedHeatOutput = 0.3f;

		private const float AnimalHeatOutputFactor = 0.6f;

		public const int DefaultBabyNamingPeriod = 60000;

		public const int DefaultGrowthMomentChoicePeriod = 120000;

		private static string NotSurgeryReadyTrans;

		private static string CannotReachTrans;

		private CompOverseerSubject overseerSubject;

		public const int MaxMoveTicks = 450;

		private static List<ExtraFaction> tmpExtraFactions = new List<ExtraFaction>();

		private static List<string> states = new List<string>();

		private int lastSleepDisturbedTick;

		private const int SleepDisturbanceMinInterval = 300;

		private List<WorkTypeDef> cachedDisabledWorkTypes;

		private List<WorkTypeDef> cachedDisabledWorkTypesPermanent;

		private Dictionary<WorkTypeDef, List<string>> cachedReasonsForDisabledWorkTypes;

		public Name Name
		{
			get
			{
				return nameInt;
			}
			set
			{
				nameInt = value;
			}
		}

		public RaceProperties RaceProps => def.race;

		public Job CurJob
		{
			get
			{
				if (jobs == null)
				{
					return null;
				}
				return jobs.curJob;
			}
		}

		public JobDef CurJobDef
		{
			get
			{
				if (CurJob == null)
				{
					return null;
				}
				return CurJob.def;
			}
		}

		public bool Downed => health.Downed;

		public bool Dead => health.Dead;

		public string KindLabel => GenLabel.BestKindLabel(this);

		public bool InMentalState
		{
			get
			{
				if (Dead)
				{
					return false;
				}
				return mindState.mentalStateHandler.InMentalState;
			}
		}

		public MentalState MentalState
		{
			get
			{
				if (Dead)
				{
					return null;
				}
				return mindState.mentalStateHandler.CurState;
			}
		}

		public MentalStateDef MentalStateDef
		{
			get
			{
				if (Dead)
				{
					return null;
				}
				return mindState.mentalStateHandler.CurStateDef;
			}
		}

		public bool InAggroMentalState
		{
			get
			{
				if (Dead)
				{
					return false;
				}
				if (mindState.mentalStateHandler.InMentalState)
				{
					return mindState.mentalStateHandler.CurStateDef.IsAggro;
				}
				return false;
			}
		}

		public bool Inspired
		{
			get
			{
				if (Dead)
				{
					return false;
				}
				if (mindState?.inspirationHandler != null)
				{
					return mindState.inspirationHandler.Inspired;
				}
				return false;
			}
		}

		public Inspiration Inspiration
		{
			get
			{
				if (Dead)
				{
					return null;
				}
				return mindState.inspirationHandler.CurState;
			}
		}

		public InspirationDef InspirationDef
		{
			get
			{
				if (Dead)
				{
					return null;
				}
				return mindState.inspirationHandler.CurStateDef;
			}
		}

		public override Vector3 DrawPos => Drawer.DrawPos;

		public VerbTracker VerbTracker => verbTracker;

		public List<VerbProperties> VerbProperties => def.Verbs;

		public List<Tool> Tools => def.tools;

		public bool ShouldAvoidFences
		{
			get
			{
				if (!def.race.FenceBlocked)
				{
					return roping.AnyRopeesFenceBlocked;
				}
				return true;
			}
		}

		public bool IsColonist
		{
			get
			{
				if (base.Faction != null && base.Faction.IsPlayer && RaceProps.Humanlike)
				{
					if (IsSlave)
					{
						return guest.SlaveIsSecure;
					}
					return true;
				}
				return false;
			}
		}

		public bool IsFreeColonist
		{
			get
			{
				if (IsColonist)
				{
					return HostFaction == null;
				}
				return false;
			}
		}

		public bool IsFreeNonSlaveColonist
		{
			get
			{
				if (IsFreeColonist)
				{
					return !IsSlave;
				}
				return false;
			}
		}

		public Faction HostFaction
		{
			get
			{
				if (guest == null)
				{
					return null;
				}
				return guest.HostFaction;
			}
		}

		public Faction SlaveFaction => guest?.SlaveFaction;

		public Ideo Ideo
		{
			get
			{
				if (ideo == null)
				{
					return null;
				}
				return ideo.Ideo;
			}
		}

		public bool Drafted
		{
			get
			{
				if (drafter != null)
				{
					return drafter.Drafted;
				}
				return false;
			}
		}

		public bool IsPrisoner
		{
			get
			{
				if (guest != null)
				{
					return guest.IsPrisoner;
				}
				return false;
			}
		}

		public bool IsPrisonerOfColony
		{
			get
			{
				if (guest != null && guest.IsPrisoner)
				{
					return guest.HostFaction.IsPlayer;
				}
				return false;
			}
		}

		public bool IsSlave
		{
			get
			{
				if (guest != null)
				{
					return guest.IsSlave;
				}
				return false;
			}
		}

		public bool IsSlaveOfColony
		{
			get
			{
				if (IsSlave)
				{
					return base.Faction.IsPlayer;
				}
				return false;
			}
		}

		public DevelopmentalStage DevelopmentalStage => ageTracker?.CurLifeStage?.developmentalStage ?? DevelopmentalStage.Adult;

		public GuestStatus? GuestStatus
		{
			get
			{
				if (guest != null && (HostFaction != null || guest.GuestStatus != 0))
				{
					return guest.GuestStatus;
				}
				return null;
			}
		}

		public bool IsColonistPlayerControlled
		{
			get
			{
				if (base.Spawned && IsColonist && MentalStateDef == null)
				{
					if (HostFaction != null)
					{
						return IsSlave;
					}
					return true;
				}
				return false;
			}
		}

		public bool IsColonyMech
		{
			get
			{
				if (ModsConfig.BiotechActive && RaceProps.IsMechanoid && base.Faction == Faction.OfPlayer && MentalStateDef == null)
				{
					if (HostFaction != null)
					{
						return IsSlave;
					}
					return true;
				}
				return false;
			}
		}

		public bool IsColonyMechPlayerControlled
		{
			get
			{
				if (base.Spawned && IsColonyMech && OverseerSubject != null)
				{
					return OverseerSubject.State == OverseerSubjectState.Overseen;
				}
				return false;
			}
		}

		public IEnumerable<IntVec3> IngredientStackCells
		{
			get
			{
				yield return InteractionCell;
			}
		}

		public bool InContainerEnclosed => base.ParentHolder.IsEnclosingContainer();

		public Corpse Corpse => base.ParentHolder as Corpse;

		public Pawn CarriedBy
		{
			get
			{
				if (base.ParentHolder == null)
				{
					return null;
				}
				if (base.ParentHolder is Pawn_CarryTracker pawn_CarryTracker)
				{
					return pawn_CarryTracker.pawn;
				}
				return null;
			}
		}

		public override string LabelNoCount
		{
			get
			{
				if (Name != null)
				{
					if (story == null || story.TitleShortCap.NullOrEmpty())
					{
						return Name.ToStringShort;
					}
					return Name.ToStringShort + (", " + story.TitleShortCap).Colorize(ColoredText.SubtleGrayColor);
				}
				return KindLabel;
			}
		}

		public override string LabelShort
		{
			get
			{
				if (Name != null)
				{
					return Name.ToStringShort;
				}
				return LabelNoCount;
			}
		}

		public TaggedString LabelNoCountColored
		{
			get
			{
				if (Name != null)
				{
					if (story == null || story.TitleShortCap.NullOrEmpty())
					{
						return Name.ToStringShort.Colorize(ColoredText.NameColor);
					}
					return Name.ToStringShort.Colorize(ColoredText.NameColor) + (", " + story.TitleShortCap).Colorize(ColoredText.SubtleGrayColor);
				}
				return KindLabel;
			}
		}

		public TaggedString NameShortColored
		{
			get
			{
				if (Name != null)
				{
					return Name.ToStringShort.Colorize(ColoredText.NameColor);
				}
				return KindLabel;
			}
		}

		public TaggedString NameFullColored
		{
			get
			{
				if (Name != null)
				{
					return Name.ToStringFull.Colorize(ColoredText.NameColor);
				}
				return KindLabel;
			}
		}

		public TaggedString LegalStatus
		{
			get
			{
				if (IsSlave)
				{
					return "Slave".Translate();
				}
				if (base.Faction != null)
				{
					return new TaggedString(base.Faction.def.pawnSingular);
				}
				return "Colonist".Translate();
			}
		}

		public override string DescriptionDetailed => DescriptionFlavor;

		public override string DescriptionFlavor
		{
			get
			{
				if (this.IsBaseliner())
				{
					return def.description;
				}
				string text = ((genes.Xenotype != XenotypeDefOf.Baseliner) ? genes.Xenotype.description : ((genes.CustomXenotype == null) ? genes.Xenotype.description : ((string)"UniqueXenotypeDesc".Translate())));
				return "StatsReport_NonBaselinerDescription".Translate(genes.XenotypeLabel) + "\n\n" + text;
			}
		}

		public override IEnumerable<DefHyperlink> DescriptionHyperlinks
		{
			get
			{
				foreach (DefHyperlink descriptionHyperlink in base.DescriptionHyperlinks)
				{
					yield return descriptionHyperlink;
				}
				if (!this.IsBaseliner() && genes.CustomXenotype == null)
				{
					yield return new DefHyperlink(genes.Xenotype);
				}
			}
		}

		public Pawn_DrawTracker Drawer
		{
			get
			{
				if (drawer == null)
				{
					drawer = new Pawn_DrawTracker(this);
				}
				return drawer;
			}
		}

		public Faction HomeFaction
		{
			get
			{
				if (base.Faction != null && base.Faction.IsPlayer)
				{
					if (IsSlave && SlaveFaction != null)
					{
						return SlaveFaction;
					}
					if (this.HasExtraMiniFaction())
					{
						return this.GetExtraMiniFaction();
					}
					return this.GetExtraHomeFaction() ?? base.Faction;
				}
				return base.Faction;
			}
		}

		public bool Deathresting
		{
			get
			{
				if (!ModsConfig.BiotechActive)
				{
					return false;
				}
				return health.hediffSet.HasHediff(HediffDefOf.Deathrest);
			}
		}

		public override bool Suspended
		{
			get
			{
				if (base.Suspended)
				{
					return true;
				}
				if (Find.WorldPawns.GetSituation(this) == WorldPawnSituation.ReservedByQuest)
				{
					return true;
				}
				return false;
			}
		}

		public BillStack BillStack => health.surgeryBills;

		public override IntVec3 InteractionCell => this.CurrentBed()?.FindPreferredInteractionCell(base.Position) ?? base.InteractionCell;

		public CompOverseerSubject OverseerSubject
		{
			get
			{
				if (ModsConfig.BiotechActive && overseerSubject == null && RaceProps.IsMechanoid)
				{
					overseerSubject = GetComp<CompOverseerSubject>();
				}
				return overseerSubject;
			}
		}

		public TraderKindDef TraderKind
		{
			get
			{
				if (trader == null)
				{
					return null;
				}
				return trader.traderKind;
			}
		}

		public IEnumerable<Thing> Goods => trader.Goods;

		public int RandomPriceFactorSeed => trader.RandomPriceFactorSeed;

		public string TraderName => trader.TraderName;

		public bool CanTradeNow
		{
			get
			{
				if (trader != null)
				{
					return trader.CanTradeNow;
				}
				return false;
			}
		}

		public float TradePriceImprovementOffsetForPlayer => 0f;

		public float BodySize => ageTracker.CurLifeStage.bodySizeFactor * RaceProps.baseBodySize;

		public float HealthScale => ageTracker.CurLifeStage.healthScaleFactor * RaceProps.baseHealthScale;

		public IEnumerable<Thing> EquippedWornOrInventoryThings => inventory.innerContainer.ConcatIfNotNull(apparel?.WornApparel).ConcatIfNotNull(equipment?.AllEquipmentListForReading);

		Thing IAttackTarget.Thing => this;

		public float TargetPriorityFactor => 1f;

		public LocalTargetInfo TargetCurrentlyAimingAt
		{
			get
			{
				if (!base.Spawned)
				{
					return LocalTargetInfo.Invalid;
				}
				Stance curStance = stances.curStance;
				if (curStance is Stance_Warmup || curStance is Stance_Cooldown)
				{
					return ((Stance_Busy)curStance).focusTarg;
				}
				return LocalTargetInfo.Invalid;
			}
		}

		Thing IAttackTargetSearcher.Thing => this;

		public LocalTargetInfo LastAttackedTarget => mindState.lastAttackedTarget;

		public int LastAttackTargetTick => mindState.lastAttackTargetTick;

		public Verb CurrentEffectiveVerb
		{
			get
			{
				if (this.MannedThing() is Building_Turret building_Turret)
				{
					return building_Turret.AttackVerb;
				}
				return TryGetAttackVerb(null, !IsColonist);
			}
		}

		private bool ForceNoDeathNotification
		{
			get
			{
				if (!forceNoDeathNotification)
				{
					return kindDef.forceNoDeathNotification;
				}
				return true;
			}
		}

		Thing IVerbOwner.ConstantCaster => this;

		ImplementOwnerTypeDef IVerbOwner.ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.Bodypart;

		public int TicksPerMoveCardinal => TicksPerMove(diagonal: false);

		public int TicksPerMoveDiagonal => TicksPerMove(diagonal: true);

		public TradeCurrency TradeCurrency => TraderKind.tradeCurrency;

		public WorkTags CombinedDisabledWorkTags
		{
			get
			{
				WorkTags workTags = ((story != null) ? story.DisabledWorkTagsBackstoryTraitsAndGenes : WorkTags.None);
				if (royalty != null)
				{
					foreach (RoyalTitle item in royalty.AllTitlesForReading)
					{
						if (item.conceited)
						{
							workTags |= item.def.disabledWorkTags;
						}
					}
				}
				if (ModsConfig.IdeologyActive && Ideo != null)
				{
					Precept_Role role = Ideo.GetRole(this);
					if (role != null)
					{
						workTags |= role.def.roleDisabledWorkTags;
					}
				}
				if (health != null && health.hediffSet != null)
				{
					foreach (Hediff hediff in health.hediffSet.hediffs)
					{
						HediffStage curStage = hediff.CurStage;
						if (curStage != null)
						{
							workTags |= curStage.disabledWorkTags;
						}
					}
				}
				foreach (QuestPart_WorkDisabled item2 in QuestUtility.GetWorkDisabledQuestPart(this))
				{
					workTags |= item2.disabledWorkTags;
				}
				return workTags;
			}
		}

		public bool HasPsylink
		{
			get
			{
				if (psychicEntropy != null)
				{
					return psychicEntropy.Psylink != null;
				}
				return false;
			}
		}

		string IVerbOwner.UniqueVerbOwnerID()
		{
			return GetUniqueLoadID();
		}

		bool IVerbOwner.VerbsStillUsableBy(Pawn p)
		{
			return p == this;
		}

		public int GetRootTile()
		{
			return base.Tile;
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return null;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
			if (inventory != null)
			{
				outChildren.Add(inventory);
			}
			if (carryTracker != null)
			{
				outChildren.Add(carryTracker);
			}
			if (equipment != null)
			{
				outChildren.Add(equipment);
			}
			if (apparel != null)
			{
				outChildren.Add(apparel);
			}
		}

		public string GetKindLabelPlural(int count = -1)
		{
			return GenLabel.BestKindLabel(this, mustNoteGender: false, mustNoteLifeStage: false, plural: true, count);
		}

		public static void ResetStaticData()
		{
			NotSurgeryReadyTrans = "NotSurgeryReady".Translate();
			CannotReachTrans = "CannotReach".Translate();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref kindDef, "kindDef");
			Scribe_Values.Look(ref gender, "gender", Gender.Male);
			Scribe_Values.Look(ref becameWorldPawnTickAbs, "becameWorldPawnTickAbs", -1);
			Scribe_Values.Look(ref teleporting, "teleporting", defaultValue: false);
			Scribe_Values.Look(ref showNamePromptOnTick, "showNamePromptOnTick", -1);
			Scribe_Values.Look(ref babyNamingDeadline, "babyNamingDeadline", -1);
			Scribe_Deep.Look(ref nameInt, "name");
			Scribe_Deep.Look(ref mindState, "mindState", this);
			Scribe_Deep.Look(ref jobs, "jobs", this);
			Scribe_Deep.Look(ref stances, "stances", this);
			Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
			Scribe_Deep.Look(ref natives, "natives", this);
			Scribe_Deep.Look(ref meleeVerbs, "meleeVerbs", this);
			Scribe_Deep.Look(ref rotationTracker, "rotationTracker", this);
			Scribe_Deep.Look(ref pather, "pather", this);
			Scribe_Deep.Look(ref carryTracker, "carryTracker", this);
			Scribe_Deep.Look(ref apparel, "apparel", this);
			Scribe_Deep.Look(ref story, "story", this);
			Scribe_Deep.Look(ref equipment, "equipment", this);
			Scribe_Deep.Look(ref drafter, "drafter", this);
			Scribe_Deep.Look(ref ageTracker, "ageTracker", this);
			Scribe_Deep.Look(ref health, "healthTracker", this);
			Scribe_Deep.Look(ref records, "records", this);
			Scribe_Deep.Look(ref inventory, "inventory", this);
			Scribe_Deep.Look(ref filth, "filth", this);
			Scribe_Deep.Look(ref roping, "roping", this);
			Scribe_Deep.Look(ref needs, "needs", this);
			Scribe_Deep.Look(ref guest, "guest", this);
			Scribe_Deep.Look(ref guilt, "guilt", this);
			Scribe_Deep.Look(ref royalty, "royalty", this);
			Scribe_Deep.Look(ref relations, "social", this);
			Scribe_Deep.Look(ref psychicEntropy, "psychicEntropy", this);
			Scribe_Deep.Look(ref ownership, "ownership", this);
			Scribe_Deep.Look(ref interactions, "interactions", this);
			Scribe_Deep.Look(ref skills, "skills", this);
			Scribe_Deep.Look(ref abilities, "abilities", this);
			Scribe_Deep.Look(ref ideo, "ideo", this);
			Scribe_Deep.Look(ref workSettings, "workSettings", this);
			Scribe_Deep.Look(ref trader, "trader", this);
			Scribe_Deep.Look(ref outfits, "outfits", this);
			Scribe_Deep.Look(ref drugs, "drugs", this);
			Scribe_Deep.Look(ref foodRestriction, "foodRestriction", this);
			Scribe_Deep.Look(ref timetable, "timetable", this);
			Scribe_Deep.Look(ref playerSettings, "playerSettings", this);
			Scribe_Deep.Look(ref training, "training", this);
			Scribe_Deep.Look(ref style, "style", this);
			Scribe_Deep.Look(ref styleObserver, "styleObserver", this);
			Scribe_Deep.Look(ref connections, "connections", this);
			Scribe_Deep.Look(ref inventoryStock, "inventoryStock", this);
			Scribe_Deep.Look(ref surroundings, "treeSightings", this);
			Scribe_Deep.Look(ref thinker, "thinker", this);
			Scribe_Deep.Look(ref mechanitor, "mechanitor", this);
			Scribe_Deep.Look(ref genes, "genes", this);
			Scribe_Deep.Look(ref learning, "learning", this);
			BackCompatibility.PostExposeData(this);
		}

		public override string ToString()
		{
			if (story != null)
			{
				return LabelShort;
			}
			if (thingIDNumber > 0)
			{
				return base.ThingID;
			}
			if (kindDef != null)
			{
				return KindLabel + "_" + base.ThingID;
			}
			if (def != null)
			{
				return base.ThingID;
			}
			return GetType().ToString();
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			if (Dead)
			{
				Log.Warning("Tried to spawn Dead Pawn " + this.ToStringSafe() + ". Replacing with corpse.");
				Corpse obj = (Corpse)ThingMaker.MakeThing(RaceProps.corpseDef);
				obj.InnerPawn = this;
				GenSpawn.Spawn(obj, base.Position, map);
				return;
			}
			if (def == null || kindDef == null)
			{
				Log.Warning("Tried to spawn pawn without def " + this.ToStringSafe() + ".");
				return;
			}
			base.SpawnSetup(map, respawningAfterLoad);
			if (Find.WorldPawns.Contains(this))
			{
				Find.WorldPawns.RemovePawn(this);
			}
			PawnComponentsUtility.AddComponentsForSpawn(this);
			if (!PawnUtility.InValidState(this))
			{
				Log.Error("Pawn " + this.ToStringSafe() + " spawned in invalid state. Destroying...");
				try
				{
					DeSpawn();
				}
				catch (Exception ex)
				{
					Log.Error("Tried to despawn " + this.ToStringSafe() + " because of the previous error but couldn't: " + ex);
				}
				Find.WorldPawns.PassToWorld(this, PawnDiscardDecideMode.Discard);
				return;
			}
			Drawer.Notify_Spawned();
			rotationTracker.Notify_Spawned();
			if (!respawningAfterLoad)
			{
				pather.ResetToCurrentPosition();
			}
			base.Map.mapPawns.RegisterPawn(this);
			base.Map.autoSlaughterManager.Notify_PawnSpawned();
			if (RaceProps.IsFlesh)
			{
				relations.everSeenByPlayer = true;
			}
			AddictionUtility.CheckDrugAddictionTeachOpportunity(this);
			if (needs != null && needs.mood != null && needs.mood.recentMemory != null)
			{
				needs.mood.recentMemory.Notify_Spawned(respawningAfterLoad);
			}
			if (equipment != null)
			{
				equipment.Notify_PawnSpawned();
			}
			if (mechanitor != null)
			{
				mechanitor.Notify_PawnSpawned(respawningAfterLoad);
			}
			if (base.Faction == Faction.OfPlayer)
			{
				Ideo?.RecacheColonistBelieverCount();
			}
			if (!respawningAfterLoad)
			{
				Find.GameEnder.CheckOrUpdateGameOver();
				if (base.Faction == Faction.OfPlayer)
				{
					Find.StoryWatcher.statsRecord.UpdateGreatestPopulation();
					Find.World.StoryState.RecordPopulationIncrease();
				}
				PawnDiedOrDownedThoughtsUtility.RemoveDiedThoughts(this);
				if (this.IsQuestLodger())
				{
					for (int num = health.hediffSet.hediffs.Count - 1; num >= 0; num--)
					{
						if (health.hediffSet.hediffs[num].def.removeOnQuestLodgers)
						{
							health.RemoveHediff(health.hediffSet.hediffs[num]);
						}
					}
				}
			}
			if (RaceProps.soundAmbience != null)
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					sustainerAmbient = RaceProps.soundAmbience.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
				});
			}
		}

		public override void PostMapInit()
		{
			base.PostMapInit();
			pather.TryResumePathingAfterLoading();
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			Drawer.DrawAt(drawLoc);
			mechanitor?.DrawCommandRadius();
		}

		public override void DrawGUIOverlay()
		{
			Drawer.ui.DrawPawnGUIOverlay();
			for (int i = 0; i < base.AllComps.Count; i++)
			{
				base.AllComps[i].DrawGUIOverlay();
			}
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			if (IsColonistPlayerControlled || IsColonyMechPlayerControlled)
			{
				if (pather.curPath != null)
				{
					pather.curPath.DrawPath(this);
				}
				jobs.DrawLinesBetweenTargets();
			}
		}

		public override void TickRare()
		{
			base.TickRare();
			if (!Suspended)
			{
				if (apparel != null)
				{
					apparel.ApparelTrackerTickRare();
				}
				inventory.InventoryTrackerTickRare();
			}
			if (training != null)
			{
				training.TrainingTrackerTickRare();
			}
			if (base.Spawned && RaceProps.IsFlesh)
			{
				GenTemperature.PushHeat(this, 0.3f * BodySize * 4.1666665f * (def.race.Humanlike ? 1f : 0.6f));
			}
		}

		public override void Tick()
		{
			if (DebugSettings.noAnimals && base.Spawned && RaceProps.Animal)
			{
				Destroy();
				return;
			}
			base.Tick();
			if (Find.TickManager.TicksGame % 250 == 0)
			{
				TickRare();
			}
			bool suspended;
			using (new ProfilerBlock("Suspended"))
			{
				suspended = Suspended;
			}
			if (!suspended)
			{
				if (base.Spawned)
				{
					pather.PatherTick();
				}
				if (base.Spawned)
				{
					stances.StanceTrackerTick();
					verbTracker.VerbsTick();
				}
				if (base.Spawned)
				{
					roping.RopingTick();
					natives.NativeVerbsTick();
				}
				if (!this.IsWorldPawn())
				{
					jobs?.JobTrackerTick();
				}
				health.HealthTick();
				if (!Dead)
				{
					mindState.MindStateTick();
					carryTracker.CarryHandsTick();
					if (showNamePromptOnTick != -1 && showNamePromptOnTick == Find.TickManager.TicksGame)
					{
						Find.WindowStack.Add(this.NamePawnDialog());
					}
				}
			}
			if (!Dead)
			{
				needs.NeedsTrackerTick();
			}
			if (!suspended)
			{
				if (equipment != null)
				{
					equipment.EquipmentTrackerTick();
				}
				if (apparel != null)
				{
					apparel.ApparelTrackerTick();
				}
				if (interactions != null && base.Spawned)
				{
					interactions.InteractionsTrackerTick();
				}
				if (caller != null)
				{
					caller.CallTrackerTick();
				}
				if (skills != null)
				{
					skills.SkillsTick();
				}
				if (abilities != null)
				{
					abilities.AbilitiesTick();
				}
				if (inventory != null)
				{
					inventory.InventoryTrackerTick();
				}
				if (drafter != null)
				{
					drafter.DraftControllerTick();
				}
				if (relations != null)
				{
					relations.RelationsTrackerTick();
				}
				if (ModsConfig.RoyaltyActive && psychicEntropy != null)
				{
					psychicEntropy.PsychicEntropyTrackerTick();
				}
				if (RaceProps.Humanlike)
				{
					guest.GuestTrackerTick();
				}
				if (ideo != null)
				{
					ideo.IdeoTrackerTick();
				}
				if (genes != null)
				{
					genes.GeneTrackerTick();
				}
				if (royalty != null && ModsConfig.RoyaltyActive)
				{
					royalty.RoyaltyTrackerTick();
				}
				if (style != null && ModsConfig.IdeologyActive)
				{
					style.StyleTrackerTick();
				}
				if (styleObserver != null && ModsConfig.IdeologyActive)
				{
					styleObserver.StyleObserverTick();
				}
				if (surroundings != null && ModsConfig.IdeologyActive)
				{
					surroundings.SurroundingsTrackerTick();
				}
				if (ModsConfig.BiotechActive && learning != null)
				{
					learning.LearningTick();
				}
				if (ModsConfig.BiotechActive)
				{
					PollutionUtility.PawnPollutionTick(this);
					GasUtility.PawnGasEffectsTick(this);
				}
				ageTracker.AgeTick();
				records.RecordsTick();
			}
			guilt?.GuiltTrackerTick();
			sustainerAmbient?.Maintain();
			drawer?.renderer.EffectersTick(suspended || this.IsWorldPawn());
		}

		public void ProcessPostTickVisuals(int ticksPassed, CellRect viewRect)
		{
			if (!Suspended && base.Spawned)
			{
				if (Current.ProgramState != ProgramState.Playing || viewRect.Contains(base.Position))
				{
					Drawer.ProcessPostTickVisuals(ticksPassed);
				}
				rotationTracker.ProcessPostTickVisuals(ticksPassed);
			}
		}

		public void TickMothballed(int interval)
		{
			if (!Suspended)
			{
				ageTracker.AgeTickMothballed(interval);
				records.RecordsTickMothballed(interval);
			}
		}

		public void Notify_Teleported(bool endCurrentJob = true, bool resetTweenedPos = true)
		{
			if (resetTweenedPos)
			{
				Drawer.tweener.ResetTweenedPosToRoot();
			}
			pather.Notify_Teleported_Int();
			if (endCurrentJob && jobs != null && jobs.curJob != null)
			{
				jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}

		public void Notify_PassedToWorld()
		{
			if (((base.Faction == null && RaceProps.Humanlike) || (base.Faction != null && base.Faction.IsPlayer) || base.Faction == Faction.OfAncients || base.Faction == Faction.OfAncientsHostile) && !Dead && Find.WorldPawns.GetSituation(this) == WorldPawnSituation.Free)
			{
				bool tryMedievalOrBetter = base.Faction != null && (int)base.Faction.def.techLevel >= 3;
				Faction faction;
				if (this.HasExtraHomeFaction() && !this.GetExtraHomeFaction().IsPlayer)
				{
					if (base.Faction != this.GetExtraHomeFaction())
					{
						SetFaction(this.GetExtraHomeFaction());
					}
				}
				else if (Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, tryMedievalOrBetter))
				{
					if (base.Faction != faction)
					{
						SetFaction(faction);
					}
				}
				else if (Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, tryMedievalOrBetter, allowDefeated: true))
				{
					if (base.Faction != faction)
					{
						SetFaction(faction);
					}
				}
				else if (base.Faction != null)
				{
					SetFaction(null);
				}
			}
			becameWorldPawnTickAbs = GenTicks.TicksAbs;
			if (!this.IsCaravanMember() && !PawnUtility.IsTravelingInTransportPodWorldObject(this))
			{
				ClearMind();
			}
			if (relations != null)
			{
				relations.Notify_PassedToWorld();
			}
		}

		public void Notify_AddBedThoughts()
		{
			foreach (ThingComp allComp in base.AllComps)
			{
				allComp.Notify_AddBedThoughts(this);
			}
			Ideo?.Notify_AddBedThoughts(this);
		}

		public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
			if (ModsConfig.BiotechActive && genes != null)
			{
				float num = genes.FactorForDamage(dinfo);
				if (num != 1f)
				{
					dinfo.SetAmount(dinfo.Amount * num);
				}
			}
			base.PreApplyDamage(ref dinfo, out absorbed);
			if (!absorbed)
			{
				health.PreApplyDamage(dinfo, out absorbed);
			}
		}

		public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			base.PostApplyDamage(dinfo, totalDamageDealt);
			if (dinfo.Def.ExternalViolenceFor(this))
			{
				records.AddTo(RecordDefOf.DamageTaken, totalDamageDealt);
			}
			if (dinfo.Def.makesBlood && !dinfo.InstantPermanentInjury && totalDamageDealt > 0f && Rand.Chance(0.5f))
			{
				health.DropBloodFilth();
			}
			health.PostApplyDamage(dinfo, totalDamageDealt);
			if (!Dead)
			{
				mindState.Notify_DamageTaken(dinfo);
			}
		}

		public override Thing SplitOff(int count)
		{
			if (count <= 0 || count >= stackCount)
			{
				return base.SplitOff(count);
			}
			throw new NotImplementedException("Split off on Pawns is not supported (unless we're taking a full stack).");
		}

		private int TicksPerMove(bool diagonal)
		{
			float num = this.GetStatValue(StatDefOf.MoveSpeed);
			if (RestraintsUtility.InRestraints(this))
			{
				num *= 0.35f;
			}
			if (carryTracker != null && carryTracker.CarriedThing != null && carryTracker.CarriedThing.def.category == ThingCategory.Pawn)
			{
				num *= 0.6f;
			}
			float num2 = num / 60f;
			float num3;
			if (num2 == 0f)
			{
				num3 = 450f;
			}
			else
			{
				num3 = 1f / num2;
				if (base.Spawned && !base.Map.roofGrid.Roofed(base.Position))
				{
					num3 /= base.Map.weatherManager.CurMoveSpeedMultiplier;
				}
				if (diagonal)
				{
					num3 *= 1.41421f;
				}
			}
			return Mathf.Clamp(Mathf.RoundToInt(num3), 1, 450);
		}

		private void DoKillSideEffects(DamageInfo? dinfo, Hediff exactCulprit, bool spawned)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				Find.Storyteller.Notify_PawnEvent(this, AdaptationEvent.Died);
			}
			if (IsColonist)
			{
				Find.StoryWatcher.statsRecord.Notify_ColonistKilled();
			}
			if (spawned && dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(this))
			{
				LifeStageUtility.PlayNearestLifestageSound(this, (LifeStageAge ls) => ls.soundDeath, (GeneDef g) => g.soundDeath);
			}
			if (dinfo.HasValue && dinfo.Value.Instigator != null && dinfo.Value.Instigator is Pawn pawn)
			{
				RecordsUtility.Notify_PawnKilled(this, pawn);
				if (pawn.equipment != null)
				{
					pawn.equipment.Notify_KilledPawn();
				}
				if (RaceProps.Humanlike)
				{
					pawn.needs?.TryGetNeed<Need_KillThirst>()?.Notify_KilledPawn(dinfo);
				}
				if (pawn.health.hediffSet != null)
				{
					for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
					{
						pawn.health.hediffSet.hediffs[i].Notify_KilledPawn(pawn, dinfo);
					}
				}
				if (HistoryEventUtility.IsKillingInnocentAnimal(pawn, this))
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.KilledInnocentAnimal, pawn.Named(HistoryEventArgsNames.Doer), this.Named(HistoryEventArgsNames.Victim)));
				}
			}
			TaleUtility.Notify_PawnDied(this, dinfo);
			if (spawned)
			{
				Find.BattleLog.Add(new BattleLogEntry_StateTransition(this, RaceProps.DeathActionWorker.DeathRules, dinfo.HasValue ? (dinfo.Value.Instigator as Pawn) : null, exactCulprit, dinfo.HasValue ? dinfo.Value.HitPart : null));
			}
		}

		private void PreDeathPawnModifications(DamageInfo? dinfo, Map map)
		{
			health.surgeryBills.Clear();
			if (apparel != null)
			{
				apparel.Notify_PawnKilled(dinfo);
			}
			if (relations != null)
			{
				relations.Notify_PawnKilled(dinfo, map);
			}
			if (connections != null)
			{
				connections.Notify_PawnKilled();
			}
			meleeVerbs.Notify_PawnKilled();
			for (int i = 0; i < health.hediffSet.hediffs.Count; i++)
			{
				health.hediffSet.hediffs[i].Notify_PawnKilled();
			}
		}

		private void DropBeforeDying(DamageInfo? dinfo, ref Map map, ref bool spawned)
		{
			if (base.ParentHolder is Pawn_CarryTracker pawn_CarryTracker && holdingOwner.TryDrop(this, pawn_CarryTracker.pawn.Position, pawn_CarryTracker.pawn.Map, ThingPlaceMode.Near, out var _))
			{
				map = pawn_CarryTracker.pawn.Map;
				spawned = true;
			}
			PawnDiedOrDownedThoughtsUtility.RemoveLostThoughts(this);
			PawnDiedOrDownedThoughtsUtility.RemoveResuedRelativeThought(this);
			PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(this, dinfo, PawnDiedOrDownedThoughtsKind.Died);
			if (RaceProps.Animal)
			{
				PawnDiedOrDownedThoughtsUtility.GiveVeneratedAnimalDiedThoughts(this, map);
			}
		}

		public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
		{
			int num = 0;
			try
			{
				num = 1;
				IntVec3 positionHeld = base.PositionHeld;
				Map map = base.Map;
				Map mapHeld = base.MapHeld;
				bool spawned = base.Spawned;
				bool spawnedOrAnyParentSpawned = base.SpawnedOrAnyParentSpawned;
				bool wasWorldPawn = this.IsWorldPawn();
				bool? flag = guilt?.IsGuilty;
				Caravan caravan = this.GetCaravan();
				Building_Grave assignedGrave = null;
				if (ownership != null)
				{
					assignedGrave = ownership.AssignedGrave;
				}
				Building_Bed currentBed = this.CurrentBed();
				ThingOwner thingOwner = null;
				bool inContainerEnclosed = InContainerEnclosed;
				if (inContainerEnclosed)
				{
					thingOwner = holdingOwner;
					thingOwner.Remove(this);
				}
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				if (Current.ProgramState == ProgramState.Playing && map != null)
				{
					flag2 = map.designationManager.DesignationOn(this, DesignationDefOf.Hunt) != null;
					flag3 = this.ShouldBeSlaughtered();
					foreach (Lord lord in map.lordManager.lords)
					{
						if (lord.LordJob is LordJob_Ritual lordJob_Ritual && lordJob_Ritual.pawnsDeathIgnored.Contains(this))
						{
							flag4 = true;
							break;
						}
					}
				}
				bool flag5 = PawnUtility.ShouldSendNotificationAbout(this) && (!(flag3 || flag4) || !dinfo.HasValue || dinfo.Value.Def != DamageDefOf.ExecutionCut) && !ForceNoDeathNotification;
				float num2 = 0f;
				Thing attachment = this.GetAttachment(ThingDefOf.Fire);
				if (attachment != null)
				{
					num2 = ((Fire)attachment).CurrentSize();
				}
				num = 2;
				DoKillSideEffects(dinfo, exactCulprit, spawned);
				num = 3;
				PreDeathPawnModifications(dinfo, map);
				num = 4;
				DropBeforeDying(dinfo, ref map, ref spawned);
				num = 5;
				health.SetDead();
				if (health.deflectionEffecter != null)
				{
					health.deflectionEffecter.Cleanup();
					health.deflectionEffecter = null;
				}
				if (health.woundedEffecter != null)
				{
					health.woundedEffecter.Cleanup();
					health.woundedEffecter = null;
				}
				caravan?.Notify_MemberDied(this);
				this.GetLord()?.Notify_PawnLost(this, PawnLostCondition.Killed, dinfo);
				if (spawned)
				{
					DropAndForbidEverything();
				}
				if (spawned)
				{
					GenLeaving.DoLeavingsFor(this, map, DestroyMode.KillFinalize);
				}
				bool num3 = DeSpawnOrDeselect();
				if (royalty != null)
				{
					royalty.Notify_PawnKilled();
				}
				Corpse corpse = null;
				if (!PawnGenerator.IsPawnBeingGeneratedAndNotAllowsDead(this))
				{
					if (inContainerEnclosed)
					{
						corpse = MakeCorpse(assignedGrave, currentBed);
						if (!thingOwner.TryAdd(corpse))
						{
							corpse.Destroy();
							corpse = null;
						}
					}
					else if (spawnedOrAnyParentSpawned)
					{
						if (holdingOwner != null)
						{
							holdingOwner.Remove(this);
						}
						corpse = MakeCorpse(assignedGrave, currentBed);
						if (GenPlace.TryPlaceThing(corpse, positionHeld, mapHeld, ThingPlaceMode.Direct))
						{
							corpse.Rotation = base.Rotation;
							if (HuntJobUtility.WasKilledByHunter(this, dinfo))
							{
								((Pawn)dinfo.Value.Instigator).Reserve(corpse, ((Pawn)dinfo.Value.Instigator).CurJob);
							}
							else if (!flag2 && !flag3)
							{
								corpse.SetForbiddenIfOutsideHomeArea();
							}
							if (num2 > 0f)
							{
								FireUtility.TryStartFireIn(corpse.Position, corpse.Map, num2);
							}
						}
						else
						{
							corpse.Destroy();
							corpse = null;
						}
					}
					else if (caravan != null && caravan.Spawned)
					{
						corpse = MakeCorpse(assignedGrave, currentBed);
						caravan.AddPawnOrItem(corpse, addCarriedPawnToWorldPawnsIfAny: true);
					}
					else if (holdingOwner != null || this.IsWorldPawn())
					{
						Corpse.PostCorpseDestroy(this);
					}
					else
					{
						corpse = MakeCorpse(assignedGrave, currentBed);
					}
				}
				if (corpse != null)
				{
					Hediff firstHediffOfDef = health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
					Hediff firstHediffOfDef2 = health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Scaria);
					CompRottable comp;
					if ((comp = corpse.GetComp<CompRottable>()) != null && ((firstHediffOfDef != null && Rand.Value < firstHediffOfDef.Severity) || (firstHediffOfDef2 != null && Rand.Chance(Find.Storyteller.difficulty.scariaRotChance))))
					{
						comp.RotImmediately();
					}
				}
				if (!base.Destroyed)
				{
					Destroy(DestroyMode.KillFinalize);
				}
				PawnComponentsUtility.RemoveComponentsOnKilled(this);
				health.hediffSet.DirtyCache();
				PortraitsCache.SetDirty(this);
				GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(this);
				if (num3 && corpse != null)
				{
					Find.Selector.Select(corpse, playSound: false, forceDesignatorDeselect: false);
				}
				num = 6;
				health.hediffSet.Notify_PawnDied();
				genes?.Notify_PawnDied();
				HomeFaction?.Notify_MemberDied(this, dinfo, wasWorldPawn, flag == true, mapHeld);
				if (corpse != null)
				{
					if (RaceProps.DeathActionWorker != null && spawned)
					{
						RaceProps.DeathActionWorker.PawnDied(corpse);
					}
					if (Find.Scenario != null)
					{
						Find.Scenario.Notify_PawnDied(corpse);
					}
				}
				if (base.Faction != null && base.Faction.IsPlayer)
				{
					BillUtility.Notify_ColonistUnavailable(this);
				}
				if (spawnedOrAnyParentSpawned)
				{
					GenHostility.Notify_PawnLostForTutor(this, mapHeld);
				}
				if (base.Faction != null && base.Faction.IsPlayer && Current.ProgramState == ProgramState.Playing)
				{
					Find.ColonistBar.MarkColonistsDirty();
				}
				psychicEntropy?.Notify_PawnDied();
				try
				{
					Ideo?.Notify_MemberDied(this);
					Ideo?.Notify_MemberLost(this, map);
				}
				catch (Exception ex)
				{
					Log.Error("Error while notifying ideo of pawn death: " + ex);
				}
				if (flag5)
				{
					health.NotifyPlayerOfKilled(dinfo, exactCulprit, caravan);
				}
				Find.QuestManager.Notify_PawnKilled(this, dinfo);
				Find.FactionManager.Notify_PawnKilled(this);
				Find.IdeoManager.Notify_PawnKilled(this);
				if (ModsConfig.BiotechActive && MechanitorUtility.IsMechanitor(this))
				{
					Find.History.Notify_MechanitorDied();
				}
				Find.BossgroupManager.Notify_PawnKilled(this);
			}
			catch (Exception arg)
			{
				Log.Error($"Error while killing {this.ToStringSafe()} during phase {num}: {arg}");
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if (mode != 0 && mode != DestroyMode.KillFinalize)
			{
				Log.Error(string.Concat("Destroyed pawn ", this, " with unsupported mode ", mode, "."));
			}
			base.Destroy(mode);
			Find.WorldPawns.Notify_PawnDestroyed(this);
			if (ownership != null)
			{
				Building_Grave assignedGrave = ownership.AssignedGrave;
				ownership.UnclaimAll();
				if (mode == DestroyMode.KillFinalize)
				{
					assignedGrave?.CompAssignableToPawn.TryAssignPawn(this);
				}
			}
			ClearMind(ifLayingKeepLaying: false, clearInspiration: true);
			Lord lord = this.GetLord();
			if (lord != null)
			{
				PawnLostCondition cond = ((mode != DestroyMode.KillFinalize) ? PawnLostCondition.Vanished : PawnLostCondition.Killed);
				lord.Notify_PawnLost(this, cond);
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				Find.GameEnder.CheckOrUpdateGameOver();
				Find.TaleManager.Notify_PawnDestroyed(this);
			}
			foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_Alive.Where((Pawn p) => p.playerSettings != null && p.playerSettings.Master == this))
			{
				item.playerSettings.Master = null;
			}
			if (equipment != null)
			{
				equipment.Notify_PawnDied();
			}
			if (mode != DestroyMode.KillFinalize)
			{
				if (equipment != null)
				{
					equipment.DestroyAllEquipment();
				}
				inventory.DestroyAll();
				if (apparel != null)
				{
					apparel.DestroyAll();
				}
			}
			WorldPawns worldPawns = Find.WorldPawns;
			if (!worldPawns.IsBeingDiscarded(this) && !worldPawns.Contains(this))
			{
				worldPawns.PassToWorld(this);
			}
			if (base.Faction.IsPlayerSafe())
			{
				Ideo?.RecacheColonistBelieverCount();
			}
			relations?.Notify_PawnDestroyed(mode);
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			Map map = base.Map;
			if (jobs != null && jobs.curJob != null)
			{
				jobs.StopAll();
			}
			base.DeSpawn(mode);
			if (pather != null)
			{
				pather.StopDead();
			}
			roping?.Notify_DeSpawned();
			mindState.droppedWeapon = null;
			if (needs != null && needs.mood != null)
			{
				needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
			}
			if (meleeVerbs != null)
			{
				meleeVerbs.Notify_PawnDespawned();
			}
			mechanitor?.Notify_DeSpawned(mode);
			ClearAllReservations(releaseDestinationsOnlyIfObsolete: false);
			if (map != null)
			{
				map.mapPawns.DeRegisterPawn(this);
				map.autoSlaughterManager.Notify_PawnDespawned();
			}
			PawnComponentsUtility.RemoveComponentsOnDespawned(this);
			if (sustainerAmbient != null)
			{
				sustainerAmbient.End();
				sustainerAmbient = null;
			}
		}

		public override void Discard(bool silentlyRemoveReferences = false)
		{
			if (Find.WorldPawns.Contains(this))
			{
				Log.Warning(string.Concat("Tried to discard a world pawn ", this, "."));
				return;
			}
			base.Discard(silentlyRemoveReferences);
			if (relations != null)
			{
				relations.ClearAllRelations();
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				Find.PlayLog.Notify_PawnDiscarded(this, silentlyRemoveReferences);
				Find.BattleLog.Notify_PawnDiscarded(this, silentlyRemoveReferences);
				Find.TaleManager.Notify_PawnDiscarded(this, silentlyRemoveReferences);
				Find.QuestManager.Notify_PawnDiscarded(this);
			}
			foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_Alive)
			{
				if (item.needs != null && item.needs.mood != null)
				{
					item.needs.mood.thoughts.memories.Notify_PawnDiscarded(this);
				}
			}
			Corpse.PostCorpseDestroy(this);
		}

		public Corpse MakeCorpse(Building_Grave assignedGrave, Building_Bed currentBed)
		{
			return MakeCorpse(assignedGrave, currentBed != null, currentBed?.Rotation.AsAngle ?? 0f);
		}

		public Corpse MakeCorpse(Building_Grave assignedGrave, bool inBed, float bedRotation)
		{
			if (holdingOwner != null)
			{
				Log.Warning("We can't make corpse because the pawn is in a ThingOwner. Remove him from the container first. This should have been already handled before calling this method. holder=" + base.ParentHolder);
				return null;
			}
			Corpse corpse = (Corpse)ThingMaker.MakeThing(RaceProps.corpseDef);
			corpse.InnerPawn = this;
			if (assignedGrave != null)
			{
				corpse.InnerPawn.ownership.ClaimGrave(assignedGrave);
			}
			if (inBed)
			{
				corpse.InnerPawn.Drawer.renderer.wiggler.SetToCustomRotation(bedRotation + 180f);
			}
			return corpse;
		}

		public void ExitMap(bool allowedToJoinOrCreateCaravan, Rot4 exitDir)
		{
			if (this.IsWorldPawn())
			{
				Log.Warning("Called ExitMap() on world pawn " + this);
				return;
			}
			Ideo?.Notify_MemberLost(this, base.Map);
			if (allowedToJoinOrCreateCaravan && CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(this))
			{
				CaravanExitMapUtility.ExitMapAndJoinOrCreateCaravan(this, exitDir);
				return;
			}
			this.GetLord()?.Notify_PawnLost(this, PawnLostCondition.ExitedMap);
			if (carryTracker != null && carryTracker.CarriedThing != null)
			{
				Pawn pawn = carryTracker.CarriedThing as Pawn;
				if (pawn != null)
				{
					if (base.Faction != null && base.Faction != pawn.Faction)
					{
						base.Faction.kidnapped.Kidnap(pawn, this);
					}
					else
					{
						if (!teleporting)
						{
							carryTracker.innerContainer.Remove(pawn);
						}
						pawn.ExitMap(allowedToJoinOrCreateCaravan: false, exitDir);
					}
				}
				else
				{
					carryTracker.CarriedThing.Destroy();
				}
				if (!teleporting || pawn == null)
				{
					carryTracker.innerContainer.Clear();
				}
			}
			bool flag = ThingOwnerUtility.AnyParentIs<ActiveDropPodInfo>(this) || ThingOwnerUtility.AnyParentIs<TravelingTransportPods>(this);
			bool flag2 = this.IsCaravanMember() || teleporting || flag;
			bool flag3 = !flag2 || (!IsPrisoner && !IsSlave && !flag) || (guest != null && guest.Released);
			bool flag4 = flag3 && (IsPrisoner || IsSlave) && guest != null && guest.Released;
			if (flag3 && !flag2)
			{
				foreach (Thing equippedWornOrInventoryThing in EquippedWornOrInventoryThings)
				{
					equippedWornOrInventoryThing.GetStyleSourcePrecept()?.Notify_ThingLost(equippedWornOrInventoryThing);
				}
			}
			if (base.Faction != null)
			{
				base.Faction.Notify_MemberExitedMap(this, flag4);
			}
			if (base.Faction == Faction.OfPlayer && IsSlave && SlaveFaction != null && SlaveFaction != Faction.OfPlayer && guest.Released)
			{
				SlaveFaction.Notify_MemberExitedMap(this, flag4);
			}
			if (ownership != null && flag4)
			{
				ownership.UnclaimAll();
			}
			if (guest != null)
			{
				bool isPrisonerOfColony = IsPrisonerOfColony;
				if (flag4)
				{
					guest.SetGuestStatus(null);
				}
				if (isPrisonerOfColony)
				{
					guest.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
					if (!guest.Released && flag3)
					{
						GuestUtility.Notify_PrisonerEscaped(this);
					}
				}
				guest.Released = false;
			}
			DeSpawnOrDeselect();
			inventory.UnloadEverything = false;
			if (flag3)
			{
				ClearMind();
			}
			if (relations != null)
			{
				relations.Notify_ExitedMap();
			}
			Find.WorldPawns.PassToWorld(this);
			QuestUtility.SendQuestTargetSignals(questTags, "LeftMap", this.Named("SUBJECT"));
			Find.FactionManager.Notify_PawnLeftMap(this);
			Find.IdeoManager.Notify_PawnLeftMap(this);
		}

		public override void PreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
			base.PreTraded(action, playerNegotiator, trader);
			if (base.SpawnedOrAnyParentSpawned)
			{
				DropAndForbidEverything();
			}
			if (ownership != null)
			{
				ownership.UnclaimAll();
			}
			if (action == TradeAction.PlayerSells)
			{
				Faction faction = this.GetExtraHomeFaction() ?? this.GetExtraHostFaction();
				if (faction != null && faction != Faction.OfPlayer)
				{
					Faction.OfPlayer.TryAffectGoodwillWith(faction, Faction.OfPlayer.GoodwillToMakeHostile(faction), canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.MemberSold, this);
				}
			}
			if (guest != null)
			{
				guest.SetGuestStatus(null);
			}
			switch (action)
			{
			case TradeAction.PlayerBuys:
				if (guest != null && guest.joinStatus == JoinStatus.JoinAsSlave)
				{
					guest.SetGuestStatus(Faction.OfPlayer, RimWorld.GuestStatus.Slave);
					break;
				}
				needs.mood?.thoughts.memories.TryGainMemory(ThoughtDefOf.FreedFromSlavery);
				SetFaction(Faction.OfPlayer);
				break;
			case TradeAction.PlayerSells:
				if (RaceProps.Humanlike)
				{
					TaleRecorder.RecordTale(TaleDefOf.SoldPrisoner, playerNegotiator, this, trader);
				}
				if (base.Faction != null)
				{
					SetFaction(null);
				}
				if (RaceProps.IsFlesh)
				{
					relations.Notify_PawnSold(playerNegotiator);
				}
				break;
			}
			ClearMind();
		}

		public void PreKidnapped(Pawn kidnapper)
		{
			Find.Storyteller.Notify_PawnEvent(this, AdaptationEvent.Kidnapped);
			if (IsColonist && kidnapper != null)
			{
				TaleRecorder.RecordTale(TaleDefOf.KidnappedColonist, kidnapper, this);
			}
			if (ownership != null)
			{
				ownership.UnclaimAll();
			}
			if (guest != null && !guest.IsSlave)
			{
				guest.SetGuestStatus(null);
			}
			if (RaceProps.IsFlesh)
			{
				relations.Notify_PawnKidnapped();
			}
			ClearMind();
		}

		public override bool ClaimableBy(Faction by, StringBuilder reason = null)
		{
			return false;
		}

		public override bool AdoptableBy(Faction by, StringBuilder reason = null)
		{
			if (base.Faction == by)
			{
				return false;
			}
			Pawn_AgeTracker pawn_AgeTracker = ageTracker;
			if (pawn_AgeTracker != null && pawn_AgeTracker.CurLifeStage?.claimable == false)
			{
				return false;
			}
			if (FactionPreventsClaimingOrAdopting(base.Faction, forClaim: false, reason))
			{
				return false;
			}
			return true;
		}

		public override void SetFaction(Faction newFaction, Pawn recruiter = null)
		{
			if (newFaction == base.Faction)
			{
				Log.Warning("Used SetFaction to change " + this.ToStringSafe() + " to same faction " + newFaction.ToStringSafe());
				return;
			}
			Faction faction = base.Faction;
			if (guest != null)
			{
				guest.SetGuestStatus(null);
			}
			if (base.Spawned)
			{
				base.Map.mapPawns.DeRegisterPawn(this);
				base.Map.pawnDestinationReservationManager.ReleaseAllClaimedBy(this);
				base.Map.designationManager.RemoveAllDesignationsOn(this);
				base.Map.autoSlaughterManager.Notify_PawnChangedFaction();
			}
			if ((newFaction == Faction.OfPlayer || base.Faction == Faction.OfPlayer) && Current.ProgramState == ProgramState.Playing)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
			this.GetLord()?.Notify_PawnLost(this, PawnLostCondition.ChangedFaction);
			if (PawnUtility.IsFactionLeader(this))
			{
				Faction factionLeaderFaction = PawnUtility.GetFactionLeaderFaction(this);
				if (newFaction != factionLeaderFaction && !this.HasExtraHomeFaction(factionLeaderFaction) && !this.HasExtraMiniFaction(factionLeaderFaction))
				{
					factionLeaderFaction.Notify_LeaderLost();
				}
			}
			if (newFaction == Faction.OfPlayer && RaceProps.Humanlike && !this.IsQuestLodger())
			{
				ChangeKind(newFaction.def.basicMemberKind);
			}
			base.SetFaction(newFaction);
			PawnComponentsUtility.AddAndRemoveDynamicComponents(this);
			if (base.Faction != null && base.Faction.IsPlayer)
			{
				if (workSettings != null)
				{
					workSettings.EnableAndInitialize();
				}
				Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(this, PopAdaptationEvent.GainedColonist);
			}
			if (Drafted)
			{
				drafter.Drafted = false;
			}
			ReachabilityUtility.ClearCacheFor(this);
			health.surgeryBills.Clear();
			if (base.Spawned)
			{
				base.Map.mapPawns.RegisterPawn(this);
			}
			GenerateNecessaryName();
			if (playerSettings != null)
			{
				playerSettings.ResetMedicalCare();
			}
			ClearMind(ifLayingKeepLaying: true);
			if (!Dead && needs.mood != null)
			{
				needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
			}
			if (base.Spawned)
			{
				base.Map.attackTargetsCache.UpdateTarget(this);
			}
			Find.GameEnder.CheckOrUpdateGameOver();
			AddictionUtility.CheckDrugAddictionTeachOpportunity(this);
			if (needs != null)
			{
				needs.AddOrRemoveNeedsAsAppropriate();
			}
			if (playerSettings != null)
			{
				playerSettings.Notify_FactionChanged();
			}
			if (relations != null)
			{
				relations.Notify_ChangedFaction();
			}
			if (RaceProps.Animal && newFaction == Faction.OfPlayer)
			{
				training.SetWantedRecursive(TrainableDefOf.Tameness, checkOn: true);
				training.Train(TrainableDefOf.Tameness, recruiter, complete: true);
				if (RaceProps.Roamer && mindState != null)
				{
					mindState.lastStartRoamCooldownTick = Find.TickManager.TicksGame;
				}
			}
			if (faction == Faction.OfPlayer)
			{
				BillUtility.Notify_ColonistUnavailable(this);
			}
			if (newFaction == Faction.OfPlayer)
			{
				Find.StoryWatcher.statsRecord.UpdateGreatestPopulation();
				Find.World.StoryState.RecordPopulationIncrease();
			}
			newFaction?.Notify_PawnJoined(this);
			if (Ideo != null)
			{
				Ideo.Notify_MemberChangedFaction(this, faction, newFaction);
			}
			ageTracker?.ResetAgeReversalDemand(Pawn_AgeTracker.AgeReversalReason.Recruited);
			roping?.BreakAllRopes();
			if (ModsConfig.BiotechActive && mechanitor != null)
			{
				mechanitor.Notify_ChangedFaction();
			}
			if (faction != null)
			{
				Find.FactionManager.Notify_PawnLeftFaction(faction);
			}
		}

		public void ClearMind(bool ifLayingKeepLaying = false, bool clearInspiration = false, bool clearMentalState = true)
		{
			if (pather != null)
			{
				pather.StopDead();
			}
			if (mindState != null)
			{
				mindState.Reset(clearInspiration, clearMentalState);
			}
			if (jobs != null)
			{
				jobs.StopAll(ifLayingKeepLaying);
			}
			VerifyReservations();
		}

		public void ClearAllReservations(bool releaseDestinationsOnlyIfObsolete = true)
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (releaseDestinationsOnlyIfObsolete)
				{
					maps[i].pawnDestinationReservationManager.ReleaseAllObsoleteClaimedBy(this);
				}
				else
				{
					maps[i].pawnDestinationReservationManager.ReleaseAllClaimedBy(this);
				}
				maps[i].reservationManager.ReleaseAllClaimedBy(this);
				maps[i].physicalInteractionReservationManager.ReleaseAllClaimedBy(this);
				maps[i].attackTargetReservationManager.ReleaseAllClaimedBy(this);
			}
		}

		public void ClearReservationsForJob(Job job)
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				maps[i].pawnDestinationReservationManager.ReleaseClaimedBy(this, job);
				maps[i].reservationManager.ReleaseClaimedBy(this, job);
				maps[i].physicalInteractionReservationManager.ReleaseClaimedBy(this, job);
				maps[i].attackTargetReservationManager.ReleaseClaimedBy(this, job);
			}
		}

		public void VerifyReservations()
		{
			if (jobs == null || CurJob != null || jobs.jobQueue.Count > 0 || jobs.startingNewJob)
			{
				return;
			}
			bool flag = false;
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				LocalTargetInfo obj = maps[i].reservationManager.FirstReservationFor(this);
				if (obj.IsValid)
				{
					Log.ErrorOnce($"Reservation manager failed to clean up properly; {this.ToStringSafe()} still reserving {obj.ToStringSafe()}", 0x5D3DFA5 ^ thingIDNumber);
					flag = true;
				}
				LocalTargetInfo obj2 = maps[i].physicalInteractionReservationManager.FirstReservationFor(this);
				if (obj2.IsValid)
				{
					Log.ErrorOnce($"Physical interaction reservation manager failed to clean up properly; {this.ToStringSafe()} still reserving {obj2.ToStringSafe()}", 0x12ADECD ^ thingIDNumber);
					flag = true;
				}
				IAttackTarget attackTarget = maps[i].attackTargetReservationManager.FirstReservationFor(this);
				if (attackTarget != null)
				{
					Log.ErrorOnce($"Attack target reservation manager failed to clean up properly; {this.ToStringSafe()} still reserving {attackTarget.ToStringSafe()}", 0x5FD7206 ^ thingIDNumber);
					flag = true;
				}
				IntVec3 obj3 = maps[i].pawnDestinationReservationManager.FirstObsoleteReservationFor(this);
				if (obj3.IsValid)
				{
					Job job = maps[i].pawnDestinationReservationManager.FirstObsoleteReservationJobFor(this);
					Log.ErrorOnce($"Pawn destination reservation manager failed to clean up properly; {this.ToStringSafe()}/{job.ToStringSafe()}/{job.def.ToStringSafe()} still reserving {obj3.ToStringSafe()}", 0x1DE312 ^ thingIDNumber);
					flag = true;
				}
			}
			if (flag)
			{
				ClearAllReservations();
			}
		}

		public void DropAndForbidEverything(bool keepInventoryAndEquipmentIfInBed = false, bool rememberPrimary = false)
		{
			if (kindDef.destroyGearOnDrop)
			{
				equipment.DestroyAllEquipment();
				apparel.DestroyAll();
			}
			if (InContainerEnclosed)
			{
				if (carryTracker != null && carryTracker.CarriedThing != null)
				{
					carryTracker.innerContainer.TryTransferToContainer(carryTracker.CarriedThing, holdingOwner);
				}
				if (equipment != null && equipment.Primary != null)
				{
					equipment.TryTransferEquipmentToContainer(equipment.Primary, holdingOwner);
				}
				if (inventory != null)
				{
					inventory.innerContainer.TryTransferAllToContainer(holdingOwner);
				}
			}
			else
			{
				if (!base.SpawnedOrAnyParentSpawned)
				{
					return;
				}
				if (carryTracker != null && carryTracker.CarriedThing != null)
				{
					carryTracker.TryDropCarriedThing(base.PositionHeld, ThingPlaceMode.Near, out var _);
				}
				if (!keepInventoryAndEquipmentIfInBed || !this.InBed())
				{
					if (equipment != null)
					{
						equipment.DropAllEquipment(base.PositionHeld, forbid: true, rememberPrimary);
					}
					if (inventory != null && inventory.innerContainer.TotalStackCount > 0)
					{
						inventory.DropAllNearPawn(base.PositionHeld, forbid: true);
					}
				}
			}
		}

		public void GenerateNecessaryName()
		{
			if (Name == null && base.Faction == Faction.OfPlayer && (RaceProps.Animal || (ModsConfig.BiotechActive && RaceProps.IsMechanoid)))
			{
				Name = PawnBioAndNameGenerator.GeneratePawnName(this, NameStyle.Numeric);
			}
		}

		public Verb TryGetAttackVerb(Thing target, bool allowManualCastWeapons = false, bool allowTurrets = false)
		{
			if (equipment != null && equipment.Primary != null && equipment.PrimaryEq.PrimaryVerb.Available() && (!equipment.PrimaryEq.PrimaryVerb.verbProps.onlyManualCast || (CurJob != null && CurJob.def != JobDefOf.Wait_Combat) || allowManualCastWeapons))
			{
				return equipment.PrimaryEq.PrimaryVerb;
			}
			if (allowManualCastWeapons && apparel != null)
			{
				Verb firstApparelVerb = apparel.FirstApparelVerb;
				if (firstApparelVerb != null && firstApparelVerb.Available())
				{
					return firstApparelVerb;
				}
			}
			if (allowTurrets)
			{
				List<ThingComp> allComps = base.AllComps;
				for (int i = 0; i < allComps.Count; i++)
				{
					if (allComps[i] is CompTurretGun compTurretGun && !compTurretGun.TurretDestroyed && compTurretGun.GunCompEq.PrimaryVerb.Available())
					{
						return compTurretGun.GunCompEq.PrimaryVerb;
					}
				}
			}
			return meleeVerbs.TryGetMeleeVerb(target);
		}

		public bool TryStartAttack(LocalTargetInfo targ)
		{
			if (stances.FullBodyBusy)
			{
				return false;
			}
			if (WorkTagIsDisabled(WorkTags.Violent))
			{
				return false;
			}
			bool allowManualCastWeapons = !IsColonist;
			Verb verb = TryGetAttackVerb(targ.Thing, allowManualCastWeapons);
			return verb?.TryStartCastOn(verb.verbProps.ai_RangedAlawaysShootGroundBelowTarget ? ((LocalTargetInfo)targ.Cell) : targ) ?? false;
		}

		public override IEnumerable<Thing> ButcherProducts(Pawn butcher, float efficiency)
		{
			if (RaceProps.meatDef != null)
			{
				int num = GenMath.RoundRandom(this.GetStatValue(StatDefOf.MeatAmount) * efficiency);
				if (num > 0)
				{
					Thing thing = ThingMaker.MakeThing(RaceProps.meatDef);
					thing.stackCount = num;
					yield return thing;
				}
			}
			foreach (Thing item in base.ButcherProducts(butcher, efficiency))
			{
				yield return item;
			}
			if (RaceProps.leatherDef != null)
			{
				int num2 = GenMath.RoundRandom(this.GetStatValue(StatDefOf.LeatherAmount) * efficiency);
				if (num2 > 0)
				{
					Thing thing2 = ThingMaker.MakeThing(RaceProps.leatherDef);
					thing2.stackCount = num2;
					yield return thing2;
				}
			}
			if (RaceProps.Humanlike)
			{
				yield break;
			}
			PawnKindLifeStage lifeStage = ageTracker.CurKindLifeStage;
			if (lifeStage.butcherBodyPart == null || (gender != 0 && (gender != Gender.Male || !lifeStage.butcherBodyPart.allowMale) && (gender != Gender.Female || !lifeStage.butcherBodyPart.allowFemale)))
			{
				yield break;
			}
			while (true)
			{
				BodyPartRecord bodyPartRecord = (from x in health.hediffSet.GetNotMissingParts()
					where x.IsInGroup(lifeStage.butcherBodyPart.bodyPartGroup)
					select x).FirstOrDefault();
				if (bodyPartRecord != null)
				{
					health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, this, bodyPartRecord));
					yield return (lifeStage.butcherBodyPart.thing == null) ? ThingMaker.MakeThing(bodyPartRecord.def.spawnThingOnRemoved) : ThingMaker.MakeThing(lifeStage.butcherBodyPart.thing);
					continue;
				}
				break;
			}
		}

		public TaggedString FactionDesc(TaggedString name, bool extraFactionsInfo, string nameLabel, string genderLabel)
		{
			tmpExtraFactions.Clear();
			QuestUtility.GetExtraFactionsFromQuestParts(this, tmpExtraFactions);
			GuestUtility.GetExtraFactionsFromGuestStatus(this, tmpExtraFactions);
			TaggedString result = ((base.Faction == null || base.Faction.Hidden) ? name : ((tmpExtraFactions.Count != 0 || SlaveFaction != null) ? "PawnMainDescUnderFactionedWrap".Translate(name, base.Faction.NameColored) : "PawnMainDescFactionedWrap".Translate(name, base.Faction.NameColored, nameLabel.Named("NAME"), genderLabel.Named("GENDER"))));
			if (extraFactionsInfo)
			{
				for (int i = 0; i < tmpExtraFactions.Count; i++)
				{
					if (base.Faction != tmpExtraFactions[i].faction)
					{
						result += $"\n{tmpExtraFactions[i].factionType.GetLabel().CapitalizeFirst()}: {tmpExtraFactions[i].faction.NameColored.Resolve()}";
					}
				}
			}
			tmpExtraFactions.Clear();
			return result;
		}

		public string MainDesc(bool writeFaction, bool writeGender = true)
		{
			bool flag = base.Faction == null || !base.Faction.IsPlayer;
			string text = ((!writeGender) ? "" : ((gender == Gender.None) ? "" : gender.GetLabel(this.AnimalOrWildMan())));
			string text2 = "";
			if (RaceProps.Animal || RaceProps.IsMechanoid)
			{
				text2 = GenLabel.BestKindLabel(this, mustNoteGender: false, mustNoteLifeStage: true);
				if (Name != null)
				{
					if (!text.NullOrEmpty())
					{
						text += " ";
					}
					text += text2;
				}
			}
			if (ageTracker != null)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += "AgeIndicator".Translate(ageTracker.AgeNumberString);
			}
			if (!RaceProps.Animal && !RaceProps.IsMechanoid && flag)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text2 = GenLabel.BestKindLabel(this, mustNoteGender: false, mustNoteLifeStage: true);
				text += text2;
			}
			if (writeFaction)
			{
				text = FactionDesc(text, extraFactionsInfo: true, text2, gender.GetLabel(RaceProps.Animal)).Resolve();
			}
			return text.CapitalizeFirst();
		}

		public string GetJobReport()
		{
			try
			{
				if (jobs?.curJob != null)
				{
					return jobs.curDriver?.GetReport().CapitalizeFirst();
				}
				return null;
			}
			catch (Exception ex)
			{
				Log.Error("JobDriver.GetReport() exception: " + ex);
				return null;
			}
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(MainDesc(writeFaction: true));
			RoyalTitle royalTitle = royalty?.MostSeniorTitle;
			if (royalTitle != null)
			{
				stringBuilder.AppendLine("PawnTitleDescWrap".Translate(royalTitle.def.GetLabelCapFor(this), royalTitle.faction.NameColored).Resolve());
			}
			string inspectString = base.GetInspectString();
			if (!inspectString.NullOrEmpty())
			{
				stringBuilder.AppendLine(inspectString);
			}
			if (TraderKind != null)
			{
				stringBuilder.AppendLine(TraderKind.LabelCap);
			}
			if (InMentalState)
			{
				stringBuilder.AppendLine(MentalState.InspectLine);
			}
			states.Clear();
			if (health != null && health.hediffSet != null)
			{
				List<Hediff> hediffs = health.hediffSet.hediffs;
				for (int i = 0; i < hediffs.Count; i++)
				{
					Hediff hediff = hediffs[i];
					if (!hediff.def.battleStateLabel.NullOrEmpty())
					{
						states.AddDistinct(hediff.def.battleStateLabel);
					}
				}
			}
			if (states.Count > 0)
			{
				states.Sort();
				stringBuilder.AppendLine(string.Format("{0}: {1}", "State".Translate(), states.ToCommaList().CapitalizeFirst()));
				states.Clear();
			}
			if (stances?.stunner != null && stances.stunner.Stunned)
			{
				stringBuilder.AppendLine("StunLower".Translate().CapitalizeFirst() + ": " + stances.stunner.StunTicksLeft.ToStringSecondsFromTicks());
			}
			if (stances?.stagger != null && stances.stagger.Staggered)
			{
				stringBuilder.AppendLine("SlowedByDamage".Translate() + ": " + stances.stagger.StaggerTicksLeft.ToStringSecondsFromTicks());
			}
			if (Inspired)
			{
				stringBuilder.AppendLine(Inspiration.InspectLine);
			}
			if (equipment != null && equipment.Primary != null)
			{
				stringBuilder.AppendLine("Equipped".TranslateSimple() + ": " + ((equipment.Primary != null) ? equipment.Primary.Label : "EquippedNothing".TranslateSimple()).CapitalizeFirst());
			}
			if (abilities != null)
			{
				for (int j = 0; j < abilities.AllAbilitiesForReading.Count; j++)
				{
					string inspectString2 = abilities.AllAbilitiesForReading[j].GetInspectString();
					if (!inspectString2.NullOrEmpty())
					{
						stringBuilder.AppendLine(inspectString2);
					}
				}
			}
			if (carryTracker != null && carryTracker.CarriedThing != null)
			{
				stringBuilder.Append("Carrying".Translate() + ": ");
				stringBuilder.AppendLine(carryTracker.CarriedThing.LabelCap);
			}
			Pawn_RopeTracker pawn_RopeTracker = roping;
			if (pawn_RopeTracker != null && pawn_RopeTracker.IsRoped)
			{
				stringBuilder.AppendLine(roping.InspectLine);
			}
			if (ModsConfig.BiotechActive && IsColonyMech && needs.energy != null)
			{
				TaggedString taggedString = "MechEnergy".Translate() + ": " + needs.energy.CurLevelPercentage.ToStringPercent();
				float maxLevel = needs.energy.MaxLevel;
				if (this.IsCharging())
				{
					taggedString += " (+" + "PerDay".Translate((50f / maxLevel).ToStringPercent()) + ")";
				}
				else if (this.IsSelfShutdown())
				{
					taggedString += " (+" + "PerDay".Translate((1f / maxLevel).ToStringPercent()) + ")";
				}
				else
				{
					taggedString += " (-" + "PerDay".Translate((needs.energy.FallPerDay / maxLevel).ToStringPercent()) + ")";
				}
				stringBuilder.AppendLine(taggedString);
			}
			if ((base.Faction == Faction.OfPlayer || HostFaction == Faction.OfPlayer) && !InMentalState)
			{
				LordJob obj = this.GetLord()?.LordJob;
				string text = obj?.GetReport(this);
				string text2 = obj?.GetJobReport(this) ?? GetJobReport();
				if (text.NullOrEmpty())
				{
					text = text2;
				}
				else if (!text2.NullOrEmpty())
				{
					text = text + ": " + text2;
				}
				if (!text.NullOrEmpty())
				{
					stringBuilder.AppendLine(text);
				}
			}
			if (jobs?.curJob != null)
			{
				Pawn_JobTracker pawn_JobTracker = jobs;
				if (pawn_JobTracker != null && pawn_JobTracker.jobQueue.Count > 0)
				{
					try
					{
						string text3 = jobs.jobQueue[0].job.GetReport(this).CapitalizeFirst();
						if (jobs.jobQueue.Count > 1)
						{
							text3 = text3 + " (+" + (jobs.jobQueue.Count - 1) + ")";
						}
						stringBuilder.AppendLine("Queued".Translate() + ": " + text3);
					}
					catch (Exception ex)
					{
						Log.Error("JobDriver.GetReport() exception: " + ex);
					}
				}
			}
			if (ModsConfig.BiotechActive && needs?.energy != null && needs.energy.IsLowEnergySelfShutdown)
			{
				stringBuilder.AppendLine("MustBeCarriedToRecharger".Translate());
			}
			if (RestraintsUtility.ShouldShowRestraintsInfo(this))
			{
				stringBuilder.AppendLine("InRestraints".Translate());
			}
			if (guest != null && !guest.Recruitable && (base.Faction != Faction.OfPlayer || IsSlaveOfColony || IsPrisonerOfColony))
			{
				stringBuilder.AppendLine("Unrecruitable".Translate().CapitalizeFirst());
			}
			if (Prefs.DevMode && DebugSettings.showLocomotionUrgency && CurJob != null)
			{
				stringBuilder.AppendLine("Locomotion Urgency: " + CurJob.locomotionUrgency);
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			Lord lord2 = this.GetLord();
			if ((IsColonistPlayerControlled || IsColonyMech) && (lord2 == null || !(lord2.LordJob is LordJob_Ritual lordJob_Ritual) || !lordJob_Ritual.BlocksDrafting))
			{
				if (drafter != null)
				{
					foreach (Gizmo gizmo2 in drafter.GetGizmos())
					{
						yield return gizmo2;
					}
				}
				foreach (Gizmo attackGizmo in PawnAttackGizmoUtility.GetAttackGizmos(this))
				{
					yield return attackGizmo;
				}
			}
			if (equipment != null)
			{
				foreach (Gizmo gizmo3 in equipment.GetGizmos())
				{
					yield return gizmo3;
				}
			}
			if (carryTracker != null)
			{
				foreach (Gizmo gizmo4 in carryTracker.GetGizmos())
				{
					yield return gizmo4;
				}
			}
			if (needs != null)
			{
				foreach (Gizmo gizmo5 in needs.GetGizmos())
				{
					yield return gizmo5;
				}
			}
			if (Find.Selector.SingleSelectedThing == this && psychicEntropy != null && psychicEntropy.NeedToShowGizmo())
			{
				yield return psychicEntropy.GetGizmo();
				if (DebugSettings.ShowDevGizmos)
				{
					yield return new Command_Action
					{
						defaultLabel = "DEV: Psyfocus -20%",
						action = delegate
						{
							psychicEntropy.OffsetPsyfocusDirectly(-0.2f);
						}
					};
					yield return new Command_Action
					{
						defaultLabel = "DEV: Psyfocus +20%",
						action = delegate
						{
							psychicEntropy.OffsetPsyfocusDirectly(0.2f);
						}
					};
					yield return new Command_Action
					{
						defaultLabel = "DEV: Neural heat -20",
						action = delegate
						{
							psychicEntropy.TryAddEntropy(-20f);
						}
					};
					yield return new Command_Action
					{
						defaultLabel = "DEV: Neural heat +20",
						action = delegate
						{
							psychicEntropy.TryAddEntropy(20f);
						}
					};
				}
			}
			if (ModsConfig.BiotechActive)
			{
				if (MechanitorUtility.IsMechanitor(this))
				{
					foreach (Gizmo gizmo6 in mechanitor.GetGizmos())
					{
						yield return gizmo6;
					}
				}
				if (RaceProps.IsMechanoid)
				{
					foreach (Gizmo mechGizmo in MechanitorUtility.GetMechGizmos(this))
					{
						yield return mechGizmo;
					}
				}
				if (RaceProps.Humanlike && ageTracker.AgeBiologicalYears < 13 && !Drafted && Find.Selector.SelectedPawns.Count < 2 && DevelopmentalStage.Child())
				{
					yield return new Gizmo_GrowthTier(this);
					if (DebugSettings.ShowDevGizmos)
					{
						yield return new Command_Action
						{
							defaultLabel = "DEV: Set growth tier",
							action = delegate
							{
								List<FloatMenuOption> list = new List<FloatMenuOption>();
								for (int i = 0; i < GrowthUtility.GrowthTierPointsRequirements.Length; i++)
								{
									int tier = i;
									list.Add(new FloatMenuOption(tier.ToString(), delegate
									{
										ageTracker.growthPoints = GrowthUtility.GrowthTierPointsRequirements[tier];
									}));
								}
								Find.WindowStack.Add(new FloatMenu(list));
							}
						};
					}
				}
			}
			if (abilities != null)
			{
				foreach (Gizmo gizmo7 in abilities.GetGizmos())
				{
					yield return gizmo7;
				}
			}
			if (IsColonistPlayerControlled || IsColonyMech || IsPrisonerOfColony)
			{
				if (playerSettings != null)
				{
					foreach (Gizmo gizmo8 in playerSettings.GetGizmos())
					{
						yield return gizmo8;
					}
				}
				foreach (Gizmo gizmo9 in health.GetGizmos())
				{
					yield return gizmo9;
				}
			}
			if (apparel != null)
			{
				foreach (Gizmo gizmo10 in apparel.GetGizmos())
				{
					yield return gizmo10;
				}
			}
			if (inventory != null)
			{
				foreach (Gizmo gizmo11 in inventory.GetGizmos())
				{
					yield return gizmo11;
				}
			}
			foreach (Gizmo gizmo12 in mindState.GetGizmos())
			{
				yield return gizmo12;
			}
			if (royalty != null && IsColonistPlayerControlled)
			{
				bool anyPermitOnCooldown = false;
				foreach (FactionPermit allFactionPermit in royalty.AllFactionPermits)
				{
					if (allFactionPermit.OnCooldown)
					{
						anyPermitOnCooldown = true;
					}
					IEnumerable<Gizmo> pawnGizmos = allFactionPermit.Permit.Worker.GetPawnGizmos(this, allFactionPermit.Faction);
					if (pawnGizmos == null)
					{
						continue;
					}
					foreach (Gizmo item in pawnGizmos)
					{
						yield return item;
					}
				}
				if (royalty.HasAidPermit)
				{
					yield return royalty.RoyalAidGizmo();
				}
				if (DebugSettings.ShowDevGizmos && anyPermitOnCooldown)
				{
					Command_Action command_Action = new Command_Action();
					command_Action.defaultLabel = "Reset permit cooldowns";
					command_Action.action = delegate
					{
						foreach (FactionPermit allFactionPermit2 in royalty.AllFactionPermits)
						{
							allFactionPermit2.ResetCooldown();
						}
					};
					yield return command_Action;
				}
				foreach (RoyalTitle item2 in royalty.AllTitlesForReading)
				{
					if (item2.def.permits == null)
					{
						continue;
					}
					Faction faction = item2.faction;
					foreach (RoyalTitlePermitDef permit in item2.def.permits)
					{
						IEnumerable<Gizmo> pawnGizmos2 = permit.Worker.GetPawnGizmos(this, faction);
						if (pawnGizmos2 == null)
						{
							continue;
						}
						foreach (Gizmo item3 in pawnGizmos2)
						{
							yield return item3;
						}
					}
				}
			}
			foreach (Gizmo questRelatedGizmo in QuestUtility.GetQuestRelatedGizmos(this))
			{
				yield return questRelatedGizmo;
			}
			if (royalty != null && ModsConfig.RoyaltyActive)
			{
				foreach (Gizmo gizmo13 in royalty.GetGizmos())
				{
					yield return gizmo13;
				}
			}
			if (connections != null && ModsConfig.IdeologyActive)
			{
				foreach (Gizmo gizmo14 in connections.GetGizmos())
				{
					yield return gizmo14;
				}
			}
			if (genes != null)
			{
				foreach (Gizmo gizmo15 in genes.GetGizmos())
				{
					yield return gizmo15;
				}
			}
			Lord lord = this.GetLord();
			if (lord != null && lord.LordJob != null)
			{
				foreach (Gizmo pawnGizmo in lord.LordJob.GetPawnGizmos(this))
				{
					yield return pawnGizmo;
				}
				if (lord.CurLordToil != null)
				{
					foreach (Gizmo pawnGizmo2 in lord.CurLordToil.GetPawnGizmos(this))
					{
						yield return pawnGizmo2;
					}
				}
			}
			if (DebugSettings.ShowDevGizmos && ModsConfig.BiotechActive && (relations?.IsTryRomanceOnCooldown ?? false))
			{
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "DEV: Reset try romance cooldown";
				command_Action2.action = delegate
				{
					relations.romanceEnableTick = -1;
				};
				yield return command_Action2;
			}
		}

		public virtual IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(IntVec3 sq)
		{
			return Enumerable.Empty<FloatMenuOption>();
		}

		public override TipSignal GetTooltip()
		{
			string text = "";
			string text2 = "";
			if (gender != 0)
			{
				text = (LabelCap.EqualsIgnoreCase(KindLabel) ? this.GetGenderLabel() : ((string)"PawnTooltipGenderAndKindLabel".Translate(this.GetGenderLabel(), KindLabel)));
			}
			else if (!LabelCap.EqualsIgnoreCase(KindLabel))
			{
				text = KindLabel;
			}
			string generalConditionLabel = HealthUtility.GetGeneralConditionLabel(this);
			bool flag = !string.IsNullOrEmpty(text);
			text2 = ((equipment != null && equipment.Primary != null) ? ((!flag) ? ((string)"PawnTooltipWithPrimaryEquipNoDesc".Translate(LabelCap, text, generalConditionLabel)) : ((string)"PawnTooltipWithDescAndPrimaryEquip".Translate(LabelCap, text, equipment.Primary.LabelCap, generalConditionLabel))) : ((!flag) ? ((string)"PawnTooltipNoDescNoPrimaryEquip".Translate(LabelCap, generalConditionLabel)) : ((string)"PawnTooltipWithDescNoPrimaryEquip".Translate(LabelCap, text, generalConditionLabel))));
			return new TipSignal(text2, thingIDNumber * 152317, TooltipPriority.Pawn);
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			foreach (StatDrawEntry item in base.SpecialDisplayStats())
			{
				yield return item;
			}
			if (ModsConfig.BiotechActive && genes != null && genes.Xenotype != XenotypeDefOf.Baseliner)
			{
				string reportText = (genes.UniqueXenotype ? "UniqueXenotypeDesc".Translate().ToString() : DescriptionFlavor);
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Race".Translate(), def.LabelCap + " (" + genes.XenotypeLabel + ")", reportText, 2100, null, genes.UniqueXenotype ? null : Gen.YieldSingle(new Dialog_InfoCard.Hyperlink(genes.Xenotype)));
			}
			if (ModsConfig.BiotechActive && RaceProps.Humanlike)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "StatsReport_AgeRateMultiplier".Translate(), ageTracker.BiologicalTicksPerTick.ToStringPercent(), "StatsReport_AgeRateMultiplier_Desc".Translate(), 4195);
			}
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "BodySize".Translate(), BodySize.ToString("F2"), "Stat_Race_BodySize_Desc".Translate(), 500);
			if (RaceProps.lifeStageAges.Count > 1 && RaceProps.Animal)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Growth".Translate(), ageTracker.Growth.ToStringPercent(), "Stat_Race_Growth_Desc".Translate(), 2206);
			}
			if (this.IsWildMan())
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Wildness".Translate(), 0.75f.ToStringPercent(), TrainableUtility.GetWildnessExplanation(def), 2050);
			}
			if (ModsConfig.RoyaltyActive && RaceProps.intelligence == Intelligence.Humanlike)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "MeditationFocuses".Translate(), MeditationUtility.FocusTypesAvailableForPawnString(this).CapitalizeFirst(), ("MeditationFocusesPawnDesc".Translate() + "\n\n" + MeditationUtility.FocusTypeAvailableExplanation(this)).Resolve(), 99995, null, MeditationUtility.FocusObjectsForPawnHyperlinks(this));
			}
			if (apparel != null && !apparel.AllRequirements.EnumerableNullOrEmpty())
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (ApparelRequirementWithSource allRequirement in apparel.AllRequirements)
				{
					string text = null;
					if (!ApparelUtility.IsRequirementActive(allRequirement.requirement, allRequirement.Source, this, out var disabledByLabel))
					{
						text = " [" + "ApparelRequirementDisabledLabel".Translate() + ": " + disabledByLabel + "]";
					}
					stringBuilder.Append("- ");
					bool flag = true;
					foreach (ThingDef item2 in allRequirement.requirement.AllRequiredApparelForPawn(this, ignoreGender: false, includeWorn: true))
					{
						if (!flag)
						{
							stringBuilder.Append(", ");
						}
						stringBuilder.Append(item2.LabelCap);
						flag = false;
					}
					if (allRequirement.Source == ApparelRequirementSource.Title)
					{
						stringBuilder.Append(" ");
						if (ModsConfig.BiotechActive)
						{
							stringBuilder.Append("ApparelRequirementOrAnyPsycasterOrPrestigeApparelOrMechlord".Translate());
						}
						else
						{
							stringBuilder.Append("ApparelRequirementOrAnyPsycasterOrPrestigeApparel".Translate());
						}
					}
					stringBuilder.Append(" (");
					stringBuilder.Append("Source".Translate());
					stringBuilder.Append(": ");
					stringBuilder.Append(allRequirement.SourceLabelCap);
					stringBuilder.Append(")");
					if (text != null)
					{
						stringBuilder.Append(text);
					}
					stringBuilder.AppendLine();
				}
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Stat_Pawn_RequiredApparel_Name".Translate(), "", "Stat_Pawn_RequiredApparel_Name".Translate() + ":\n\n" + stringBuilder.ToString(), 100);
			}
			if (ModsConfig.IdeologyActive && Ideo != null)
			{
				foreach (StatDrawEntry item3 in DarknessCombatUtility.GetStatEntriesForPawn(this))
				{
					yield return item3;
				}
			}
			if (genes != null)
			{
				foreach (StatDrawEntry item4 in genes.SpecialDisplayStats())
				{
					yield return item4;
				}
			}
			if (!ModsConfig.BiotechActive)
			{
				yield break;
			}
			if (RaceProps.Humanlike)
			{
				TaggedString taggedString = "DevelopmentStage_Adult".Translate();
				TaggedString taggedString2 = "StatsReport_DevelopmentStageDesc_Adult".Translate();
				if (ageTracker.CurLifeStage.developmentalStage == DevelopmentalStage.Child)
				{
					taggedString = "DevelopmentStage_Child".Translate();
					taggedString2 = "StatsReport_DevelopmentStageDesc_ChildPart1".Translate() + ":\n\n" + (from w in RaceProps.lifeStageWorkSettings
						where w.minAge > 0 && w.workType.visible
						select w into d
						select (d.workType.labelShort + " (" + "AgeIndicator".Translate(d.minAge) + ")").RawText).ToLineList("  - ", capitalizeItems: true) + "\n\n" + "StatsReport_DevelopmentStageDesc_ChildPart2".Translate();
				}
				else if (ageTracker.CurLifeStage.developmentalStage == DevelopmentalStage.Baby)
				{
					taggedString = "DevelopmentStage_Baby".Translate();
					taggedString2 = "StatsReport_DevelopmentStageDesc_Baby".Translate();
				}
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "StatsReport_DevelopmentStage".Translate(), taggedString, taggedString2, 4200);
			}
			if (IsFreeColonist && DevelopmentalStage.Child())
			{
				_ = needs.learning;
			}
		}

		public bool Sterile(bool forEmbryoImplantation = false)
		{
			if (!ageTracker.CurLifeStage.reproductive)
			{
				return true;
			}
			if (RaceProps.Humanlike)
			{
				if (!ModsConfig.BiotechActive)
				{
					return true;
				}
				if (this.GetStatValue(StatDefOf.Fertility) <= 0f)
				{
					return true;
				}
			}
			if (health.hediffSet.HasHediffPreventsPregnancy())
			{
				return true;
			}
			if (this.SterileGenes())
			{
				return true;
			}
			return false;
		}

		public bool CurrentlyUsableForBills()
		{
			if (!this.InBed())
			{
				JobFailReason.Is(NotSurgeryReadyTrans);
				return false;
			}
			if (!InteractionCell.IsValid)
			{
				JobFailReason.Is(CannotReachTrans);
				return false;
			}
			return true;
		}

		public bool UsableForBillsAfterFueling()
		{
			return CurrentlyUsableForBills();
		}

		public void Notify_BillDeleted(Bill bill)
		{
			bill.xenogerm?.Notify_BillRemoved();
		}

		public bool AnythingToStrip()
		{
			if (!kindDef.canStrip)
			{
				return false;
			}
			if (equipment != null && equipment.HasAnything())
			{
				return true;
			}
			if (inventory != null && inventory.innerContainer.Count > 0)
			{
				return true;
			}
			if (apparel != null)
			{
				if (base.Destroyed)
				{
					if (apparel.AnyApparel)
					{
						return true;
					}
				}
				else if (apparel.AnyApparelUnlocked)
				{
					return true;
				}
			}
			return false;
		}

		public void Strip()
		{
			Caravan caravan = this.GetCaravan();
			if (caravan != null)
			{
				CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(this, caravan.PawnsListForReading);
				if (apparel != null)
				{
					CaravanInventoryUtility.MoveAllApparelToSomeonesInventory(this, caravan.PawnsListForReading, base.Destroyed);
				}
				if (equipment != null)
				{
					CaravanInventoryUtility.MoveAllEquipmentToSomeonesInventory(this, caravan.PawnsListForReading);
				}
			}
			else
			{
				IntVec3 pos = ((Corpse != null) ? Corpse.PositionHeld : base.PositionHeld);
				if (equipment != null)
				{
					equipment.DropAllEquipment(pos, forbid: false);
				}
				if (apparel != null)
				{
					apparel.DropAll(pos, forbid: false, base.Destroyed);
				}
				if (inventory != null)
				{
					inventory.DropAllNearPawn(pos);
				}
			}
			if (base.Faction != null)
			{
				base.Faction.Notify_MemberStripped(this, Faction.OfPlayer);
			}
		}

		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			return trader.ColonyThingsWillingToBuy(playerNegotiator);
		}

		public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			trader.GiveSoldThingToTrader(toGive, countToGive, playerNegotiator);
		}

		public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			trader.GiveSoldThingToPlayer(toGive, countToGive, playerNegotiator);
		}

		public void HearClamor(Thing source, ClamorDef type)
		{
			if (Dead || Downed || Deathresting || this.IsSelfShutdown())
			{
				return;
			}
			if (type == ClamorDefOf.Movement || type == ClamorDefOf.BabyCry)
			{
				if (source is Pawn source2)
				{
					CheckForDisturbedSleep(source2);
				}
				NotifyLordOfClamor(source, type);
			}
			if (type == ClamorDefOf.Harm && base.Faction != Faction.OfPlayer && !this.Awake() && base.Faction == source.Faction && HostFaction == null)
			{
				mindState.canSleepTick = Find.TickManager.TicksGame + 1000;
				if (CurJob != null)
				{
					jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				NotifyLordOfClamor(source, type);
			}
			if (type == ClamorDefOf.Construction && base.Faction != Faction.OfPlayer && !this.Awake() && base.Faction != source.Faction && HostFaction == null)
			{
				mindState.canSleepTick = Find.TickManager.TicksGame + 1000;
				if (CurJob != null)
				{
					jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				NotifyLordOfClamor(source, type);
			}
			if (type == ClamorDefOf.Ability && base.Faction != Faction.OfPlayer && base.Faction != source.Faction && HostFaction == null)
			{
				if (!this.Awake())
				{
					mindState.canSleepTick = Find.TickManager.TicksGame + 1000;
					if (CurJob != null)
					{
						jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
				}
				NotifyLordOfClamor(source, type);
			}
			if (type == ClamorDefOf.Impact)
			{
				mindState.Notify_ClamorImpact(source);
				if (CurJob != null && !this.Awake())
				{
					jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				NotifyLordOfClamor(source, type);
			}
		}

		private void NotifyLordOfClamor(Thing source, ClamorDef type)
		{
			this.GetLord()?.Notify_Clamor(source, type);
		}

		public override void Notify_Explosion(Explosion explosion)
		{
			base.Notify_Explosion(explosion);
			mindState.Notify_Explosion(explosion);
		}

		public override void Notify_BulletImpactNearby(BulletImpactData impactData)
		{
			apparel?.Notify_BulletImpactNearby(impactData);
		}

		private void CheckForDisturbedSleep(Pawn source)
		{
			if (needs.mood != null && !this.Awake() && base.Faction == Faction.OfPlayer && Find.TickManager.TicksGame >= lastSleepDisturbedTick + 300 && !Deathresting && (source == null || (!LovePartnerRelationUtility.LovePartnerRelationExists(this, source) && !(source.RaceProps.petness > 0f) && (source.relations == null || !source.relations.DirectRelations.Any((DirectPawnRelation dr) => dr.def == PawnRelationDefOf.Bond)))))
			{
				lastSleepDisturbedTick = Find.TickManager.TicksGame;
				needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleepDisturbed);
			}
		}

		public float GetAcceptArrestChance(Pawn arrester)
		{
			if (Downed || WorkTagIsDisabled(WorkTags.Violent) || (guilt != null && guilt.IsGuilty && IsColonist && !this.IsQuestLodger()))
			{
				return 1f;
			}
			if (ModsConfig.BiotechActive && genes != null && genes.AggroMentalBreakSelectionChanceFactor <= 0f)
			{
				return 1f;
			}
			return (StatDefOf.ArrestSuccessChance.Worker.IsDisabledFor(arrester) ? StatDefOf.ArrestSuccessChance.valueIfMissing : arrester.GetStatValue(StatDefOf.ArrestSuccessChance)) * kindDef.acceptArrestChanceFactor;
		}

		public bool CheckAcceptArrest(Pawn arrester)
		{
			Faction homeFaction = HomeFaction;
			if (homeFaction != null && homeFaction != arrester.factionInt)
			{
				homeFaction.Notify_MemberCaptured(this, arrester.Faction);
			}
			if (Downed)
			{
				return true;
			}
			if (WorkTagIsDisabled(WorkTags.Violent))
			{
				return true;
			}
			float acceptArrestChance = GetAcceptArrestChance(arrester);
			if (Rand.Value < acceptArrestChance)
			{
				return true;
			}
			Messages.Message("MessageRefusedArrest".Translate(LabelShort, this), this, MessageTypeDefOf.ThreatSmall);
			if (base.Faction == null || !arrester.HostileTo(this))
			{
				mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
			}
			return false;
		}

		public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
		{
			if (!base.Spawned)
			{
				return true;
			}
			if (!InMentalState && this.GetTraderCaravanRole() == TraderCaravanRole.Carrier && !(jobs.curDriver is JobDriver_AttackMelee))
			{
				return true;
			}
			if (mindState.duty != null && mindState.duty.def.threatDisabled)
			{
				return true;
			}
			if (!mindState.Active)
			{
				return true;
			}
			if (this.IsColonyMechRequiringMechanitor())
			{
				return true;
			}
			Pawn pawn = disabledFor?.Thing as Pawn;
			if (Downed)
			{
				if (disabledFor == null)
				{
					return true;
				}
				if (pawn == null || pawn.mindState == null || pawn.mindState.duty == null || !pawn.mindState.duty.attackDownedIfStarving || !pawn.Starving())
				{
					return true;
				}
			}
			if (this.IsInvisible())
			{
				return true;
			}
			if (pawn != null && (ThreatDisabledBecauseNonAggressiveRoamer(pawn) || pawn.ThreatDisabledBecauseNonAggressiveRoamer(this)))
			{
				return true;
			}
			return false;
		}

		public bool ThreatDisabledBecauseNonAggressiveRoamer(Pawn otherPawn)
		{
			if (!RaceProps.Roamer || base.Faction != Faction.OfPlayer)
			{
				return false;
			}
			Lord lord = otherPawn.GetLord();
			if (lord == null || lord.CurLordToil.AllowAggressiveTargettingOfRoamers)
			{
				return false;
			}
			if (InAggroMentalState || this.IsFighting() || Find.TickManager.TicksGame < mindState.lastEngageTargetTick + 360)
			{
				return false;
			}
			return true;
		}

		public List<WorkTypeDef> GetDisabledWorkTypes(bool permanentOnly = false)
		{
			if (permanentOnly)
			{
				if (cachedDisabledWorkTypesPermanent == null)
				{
					cachedDisabledWorkTypesPermanent = new List<WorkTypeDef>();
				}
				FillList(cachedDisabledWorkTypesPermanent);
				return cachedDisabledWorkTypesPermanent;
			}
			if (cachedDisabledWorkTypes == null)
			{
				cachedDisabledWorkTypes = new List<WorkTypeDef>();
			}
			FillList(cachedDisabledWorkTypes);
			return cachedDisabledWorkTypes;
			void FillList(List<WorkTypeDef> list)
			{
				if (story != null && !IsSlave)
				{
					foreach (BackstoryDef allBackstory in story.AllBackstories)
					{
						foreach (WorkTypeDef disabledWorkType in allBackstory.DisabledWorkTypes)
						{
							if (!list.Contains(disabledWorkType))
							{
								list.Add(disabledWorkType);
							}
						}
					}
					for (int i = 0; i < story.traits.allTraits.Count; i++)
					{
						if (!story.traits.allTraits[i].Suppressed)
						{
							foreach (WorkTypeDef disabledWorkType2 in story.traits.allTraits[i].GetDisabledWorkTypes())
							{
								if (!list.Contains(disabledWorkType2))
								{
									list.Add(disabledWorkType2);
								}
							}
						}
					}
				}
				if (ModsConfig.BiotechActive && IsColonyMech)
				{
					List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
					for (int j = 0; j < allDefsListForReading.Count; j++)
					{
						if (!RaceProps.mechEnabledWorkTypes.Contains(allDefsListForReading[j]) && !list.Contains(allDefsListForReading[j]))
						{
							list.Add(allDefsListForReading[j]);
						}
					}
				}
				if (!permanentOnly)
				{
					if (royalty != null && !IsSlave)
					{
						foreach (RoyalTitle item in royalty.AllTitlesForReading)
						{
							if (item.conceited)
							{
								foreach (WorkTypeDef disabledWorkType3 in item.def.DisabledWorkTypes)
								{
									if (!list.Contains(disabledWorkType3))
									{
										list.Add(disabledWorkType3);
									}
								}
							}
						}
					}
					if (ModsConfig.IdeologyActive && Ideo != null)
					{
						Precept_Role role = Ideo.GetRole(this);
						if (role != null)
						{
							foreach (WorkTypeDef disabledWorkType4 in role.DisabledWorkTypes)
							{
								if (!list.Contains(disabledWorkType4))
								{
									list.Add(disabledWorkType4);
								}
							}
						}
					}
					if (ModsConfig.BiotechActive && genes != null)
					{
						foreach (Gene item2 in genes.GenesListForReading)
						{
							foreach (WorkTypeDef disabledWorkType5 in item2.DisabledWorkTypes)
							{
								if (!list.Contains(disabledWorkType5))
								{
									list.Add(disabledWorkType5);
								}
							}
						}
					}
					foreach (QuestPart_WorkDisabled item3 in QuestUtility.GetWorkDisabledQuestPart(this))
					{
						foreach (WorkTypeDef disabledWorkType6 in item3.DisabledWorkTypes)
						{
							if (!list.Contains(disabledWorkType6))
							{
								list.Add(disabledWorkType6);
							}
						}
					}
					if (guest != null)
					{
						foreach (WorkTypeDef disabledWorkType7 in guest.GetDisabledWorkTypes())
						{
							if (!list.Contains(disabledWorkType7))
							{
								list.Add(disabledWorkType7);
							}
						}
					}
					for (int k = 0; k < RaceProps.lifeStageWorkSettings.Count; k++)
					{
						LifeStageWorkSettings lifeStageWorkSettings = RaceProps.lifeStageWorkSettings[k];
						if (lifeStageWorkSettings.IsDisabled(this) && !list.Contains(lifeStageWorkSettings.workType))
						{
							list.Add(lifeStageWorkSettings.workType);
						}
					}
				}
			}
		}

		public List<string> GetReasonsForDisabledWorkType(WorkTypeDef workType)
		{
			if (cachedReasonsForDisabledWorkTypes != null && cachedReasonsForDisabledWorkTypes.ContainsKey(workType))
			{
				return cachedReasonsForDisabledWorkTypes[workType];
			}
			List<string> list = new List<string>();
			foreach (BackstoryDef allBackstory in story.AllBackstories)
			{
				foreach (WorkTypeDef disabledWorkType in allBackstory.DisabledWorkTypes)
				{
					if (workType == disabledWorkType)
					{
						list.Add("WorkDisabledByBackstory".Translate(allBackstory.TitleCapFor(gender)));
						break;
					}
				}
			}
			for (int i = 0; i < story.traits.allTraits.Count; i++)
			{
				Trait trait = story.traits.allTraits[i];
				foreach (WorkTypeDef disabledWorkType2 in trait.GetDisabledWorkTypes())
				{
					if (disabledWorkType2 == workType && !trait.Suppressed)
					{
						list.Add("WorkDisabledByTrait".Translate(trait.LabelCap));
						break;
					}
				}
			}
			if (royalty != null)
			{
				foreach (RoyalTitle item in royalty.AllTitlesForReading)
				{
					if (!item.conceited)
					{
						continue;
					}
					foreach (WorkTypeDef disabledWorkType3 in item.def.DisabledWorkTypes)
					{
						if (workType == disabledWorkType3)
						{
							list.Add("WorkDisabledByRoyalTitle".Translate(item.Label));
							break;
						}
					}
				}
			}
			if (ModsConfig.IdeologyActive && Ideo != null)
			{
				Precept_Role role = Ideo.GetRole(this);
				if (role != null)
				{
					foreach (WorkTypeDef disabledWorkType4 in role.DisabledWorkTypes)
					{
						if (workType == disabledWorkType4)
						{
							list.Add("WorkDisabledRole".Translate(role.LabelForPawn(this)));
							break;
						}
					}
				}
			}
			foreach (QuestPart_WorkDisabled item2 in QuestUtility.GetWorkDisabledQuestPart(this))
			{
				foreach (WorkTypeDef disabledWorkType5 in item2.DisabledWorkTypes)
				{
					if (workType == disabledWorkType5)
					{
						list.Add("WorkDisabledByQuest".Translate(item2.quest.name));
						break;
					}
				}
			}
			if (guest != null && guest.IsSlave)
			{
				foreach (WorkTypeDef disabledWorkType6 in guest.GetDisabledWorkTypes())
				{
					if (workType == disabledWorkType6)
					{
						list.Add("WorkDisabledSlave".Translate());
						break;
					}
				}
			}
			if (this.IsWorkTypeDisabledByAge(workType, out var minAgeRequired))
			{
				list.Add("WorkDisabledAge".Translate(this, ageTracker.AgeBiologicalYears, workType.labelShort, minAgeRequired));
			}
			if (cachedReasonsForDisabledWorkTypes == null)
			{
				cachedReasonsForDisabledWorkTypes = new Dictionary<WorkTypeDef, List<string>>();
			}
			cachedReasonsForDisabledWorkTypes[workType] = list;
			return list;
		}

		public bool WorkTypeIsDisabled(WorkTypeDef w)
		{
			return GetDisabledWorkTypes().Contains(w);
		}

		public bool OneOfWorkTypesIsDisabled(List<WorkTypeDef> wts)
		{
			for (int i = 0; i < wts.Count; i++)
			{
				if (WorkTypeIsDisabled(wts[i]))
				{
					return true;
				}
			}
			return false;
		}

		public void Notify_DisabledWorkTypesChanged()
		{
			cachedDisabledWorkTypes = null;
			cachedDisabledWorkTypesPermanent = null;
			cachedReasonsForDisabledWorkTypes = null;
			workSettings?.Notify_DisabledWorkTypesChanged();
			skills?.Notify_SkillDisablesChanged();
		}

		public bool WorkTagIsDisabled(WorkTags w)
		{
			return (CombinedDisabledWorkTags & w) != 0;
		}

		public override bool PreventPlayerSellingThingsNearby(out string reason)
		{
			if (base.Faction.HostileTo(Faction.OfPlayer) && HostFaction == null && !Downed && !InMentalState)
			{
				reason = "Enemies".Translate();
				return true;
			}
			reason = null;
			return false;
		}

		public void ChangeKind(PawnKindDef newKindDef)
		{
			if (kindDef != newKindDef)
			{
				kindDef = newKindDef;
				if (kindDef == PawnKindDefOf.WildMan)
				{
					mindState.WildManEverReachedOutside = false;
					ReachabilityUtility.ClearCacheFor(this);
				}
			}
		}
	}
}
