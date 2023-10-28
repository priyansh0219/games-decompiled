using UnityEngine;

namespace RimWorld
{
	public class Building_TurretRocket : Building_TurretGun
	{
		public override Material TurretTopMaterial
		{
			get
			{
				if (refuelableComp.IsFull)
				{
					return def.building.turretGunDef.building.turretTopLoadedMat;
				}
				return def.building.turretTopMat;
			}
		}
	}
}
