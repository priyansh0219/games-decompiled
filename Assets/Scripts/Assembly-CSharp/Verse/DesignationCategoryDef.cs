using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse
{
	public class DesignationCategoryDef : Def
	{
		public struct BuildablePreceptBuilding
		{
			private BuildableDef buildable;

			private Precept_Building precept;

			public BuildableDef Buildable => buildable;

			public Precept_Building Precept => precept;

			public BuildablePreceptBuilding(BuildableDef buildable, Precept_Building precept)
			{
				this.buildable = buildable;
				this.precept = precept;
			}
		}

		public List<Type> specialDesignatorClasses = new List<Type>();

		public int order;

		public bool showPowerGrid;

		public List<ResearchProjectDef> researchPrerequisites;

		public int? preferredColumn;

		[Unsaved(false)]
		private List<Designator> resolvedDesignators = new List<Designator>();

		[Unsaved(false)]
		public KeyBindingCategoryDef bindingCatDef;

		[Unsaved(false)]
		public string cachedHighlightClosedTag;

		[Unsaved(false)]
		private Dictionary<BuildablePreceptBuilding, Designator> ideoBuildingDesignatorsCached = new Dictionary<BuildablePreceptBuilding, Designator>();

		[Unsaved(false)]
		private Dictionary<DesignatorDropdownGroupDef, Designator_Dropdown> ideoDropdownsCached = new Dictionary<DesignatorDropdownGroupDef, Designator_Dropdown>();

		[Unsaved(false)]
		private Faction cachedPlayerFaction;

		public IEnumerable<Designator> ResolvedAllowedDesignators
		{
			get
			{
				GameRules rules = Current.Game.Rules;
				for (int i = 0; i < resolvedDesignators.Count; i++)
				{
					Designator designator = resolvedDesignators[i];
					if (rules == null || rules.DesignatorAllowed(designator))
					{
						yield return designator;
					}
				}
				foreach (Designator allIdeoDesignator in AllIdeoDesignators)
				{
					yield return allIdeoDesignator;
				}
			}
		}

		public List<Designator> AllResolvedDesignators => resolvedDesignators;

		public IEnumerable<Designator> AllIdeoDesignators
		{
			get
			{
				if (!ModsConfig.IdeologyActive)
				{
					yield break;
				}
				if (cachedPlayerFaction != Faction.OfPlayer)
				{
					ideoBuildingDesignatorsCached.Clear();
					ideoDropdownsCached.Clear();
					cachedPlayerFaction = Faction.OfPlayer;
				}
				foreach (Ideo ideo in Faction.OfPlayer.ideos.AllIdeos)
				{
					for (int k = 0; k < ideo.PreceptsListForReading.Count; k++)
					{
						Precept precept = ideo.PreceptsListForReading[k];
						bool flag = precept is Precept_Building;
						bool flag2 = precept is Precept_RitualSeat;
						if (flag || flag2)
						{
							Precept_ThingDef precept_ThingDef = (Precept_ThingDef)precept;
							if (precept_ThingDef.ThingDef.designationCategory == this)
							{
								yield return GetCachedDesignator(precept_ThingDef.ThingDef, precept_ThingDef as Precept_Building);
							}
						}
					}
					for (int k = 0; k < ideo.thingStyleCategories.Count; k++)
					{
						ThingStyleCategoryWithPriority thingStyleCategoryWithPriority = ideo.thingStyleCategories[k];
						foreach (Designator item in GetDesignatorsFromStyleCategory(thingStyleCategoryWithPriority.category))
						{
							yield return item;
						}
					}
					for (int k = 0; k < ideo.memes.Count; k++)
					{
						MemeDef meme = ideo.memes[k];
						if (meme.addDesignators != null)
						{
							for (int m = 0; m < meme.addDesignators.Count; m++)
							{
								if (meme.addDesignators[m].designationCategory == this)
								{
									yield return GetCachedDesignator(meme.addDesignators[m], null);
								}
							}
						}
						if (meme.addDesignatorGroups == null)
						{
							continue;
						}
						for (int m = 0; m < meme.addDesignatorGroups.Count; m++)
						{
							Designator_Dropdown designator_Dropdown = GetCachedDropdown(meme.addDesignatorGroups[m]);
							if (designator_Dropdown != null)
							{
								yield return designator_Dropdown;
							}
						}
					}
				}
				if (!Find.IdeoManager.classicMode)
				{
					yield break;
				}
				foreach (StyleCategoryDef selectedStyleCategory in Find.IdeoManager.selectedStyleCategories)
				{
					foreach (Designator item2 in GetDesignatorsFromStyleCategory(selectedStyleCategory))
					{
						yield return item2;
					}
				}
				Designator GetCachedDesignator(BuildableDef def, Precept_Building buildingPrecept)
				{
					BuildablePreceptBuilding key = new BuildablePreceptBuilding(def, buildingPrecept);
					if (!ideoBuildingDesignatorsCached.TryGetValue(key, out var value2))
					{
						Designator_Build designator_Build = new Designator_Build(def);
						value2 = designator_Build;
						if (buildingPrecept != null)
						{
							designator_Build.sourcePrecept = buildingPrecept;
						}
						ideoBuildingDesignatorsCached[key] = value2;
					}
					return value2;
				}
				Designator_Dropdown GetCachedDropdown(DesignatorDropdownGroupDef group)
				{
					if (!ideoDropdownsCached.TryGetValue(group, out var value))
					{
						IEnumerable<BuildableDef> enumerable = from tDef in DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>().Concat(DefDatabase<ThingDef>.AllDefs.Cast<BuildableDef>())
							where tDef.designationCategory == this && !tDef.canGenerateDefaultDesignator && tDef.designatorDropdown == @group
							select tDef;
						if (!enumerable.Any())
						{
							ideoDropdownsCached[group] = null;
							return ideoDropdownsCached[group];
						}
						foreach (BuildableDef item3 in enumerable)
						{
							if (!ideoDropdownsCached.ContainsKey(item3.designatorDropdown))
							{
								ideoDropdownsCached[item3.designatorDropdown] = new Designator_Dropdown();
								ideoDropdownsCached[item3.designatorDropdown].Order = item3.uiOrder;
							}
							ideoDropdownsCached[item3.designatorDropdown].Add(new Designator_Build(item3));
						}
						return ideoDropdownsCached[group];
					}
					return value;
				}
				IEnumerable<Designator> GetDesignatorsFromStyleCategory(StyleCategoryDef categoryDef)
				{
					if (categoryDef.addDesignators != null)
					{
						for (int n = 0; n < categoryDef.addDesignators.Count; n++)
						{
							if (categoryDef.addDesignators[n].designationCategory == this)
							{
								yield return GetCachedDesignator(categoryDef.addDesignators[n], null);
							}
						}
					}
					if (categoryDef.addDesignatorGroups != null)
					{
						for (int n = 0; n < categoryDef.addDesignatorGroups.Count; n++)
						{
							Designator_Dropdown cachedDropdown = GetCachedDropdown(categoryDef.addDesignatorGroups[n]);
							if (cachedDropdown != null)
							{
								yield return cachedDropdown;
							}
						}
					}
				}
			}
		}

		public IEnumerable<Designator> AllResolvedAndIdeoDesignators
		{
			get
			{
				foreach (Designator resolvedDesignator in resolvedDesignators)
				{
					yield return resolvedDesignator;
				}
				foreach (Designator allIdeoDesignator in AllIdeoDesignators)
				{
					yield return allIdeoDesignator;
				}
			}
		}

		public bool Visible
		{
			get
			{
				if (DebugSettings.godMode)
				{
					return true;
				}
				if (researchPrerequisites == null)
				{
					return true;
				}
				foreach (ResearchProjectDef researchPrerequisite in researchPrerequisites)
				{
					if (!researchPrerequisite.IsFinished)
					{
						return false;
					}
				}
				return true;
			}
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				ResolveDesignators();
			});
			cachedHighlightClosedTag = "DesignationCategoryButton-" + defName + "-Closed";
		}

		private void ResolveDesignators()
		{
			resolvedDesignators.Clear();
			foreach (Type specialDesignatorClass in specialDesignatorClasses)
			{
				Designator designator = null;
				try
				{
					designator = (Designator)Activator.CreateInstance(specialDesignatorClass);
					designator.isOrder = true;
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat("DesignationCategoryDef", defName, " could not instantiate special designator from class ", specialDesignatorClass, ".\n Exception: \n", ex.ToString()));
				}
				if (designator != null)
				{
					resolvedDesignators.Add(designator);
				}
			}
			IEnumerable<BuildableDef> enumerable = from tDef in DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>().Concat(DefDatabase<ThingDef>.AllDefs.Cast<BuildableDef>())
				where tDef.designationCategory == this && tDef.canGenerateDefaultDesignator
				select tDef;
			Dictionary<DesignatorDropdownGroupDef, Designator_Dropdown> dictionary = new Dictionary<DesignatorDropdownGroupDef, Designator_Dropdown>();
			foreach (BuildableDef item in enumerable)
			{
				if (item.designatorDropdown != null)
				{
					if (!dictionary.ContainsKey(item.designatorDropdown))
					{
						dictionary[item.designatorDropdown] = new Designator_Dropdown();
						dictionary[item.designatorDropdown].Order = item.uiOrder;
						resolvedDesignators.Add(dictionary[item.designatorDropdown]);
					}
					dictionary[item.designatorDropdown].Add(new Designator_Build(item));
				}
				else
				{
					resolvedDesignators.Add(new Designator_Build(item));
				}
			}
		}

		public void DirtyCache()
		{
			ideoBuildingDesignatorsCached.Clear();
			ideoDropdownsCached.Clear();
		}

		private static bool RequirementSatisfied(string requirement)
		{
			if (!ModLister.HasActiveModWithName(requirement))
			{
				return DefDatabase<ResearchProjectDef>.GetNamedSilentFail(requirement)?.IsFinished ?? false;
			}
			return true;
		}
	}
}
