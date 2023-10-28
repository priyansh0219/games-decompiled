using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompUsable : ThingComp
	{
		private Texture2D icon;

		private Color? iconColor;

		public CompProperties_Usable Props => (CompProperties_Usable)props;

		private Texture2D Icon
		{
			get
			{
				if (icon == null && Props.floatMenuFactionIcon != null)
				{
					icon = Find.FactionManager.FirstFactionOfDef(Props.floatMenuFactionIcon)?.def?.FactionIcon;
				}
				return icon;
			}
		}

		private Color IconColor
		{
			get
			{
				if (!iconColor.HasValue && Props.floatMenuFactionIcon != null)
				{
					iconColor = Find.FactionManager.FirstFactionOfDef(Props.floatMenuFactionIcon)?.Color;
				}
				return iconColor ?? Color.white;
			}
		}

		protected virtual string FloatMenuOptionLabel(Pawn pawn)
		{
			return Props.useLabel.Formatted(parent);
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
		{
			if (!CanBeUsedBy(myPawn, out var failReason))
			{
				yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn) + ((failReason != null) ? (" (" + failReason + ")") : ""), null);
				yield break;
			}
			if (!myPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
			{
				yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn) + " (" + "NoPath".Translate() + ")", null);
				yield break;
			}
			if (!myPawn.CanReserve(parent, 1, -1, null, Props.ignoreOtherReservations))
			{
				string text = FloatMenuOptionLabel(myPawn);
				Pawn pawn = myPawn.Map.reservationManager.FirstRespectedReserver(parent, myPawn) ?? myPawn.Map.physicalInteractionReservationManager.FirstReserverOf(parent);
				text = ((pawn == null) ? ((string)(text + (" (" + "Reserved".Translate() + ")"))) : ((string)(text + (" (" + "ReservedBy".Translate(pawn.LabelShort, pawn) + ")"))));
				yield return new FloatMenuOption(text, null);
				yield break;
			}
			if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn) + " (" + "Incapable".Translate().CapitalizeFirst() + ")", null);
				yield break;
			}
			if (Props.userMustHaveHediff != null && !myPawn.health.hediffSet.HasHediff(Props.userMustHaveHediff))
			{
				yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn) + " (" + "MustHaveHediff".Translate(Props.userMustHaveHediff) + ")", null);
				yield break;
			}
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(FloatMenuOptionLabel(myPawn), delegate
			{
				if (myPawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, Props.ignoreOtherReservations))
				{
					foreach (CompUseEffect comp in parent.GetComps<CompUseEffect>())
					{
						if (comp.SelectedUseOption(myPawn))
						{
							return;
						}
					}
					TryStartUseJob(myPawn, GetExtraTarget(myPawn), Props.ignoreOtherReservations);
				}
			}, priority: Props.floatMenuOptionPriority, itemIcon: Icon, iconColor: IconColor), myPawn, parent);
		}

		public virtual LocalTargetInfo GetExtraTarget(Pawn pawn)
		{
			return LocalTargetInfo.Invalid;
		}

		public virtual void TryStartUseJob(Pawn pawn, LocalTargetInfo extraTarget, bool forced = false)
		{
			if (!pawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, forced) || !CanBeUsedBy(pawn, out var _))
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (CompUseEffect comp in parent.GetComps<CompUseEffect>())
			{
				TaggedString taggedString = comp.ConfirmMessage(pawn);
				if (!taggedString.NullOrEmpty())
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendTagged(taggedString);
				}
			}
			string text = stringBuilder.ToString();
			if (text.NullOrEmpty())
			{
				StartJob();
			}
			else
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, StartJob));
			}
			void StartJob()
			{
				Job job = (extraTarget.IsValid ? JobMaker.MakeJob(Props.useJob, parent, extraTarget) : JobMaker.MakeJob(Props.useJob, parent));
				job.count = 1;
				pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
		}

		public virtual void UsedBy(Pawn p)
		{
			if (!CanBeUsedBy(p, out var _))
			{
				return;
			}
			foreach (CompUseEffect item in from x in parent.GetComps<CompUseEffect>()
				orderby x.OrderPriority descending
				select x)
			{
				try
				{
					item.DoEffect(p);
				}
				catch (Exception ex)
				{
					Log.Error("Error in CompUseEffect: " + ex);
				}
			}
		}

		public virtual bool CanBeUsedBy(Pawn p, out string failReason)
		{
			CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
			if (compPowerTrader != null && !compPowerTrader.PowerOn)
			{
				failReason = "NoPower".Translate().CapitalizeFirst();
				return false;
			}
			List<ThingComp> allComps = parent.AllComps;
			for (int i = 0; i < allComps.Count; i++)
			{
				if (allComps[i] is CompUseEffect compUseEffect && !compUseEffect.CanBeUsedBy(p, out failReason))
				{
					return false;
				}
			}
			failReason = null;
			return true;
		}
	}
}
