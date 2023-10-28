using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Bill_ResurrectMech : Bill_Mech
	{
		public override float BandwidthCost
		{
			get
			{
				if (base.Gestator.ActiveBill == this)
				{
					Pawn gestatingMech = base.Gestator.GestatingMech;
					if (gestatingMech != null)
					{
						return gestatingMech.GetStatValue(StatDefOf.BandwidthCost);
					}
					Corpse resurrectingMechCorpse = base.Gestator.ResurrectingMechCorpse;
					if (resurrectingMechCorpse != null)
					{
						return resurrectingMechCorpse.InnerPawn.GetStatValue(StatDefOf.BandwidthCost);
					}
				}
				return 0f;
			}
		}

		public Bill_ResurrectMech()
		{
		}

		public Bill_ResurrectMech(RecipeDef recipe, Precept_ThingStyle precept = null)
			: base(recipe, precept)
		{
		}

		public override bool PawnAllowedToStartAnew(Pawn p)
		{
			if (!base.PawnAllowedToStartAnew(p))
			{
				return false;
			}
			if (base.State == FormingCycleState.Gathering && base.Gestator.ResurrectingMechCorpse == null)
			{
				IEnumerable<Corpse> source = AllAvailableResurrectionCorpses(p);
				if (source.Any() && !source.Any((Corpse c) => p.mechanitor.HasBandwidthToResurrect(c)))
				{
					JobFailReason.Is("NotEnoughBandwidth".Translate());
					return false;
				}
			}
			return true;
		}

		public override Pawn ProducePawn()
		{
			Pawn innerPawn = base.Gestator.ResurrectingMechCorpse.InnerPawn;
			ResurrectionUtility.Resurrect(innerPawn);
			innerPawn.needs.energy.CurLevel = innerPawn.needs.energy.MaxLevel * 0.5f;
			innerPawn.health.RemoveAllHediffs();
			base.BoundPawn.relations.AddDirectRelation(PawnRelationDefOf.Overseer, innerPawn);
			if (innerPawn.IsWorldPawn())
			{
				Find.WorldPawns.RemovePawn(innerPawn);
			}
			return innerPawn;
		}

		public override bool IsFixedOrAllowedIngredient(Thing thing)
		{
			if (!base.IsFixedOrAllowedIngredient(thing))
			{
				return false;
			}
			if (thing is Corpse corpse && corpse.InnerPawn.Faction != Faction.OfPlayer)
			{
				return false;
			}
			return true;
		}

		private IEnumerable<Corpse> AllAvailableResurrectionCorpses(Pawn pawn)
		{
			List<Thing> list = base.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
			foreach (Corpse item in list)
			{
				if (recipe.fixedIngredientFilter.Allows(item) && !item.IsForbidden(pawn) && pawn.CanReach(item, PathEndMode.Touch, Danger.Deadly))
				{
					yield return item;
				}
			}
		}

		protected override void AppendFormingInspectionData(StringBuilder sb)
		{
			switch (base.State)
			{
			case FormingCycleState.Gathering:
				AppendCurrentIngredientCount(sb);
				break;
			case FormingCycleState.Forming:
				if (base.Gestator?.ResurrectingMechCorpse != null)
				{
					sb.AppendLine("ResurrectingMech".Translate(base.Gestator.ResurrectingMechCorpse.InnerPawn.LabelCap));
				}
				break;
			case FormingCycleState.Formed:
				if (base.Gestator?.GestatingMech != null && base.BoundPawn != null)
				{
					sb.AppendLine("ResurrectedMech".Translate(base.Gestator.GestatingMech.LabelCap, base.BoundPawn.Named("PAWN")) + " (" + "GestatedMechRequiresMechanitor".Translate(base.BoundPawn.Named("PAWN")) + ")");
				}
				break;
			case FormingCycleState.Preparing:
				break;
			}
		}
	}
}
