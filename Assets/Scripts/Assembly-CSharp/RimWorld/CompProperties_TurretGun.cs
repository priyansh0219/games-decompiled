using Verse;

namespace RimWorld
{
	public class CompProperties_TurretGun : CompProperties
	{
		public ThingDef turretDef;

		public float angleOffset;

		public bool autoAttack = true;

		public CompProperties_TurretGun()
		{
			compClass = typeof(CompTurretGun);
		}
	}
}
