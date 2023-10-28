using System;
using RimWorld;
using UnityEngine;
using Verse.Noise;

namespace Verse
{
	public class HediffComp_Disappears : HediffComp
	{
		public int ticksToDisappear;

		public int disappearsAfterTicks;

		public int seed;

		private const float NoiseScale = 0.25f;

		private const float NoiseWiggliness = 9f;

		public HediffCompProperties_Disappears Props => (HediffCompProperties_Disappears)props;

		public override bool CompShouldRemove
		{
			get
			{
				if (!base.CompShouldRemove && ticksToDisappear > 0)
				{
					if (Props.requiredMentalState != null)
					{
						return base.Pawn.MentalStateDef != Props.requiredMentalState;
					}
					return false;
				}
				return true;
			}
		}

		public float Progress => 1f - (float)ticksToDisappear / (float)Math.Max(1, disappearsAfterTicks);

		public float NoisyProgress => AddNoiseToProgress(Progress, seed);

		public override string CompLabelInBracketsExtra
		{
			get
			{
				if (!Props.showRemainingTime)
				{
					return base.CompLabelInBracketsExtra;
				}
				return ticksToDisappear.ToStringTicksToPeriod(allowSeconds: true, shortForm: true, canUseDecimals: true, allowYears: true, Props.canUseDecimalsShortForm);
			}
		}

		private static float AddNoiseToProgress(float progress, int seed)
		{
			float num = (float)Perlin.GetValue(progress, 0.0, 0.0, 9.0, seed);
			float num2 = 0.25f * (1f - progress);
			return Mathf.Clamp01(progress + num2 * num);
		}

		public override void CompPostMake()
		{
			base.CompPostMake();
			disappearsAfterTicks = Props.disappearsAfterTicks.RandomInRange;
			seed = Rand.Int;
			ticksToDisappear = disappearsAfterTicks;
		}

		public override void CompPostPostRemoved()
		{
			if (!Props.messageOnDisappear.NullOrEmpty() && PawnUtility.ShouldSendNotificationAbout(base.Pawn))
			{
				Messages.Message(Props.messageOnDisappear.Formatted(base.Pawn.Named("PAWN")), base.Pawn, MessageTypeDefOf.PositiveEvent);
			}
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			ticksToDisappear--;
		}

		public override void CompPostMerged(Hediff other)
		{
			base.CompPostMerged(other);
			HediffComp_Disappears hediffComp_Disappears = other.TryGetComp<HediffComp_Disappears>();
			if (hediffComp_Disappears != null && hediffComp_Disappears.ticksToDisappear > ticksToDisappear)
			{
				ticksToDisappear = hediffComp_Disappears.ticksToDisappear;
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.Look(ref ticksToDisappear, "ticksToDisappear", 0);
			Scribe_Values.Look(ref disappearsAfterTicks, "disappearsAfterTicks", 0);
			Scribe_Values.Look(ref seed, "seed", 0);
		}

		public override string CompDebugString()
		{
			return "ticksToDisappear: " + ticksToDisappear;
		}
	}
}
