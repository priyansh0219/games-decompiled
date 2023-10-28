using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PreceptComp_UnwillingToDo : PreceptComp
	{
		public HistoryEventDef eventDef;

		public List<TraitRequirement> nullifyingTraits;

		public override IEnumerable<TraitRequirement> TraitsAffecting
		{
			get
			{
				if (nullifyingTraits != null)
				{
					for (int i = 0; i < nullifyingTraits.Count; i++)
					{
						yield return nullifyingTraits[i];
					}
				}
			}
		}

		public override bool MemberWillingToDo(HistoryEvent ev)
		{
			if (eventDef != null && ev.def != eventDef)
			{
				return true;
			}
			ev.args.TryGetArg(HistoryEventArgsNames.Doer, out Pawn arg);
			if (nullifyingTraits != null && arg != null && arg.story != null)
			{
				if (!preceptDef.enabledForNPCFactions && !arg.Faction.IsPlayer)
				{
					return true;
				}
				for (int i = 0; i < nullifyingTraits.Count; i++)
				{
					if (nullifyingTraits[i].HasTrait(arg))
					{
						return true;
					}
				}
			}
			return false;
		}

		public virtual string GetProhibitionText()
		{
			return description ?? ((string)eventDef.LabelCap);
		}

		public override IEnumerable<string> GetDescriptions()
		{
			yield return "UnwillingToDoIdeoAction".Translate() + ": " + eventDef.LabelCap;
		}
	}
}
