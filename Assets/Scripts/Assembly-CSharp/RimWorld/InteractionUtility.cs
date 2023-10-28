using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class InteractionUtility
	{
		public const float MaxInteractRange = 6f;

		public static bool CanInitiateInteraction(Pawn pawn, InteractionDef interactionDef = null)
		{
			if (pawn.interactions == null)
			{
				return false;
			}
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
			{
				return false;
			}
			if (!pawn.Awake())
			{
				return false;
			}
			if (pawn.IsBurning())
			{
				return false;
			}
			if (pawn.IsInteractionBlocked(interactionDef, isInitiator: true, isRandom: false))
			{
				return false;
			}
			return true;
		}

		public static bool CanReceiveInteraction(Pawn pawn, InteractionDef interactionDef = null)
		{
			if (!pawn.Awake())
			{
				return false;
			}
			if (pawn.IsBurning())
			{
				return false;
			}
			if (pawn.IsInteractionBlocked(interactionDef, isInitiator: false, isRandom: false))
			{
				return false;
			}
			return true;
		}

		public static bool CanInitiateRandomInteraction(Pawn p)
		{
			if (!CanInitiateInteraction(p))
			{
				return false;
			}
			if (!p.RaceProps.Humanlike || p.Downed || p.InAggroMentalState || p.IsInteractionBlocked(null, isInitiator: true, isRandom: true))
			{
				return false;
			}
			if (p.Faction == null)
			{
				return false;
			}
			if (!p.ageTracker.CurLifeStage.canInitiateSocialInteraction)
			{
				return false;
			}
			return true;
		}

		public static bool CanReceiveRandomInteraction(Pawn p)
		{
			if (!CanReceiveInteraction(p))
			{
				return false;
			}
			if (!p.RaceProps.Humanlike || p.Downed || p.InAggroMentalState)
			{
				return false;
			}
			return true;
		}

		public static bool IsGoodPositionForInteraction(Pawn p, Pawn recipient)
		{
			return IsGoodPositionForInteraction(p.Position, recipient.Position, p.Map);
		}

		public static bool IsGoodPositionForInteraction(IntVec3 cell, IntVec3 recipientCell, Map map)
		{
			if (cell.InHorDistOf(recipientCell, 6f))
			{
				return GenSight.LineOfSight(cell, recipientCell, map, skipFirstCell: true);
			}
			return false;
		}

		public static bool HasAnyVerbForSocialFight(Pawn p)
		{
			if (p.Dead)
			{
				return false;
			}
			List<Verb> allVerbs = p.verbTracker.AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				if (allVerbs[i].IsMeleeAttack && allVerbs[i].IsStillUsableBy(p))
				{
					return true;
				}
			}
			return false;
		}

		public static bool TryGetRandomVerbForSocialFight(Pawn p, out Verb verb)
		{
			if (p.Dead)
			{
				verb = null;
				return false;
			}
			return p.verbTracker.AllVerbs.Where((Verb x) => x.IsMeleeAttack && x.IsStillUsableBy(p)).TryRandomElementByWeight((Verb x) => x.verbProps.AdjustedMeleeDamageAmount(x, p) * ((x.tool != null) ? x.tool.chanceFactor : 1f), out verb);
		}

		public static void ImitateSocialInteractionWithManyPawns(Pawn initiator, List<Pawn> targets, InteractionDef intDef)
		{
			List<Pawn> list = targets.Except(initiator).ToList();
			if (targets.NullOrEmpty())
			{
				Log.Error(string.Concat(initiator, " tried to do interaction ", intDef, " with no targets. "));
				return;
			}
			if (intDef.initiatorXpGainSkill != null)
			{
				initiator.skills.Learn(intDef.initiatorXpGainSkill, intDef.initiatorXpGainAmount);
			}
			foreach (Pawn item in list)
			{
				if (initiator != item && initiator.interactions.CanInteractNowWith(item, intDef))
				{
					if (intDef.recipientThought != null && item.needs.mood != null)
					{
						Pawn_InteractionsTracker.AddInteractionThought(item, initiator, intDef.recipientThought);
					}
					if (intDef.recipientXpGainSkill != null && item.RaceProps.Humanlike)
					{
						item.skills.Learn(intDef.recipientXpGainSkill, intDef.recipientXpGainAmount);
					}
				}
			}
			MoteMaker.MakeInteractionBubble(initiator, list.RandomElement(), intDef.interactionMote, intDef.GetSymbol(initiator.Faction, initiator.Ideo), intDef.GetSymbolColor(initiator.Faction));
			PlayLogEntry_InteractionWithMany entry = new PlayLogEntry_InteractionWithMany(intDef, initiator, list, null);
			Find.PlayLog.Add(entry);
		}
	}
}
