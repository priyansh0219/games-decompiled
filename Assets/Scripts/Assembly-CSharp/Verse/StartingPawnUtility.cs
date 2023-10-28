using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse
{
	public static class StartingPawnUtility
	{
		private const float ChanceToHavePossessionsFromBackground = 0.25f;

		private static readonly IntRange BabyFoodCountRange = new IntRange(30, 40);

		private static readonly IntRange HemogenCountRange = new IntRange(8, 12);

		private static readonly FloatRange ExcludeBiologicalAgeRange = new FloatRange(12.1f, 13f);

		private static List<PawnGenerationRequest> StartingAndOptionalPawnGenerationRequests = new List<PawnGenerationRequest>();

		private const int MaxPossessionsCount = 2;

		private static readonly FloatRange DaysSatisfied = new FloatRange(25f, 35f);

		private const float ChanceForRandomPossession = 0.06f;

		private static List<Pawn> StartingAndOptionalPawns => Find.GameInitData.startingAndOptionalPawns;

		private static Dictionary<Pawn, List<ThingDefCount>> StartingPossessions => Find.GameInitData.startingPossessions;

		private static PawnGenerationRequest DefaultStartingPawnRequest => new PawnGenerationRequest(Find.GameInitData.startingPawnKind ?? Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, -1, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, TutorSystem.TutorialMode, 20f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, ModsConfig.BiotechActive ? XenotypeDefOf.Baseliner : null, null, null, 0f, DevelopmentalStage.Adult, null, ModsConfig.BiotechActive ? new FloatRange?(ExcludeBiologicalAgeRange) : null);

		public static void ClearAllStartingPawns()
		{
			for (int num = StartingAndOptionalPawns.Count - 1; num >= 0; num--)
			{
				StartingAndOptionalPawns[num].relations.ClearAllRelations();
				if (Find.World != null)
				{
					PawnUtility.DestroyStartingColonistFamily(StartingAndOptionalPawns[num]);
					PawnComponentsUtility.RemoveComponentsOnDespawned(StartingAndOptionalPawns[num]);
					Find.WorldPawns.PassToWorld(StartingAndOptionalPawns[num], PawnDiscardDecideMode.Discard);
				}
				StartingPossessions.Remove(StartingAndOptionalPawns[num]);
				StartingAndOptionalPawns.RemoveAt(num);
			}
			StartingAndOptionalPawnGenerationRequests.Clear();
		}

		public static Pawn RandomizeInPlace(Pawn p)
		{
			return RegenerateStartingPawnInPlace(StartingAndOptionalPawns.IndexOf(p));
		}

		private static Pawn RegenerateStartingPawnInPlace(int index)
		{
			Pawn pawn = StartingAndOptionalPawns[index];
			PawnUtility.TryDestroyStartingColonistFamily(pawn);
			pawn.relations.ClearAllRelations();
			PawnComponentsUtility.RemoveComponentsOnDespawned(pawn);
			Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
			StartingPossessions.Remove(pawn);
			StartingAndOptionalPawns[index] = null;
			for (int i = 0; i < StartingAndOptionalPawns.Count; i++)
			{
				if (StartingAndOptionalPawns[i] != null)
				{
					PawnUtility.TryDestroyStartingColonistFamily(StartingAndOptionalPawns[i]);
				}
			}
			Pawn pawn2 = NewGeneratedStartingPawn(index);
			StartingAndOptionalPawns[index] = pawn2;
			return pawn2;
		}

		public static PawnGenerationRequest GetGenerationRequest(int index)
		{
			EnsureGenerationRequestInRangeOf(index);
			return StartingAndOptionalPawnGenerationRequests[index];
		}

		public static void SetGenerationRequest(int index, PawnGenerationRequest request)
		{
			EnsureGenerationRequestInRangeOf(index);
			StartingAndOptionalPawnGenerationRequests[index] = request;
		}

		public static void ReorderRequests(int from, int to)
		{
			EnsureGenerationRequestInRangeOf((from > to) ? from : to);
			PawnGenerationRequest generationRequest = GetGenerationRequest(from);
			StartingAndOptionalPawnGenerationRequests.Insert(to, generationRequest);
			StartingAndOptionalPawnGenerationRequests.RemoveAt((from < to) ? from : (from + 1));
		}

		private static void EnsureGenerationRequestInRangeOf(int index)
		{
			while (StartingAndOptionalPawnGenerationRequests.Count <= index)
			{
				StartingAndOptionalPawnGenerationRequests.Add(DefaultStartingPawnRequest);
			}
		}

		public static int PawnIndex(Pawn pawn)
		{
			return Mathf.Max(StartingAndOptionalPawns.IndexOf(pawn), 0);
		}

		public static Pawn NewGeneratedStartingPawn(int index = -1)
		{
			PawnGenerationRequest request = ((index < 0) ? DefaultStartingPawnRequest : GetGenerationRequest(index));
			Pawn pawn = null;
			try
			{
				pawn = PawnGenerator.GeneratePawn(request);
			}
			catch (Exception ex)
			{
				Log.Error("There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: " + ex);
				pawn = PawnGenerator.GeneratePawn(request);
			}
			pawn.relations.everSeenByPlayer = true;
			PawnComponentsUtility.AddComponentsForSpawn(pawn);
			GeneratePossessions(pawn);
			return pawn;
		}

		private static void GeneratePossessions(Pawn pawn)
		{
			if (!StartingPossessions.ContainsKey(pawn))
			{
				StartingPossessions.Add(pawn, new List<ThingDefCount>());
			}
			else
			{
				StartingPossessions[pawn].Clear();
			}
			if (Find.Scenario.AllParts.Any((ScenPart x) => x is ScenPart_NoPossessions))
			{
				return;
			}
			if (ModsConfig.BiotechActive && pawn.DevelopmentalStage.Baby())
			{
				StartingPossessions[pawn].Add(new ThingDefCount(ThingDefOf.BabyFood, BabyFoodCountRange.RandomInRange));
				return;
			}
			foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
			{
				if (StartingPossessions[pawn].Count >= 2)
				{
					return;
				}
				if (hediff is Hediff_Addiction hediff_Addiction)
				{
					Need_Chemical need = hediff_Addiction.Need;
					ThingDef thingDef = GetDrugFor(hediff_Addiction.Chemical);
					if (need != null && thingDef != null)
					{
						int count = GenMath.RoundRandom(need.def.fallPerDay * DaysSatisfied.RandomInRange / thingDef.GetCompProperties<CompProperties_Drug>().needLevelOffset);
						StartingPossessions[pawn].Add(new ThingDefCount(thingDef, count));
					}
				}
			}
			if (ModsConfig.BiotechActive)
			{
				foreach (Hediff hediff2 in pawn.health.hediffSet.hediffs)
				{
					if (StartingPossessions[pawn].Count >= 2)
					{
						return;
					}
					if (hediff2 is Hediff_ChemicalDependency hediff_ChemicalDependency && hediff_ChemicalDependency.Visible)
					{
						ThingDef thingDef2 = GetDrugFor(hediff_ChemicalDependency.chemical);
						if (thingDef2 != null)
						{
							float num = hediff_ChemicalDependency.def.CompProps<HediffCompProperties_SeverityPerDay>()?.severityPerDay ?? 1f;
							StartingPossessions[pawn].Add(new ThingDefCount(thingDef2, GenMath.RoundRandom(DaysSatisfied.RandomInRange * num)));
						}
					}
				}
			}
			if (StartingPossessions[pawn].Count >= 2)
			{
				return;
			}
			if (ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.HasGene(GeneDefOf.Hemogenic))
			{
				StartingPossessions[pawn].Add(new ThingDefCount(ThingDefOf.HemogenPack, HemogenCountRange.RandomInRange));
				if (StartingPossessions[pawn].Count >= 2)
				{
					return;
				}
			}
			if (Rand.Value < 0.25f)
			{
				BackstoryDef backstory = pawn.story.GetBackstory(BackstorySlot.Adulthood);
				if (backstory != null)
				{
					foreach (BackstoryThingDefCountClass possession in backstory.possessions)
					{
						if (StartingPossessions[pawn].Count >= 2)
						{
							return;
						}
						StartingPossessions[pawn].Add(new ThingDefCount(possession.key, Mathf.Min(possession.key.stackLimit, possession.value)));
					}
				}
			}
			if (StartingPossessions[pawn].Count < 2 && Rand.Value < 0.06f && DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.possessionCount > 0).TryRandomElement(out var result))
			{
				StartingPossessions[pawn].Add(new ThingDefCount(result, Mathf.Min(result.stackLimit, result.possessionCount)));
			}
			ThingDef GetDrugFor(ChemicalDef chemical)
			{
				if (DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.GetCompProperties<CompProperties_Drug>()?.chemical == chemical).TryRandomElementByWeight((ThingDef x) => x.generateCommonality, out var result2))
				{
					return result2;
				}
				return null;
			}
		}

		public static void AddNewPawn(int index = -1)
		{
			Pawn pawn = NewGeneratedStartingPawn(index);
			StartingAndOptionalPawns.Add(pawn);
			GeneratePossessions(pawn);
		}

		public static bool WorkTypeRequirementsSatisfied()
		{
			if (StartingAndOptionalPawns.Count == 0)
			{
				return false;
			}
			List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				WorkTypeDef workTypeDef = allDefsListForReading[i];
				if (!workTypeDef.requireCapableColonist)
				{
					continue;
				}
				bool flag = false;
				for (int j = 0; j < Find.GameInitData.startingPawnCount; j++)
				{
					if (!StartingAndOptionalPawns[j].WorkTypeIsDisabled(workTypeDef))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			if (TutorSystem.TutorialMode && StartingAndOptionalPawns.Take(Find.GameInitData.startingPawnCount).Any((Pawn p) => p.WorkTagIsDisabled(WorkTags.Violent)))
			{
				return false;
			}
			return true;
		}

		public static IEnumerable<WorkTypeDef> RequiredWorkTypesDisabledForEveryone()
		{
			List<WorkTypeDef> workTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			for (int i = 0; i < workTypes.Count; i++)
			{
				WorkTypeDef workTypeDef = workTypes[i];
				if (!workTypeDef.requireCapableColonist)
				{
					continue;
				}
				bool flag = false;
				List<Pawn> startingAndOptionalPawns = StartingAndOptionalPawns;
				for (int j = 0; j < Find.GameInitData.startingPawnCount; j++)
				{
					if (!startingAndOptionalPawns[j].WorkTypeIsDisabled(workTypeDef))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					yield return workTypeDef;
				}
			}
		}
	}
}
