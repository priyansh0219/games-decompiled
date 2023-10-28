namespace Verse
{
	public class Projectile_Explosive : Projectile
	{
		private int ticksToDetonation;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksToDetonation, "ticksToDetonation", 0);
		}

		public override void Tick()
		{
			base.Tick();
			if (ticksToDetonation > 0)
			{
				ticksToDetonation--;
				if (ticksToDetonation <= 0)
				{
					Explode();
				}
			}
		}

		protected override void Impact(Thing hitThing, bool blockedByShield = false)
		{
			if (blockedByShield || def.projectile.explosionDelay == 0)
			{
				Explode();
				return;
			}
			landed = true;
			ticksToDetonation = def.projectile.explosionDelay;
			GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, def.projectile.damageDef, launcher.Faction, launcher);
		}

		protected virtual void Explode()
		{
			Map map = base.Map;
			Destroy();
			if (def.projectile.explosionEffect != null)
			{
				Effecter effecter = def.projectile.explosionEffect.Spawn();
				effecter.Trigger(new TargetInfo(base.Position, map), new TargetInfo(base.Position, map));
				effecter.Cleanup();
			}
			GenExplosion.DoExplosion(base.Position, map, def.projectile.explosionRadius, def.projectile.damageDef, launcher, DamageAmount, ArmorPenetration, def.projectile.soundExplode, equipmentDef, def, intendedTarget.Thing, def.projectile.postExplosionSpawnThingDef, postExplosionSpawnThingDefWater: def.projectile.postExplosionSpawnThingDefWater, postExplosionSpawnChance: def.projectile.postExplosionSpawnChance, postExplosionSpawnThingCount: def.projectile.postExplosionSpawnThingCount, postExplosionGasType: def.projectile.postExplosionGasType, preExplosionSpawnThingDef: def.projectile.preExplosionSpawnThingDef, preExplosionSpawnChance: def.projectile.preExplosionSpawnChance, preExplosionSpawnThingCount: def.projectile.preExplosionSpawnThingCount, applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination), ignoredThings: null, affectedAngle: null, doVisualEffects: true, propagationSpeed: def.projectile.damageDef.expolosionPropagationSpeed, excludeRadius: 0f, doSoundEffects: true, screenShakeFactor: def.projectile.screenShakeFactor);
		}
	}
}
