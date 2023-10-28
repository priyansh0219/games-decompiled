using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Root_SanguophageMeetingHost : QuestNode
	{
		private const string QuestTag = "SanguophageMeeting";

		private const string EnemyListAlias = "enemies";

		public const string SanguophageFactionAlias = "faction";

		public const string SanguophageListAlias = "sanguophages";

		public const string ReimplantWaitTicksAlias = "reimplantWaitTicks";

		public const string ReimplantedSignalAlias = "xenogermReimplantedSignal";

		public const string GatherSpotAlias = "gatherSpot";

		private const float ExtraRaidPointsPerSanguophage = 200f;

		public const int ReimplantWaitDurationTicks = 15000;

		private const int JoinerWaitDurationTicks = 7500;

		private static readonly List<CountChance> SanguophageCountChances = new List<CountChance>
		{
			new CountChance
			{
				count = 2,
				chance = 0.45f
			},
			new CountChance
			{
				count = 3,
				chance = 0.25f
			},
			new CountChance
			{
				count = 4,
				chance = 0.15f
			},
			new CountChance
			{
				count = 5,
				chance = 0.08f
			},
			new CountChance
			{
				count = 6,
				chance = 0.04f
			},
			new CountChance
			{
				count = 7,
				chance = 0.02f
			},
			new CountChance
			{
				count = 8,
				chance = 0.01f
			}
		};

		private static readonly IntRange MeetingDurationRange = new IntRange(10000, 20000);

		private static readonly IntRange EventDelayRange = new IntRange(5000, 7500);

		private static readonly IntRange ArrivalDelayRange = new IntRange(2500, 7500);

		private static readonly SimpleCurve RewardValueCurve = new SimpleCurve
		{
			new CurvePoint(500f, 400f),
			new CurvePoint(3000f, 2000f),
			new CurvePoint(30000f, 10000f)
		};

		private List<Room> tmpRooms = new List<Room>();

		protected override void RunInt()
		{
			if (!ModLister.CheckBiotech("Sanguophage meeting host"))
			{
				return;
			}
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			int? num = null;
			Map map = QuestGen_Get.GetMap(mustBeInfestable: false, num);
			float num2 = slate.Get("points", 0f);
			string questTag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("SanguophageMeeting");
			slate.Set("map", map);
			if (!TryFindGatheringSpot(map, out var gatherSpot))
			{
				return;
			}
			slate.Set("gatherSpot", gatherSpot);
			string meetingStartedSignal = QuestGen.GenerateNewSignal("MeetingStarted");
			string meetingCompletedSignal = QuestGen.GenerateNewSignal("MeetingCompleted");
			string raidArrivedSignal = QuestGen.GenerateNewSignal("RaidArrived");
			string beginDefendSignal = QuestGen.GenerateNewSignal("BeginDefend");
			string text = QuestGen.GenerateNewSignal("SanguophagesBecameHostile");
			string questFailedSignal = QuestGen.GenerateNewSignal("QuestFailed");
			string questSucceededSignal = QuestGen.GenerateNewSignal("QuestSucceeded");
			string allSanguophagesGone = QuestGen.GenerateNewSignal("AllSanguophagesGone");
			string text2 = QuestGenUtility.QuestTagSignal(questTag, "BeingAttacked");
			string factionBecameHostileSignal = QuestGenUtility.HardcodedSignalWithQuestID("faction.BecameHostileToPlayer");
			string text3 = QuestGenUtility.HardcodedSignalWithQuestID("sanguophages.Arrested");
			string text4 = QuestGenUtility.HardcodedSignalWithQuestID("sanguophages.SurgeryViolation");
			string text5 = QuestGenUtility.HardcodedSignalWithQuestID("sanguophages.XenogermAbsorbed");
			string text6 = QuestGenUtility.HardcodedSignalWithQuestID("sanguophages.XenogermReimplanted");
			QuestGenUtility.HardcodedSignalWithQuestID("sanguophages.ChangedFaction");
			string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("enemies.Despawned");
			slate.Set("xenogermReimplantedSignal", text6);
			slate.Set("reimplantWaitTicks", 15000);
			int meetingDurationTicks = MeetingDurationRange.RandomInRange;
			slate.Set("meetingDurationTicks", meetingDurationTicks);
			List<FactionRelation> list = new List<FactionRelation>();
			foreach (Faction item2 in Find.FactionManager.AllFactionsListForReading)
			{
				if (!item2.def.permanentEnemy)
				{
					list.Add(new FactionRelation(item2, FactionRelationKind.Neutral));
				}
			}
			Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(FactionDefOf.Sanguophages, list, hidden: true);
			faction.temporary = true;
			Find.FactionManager.Add(faction);
			slate.Set("faction", faction);
			quest.ReserveFaction(faction);
			List<Pawn> sanguophages = new List<Pawn>();
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Sanguophage, faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true);
			int num3 = CountChanceUtility.RandomCount(SanguophageCountChances);
			for (int i = 0; i < num3; i++)
			{
				Pawn item = quest.GeneratePawn(request);
				sanguophages.Add(item);
			}
			slate.Set("sanguophages", sanguophages);
			slate.Set("sanguophageCount", num3);
			slate.Set("sanguophageCountMinusOne", num3 - 1);
			Pawn pawn = sanguophages.First();
			slate.Set("asker", pawn);
			Faction enemyFaction = null;
			List<Pawn> enemies = null;
			if (Find.Storyteller.difficulty.allowViolentQuests && Find.FactionManager.AllFactionsVisible.Where((Faction x) => x.def.permanentEnemy).TryRandomElement(out enemyFaction))
			{
				slate.Set("enemyFaction", enemyFaction);
				PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
				pawnGroupMakerParms.faction = enemyFaction;
				pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Combat;
				pawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(num2 + 200f * (float)num3, PawnsArrivalModeDefOf.EdgeWalkIn, RaidStrategyDefOf.ImmediateAttack, enemyFaction, PawnGroupKindDefOf.Combat);
				pawnGroupMakerParms.tile = map.Tile;
				enemies = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms).ToList();
				if (enemies.Any())
				{
					for (int j = 0; j < enemies.Count; j++)
					{
						Find.WorldPawns.PassToWorld(enemies[j]);
						QuestGen.AddToGeneratedPawns(enemies[j]);
					}
					slate.Set("enemies", enemies);
					QuestPart_PawnsArrive questPart_PawnsArrive = new QuestPart_PawnsArrive();
					questPart_PawnsArrive.pawns.AddRange(enemies);
					questPart_PawnsArrive.arrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
					questPart_PawnsArrive.mapParent = map.Parent;
					questPart_PawnsArrive.inSignal = raidArrivedSignal;
					questPart_PawnsArrive.sendStandardLetter = false;
					questPart_PawnsArrive.addPawnsToLookTargets = false;
					quest.AddPart(questPart_PawnsArrive);
					quest.AssaultThings(map.Parent, enemies, enemyFaction, sanguophages, raidArrivedSignal, null, excludeFromLookTargets: true);
					quest.Letter(LetterDefOf.ThreatBig, raidArrivedSignal, null, enemyFaction, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, enemies, filterDeadPawnsFromLookTargets: false, "[raidArrivedLetterText]", null, "[raidArrivedLetterLabel]");
					quest.AllPawnsDespawned(enemies, delegate
					{
						quest.SignalPass(null, null, meetingCompletedSignal);
					}, null, inSignal);
				}
			}
			quest.Message("SanguophagesArrivingSoon".Translate(), MessageTypeDefOf.NeutralEvent).historical = false;
			int randomInRange = ArrivalDelayRange.RandomInRange;
			quest.Delay(randomInRange, delegate
			{
				quest.PawnsArrive(sanguophages, null, map.Parent, PawnsArrivalModeDefOf.EdgeWalkInGroups, joinPlayer: false, null, "[sanguophagesArriveLetterLabel]", "[sanguophagesArriveLetterText]");
				QuestPart_SagnuophageMeeting questPart_SagnuophageMeeting = new QuestPart_SagnuophageMeeting();
				questPart_SagnuophageMeeting.inSignal = QuestGen.slate.Get<string>("inSignal");
				questPart_SagnuophageMeeting.questTag = questTag;
				questPart_SagnuophageMeeting.faction = faction;
				questPart_SagnuophageMeeting.pawns.AddRange(sanguophages);
				questPart_SagnuophageMeeting.targetCell = gatherSpot;
				questPart_SagnuophageMeeting.meetingDurationTicks = meetingDurationTicks;
				questPart_SagnuophageMeeting.mapParent = map.Parent;
				questPart_SagnuophageMeeting.inSignalDefend = beginDefendSignal;
				questPart_SagnuophageMeeting.inSignalQuestSuccess = questSucceededSignal;
				questPart_SagnuophageMeeting.inSignalQuestFail = questFailedSignal;
				questPart_SagnuophageMeeting.outSignalMeetingStarted = meetingStartedSignal;
				questPart_SagnuophageMeeting.outSignalMeetingCompleted = meetingCompletedSignal;
				questPart_SagnuophageMeeting.outSignalAllSanguophagesGone = allSanguophagesGone;
				if (enemies != null)
				{
					questPart_SagnuophageMeeting.enemyTargets.AddRange(enemies);
				}
				quest.AddPart(questPart_SagnuophageMeeting);
			}).debugLabel = "Arrival delay";
			Action joinEvent = delegate
			{
				Pawn joiner = sanguophages.FirstOrDefault((Pawn x) => !x.Dead && !x.Downed);
				if (joiner != null)
				{
					quest.WaitForDuration(map.Parent, sanguophages, faction, gatherSpot, 7500);
					quest.PawnJoinOffer(joiner, "LetterJoinOfferLabel".Translate(joiner.Named("PAWN")), "LetterJoinOfferTitle".Translate(joiner.Named("PAWN")), "LetterJoinOfferText".Translate(joiner.Named("PAWN"), map.Parent.Named("MAP")).Resolve(), delegate
					{
						quest.JoinPlayer(map.Parent, Gen.YieldSingle(joiner), joinPlayer: true);
						quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, label: "LetterLabelMessageRecruitSuccess".Translate() + ": " + joiner.LabelShortCap, text: "MessageRecruitJoinOfferAccepted".Translate(joiner.Named("RECRUITEE")).Resolve());
						quest.SignalPass(null, null, meetingCompletedSignal);
					}, delegate
					{
						quest.SignalPass(null, null, meetingCompletedSignal);
					}, null, null, null, charity: false, sendLetterOnEnable: true);
				}
			};
			Action raidEvent = delegate
			{
				quest.SignalPass(null, null, raidArrivedSignal);
				quest.Delay(1250, delegate
				{
					quest.SignalPass(null, null, beginDefendSignal);
				}).debugLabel = "Defend delay";
			};
			int randomInRange2 = EventDelayRange.RandomInRange;
			quest.Delay(randomInRange2, delegate
			{
				List<Action> list2 = new List<Action>
				{
					delegate
					{
					}
				};
				if (enemyFaction != null)
				{
					list2.Add(raidEvent);
				}
				list2.Add(joinEvent);
				quest.SignalRandom(list2);
			}, meetingStartedSignal).debugLabel = "Event delay";
			bool flag = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Any((Pawn x) => x.genes != null && x.genes.Xenotype == XenotypeDefOf.Sanguophage);
			Quest quest2 = quest;
			RewardsGeneratorParams parms = new RewardsGeneratorParams
			{
				rewardValue = RewardValueCurve.Evaluate(num2),
				thingRewardRequired = true,
				allowXenogermReimplantation = !flag
			};
			num = (flag ? 3 : 2);
			quest2.GiveRewards(parms, questSucceededSignal, null, null, null, null, null, null, num, addCampLootReward: false, pawn, addShuttleLootReward: false, addPossibleFutureReward: false, 0f);
			quest.AnySignal(new string[4] { text3, text4, text5, text2 }, null, new string[2] { text, questFailedSignal });
			quest.FactionRelationToPlayerChange(faction, FactionRelationKind.Hostile, canSendHostilityLetter: false, text);
			quest.End(QuestEndOutcome.Fail, 0, null, text);
			quest.End(QuestEndOutcome.Fail, 0, null, allSanguophagesGone);
			quest.AnySignal(new string[2] { text6, meetingCompletedSignal }, null, new string[1] { questSucceededSignal });
			quest.Letter(LetterDefOf.PositiveEvent, meetingCompletedSignal, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, sanguophages, filterDeadPawnsFromLookTargets: true, "[successLetterText]", null, "[successLetterLabel]");
			quest.End(QuestEndOutcome.Success, 0, null, questSucceededSignal);
		}

		protected override bool TestRunInt(Slate slate)
		{
			Map map = QuestGen_Get.GetMap();
			if (map == null)
			{
				return false;
			}
			slate.Get("points", 0f);
			if (!FactionDefOf.Sanguophages.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp))
			{
				return false;
			}
			if (!TryFindGatheringSpot(map, out var _))
			{
				return false;
			}
			return true;
		}

		private bool TryFindGatheringSpot(Map map, out IntVec3 cell)
		{
			tmpRooms.Clear();
			if (map == null)
			{
				cell = IntVec3.Invalid;
				return false;
			}
			foreach (Building item in map.listerBuildings.allBuildingsColonist)
			{
				Room room = item.Position.GetRoom(map);
				if (room != null && !tmpRooms.Contains(room))
				{
					tmpRooms.Add(room);
				}
			}
			tmpRooms.SortBy((Room r) => 0f - RoomScore(r));
			foreach (Room tmpRoom in tmpRooms)
			{
				for (int i = 0; i < 10; i++)
				{
					IntVec3 intVec = CellFinder.RandomClosewalkCellNear(tmpRoom.Cells.RandomElement(), map, 5, delegate(IntVec3 curCell)
					{
						if (curCell.GetEdifice(map) != null)
						{
							return false;
						}
						for (int j = 0; j < GenAdj.AdjacentCellsAround.Length; j++)
						{
							IntVec3 c = curCell + GenAdj.AdjacentCellsAround[j];
							if (!c.InBounds(map) || c.Fogged(map) || !c.Standable(map))
							{
								return false;
							}
						}
						return true;
					});
					if (map.reachability.CanReachColony(intVec))
					{
						cell = intVec;
						return true;
					}
				}
			}
			if (RCellFinder.TryFindRandomSpotJustOutsideColony(CellFinder.RandomEdgeCell(map), map, out cell))
			{
				return true;
			}
			cell = IntVec3.Invalid;
			return false;
			float RoomScore(Room r)
			{
				if (r.CellCount < 10 || r.PsychologicallyOutdoors)
				{
					return -100f;
				}
				if (r.Role == RoomRoleDefOf.Barracks || r.Role == RoomRoleDefOf.Bedroom)
				{
					return -100f;
				}
				if (r.Role == RoomRoleDefOf.PrisonCell || r.Role == RoomRoleDefOf.PrisonBarracks)
				{
					return -1000f;
				}
				return r.GetStat(RoomStatDefOf.Impressiveness) + r.GetStat(RoomStatDefOf.Beauty);
			}
		}
	}
}
