using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public sealed class StudyManager : IExposable
	{
		private Dictionary<ThingDef, float> studyProgress = new Dictionary<ThingDef, float>();

		public void SetStudied(ThingDef thingDef, float amount)
		{
			if (!thingDef.IsStudiable)
			{
				Log.Error("Tried to study " + thingDef.label + " which is not studiable.");
				return;
			}
			if (!studyProgress.ContainsKey(thingDef))
			{
				studyProgress.Add(thingDef, 0f);
			}
			studyProgress[thingDef] += amount / (float)thingDef.GetCompProperties<CompProperties_Studiable>().cost;
			studyProgress[thingDef] = Mathf.Clamp01(studyProgress[thingDef]);
		}

		public void ForceSetStudiedProgress(ThingDef thingDef, float progress)
		{
			if (!thingDef.IsStudiable)
			{
				Log.Error("Tried to study " + thingDef.label + " which is not studiable.");
				return;
			}
			if (!studyProgress.ContainsKey(thingDef))
			{
				studyProgress.Add(thingDef, 0f);
			}
			studyProgress[thingDef] = Mathf.Clamp01(progress);
		}

		public bool StudyComplete(ThingDef thingDef)
		{
			if (thingDef.GetCompProperties<CompProperties_Studiable>() == null)
			{
				return false;
			}
			if (studyProgress.ContainsKey(thingDef))
			{
				return studyProgress[thingDef] >= 1f;
			}
			return false;
		}

		public float GetStudyProgress(ThingDef thingDef)
		{
			if (!thingDef.IsStudiable)
			{
				Log.Error("Tried to get study progress for " + thingDef.label + " which is not studiable.");
				return 0f;
			}
			if (!studyProgress.ContainsKey(thingDef))
			{
				return 0f;
			}
			return studyProgress[thingDef];
		}

		public IEnumerable<ResearchProjectDef> GetAllUnlockedResearch(ThingDef thingDef)
		{
			if (!thingDef.IsStudiable)
			{
				yield break;
			}
			foreach (ResearchProjectDef allDef in DefDatabase<ResearchProjectDef>.AllDefs)
			{
				if (!allDef.requiredStudied.NullOrEmpty() && allDef.requiredStudied.Contains(thingDef))
				{
					yield return allDef;
				}
			}
		}

		public void ResetAllProgress()
		{
			studyProgress.Clear();
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref studyProgress, "studyProgress", LookMode.Def, LookMode.Value);
		}
	}
}
