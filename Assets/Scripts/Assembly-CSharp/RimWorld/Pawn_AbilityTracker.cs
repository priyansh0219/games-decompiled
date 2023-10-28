using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Pawn_AbilityTracker : IExposable
	{
		public Pawn pawn;

		public List<Ability> abilities = new List<Ability>();

		private bool allAbilitiesCachedDirty = true;

		private List<Ability> allAbilitiesCached = new List<Ability>();

		private List<Ability> tmpAbilities = new List<Ability>();

		public List<Ability> AllAbilitiesForReading
		{
			get
			{
				if (allAbilitiesCachedDirty)
				{
					allAbilitiesCached.Clear();
					allAbilitiesCached.AddRange(abilities);
					if (pawn.royalty != null)
					{
						allAbilitiesCached.AddRange(pawn.royalty.AllAbilitiesForReading);
					}
					if (ModsConfig.IdeologyActive)
					{
						Precept_Role precept_Role = pawn.Ideo?.GetRole(pawn);
						if (precept_Role != null && precept_Role.Active && !precept_Role.AbilitiesFor(pawn).NullOrEmpty())
						{
							foreach (Ability item in precept_Role.AbilitiesFor(pawn))
							{
								bool flag = false;
								if (!item.def.requiredMemes.NullOrEmpty())
								{
									foreach (MemeDef requiredMeme in item.def.requiredMemes)
									{
										if (!pawn.Ideo.memes.Contains(requiredMeme))
										{
											flag = true;
											break;
										}
									}
								}
								if (!flag)
								{
									allAbilitiesCached.Add(item);
								}
							}
						}
					}
					allAbilitiesCached.SortBy((Ability a) => a.def.category?.displayOrder ?? 0, (Ability a) => a.def.displayOrder, (Ability a) => a.def.level);
					allAbilitiesCachedDirty = false;
				}
				return allAbilitiesCached;
			}
		}

		public Pawn_AbilityTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void AbilitiesTick()
		{
			for (int i = 0; i < AllAbilitiesForReading.Count; i++)
			{
				AllAbilitiesForReading[i].AbilityTick();
			}
		}

		public void GainAbility(AbilityDef def)
		{
			if (!abilities.Any((Ability a) => a.def == def))
			{
				abilities.Add(AbilityUtility.MakeAbility(def, pawn));
			}
			Notify_TemporaryAbilitiesChanged();
		}

		public void RemoveAbility(AbilityDef def)
		{
			Ability ability = abilities.FirstOrDefault((Ability x) => x.def == def);
			if (ability != null)
			{
				abilities.Remove(ability);
			}
			Notify_TemporaryAbilitiesChanged();
		}

		public Ability GetAbility(AbilityDef def, bool includeTemporary = false)
		{
			List<Ability> list = (includeTemporary ? AllAbilitiesForReading : abilities);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def == def)
				{
					return list[i];
				}
			}
			return null;
		}

		public List<Ability> CastableOffensiveAbilities(LocalTargetInfo target)
		{
			tmpAbilities.Clear();
			foreach (Ability item in AllAbilitiesForReading)
			{
				if (item.AICanTargetNow(target))
				{
					tmpAbilities.Add(item);
				}
			}
			return tmpAbilities;
		}

		public IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Ability a in AllAbilitiesForReading)
			{
				if ((pawn.IsColonistPlayerControlled || pawn.IsColonyMechPlayerControlled || DebugSettings.ShowDevGizmos) && (pawn.Drafted || a.def.displayGizmoWhileUndrafted || (!pawn.IsColonistPlayerControlled && DebugSettings.ShowDevGizmos)) && a.GizmosVisible())
				{
					foreach (Command gizmo in a.GetGizmos())
					{
						yield return gizmo;
					}
				}
				foreach (Gizmo item in a.GetGizmosExtra())
				{
					yield return item;
				}
			}
		}

		public void Notify_TemporaryAbilitiesChanged()
		{
			allAbilitiesCachedDirty = true;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref abilities, "abilities", LookMode.Deep, pawn);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				abilities.RemoveAll((Ability a) => a.def == null || a.def == AbilityDefOf.Speech);
			}
		}
	}
}
