using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class RitualRoleAssignments : IExposable
	{
		public enum FailReason
		{
			MaxPawnsAlreadyAssigned = 0,
			Undefined = 1
		}

		private List<Pawn> allPawns;

		private List<RitualRole> allRoles;

		private Dictionary<string, Pawn> forcedRoles;

		private Dictionary<string, SerializablePawnList> assignedRoles = new Dictionary<string, SerializablePawnList>();

		private Precept_Role roleChangeSelection;

		private List<Pawn> spectators = new List<Pawn>();

		private List<Pawn> requiredPawns;

		private Precept_Ritual ritual;

		private Pawn selectedPawn;

		private List<Pawn> tmpParticipants = new List<Pawn>();

		private List<Pawn> tmpForcedRolePawns;

		private List<string> tmpForcedRoleIds;

		private List<string> tmpAssignedRoleIds;

		private List<SerializablePawnList> tmpAssignedRolePawns;

		private static List<Pawn> tmpOrderedPawns = new List<Pawn>(32);

		public List<Pawn> SpectatorsForReading => spectators;

		public Precept_Ritual Ritual => ritual;

		public List<Pawn> Participants
		{
			get
			{
				tmpParticipants.Clear();
				foreach (Pawn allPawn in allPawns)
				{
					if (PawnParticipating(allPawn))
					{
						tmpParticipants.Add(allPawn);
					}
				}
				return tmpParticipants;
			}
		}

		public List<Pawn> AllPawns => allPawns;

		public List<RitualRole> AllRolesForReading => allRoles;

		public List<Pawn> ExtraRequiredPawnsForReading => requiredPawns;

		public Dictionary<string, Pawn> ForcedRolesForReading => forcedRoles;

		public Precept_Role RoleChangeSelection => roleChangeSelection;

		public void ExposeData()
		{
			Scribe_Collections.Look(ref allPawns, "allPawns", LookMode.Reference);
			Scribe_Collections.Look(ref spectators, "spectators", LookMode.Reference);
			Scribe_Collections.Look(ref requiredPawns, "requiredPawns", LookMode.Reference);
			Scribe_Collections.Look(ref forcedRoles, "forcedRoles", LookMode.Value, LookMode.Reference, ref tmpForcedRoleIds, ref tmpForcedRolePawns);
			Scribe_Collections.Look(ref assignedRoles, "assignedRoles", LookMode.Value, LookMode.Deep, ref tmpAssignedRoleIds, ref tmpAssignedRolePawns);
			Scribe_References.Look(ref ritual, "ritual");
			Scribe_References.Look(ref selectedPawn, "selectedPawn");
			Scribe_References.Look(ref roleChangeSelection, "roleChangeSelection");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				allRoles = ((ritual != null) ? new List<RitualRole>(ritual.behavior.def.roles) : new List<RitualRole>());
			}
		}

		public RitualRoleAssignments()
		{
		}

		public RitualRoleAssignments(Precept_Ritual ritual)
		{
			this.ritual = ritual;
		}

		public void Setup(List<Pawn> allPawns, Dictionary<string, Pawn> forcedRoles = null, List<Pawn> requiredPawns = null, Pawn selectedPawn = null)
		{
			this.allPawns = allPawns;
			this.forcedRoles = forcedRoles;
			allRoles = ((ritual != null) ? new List<RitualRole>(ritual.behavior.def.roles) : new List<RitualRole>());
			this.selectedPawn = selectedPawn;
			this.requiredPawns = new List<Pawn>();
			if (forcedRoles != null)
			{
				this.requiredPawns.AddRange(forcedRoles.Values);
			}
			if (requiredPawns != null)
			{
				this.requiredPawns.AddRange(requiredPawns);
			}
			allPawns.SortBy((Pawn p) => p.Faction == null || !p.Faction.IsPlayer, (Pawn p) => !Faction.OfPlayer.ideos.Has(p.Ideo), (Pawn p) => !p.IsFreeNonSlaveColonist);
		}

		public bool Forced(Pawn pawn)
		{
			if (forcedRoles != null)
			{
				return forcedRoles.ContainsValue(pawn);
			}
			return false;
		}

		public string ForcedRole(Pawn pawn)
		{
			if (forcedRoles == null)
			{
				return null;
			}
			foreach (KeyValuePair<string, Pawn> forcedRole in forcedRoles)
			{
				if (forcedRole.Value == pawn)
				{
					return forcedRole.Key;
				}
			}
			return null;
		}

		public void RemoveParticipant(Pawn pawn)
		{
			TryUnassignAnyRole(pawn);
			spectators.Remove(pawn);
			allPawns.Remove(pawn);
			allPawns.Add(pawn);
		}

		public bool TryUnassignAnyRole(Pawn pawn)
		{
			foreach (KeyValuePair<string, SerializablePawnList> assignedRole in assignedRoles)
			{
				if (assignedRole.Value.Pawns.Remove(pawn))
				{
					if (CanEverSpectate(pawn))
					{
						spectators.Add(pawn);
					}
					return true;
				}
			}
			return false;
		}

		public bool TryAssign(Pawn pawn, RitualRole role, TargetInfo ritualTarget, out FailReason failReason, Pawn insertAfter = null, Pawn insertBefore = null)
		{
			failReason = FailReason.Undefined;
			if (forcedRoles != null && forcedRoles.ContainsValue(pawn))
			{
				return false;
			}
			if (PawnNotAssignableReason(pawn, role, ritualTarget) != null)
			{
				return false;
			}
			if (role == null)
			{
				return TryAssignSpectate(pawn);
			}
			if (role.AppliesToPawn(pawn, out var _, ritualTarget, null, this, null, skipReason: true))
			{
				if (role.maxCount <= 0 || AssignedPawns(role).Count() < role.maxCount)
				{
					TryUnassignAnyRole(pawn);
					spectators.Remove(pawn);
					if (!assignedRoles.TryGetValue(role.id, out var value))
					{
						value = new SerializablePawnList(new List<Pawn>());
						assignedRoles.Add(role.id, value);
					}
					if (insertAfter != null && value.Pawns.Contains(insertAfter))
					{
						value.Pawns.Insert(value.Pawns.IndexOf(insertAfter) + 1, pawn);
					}
					else if (insertBefore != null && value.Pawns.Contains(insertBefore))
					{
						value.Pawns.Insert(value.Pawns.IndexOf(insertBefore), pawn);
					}
					else
					{
						value.Pawns.Add(pawn);
					}
					return true;
				}
				failReason = FailReason.MaxPawnsAlreadyAssigned;
			}
			return false;
		}

		public bool TryAssign(Pawn pawn, string roleId, TargetInfo ritualTarget, out FailReason failReason, Pawn insertAfter = null, Pawn insertBefore = null)
		{
			return TryAssign(pawn, GetRole(roleId), ritualTarget, out failReason, insertAfter, insertBefore);
		}

		public bool TryAssignSpectate(Pawn pawn, Pawn insertBefore = null)
		{
			if (spectators.Contains(pawn) || !CanEverSpectate(pawn) || PawnNotAssignableReason(pawn, null, TargetInfo.Invalid) != null)
			{
				return false;
			}
			TryUnassignAnyRole(pawn);
			if (!spectators.Contains(pawn))
			{
				if (insertBefore != null && spectators.Contains(insertBefore))
				{
					spectators.Insert(spectators.IndexOf(insertBefore), pawn);
				}
				else
				{
					spectators.Add(pawn);
				}
			}
			return RoleForPawn(pawn) == null;
		}

		public RitualRole GetRole(string roleId)
		{
			if (!AllRolesForReading.NullOrEmpty())
			{
				foreach (RitualRole item in AllRolesForReading)
				{
					if (item.id == roleId)
					{
						return item;
					}
				}
			}
			return null;
		}

		public bool CanParticipate(Pawn pawn, TargetInfo ritualTarget)
		{
			if (forcedRoles != null && forcedRoles.ContainsValue(pawn))
			{
				return true;
			}
			if (Required(pawn))
			{
				return true;
			}
			if (pawn == selectedPawn)
			{
				return true;
			}
			foreach (RitualRole item in AllRolesForReading)
			{
				if (PawnNotAssignableReason(pawn, item, ritualTarget) == null)
				{
					return true;
				}
			}
			return CanEverSpectate(pawn);
		}

		public Pawn FirstAssignedPawn(RitualRole role)
		{
			return FirstAssignedPawn(role.id);
		}

		public Pawn FirstAssignedPawn(string roleId)
		{
			if (forcedRoles != null && forcedRoles.TryGetValue(roleId, out var value))
			{
				return value;
			}
			if (assignedRoles.TryGetValue(roleId, out var value2) && value2.Pawns.Count > 0)
			{
				return value2.Pawns[0];
			}
			return null;
		}

		public bool CanEverSpectate(Pawn pawn)
		{
			if (ritual != null && ritual.ritualOnlyForIdeoMembers && pawn.Ideo != ritual.ideo && !ritual.def.allowSpectatorsFromOtherIdeos)
			{
				return false;
			}
			if (ritual != null && ritual.behavior.def.spectatorFilter != null && !ritual.behavior.def.spectatorFilter.Allowed(pawn))
			{
				return false;
			}
			if (pawn.RaceProps.Humanlike && !pawn.IsPrisoner)
			{
				return GatheringsUtility.ShouldPawnKeepAttendingRitual(pawn, ritual);
			}
			return false;
		}

		public IEnumerable<Pawn> SpectatorCandidates()
		{
			foreach (Pawn allPawn in allPawns)
			{
				if (CanEverSpectate(allPawn) && RoleForPawn(allPawn) == null)
				{
					Precept_Ritual precept_Ritual = ritual;
					if (precept_Ritual == null || precept_Ritual.behavior?.ShouldInitAsSpectator(allPawn, this) != false)
					{
						yield return allPawn;
					}
				}
			}
		}

		public IEnumerable<Pawn> CandidatesForRole(string roleId, TargetInfo ritualTarget, bool includeAssigned = false, bool includeAssignedForSameRole = false, bool includeForced = true)
		{
			return CandidatesForRole(GetRole(roleId), ritualTarget, includeAssigned, includeAssignedForSameRole, includeForced);
		}

		public IEnumerable<Pawn> CandidatesForRole(RitualRole role, TargetInfo ritualTarget, bool includeAssigned = false, bool includeAssignedForSameRole = false, bool includeForced = true)
		{
			if (forcedRoles != null && forcedRoles.TryGetValue(role.id, out var value))
			{
				yield return value;
				yield break;
			}
			foreach (Pawn allPawn in allPawns)
			{
				if (role.AppliesToPawn(allPawn, out var _, ritualTarget, null, this, null, skipReason: true) && ShouldIncludePawn(allPawn))
				{
					yield return allPawn;
				}
			}
			bool ShouldIncludePawn(Pawn pawn)
			{
				if (includeAssigned || (includeAssignedForSameRole && RoleForPawn(pawn) == role) || RoleForPawn(pawn) == null)
				{
					return GatheringsUtility.ShouldPawnKeepAttendingRitual(pawn, ritual, role != null && role.ignoreBleeding);
				}
				return false;
			}
		}

		public IEnumerable<Pawn> AssignedPawns(RitualRole role)
		{
			if (forcedRoles != null)
			{
				foreach (KeyValuePair<string, Pawn> forcedRole in forcedRoles)
				{
					if (forcedRole.Key == role.id)
					{
						yield return forcedRole.Value;
					}
				}
			}
			if (!assignedRoles.TryGetValue(role.id, out var value))
			{
				yield break;
			}
			foreach (Pawn pawn in value.Pawns)
			{
				yield return pawn;
			}
		}

		public bool AnyPawnAssigned(string roleId)
		{
			if (forcedRoles == null || !forcedRoles.ContainsKey(roleId))
			{
				return assignedRoles.ContainsKey(roleId);
			}
			return true;
		}

		public bool AnyPawnAssigned(RitualRole role)
		{
			return AnyPawnAssigned(role.id);
		}

		public IEnumerable<Pawn> AssignedPawns(string roleId)
		{
			return AssignedPawns(GetRole(roleId));
		}

		public RitualRole RoleForPawn(Pawn pawn, bool includeForced = true)
		{
			if (spectators.Contains(pawn))
			{
				return null;
			}
			if (includeForced && forcedRoles != null)
			{
				foreach (KeyValuePair<string, Pawn> forcedRole in forcedRoles)
				{
					if (forcedRole.Value == pawn)
					{
						return GetRole(forcedRole.Key);
					}
				}
			}
			foreach (KeyValuePair<string, SerializablePawnList> assignedRole in assignedRoles)
			{
				if (!assignedRole.Value.Pawns.NullOrEmpty() && assignedRole.Value.Pawns.Contains(pawn))
				{
					return GetRole(assignedRole.Key);
				}
			}
			return null;
		}

		public bool PawnParticipating(Pawn pawn)
		{
			if (RoleForPawn(pawn) == null)
			{
				return PawnSpectating(pawn);
			}
			return true;
		}

		public bool PawnSpectating(Pawn pawn)
		{
			return spectators.Contains(pawn);
		}

		public void FillPawns(Func<Pawn, bool, bool, bool> filter, TargetInfo ritualTarget)
		{
			if (!requiredPawns.NullOrEmpty())
			{
				foreach (Pawn requiredPawn in requiredPawns)
				{
					if (forcedRoles == null || !forcedRoles.ContainsValue(requiredPawn))
					{
						TryAssignSpectate(requiredPawn);
					}
				}
			}
			string reason;
			FailReason failReason;
			if (selectedPawn != null && RoleForPawn(selectedPawn) == null)
			{
				foreach (RitualRole item in AllRolesForReading)
				{
					if (item.defaultForSelectedColonist && item.AppliesToPawn(selectedPawn, out reason, ritualTarget, null, this))
					{
						TryAssign(selectedPawn, item, ritualTarget, out failReason);
						break;
					}
				}
			}
			foreach (RitualRole role in AllRolesForReading)
			{
				tmpOrderedPawns.Clear();
				tmpOrderedPawns.AddRange(allPawns.Where((Pawn pawn) => filter == null || filter(pawn, !(role is RitualRoleForced), role.allowOtherIdeos)));
				role.OrderByDesirability(tmpOrderedPawns);
				foreach (Pawn tmpOrderedPawn in tmpOrderedPawns)
				{
					if (RoleForPawn(tmpOrderedPawn) == null)
					{
						if (role.maxCount > 0 && AssignedPawns(role).Count() >= role.maxCount)
						{
							break;
						}
						if (role.AppliesToPawn(tmpOrderedPawn, out reason, ritualTarget, null, this, null, skipReason: true))
						{
							TryAssign(tmpOrderedPawn, role, ritualTarget, out failReason);
						}
					}
				}
			}
			foreach (Pawn item2 in SpectatorCandidates())
			{
				TryAssignSpectate(item2);
			}
			List<Pawn> pawnsToRemove = new List<Pawn>();
			foreach (Pawn allPawn in allPawns)
			{
				RitualRole ritualRole = RoleForPawn(allPawn);
				if (ritualRole != null && ritualRole.required && ritualRole.substitutable && PawnNotAssignableReason(allPawn, ritualRole, ritualTarget) != null)
				{
					RemoveParticipant(allPawn);
					pawnsToRemove.Add(allPawn);
				}
			}
			allPawns.RemoveAll((Pawn p) => pawnsToRemove.Contains(p));
		}

		public bool Required(Pawn pawn)
		{
			if (!requiredPawns.NullOrEmpty())
			{
				return requiredPawns.Contains(pawn);
			}
			return false;
		}

		public bool RoleSubstituted(string roleId)
		{
			if (ritual.behavior.def.roles.NullOrEmpty())
			{
				return false;
			}
			RitualRole role = ritual.behavior.def.roles.FirstOrDefault((RitualRole r) => r.id == roleId);
			if (role == null)
			{
				return false;
			}
			if (!role.substitutable || role.precept == null)
			{
				return false;
			}
			Precept precept = ritual.ideo.PreceptsListForReading.FirstOrDefault((Precept p) => p.def == role.precept);
			if (precept == null)
			{
				return false;
			}
			bool result = false;
			foreach (Pawn item in AssignedPawns(roleId))
			{
				if (item.Ideo?.GetRole(item) != precept)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public string PawnNotAssignableReason(Pawn p, RitualRole role, TargetInfo ritualTarget)
		{
			bool stillAddToPawnList;
			return PawnNotAssignableReason(p, role, ritual, this, ritualTarget, out stillAddToPawnList);
		}

		public static string PawnNotAssignableReason(Pawn p, RitualRole role, Precept_Ritual ritual, RitualRoleAssignments assignments, TargetInfo ritualTarget, out bool stillAddToPawnList)
		{
			stillAddToPawnList = false;
			if (p.Downed && (role == null || !role.allowDowned))
			{
				return "MessageRitualPawnDowned".Translate(p);
			}
			if (p.health.hediffSet.BleedRateTotal > 0f && (role == null || !role.ignoreBleeding))
			{
				return "MessageRitualPawnInjured".Translate(p);
			}
			if (p.InAggroMentalState || (role != null && !role.allowNonAggroMentalState && p.InMentalState))
			{
				return "MessageRitualPawnMentalState".Translate(p);
			}
			if (p.IsPrisoner && role == null)
			{
				return "MessageRitualRoleMustNotBePrisonerToSpectate".Translate(ritual?.behavior?.def.spectatorGerund ?? ((string)"Spectate".Translate()));
			}
			if (ModsConfig.BiotechActive && role != null && !role.allowBaby && (p.DevelopmentalStage == DevelopmentalStage.Baby || p.DevelopmentalStage == DevelopmentalStage.Newborn))
			{
				return "MessageRitualRoleCannotBeBaby".Translate(role.LabelCap);
			}
			if (p.IsPrisoner)
			{
				if (p.guest.Released)
				{
					stillAddToPawnList = true;
					return "MessageRitualPawnReleased".Translate(p);
				}
				if (!p.guest.PrisonerIsSecure)
				{
					stillAddToPawnList = true;
					return "MessageRitualPawnPrisonerNotSecured".Translate(p);
				}
			}
			else if (ritualTarget.IsValid && !p.SafeTemperatureRange().IncludesEpsilon(ritualTarget.Cell.GetTemperature(ritualTarget.Map)))
			{
				return "MessageRitualWontAttendExtremeTemperature".Translate(p);
			}
			if (p.IsSlave)
			{
				if (p.guest.Released)
				{
					stillAddToPawnList = true;
					return "MessageRitualPawnReleased".Translate(p);
				}
				if (!p.guest.SlaveIsSecure)
				{
					stillAddToPawnList = true;
					return "MessageRitualPawnSlaveNotSecured".Translate(p);
				}
			}
			if (role == null && !p.RaceProps.Humanlike)
			{
				return "MessageRitualRoleMustBeHumanlike".Translate("Spectators".Translate());
			}
			if (role == null && ritual != null && !ritual.def.allowSpectatorsFromOtherIdeos && ritual.ritualOnlyForIdeoMembers && p.Ideo != ritual.ideo)
			{
				return "MessageRitualRoleMustHaveIdeoToSpectate".Translate(ritual.ideo.MemberNamePlural, ritual?.behavior?.def.spectatorGerund ?? ((string)"Spectate".Translate()));
			}
			if (role != null && !role.AppliesToPawn(p, out var reason, ritualTarget, null, assignments))
			{
				return reason;
			}
			return null;
		}

		public void SetRoleChangeSelection(Precept_Role role)
		{
			roleChangeSelection = role;
		}
	}
}
