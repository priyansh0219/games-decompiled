using Verse;

namespace RimWorld
{
	public class Verb_SmokePop : Verb
	{
		protected override bool TryCastShot()
		{
			Pop(base.ReloadableCompSource);
			return true;
		}

		public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
		{
			needLOSToCenter = false;
			return base.EquipmentSource.GetStatValue(StatDefOf.SmokepopBeltRadius);
		}

		public override void DrawHighlight(LocalTargetInfo target)
		{
			DrawHighlightFieldRadiusAroundTarget(caster);
		}

		public static void Pop(CompReloadable comp)
		{
			if (comp != null && comp.CanBeUsed)
			{
				ThingWithComps parent = comp.parent;
				Pawn wearer = comp.Wearer;
				float statValue = parent.GetStatValue(StatDefOf.SmokepopBeltRadius);
				GenExplosion.DoExplosion(wearer.Position, wearer.Map, statValue, DamageDefOf.Smoke, null, -1, -1f, null, null, null, null, null, 0f, 1, GasType.BlindSmoke);
				comp.UsedOnce();
			}
		}
	}
}
