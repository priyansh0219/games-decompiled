using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ConversionUtility
	{
		public static TaggedString GetCertaintyReductionFactorsDescription(Pawn pawn)
		{
			TaggedString result = string.Empty;
			if (pawn.Ideo != null)
			{
				float num = 1f;
				List<Precept> preceptsListForReading = pawn.Ideo.PreceptsListForReading;
				for (int i = 0; i < preceptsListForReading.Count; i++)
				{
					if (preceptsListForReading[i].def.statFactors != null)
					{
						num *= preceptsListForReading[i].def.statFactors.GetStatFactorFromList(StatDefOf.CertaintyLossFactor);
					}
				}
				if (num != 1f)
				{
					result = "AbilityIdeoConvertBreakdownIdeoCertaintyReduction".Translate(pawn.Named("PAWN"), pawn.Ideo.Named("IDEO")) + ": " + num.ToStringPercent();
					foreach (Trait allTrait in pawn.story.traits.allTraits)
					{
						if (!allTrait.Suppressed)
						{
							float num2 = allTrait.MultiplierOfStat(StatDefOf.CertaintyLossFactor);
							if (num2 != 1f)
							{
								result += "\n -  " + "AbilityIdeoConvertBreakdownTrait".Translate(allTrait.LabelCap.Named("TRAIT")) + ": x" + num2.ToStringPercent();
							}
						}
					}
				}
				float num3 = Find.Storyteller.difficulty.CertaintyReductionFactor(null, pawn);
				if (num3 != 1f)
				{
					if (!result.NullOrEmpty())
					{
						result += "\n";
					}
					result += " -  " + "Difficulty_LowPopConversionBoost_Label".Translate() + ": " + num3.ToStringPercent();
				}
			}
			return result;
		}

		public static float ConversionPowerFactor_MemesVsTraits(Pawn initiator, Pawn recipient, StringBuilder sb = null)
		{
			return Mathf.Max(1f + OffsetFromIdeo(initiator, invert: false) + OffsetFromIdeo(recipient, invert: true), -0.4f);
			string MemeAndTraitDesc(MemeDef meme, Trait trait, float offset)
			{
				if (sb == null)
				{
					return string.Empty;
				}
				return "\n   -  " + "AbilityIdeoConvertBreakdownMemeVsTrait".Translate(meme.label.Named("MEME"), trait.Label.Named("TRAIT")).CapitalizeFirst() + ": " + (1f + offset).ToStringPercent();
			}
			float OffsetFromIdeo(Pawn pawn, bool invert)
			{
				Ideo ideo = pawn.Ideo;
				string text = string.Empty;
				float num = 0f;
				if (pawn.Ideo == null)
				{
					return num;
				}
				foreach (MemeDef meme in ideo.memes)
				{
					if (!meme.agreeableTraits.NullOrEmpty())
					{
						foreach (TraitRequirement agreeableTrait in meme.agreeableTraits)
						{
							if (agreeableTrait.HasTrait(recipient))
							{
								float num2 = (invert ? (-0.2f) : 0.2f);
								num += num2;
								text += MemeAndTraitDesc(meme, agreeableTrait.GetTrait(recipient), num2);
							}
						}
					}
					if (!meme.disagreeableTraits.NullOrEmpty())
					{
						foreach (TraitRequirement disagreeableTrait in meme.disagreeableTraits)
						{
							if (disagreeableTrait.HasTrait(recipient))
							{
								float num3 = (invert ? 0.2f : (-0.2f));
								num += num3;
								text += MemeAndTraitDesc(meme, disagreeableTrait.GetTrait(recipient), num3);
							}
						}
					}
				}
				if (sb != null && !text.NullOrEmpty())
				{
					sb.AppendInNewLine(" -  " + "AbilityIdeoConvertBreakdownPawnIdeo".Translate(pawn.Named("PAWN")) + ": " + text);
				}
				return num;
			}
		}
	}
}
