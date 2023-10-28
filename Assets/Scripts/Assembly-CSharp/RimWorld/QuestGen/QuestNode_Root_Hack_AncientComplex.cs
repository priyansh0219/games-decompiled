using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Root_Hack_AncientComplex : QuestNode_Root_AncientComplex
	{
		private const int MinDistanceFromColony = 2;

		private const int MaxDistanceFromColony = 10;

		private static IntRange HackDefenceRange = new IntRange(300, 1800);

		private const float MinMaxHackDefenceChance = 0.5f;

		private static readonly FloatRange RandomRaidPointsFactorRange = new FloatRange(0.25f, 0.35f);

		private const float ChanceToSpawnAllTerminalsHackedRaid = 0.5f;

		protected override void RunInt()
		{
			if (ModLister.CheckIdeology("Ancient complex rescue"))
			{
				Slate slate = QuestGen.slate;
				Quest quest = QuestGen.quest;
				Map map = QuestGen_Get.GetMap();
				float num = slate.Get("points", 0f);
				Precept_Relic precept_Relic = slate.Get<Precept_Relic>("relic");
				TryFindSiteTile(out var tile);
				TryFindEnemyFaction(out var enemyFaction);
				if (precept_Relic == null)
				{
					precept_Relic = Faction.OfPlayer.ideos.PrimaryIdeo.GetAllPreceptsOfType<Precept_Relic>().RandomElement();
					Log.Warning("Ancient Complex quest requires relic from parent quest. None found so picking random player relic");
				}
				string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("terminals.Destroyed");
				string text = QuestGen.GenerateNewSignal("TerminalHacked");
				string text2 = QuestGen.GenerateNewSignal("AllTerminalsHacked");
				QuestGen.GenerateNewSignal("RaidArrives");
				string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("site.Destroyed");
				ComplexSketch complexSketch = GenerateSketch(num);
				complexSketch.thingDiscoveredMessage = "MessageAncientTerminalDiscovered".Translate(precept_Relic.Label);
				List<string> list = new List<string>();
				for (int i = 0; i < complexSketch.thingsToSpawn.Count; i++)
				{
					Thing thing = complexSketch.thingsToSpawn[i];
					string text3 = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("terminal" + i);
					QuestUtility.AddQuestTag(thing, text3);
					string item = QuestGenUtility.HardcodedSignalWithQuestID(text3 + ".Hacked");
					list.Add(item);
					thing.TryGetComp<CompHackable>().defence = (Rand.Chance(0.5f) ? HackDefenceRange.min : HackDefenceRange.max);
				}
				float num2 = (Find.Storyteller.difficulty.allowViolentQuests ? QuestNode_Root_AncientComplex.ThreatPointsOverPointsCurve.Evaluate(num) : 0f);
				SitePartParams parms = new SitePartParams
				{
					ancientComplexSketch = complexSketch,
					ancientComplexRewardMaker = ThingSetMakerDefOf.MapGen_AncientComplexRoomLoot_Default,
					threatPoints = num2
				};
				Site site = QuestGen_Sites.GenerateSite(Gen.YieldSingle(new SitePartDefWithParams(SitePartDefOf.AncientComplex, parms)), tile, Faction.OfAncients);
				quest.SpawnWorldObject(site);
				TimedDetectionRaids component = site.GetComponent<TimedDetectionRaids>();
				if (component != null)
				{
					component.alertRaidsArrivingIn = true;
				}
				QuestPart_PassAllActivable questPart_PassAllActivable = new QuestPart_PassAllActivable();
				questPart_PassAllActivable.inSignalEnable = QuestGen.slate.Get<string>("inSignal");
				questPart_PassAllActivable.inSignals = list;
				questPart_PassAllActivable.outSignalsCompleted.Add(text2);
				questPart_PassAllActivable.outSignalAny = text;
				questPart_PassAllActivable.expiryInfoPartKey = "TerminalsHacked";
				quest.AddPart(questPart_PassAllActivable);
				quest.Message("[terminalHackedMessage]", null, getLookTargetsFromSignal: true, null, null, text);
				quest.Message("[allTerminalsHackedMessage]", MessageTypeDefOf.PositiveEvent, getLookTargetsFromSignal: false, null, null, text2);
				if (Find.Storyteller.difficulty.allowViolentQuests && Rand.Chance(0.5f))
				{
					quest.RandomRaid(site, RandomRaidPointsFactorRange * num2, enemyFaction, text2, PawnsArrivalModeDefOf.EdgeWalkIn, RaidStrategyDefOf.ImmediateAttack);
				}
				Reward_RelicInfo reward_RelicInfo = new Reward_RelicInfo();
				reward_RelicInfo.relic = precept_Relic;
				reward_RelicInfo.quest = quest;
				QuestPart_Choice questPart_Choice = quest.RewardChoice();
				QuestPart_Choice.Choice item2 = new QuestPart_Choice.Choice
				{
					rewards = { (Reward)reward_RelicInfo }
				};
				questPart_Choice.choices.Add(item2);
				QuestPart_Filter_Hacked questPart_Filter_Hacked = new QuestPart_Filter_Hacked();
				questPart_Filter_Hacked.inSignal = inSignal;
				questPart_Filter_Hacked.outSignalElse = QuestGen.GenerateNewSignal("FailQuestTerminalDestroyed");
				quest.AddPart(questPart_Filter_Hacked);
				quest.End(QuestEndOutcome.Success, 0, null, text2, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
				quest.End(QuestEndOutcome.Fail, 0, null, inSignal2, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
				quest.End(QuestEndOutcome.Fail, 0, null, questPart_Filter_Hacked.outSignalElse, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
				slate.Set("terminals", complexSketch.thingsToSpawn);
				slate.Set("terminalCount", complexSketch.thingsToSpawn.Count);
				slate.Set("map", map);
				slate.Set("relic", precept_Relic);
				slate.Set("site", site);
			}
		}

		private bool TryFindSiteTile(out int tile)
		{
			return TileFinder.TryFindNewSiteTile(out tile, 2, 10);
		}

		private bool TryFindEnemyFaction(out Faction enemyFaction)
		{
			enemyFaction = Find.FactionManager.RandomRaidableEnemyFaction();
			return enemyFaction != null;
		}

		protected override bool TestRunInt(Slate slate)
		{
			Faction enemyFaction;
			if (TryFindSiteTile(out var _))
			{
				return TryFindEnemyFaction(out enemyFaction);
			}
			return false;
		}
	}
}
