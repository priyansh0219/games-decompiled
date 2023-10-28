using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompStudiable : ThingComp
	{
		private List<ResearchProjectDef> researchUnlocked;

		public CompProperties_Studiable Props => (CompProperties_Studiable)props;

		public float ProgressPercent => Find.StudyManager.GetStudyProgress(parent.def);

		public bool Completed => ProgressPercent >= 1f;

		public int ProgressInt => Mathf.RoundToInt(ProgressPercent * (float)Props.cost);

		public List<ResearchProjectDef> ResearchUnlocked
		{
			get
			{
				if (researchUnlocked == null)
				{
					researchUnlocked = new List<ResearchProjectDef>();
					researchUnlocked.AddRange(Find.StudyManager.GetAllUnlockedResearch(parent.def));
				}
				return researchUnlocked;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (ModLister.CheckIdeologyOrBiotech("CompStudiable"))
			{
				base.PostSpawnSetup(respawningAfterLoad);
			}
		}

		public void Study(float amount, Pawn studier = null)
		{
			bool completed = Completed;
			amount *= 0.00825f;
			amount *= Find.Storyteller.difficulty.researchSpeedFactor;
			Find.StudyManager.SetStudied(parent.def, amount);
			studier?.skills.Learn(SkillDefOf.Intellectual, 0.1f);
			if (completed || !Completed)
			{
				return;
			}
			QuestUtility.SendQuestTargetSignals(parent.questTags, "Researched", parent.Named("SUBJECT"));
			if (studier != null && !Props.completedLetterText.NullOrEmpty() && !Props.completedLetterTitle.NullOrEmpty())
			{
				string arg = ResearchUnlocked.Select((ResearchProjectDef r) => r.label).ToCommaList(useAnd: true);
				Find.LetterStack.ReceiveLetter(Props.completedLetterTitle.Formatted(studier.Named("STUDIER"), parent.Named("PARENT"), arg.Named("RESEARCH")).CapitalizeFirst(), Props.completedLetterText.Formatted(studier.Named("STUDIER"), parent.Named("PARENT"), arg.Named("RESEARCH")), Props.completedLetterDef ?? LetterDefOf.NeutralEvent, new List<Thing> { parent, studier });
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (!DebugSettings.ShowDevGizmos || Completed)
			{
				yield break;
			}
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Complete study";
			command_Action.action = delegate
			{
				int num = 100;
				while (!Completed && num > 0)
				{
					Study(float.MaxValue);
					num--;
				}
			};
			yield return command_Action;
		}

		private IEnumerable<Dialog_InfoCard.Hyperlink> GetRelatedQuestHyperlinks()
		{
			List<Quest> quests = Find.QuestManager.QuestsListForReading;
			for (int i = 0; i < quests.Count; i++)
			{
				if (quests[i].hidden || (quests[i].State != QuestState.Ongoing && quests[i].State != 0))
				{
					continue;
				}
				List<QuestPart> partsListForReading = quests[i].PartsListForReading;
				for (int j = 0; j < partsListForReading.Count; j++)
				{
					if (partsListForReading[j] is QuestPart_RequirementsToAcceptThingStudied questPart_RequirementsToAcceptThingStudied && questPart_RequirementsToAcceptThingStudied.thing == parent)
					{
						yield return new Dialog_InfoCard.Hyperlink(quests[i]);
						break;
					}
				}
			}
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			IEnumerable<StatDrawEntry> enumerable = base.SpecialDisplayStats();
			if (enumerable != null)
			{
				foreach (StatDrawEntry item in enumerable)
				{
					yield return item;
				}
			}
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn, "Study".Translate(), ProgressInt + " / " + Props.cost, "Stat_Studiable_Desc".Translate(), 3000, null, GetRelatedQuestHyperlinks());
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
		{
			if (!ModsConfig.BiotechActive || parent.def.category != ThingCategory.Item)
			{
				yield break;
			}
			if (Completed)
			{
				yield return new FloatMenuOption("CannotStudy".Translate() + " (" + "AlreadyStudied".Translate() + ")", null);
				yield break;
			}
			if (Props.requiresMechanitor && !MechanitorUtility.IsMechanitor(pawn))
			{
				yield return new FloatMenuOption("CannotStudy".Translate() + " (" + "RequiresMechanitor".Translate() + ")", null);
				yield break;
			}
			if (!CanBeUsedBy(pawn, out var failReason))
			{
				yield return new FloatMenuOption("CannotStudy".Translate() + ((failReason != null) ? (" (" + failReason + ")") : ""), null);
				yield break;
			}
			if (!pawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
			{
				yield return new FloatMenuOption("CannotStudy".Translate() + " (" + "NoPath".Translate() + ")", null);
				yield break;
			}
			if (!pawn.CanReserve(parent))
			{
				yield return new FloatMenuOption("CannotStudy".Translate() + " (" + "Reserved".Translate() + ")", null);
				yield break;
			}
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				yield return new FloatMenuOption("CannotStudy".Translate() + " (" + "Incapable".Translate().CapitalizeFirst() + ")", null);
				yield break;
			}
			Thing researchBench = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.ResearchBench), PathEndMode.InteractionCell, TraverseParms.For(pawn, Danger.Some), 9999f, (Thing t) => pawn.CanReserve(t));
			if (researchBench == null)
			{
				yield return new FloatMenuOption("CannotStudy".Translate() + " (" + "NoResearchBench".Translate() + ")", null);
				yield break;
			}
			TaggedString taggedString = "StudyThing".Translate(parent.Label);
			_ = ResearchUnlocked;
			if (researchUnlocked.Count > 0)
			{
				taggedString += " (" + researchUnlocked.Select((ResearchProjectDef r) => r.label).ToCommaList(useAnd: true) + ")";
			}
			yield return new FloatMenuOption(taggedString, delegate
			{
				if (pawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly))
				{
					Job job = JobMaker.MakeJob(JobDefOf.StudyItem, parent, researchBench, researchBench.Position);
					pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				}
			});
		}

		public bool CanBeUsedBy(Pawn p, out string failReason)
		{
			failReason = null;
			return true;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			if (Scribe.mode != LoadSaveMode.LoadingVars)
			{
				return;
			}
			float value = 0f;
			Scribe_Values.Look(ref value, "progress", 0f);
			if (value > 0f)
			{
				float num = value / (float)Props.cost;
				float studyProgress = Find.StudyManager.GetStudyProgress(parent.def);
				if (num > studyProgress)
				{
					Find.StudyManager.ForceSetStudiedProgress(parent.def, num);
				}
			}
		}

		public override string CompInspectStringExtra()
		{
			string text = (string)("StudyProgress".Translate() + ": ") + ProgressInt + " / " + Props.cost;
			if (Completed)
			{
				text += " (" + "StudyCompleted".Translate() + ")";
			}
			return text;
		}
	}
}
