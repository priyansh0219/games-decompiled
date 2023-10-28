using Verse;

namespace RimWorld
{
	public class CompUseEffect_InstallImplantMechlink : CompUseEffect_InstallImplant
	{
		public override bool CanBeUsedBy(Pawn p, out string failReason)
		{
			if (!ModLister.CheckBiotech("install implant mechlink"))
			{
				failReason = null;
				return false;
			}
			return base.CanBeUsedBy(p, out failReason);
		}

		public override TaggedString ConfirmMessage(Pawn p)
		{
			if (p.WorkTypeIsDisabled(WorkTypeDefOf.Smithing))
			{
				return "ConfirmInstallMechlink".Translate();
			}
			return null;
		}
	}
}
