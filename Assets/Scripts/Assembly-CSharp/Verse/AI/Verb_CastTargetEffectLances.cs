using RimWorld;

namespace Verse.AI
{
	public class Verb_CastTargetEffectLances : Verb_CastTargetEffect
	{
		public override void OnGUI(LocalTargetInfo target)
		{
			if (CanHitTarget(target) && verbProps.targetParams.CanTarget(target.ToTargetInfo(caster.Map)))
			{
				if (target.Pawn != null && IsMechBoss(target.Pawn))
				{
					GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
					if (!string.IsNullOrEmpty(verbProps.invalidTargetPawn))
					{
						Widgets.MouseAttachedLabel(verbProps.invalidTargetPawn, 0f, -20f);
					}
				}
				else
				{
					base.OnGUI(target);
				}
			}
			else
			{
				GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
			}
		}

		public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
		{
			Pawn pawn = target.Pawn;
			if (pawn != null && IsMechBoss(pawn))
			{
				return false;
			}
			return base.ValidateTarget(target, showMessages);
		}

		private static bool IsMechBoss(Pawn pawn)
		{
			if (pawn.kindDef != PawnKindDefOf.Mech_Warqueen && pawn.kindDef != PawnKindDefOf.Mech_Apocriton)
			{
				return pawn.kindDef == PawnKindDefOf.Mech_Diabolus;
			}
			return true;
		}
	}
}
