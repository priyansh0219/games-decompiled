using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class Bill_Mech : Bill_Production
	{
		public const int CompleteBillTicks = 300;

		private const float StatusStringLineHeight = 20f;

		private Pawn boundPawn;

		private int gestationCycles;

		public float formingTicks;

		private FormingCycleState state;

		private int startedTick;

		private List<IngredientCount> ingredients = new List<IngredientCount>();

		public Pawn BoundPawn => boundPawn;

		public FormingCycleState State => state;

		public override bool CanFinishNow => state == FormingCycleState.Formed;

		public int GestationCyclesCompleted => gestationCycles;

		public int StartedTick => startedTick;

		public float WorkSpeedMultiplier
		{
			get
			{
				if (recipe.workSpeedStat != null)
				{
					return boundPawn.GetStatValue(recipe.workSpeedStat);
				}
				return 1f;
			}
		}

		public Building_MechGestator Gestator => (Building_MechGestator)billStack.billGiver;

		public abstract float BandwidthCost { get; }

		private List<IngredientCount> CurrentBillIngredients
		{
			get
			{
				if (ingredients.Count == 0)
				{
					this.MakeIngredientsListInProcessingOrder(ingredients);
				}
				return ingredients;
			}
		}

		protected override string StatusString
		{
			get
			{
				switch (State)
				{
				case FormingCycleState.Gathering:
				case FormingCycleState.Preparing:
					if (BoundPawn != null)
					{
						return "Worker".Translate() + ": " + BoundPawn.LabelShortCap;
					}
					break;
				case FormingCycleState.Forming:
					return "Gestating".Translate();
				case FormingCycleState.Formed:
					if (BoundPawn != null)
					{
						return "WaitingFor".Translate() + ": " + BoundPawn.LabelShortCap;
					}
					break;
				}
				return null;
			}
		}

		protected override float StatusLineMinHeight => 20f;

		protected override Color BaseColor
		{
			get
			{
				if (suspended)
				{
					return base.BaseColor;
				}
				return Color.white;
			}
		}

		public Bill_Mech()
		{
		}

		public Bill_Mech(RecipeDef recipe, Precept_ThingStyle precept = null)
			: base(recipe, precept)
		{
		}

		protected override Window GetBillDialog()
		{
			return new Dialog_MechBillConfig(this, ((Thing)billStack.billGiver).Position);
		}

		public abstract Pawn ProducePawn();

		public override bool ShouldDoNow()
		{
			if (BoundPawn?.mechanitor != null && !BoundPawn.mechanitor.HasBandwidthForBill(this))
			{
				JobFailReason.Is("NotEnoughBandwidth".Translate());
				return false;
			}
			if (!base.ShouldDoNow())
			{
				return false;
			}
			return state != FormingCycleState.Forming;
		}

		public override bool PawnAllowedToStartAnew(Pawn p)
		{
			if (!ModLister.CheckBiotech("Mech bill"))
			{
				return false;
			}
			if (!base.PawnAllowedToStartAnew(p))
			{
				return false;
			}
			if (Gestator.ActiveBill != null && Gestator.ActiveBill != this && (!Gestator.ActiveBill.suspended || Gestator.ActiveBill.State != 0 || Gestator.innerContainer.Any()))
			{
				return false;
			}
			if (BoundPawn != null && BoundPawn != p)
			{
				JobFailReason.Is("AlreadyAssigned".Translate() + " (" + BoundPawn.LabelShort + ")");
				return false;
			}
			if (!p.mechanitor.HasBandwidthForBill(this))
			{
				JobFailReason.Is("NotEnoughBandwidth".Translate());
				return false;
			}
			return true;
		}

		public override void Notify_DoBillStarted(Pawn billDoer)
		{
			base.Notify_DoBillStarted(billDoer);
			if (boundPawn != billDoer)
			{
				boundPawn = billDoer;
			}
			Gestator.ActiveBill = this;
			startedTick = Find.TickManager.TicksGame;
		}

		public override void Notify_BillWorkFinished(Pawn billDoer)
		{
			base.Notify_BillWorkFinished(billDoer);
			switch (state)
			{
			case FormingCycleState.Gathering:
				state = FormingCycleState.Forming;
				formingTicks = recipe.formingTicks;
				Gestator.Notify_StartGestation();
				break;
			case FormingCycleState.Preparing:
				formingTicks = recipe.formingTicks;
				state = FormingCycleState.Forming;
				break;
			case FormingCycleState.Forming:
				state = FormingCycleState.Formed;
				break;
			case FormingCycleState.Formed:
				break;
			}
		}

		public void Reset()
		{
			ingredients.Clear();
			state = FormingCycleState.Gathering;
			gestationCycles = 0;
			boundPawn = null;
		}

		public void ForceCompleteAllCycles()
		{
			gestationCycles = recipe.gestationCycles;
			formingTicks = 0f;
		}

		public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
		{
			base.Notify_IterationCompleted(billDoer, ingredients);
			Reset();
			if (Gestator.ActiveBill == this)
			{
				Gestator.ActiveBill = null;
			}
		}

		public override float GetWorkAmount(UnfinishedThing uft = null)
		{
			if (state == FormingCycleState.Formed)
			{
				return 300f;
			}
			return base.GetWorkAmount(uft);
		}

		public void FormerBillTick()
		{
			if (suspended || state != FormingCycleState.Forming)
			{
				return;
			}
			formingTicks -= 1f * WorkSpeedMultiplier;
			if (formingTicks <= 0f)
			{
				gestationCycles++;
				if (gestationCycles >= recipe.gestationCycles)
				{
					state = FormingCycleState.Formed;
					Gestator.Notify_AllGestationCyclesCompleted();
				}
				else
				{
					formingTicks = recipe.formingTicks;
					state = FormingCycleState.Preparing;
				}
			}
		}

		public void AppendCurrentIngredientCount(StringBuilder sb)
		{
			foreach (IngredientCount currentBillIngredient in CurrentBillIngredients)
			{
				if (currentBillIngredient != null && currentBillIngredient.IsFixedIngredient)
				{
					TaggedString labelCap = currentBillIngredient.FixedIngredient.LabelCap;
					int num = Gestator.innerContainer.TotalStackCountOfDef(currentBillIngredient.FixedIngredient);
					labelCap += $" {num} / {currentBillIngredient.CountRequiredOfFor(currentBillIngredient.FixedIngredient, recipe, this)}";
					sb.AppendLine(labelCap);
				}
			}
		}

		public virtual void AppendInspectionData(StringBuilder sb)
		{
			AppendFormingInspectionData(sb);
			if (State == FormingCycleState.Forming || State == FormingCycleState.Preparing)
			{
				sb.AppendLine("CurrentGestationCycle".Translate() + ": " + ((int)(formingTicks * (1f / WorkSpeedMultiplier))).ToStringTicksToPeriod());
				sb.AppendLine((string)((string)("RemainingGestationCycles".Translate() + ": ") + (recipe.gestationCycles - GestationCyclesCompleted) + " (" + "OfLower".Translate() + " ") + recipe.gestationCycles + ")");
			}
		}

		protected abstract void AppendFormingInspectionData(StringBuilder sb);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref boundPawn, "boundPawn");
			Scribe_Values.Look(ref gestationCycles, "gestationCycles", 0);
			Scribe_Values.Look(ref formingTicks, "formingTicks", 0f);
			Scribe_Values.Look(ref state, "state", FormingCycleState.Gathering);
			Scribe_Values.Look(ref startedTick, "startedTick", 0);
		}
	}
}
