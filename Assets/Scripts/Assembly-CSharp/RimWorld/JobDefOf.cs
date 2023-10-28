using Verse;

namespace RimWorld
{
	[DefOf]
	public static class JobDefOf
	{
		public static JobDef IdleWhileDespawned;

		public static JobDef Goto;

		public static JobDef PickupToHold;

		public static JobDef Wait;

		public static JobDef Wait_MaintainPosture;

		public static JobDef Wait_Asleep;

		public static JobDef Wait_Downed;

		public static JobDef GotoWander;

		public static JobDef Wait_Wander;

		public static JobDef GotoSafeTemperature;

		public static JobDef Wait_SafeTemperature;

		public static JobDef Wait_Combat;

		public static JobDef Equip;

		public static JobDef AttackMelee;

		public static JobDef AttackStatic;

		public static JobDef UseVerbOnThing;

		public static JobDef UseVerbOnThingStatic;

		public static JobDef CastJump;

		public static JobDef CastAbilityOnThing;

		public static JobDef CastAbilityOnWorldTile;

		public static JobDef TakeInventory;

		public static JobDef Follow;

		public static JobDef FollowClose;

		public static JobDef Wear;

		public static JobDef RemoveApparel;

		public static JobDef DropEquipment;

		public static JobDef Strip;

		public static JobDef Open;

		public static JobDef Hunt;

		public static JobDef ManTurret;

		public static JobDef EnterCryptosleepCasket;

		public static JobDef UseNeurotrainer;

		public static JobDef UseArtifact;

		public static JobDef ClearSnow;

		public static JobDef Vomit;

		public static JobDef Flick;

		public static JobDef DoBill;

		public static JobDef Research;

		public static JobDef Mine;

		public static JobDef OperateDeepDrill;

		public static JobDef OperateScanner;

		public static JobDef Repair;

		[MayRequireBiotech]
		public static JobDef RepairMech;

		[MayRequireBiotech]
		public static JobDef ControlMech;

		public static JobDef FixBrokenDownBuilding;

		public static JobDef UseCommsConsole;

		public static JobDef Clean;

		public static JobDef PaintBuilding;

		public static JobDef PaintFloor;

		public static JobDef RemovePaintBuilding;

		public static JobDef RemovePaintFloor;

		public static JobDef TradeWithPawn;

		public static JobDef Flee;

		public static JobDef FleeAndCower;

		public static JobDef Lovin;

		public static JobDef SocialFight;

		public static JobDef Maintain;

		public static JobDef GiveToPackAnimal;

		public static JobDef EnterTransporter;

		public static JobDef Resurrect;

		public static JobDef Insult;

		public static JobDef HaulCorpseToPublicPlace;

		public static JobDef InducePrisonerToEscape;

		public static JobDef OfferHelp;

		public static JobDef ApplyTechprint;

		public static JobDef GotoMindControlled;

		public static JobDef EmptyThingContainer;

		public static JobDef ActivateThing;

		[MayRequireIdeology]
		public static JobDef ActivateArchonexusCore;

		public static JobDef StudyBuilding;

		[MayRequireBiotech]
		public static JobDef RemoveMechlink;

		[MayRequireBiotech]
		public static JobDef GetReimplanted;

		public static JobDef TakeFromOtherInventory;

		[MayRequireBiotech]
		public static JobDef StudyItem;

		[MayRequireBiotech]
		public static JobDef ClearPollution;

		[MayRequireBiotech]
		public static JobDef TryRomance;

		public static JobDef MarryAdjacentPawn;

		public static JobDef SpectateCeremony;

		public static JobDef StandAndBeSociallyActive;

		public static JobDef GiveSpeech;

		[MayRequireIdeology]
		public static JobDef AcceptRole;

		[MayRequireIdeology]
		public static JobDef Dance;

		[MayRequireIdeology]
		public static JobDef EatAtCannibalPlatter;

		public static JobDef PrepareCaravan_GatherItems;

		public static JobDef PrepareCaravan_GatherAnimals;

		public static JobDef PrepareCaravan_CollectAnimals;

		public static JobDef PrepareCaravan_GatherDownedPawns;

		public static JobDef ReturnedCaravan_PenAnimals;

		public static JobDef Ignite;

		public static JobDef BeatFire;

		public static JobDef ExtinguishSelf;

		public static JobDef LayDown;

		public static JobDef LayDownAwake;

		public static JobDef LayDownResting;

		public static JobDef Ingest;

		[MayRequireBiotech]
		public static JobDef SelfShutdown;

		public static JobDef SocialRelax;

		public static JobDef HaulToCell;

		public static JobDef HaulToContainer;

		public static JobDef Steal;

		public static JobDef Reload;

		public static JobDef Refuel;

		public static JobDef RefuelAtomic;

		public static JobDef RearmTurret;

		public static JobDef RearmTurretAtomic;

		public static JobDef FillFermentingBarrel;

		public static JobDef TakeBeerOutOfFermentingBarrel;

		public static JobDef UnloadInventory;

		public static JobDef UnloadYourInventory;

		public static JobDef HaulToTransporter;

		[MayRequireIdeology]
		public static JobDef GiveToPawn;

		[MayRequireIdeology]
		public static JobDef ExtractRelic;

		[MayRequireIdeology]
		public static JobDef InstallRelic;

		[MayRequireIdeology]
		public static JobDef ExtractToInventory;

		[MayRequireBiotech]
		public static JobDef EmptyWasteContainer;

		[MayRequireBiotech]
		public static JobDef CarryGenepackToContainer;

		public static JobDef Carried;

		public static JobDef Rescue;

		public static JobDef Arrest;

		public static JobDef Capture;

		public static JobDef TakeWoundedPrisonerToBed;

		public static JobDef TakeToBedToOperate;

		public static JobDef TakeDownedPawnToBedDrafted;

		public static JobDef EscortPrisonerToBed;

		public static JobDef CarryToCryptosleepCasket;

		public static JobDef CarryToCryptosleepCasketDrafted;

		public static JobDef ReleasePrisoner;

		public static JobDef Kidnap;

		public static JobDef CarryDownedPawnToExit;

		public static JobDef CarryDownedPawnDrafted;

		public static JobDef CarryToPrisonerBedDrafted;

		public static JobDef DeliverToCell;

		public static JobDef DeliverToBed;

		[MayRequireIdeology]
		public static JobDef DeliverToAltar;

		[MayRequireIdeology]
		public static JobDef Sacrifice;

		[MayRequireIdeology]
		public static JobDef Scarify;

		[MayRequireIdeology]
		public static JobDef Blind;

		[MayRequireBiotech]
		public static JobDef HaulMechToCharger;

		[MayRequireBiotech]
		public static JobDef HaulToAtomizer;

		public static JobDef PlaceNoCostFrame;

		public static JobDef Replant;

		public static JobDef FinishFrame;

		public static JobDef Deconstruct;

		public static JobDef Uninstall;

		public static JobDef ExtractTree;

		public static JobDef SmoothFloor;

		public static JobDef RemoveFloor;

		public static JobDef BuildRoof;

		public static JobDef RemoveRoof;

		public static JobDef SmoothWall;

		public static JobDef PrisonerAttemptRecruit;

		public static JobDef PrisonerExecution;

		public static JobDef GuiltyColonistExecution;

		public static JobDef DeliverFood;

		[MayRequireIdeology]
		public static JobDef PrisonerEnslave;

		[MayRequireIdeology]
		public static JobDef PrisonerConvert;

		[MayRequireBiotech]
		public static JobDef PrisonerBloodfeed;

		[MayRequireIdeology]
		public static JobDef SlaveSuppress;

		[MayRequireIdeology]
		public static JobDef SlaveEmancipation;

		[MayRequireIdeology]
		public static JobDef SlaveExecution;

		public static JobDef FeedPatient;

		public static JobDef TendPatient;

		public static JobDef VisitSickPawn;

		public static JobDef Sow;

		public static JobDef Harvest;

		public static JobDef CutPlant;

		public static JobDef HarvestDesignated;

		public static JobDef CutPlantDesignated;

		[MayRequireIdeology]
		public static JobDef PlantSeed;

		[MayRequireIdeology]
		public static JobDef PruneGauranlenTree;

		public static JobDef Slaughter;

		public static JobDef Milk;

		public static JobDef Shear;

		public static JobDef Tame;

		public static JobDef Train;

		public static JobDef RopeToPen;

		public static JobDef RopeRoamerToUnenclosedPen;

		public static JobDef RopeRoamerToHitchingPost;

		public static JobDef Unrope;

		public static JobDef ReleaseAnimalToWild;

		[MayRequireIdeology]
		public static JobDef ExtractSkull;

		public static JobDef Nuzzle;

		public static JobDef Mate;

		public static JobDef LayEgg;

		public static JobDef PredatorHunt;

		public static JobDef FollowRoper;

		[MayRequireRoyalty]
		public static JobDef Reign;

		[MayRequireRoyalty]
		public static JobDef Meditate;

		[MayRequireRoyalty]
		public static JobDef Play_MusicalInstrument;

		[MayRequireRoyalty]
		public static JobDef LinkPsylinkable;

		[MayRequireRoyalty]
		public static JobDef BestowingCeremony;

		[MayRequireIdeology]
		public static JobDef InduceSlaveToRebel;

		[MayRequireIdeology]
		public static JobDef OpenStylingStationDialog;

		[MayRequireIdeology]
		public static JobDef UseStylingStation;

		[MayRequireIdeology]
		public static JobDef UseStylingStationAutomatic;

		[MayRequireIdeology]
		public static JobDef DyeHair;

		[MayRequireIdeology]
		public static JobDef Hack;

		[MayRequireIdeology]
		public static JobDef TakeCountToInventory;

		[MayRequireIdeology]
		public static JobDef GotoAndBeSociallyActive;

		[MayRequireIdeology]
		public static JobDef PrepareSkylantern;

		[MayRequireIdeology]
		public static JobDef MeditatePray;

		[MayRequireIdeology]
		public static JobDef GetNeuralSupercharge;

		[MayRequireIdeology]
		public static JobDef CreateAndEnterCocoon;

		[MayRequireIdeology]
		public static JobDef CreateAndEnterHealingPod;

		[MayRequireIdeology]
		public static JobDef ReturnToGauranlenTree;

		[MayRequireIdeology]
		public static JobDef MergeIntoGaumakerPod;

		[MayRequireIdeology]
		public static JobDef ChangeTreeMode;

		[MayRequireIdeology]
		public static JobDef RecolorApparel;

		[MayRequireIdeology]
		public static JobDef EnterBiosculpterPod;

		[MayRequireIdeology]
		public static JobDef CarryToBiosculpterPod;

		[MayRequireBiotech]
		public static JobDef MechCharge;

		[MayRequireBiotech]
		public static JobDef Deathrest;

		[MayRequireBiotech]
		public static JobDef CreateXenogerm;

		[MayRequireBiotech]
		public static JobDef EnterBuilding;

		[MayRequireBiotech]
		public static JobDef CarryToBuilding;

		[MayRequireBiotech]
		public static JobDef AbsorbXenogerm;

		[MayRequireBiotech]
		public static JobDef Breastfeed;

		[MayRequireBiotech]
		public static JobDef BreastfeedCarryToMom;

		[MayRequireBiotech]
		public static JobDef BringBabyToSafety;

		[MayRequireBiotech]
		public static JobDef BringBabyToSafetyUnforced;

		[MayRequireBiotech]
		public static JobDef BottleFeedBaby;

		[MayRequireBiotech]
		public static JobDef CarryToMomAfterBirth;

		[MayRequireBiotech]
		public static JobDef Wait_WithSleeping;

		[MayRequireBiotech]
		public static JobDef ReleaseMechs;

		[MayRequireBiotech]
		public static JobDef Lessontaking;

		[MayRequireBiotech]
		public static JobDef Lessongiving;

		[MayRequireBiotech]
		public static JobDef FertilizeOvum;

		[MayRequireBiotech]
		public static JobDef DisassembleMech;

		[MayRequireBiotech]
		public static JobDef BabySuckle;

		[MayRequireBiotech]
		public static JobDef BabyPlay;

		static JobDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
		}
	}
}
