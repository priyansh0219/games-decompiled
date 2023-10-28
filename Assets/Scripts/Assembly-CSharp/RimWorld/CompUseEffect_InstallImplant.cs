using Verse;

namespace RimWorld
{
	public class CompUseEffect_InstallImplant : CompUseEffect
	{
		public CompProperties_UseEffectInstallImplant Props => (CompProperties_UseEffectInstallImplant)props;

		public override void DoEffect(Pawn user)
		{
			BodyPartRecord bodyPartRecord = user.RaceProps.body.GetPartsWithDef(Props.bodyPart).FirstOrFallback();
			if (bodyPartRecord != null)
			{
				Hediff firstHediffOfDef = user.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
				if (firstHediffOfDef == null && !Props.requiresExistingHediff)
				{
					user.health.AddHediff(Props.hediffDef, bodyPartRecord);
				}
				else if (Props.canUpgrade)
				{
					((Hediff_Level)firstHediffOfDef).ChangeLevel(1);
				}
			}
		}

		public override bool CanBeUsedBy(Pawn p, out string failReason)
		{
			if ((!p.IsFreeColonist || p.HasExtraHomeFaction()) && !Props.allowNonColonists)
			{
				failReason = "InstallImplantNotAllowedForNonColonists".Translate();
				return false;
			}
			if (p.RaceProps.body.GetPartsWithDef(Props.bodyPart).FirstOrFallback() == null)
			{
				failReason = "InstallImplantNoBodyPart".Translate() + ": " + Props.bodyPart.LabelShort;
				return false;
			}
			if (Props.requiresPsychicallySensitive && !p.psychicEntropy.IsPsychicallySensitive)
			{
				failReason = "InstallImplantPsychicallyDeaf".Translate();
				return false;
			}
			Hediff existingImplant = GetExistingImplant(p);
			if (Props.requiresExistingHediff && existingImplant == null)
			{
				failReason = "InstallImplantHediffRequired".Translate(Props.hediffDef.label);
				return false;
			}
			if (existingImplant != null)
			{
				if (!Props.canUpgrade)
				{
					failReason = "InstallImplantAlreadyInstalled".Translate();
					return false;
				}
				Hediff_Level hediff_Level = (Hediff_Level)existingImplant;
				if ((float)hediff_Level.level >= hediff_Level.def.maxSeverity)
				{
					failReason = "InstallImplantAlreadyMaxLevel".Translate();
					return false;
				}
				if (Props.maxSeverity <= (float)hediff_Level.level)
				{
					failReason = (string)("InstallImplantAlreadyMaxLevel".Translate() + " ") + Props.maxSeverity;
					return false;
				}
				if (Props.minSeverity > (float)hediff_Level.level)
				{
					failReason = "InstallImplantMinLevel".Translate(Props.minSeverity);
					return false;
				}
			}
			failReason = null;
			return true;
		}

		public Hediff GetExistingImplant(Pawn p)
		{
			for (int i = 0; i < p.health.hediffSet.hediffs.Count; i++)
			{
				Hediff hediff = p.health.hediffSet.hediffs[i];
				if (hediff.def == Props.hediffDef && hediff.Part == p.RaceProps.body.GetPartsWithDef(Props.bodyPart).FirstOrFallback())
				{
					return hediff;
				}
			}
			return null;
		}
	}
}
