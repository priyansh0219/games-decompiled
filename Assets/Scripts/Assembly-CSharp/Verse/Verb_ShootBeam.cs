using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class Verb_ShootBeam : Verb
	{
		private List<Vector3> path = new List<Vector3>();

		private int ticksToNextPathStep;

		private Vector3 initialTargetPosition;

		private MoteDualAttached mote;

		private Effecter endEffecter;

		private Sustainer sustainer;

		private const int NumSubdivisionsPerUnitLength = 1;

		protected override int ShotsPerBurst => verbProps.burstShotCount;

		public float ShotProgress => (float)ticksToNextPathStep / (float)verbProps.ticksBetweenBurstShots;

		public Vector3 InterpolatedPosition
		{
			get
			{
				Vector3 vector = base.CurrentTarget.CenterVector3 - initialTargetPosition;
				return Vector3.Lerp(path[burstShotsLeft], path[Mathf.Min(burstShotsLeft + 1, path.Count - 1)], ShotProgress) + vector;
			}
		}

		public override float? AimAngleOverride
		{
			get
			{
				if (state != VerbState.Bursting)
				{
					return null;
				}
				return (InterpolatedPosition - caster.DrawPos).AngleFlat();
			}
		}

		protected override bool TryCastShot()
		{
			if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
			{
				return false;
			}
			ShootLine resultingLine;
			bool flag = TryFindShootLineFromTo(caster.Position, currentTarget, out resultingLine);
			if (verbProps.stopBurstWithoutLos && !flag)
			{
				return false;
			}
			if (base.EquipmentSource != null)
			{
				base.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
				base.EquipmentSource.GetComp<CompReloadable>()?.UsedOnce();
			}
			lastShotTick = Find.TickManager.TicksGame;
			ticksToNextPathStep = verbProps.ticksBetweenBurstShots;
			IntVec3 intVec = InterpolatedPosition.Yto0().ToIntVec3();
			IntVec3 intVec2 = GenSight.LastPointOnLineOfSight(caster.Position, intVec, (IntVec3 c) => c.CanBeSeenOverFast(caster.Map), skipFirstCell: true);
			HitCell(intVec2.IsValid ? intVec2 : intVec);
			return true;
		}

		public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
		{
			return base.TryStartCastOn(verbProps.beamTargetsGround ? ((LocalTargetInfo)castTarg.Cell) : castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire, nonInterruptingSelfCast);
		}

		public override void BurstingTick()
		{
			ticksToNextPathStep--;
			Vector3 vector = InterpolatedPosition;
			IntVec3 intVec = vector.ToIntVec3();
			Vector3 vector2 = InterpolatedPosition - caster.Position.ToVector3Shifted();
			float num = vector2.MagnitudeHorizontal();
			Vector3 normalized = vector2.Yto0().normalized;
			IntVec3 intVec2 = GenSight.LastPointOnLineOfSight(caster.Position, intVec, (IntVec3 c) => c.CanBeSeenOverFast(caster.Map), skipFirstCell: true);
			if (intVec2.IsValid)
			{
				num -= (intVec - intVec2).LengthHorizontal;
				vector = caster.Position.ToVector3Shifted() + normalized * num;
				intVec = vector.ToIntVec3();
			}
			Vector3 offsetA = normalized * verbProps.beamStartOffset;
			Vector3 vector3 = vector - intVec.ToVector3Shifted();
			if (mote != null)
			{
				mote.UpdateTargets(new TargetInfo(caster.Position, caster.Map), new TargetInfo(intVec, caster.Map), offsetA, vector3);
				mote.Maintain();
			}
			if (verbProps.beamGroundFleckDef != null && Rand.Chance(verbProps.beamFleckChancePerTick))
			{
				FleckMaker.Static(vector, caster.Map, verbProps.beamGroundFleckDef);
			}
			if (endEffecter == null && verbProps.beamEndEffecterDef != null)
			{
				endEffecter = verbProps.beamEndEffecterDef.Spawn(intVec, caster.Map, vector3);
			}
			if (endEffecter != null)
			{
				endEffecter.offset = vector3;
				endEffecter.EffectTick(new TargetInfo(intVec, caster.Map), TargetInfo.Invalid);
				endEffecter.ticksLeft--;
			}
			if (verbProps.beamLineFleckDef != null)
			{
				float num2 = 1f * num;
				for (int i = 0; (float)i < num2; i++)
				{
					if (Rand.Chance(verbProps.beamLineFleckChanceCurve.Evaluate((float)i / num2)))
					{
						Vector3 vector4 = i * normalized - normalized * Rand.Value + normalized / 2f;
						FleckMaker.Static(caster.Position.ToVector3Shifted() + vector4, caster.Map, verbProps.beamLineFleckDef);
					}
				}
			}
			sustainer?.Maintain();
		}

		public override void WarmupComplete()
		{
			burstShotsLeft = ShotsPerBurst;
			state = VerbState.Bursting;
			initialTargetPosition = currentTarget.CenterVector3;
			path.Clear();
			Vector3 vector = (currentTarget.CenterVector3 - caster.Position.ToVector3Shifted()).Yto0();
			float magnitude = vector.magnitude;
			Vector3 normalized = vector.normalized;
			Vector3 vector2 = normalized.RotatedBy(-90f);
			float num = ((verbProps.beamFullWidthRange > 0f) ? Mathf.Min(magnitude / verbProps.beamFullWidthRange, 1f) : 1f);
			float num2 = (verbProps.beamWidth + 1f) * num / (float)ShotsPerBurst;
			Vector3 vector3 = currentTarget.CenterVector3.Yto0() - vector2 * verbProps.beamWidth / 2f * num;
			path.Add(vector3);
			for (int i = 0; i < ShotsPerBurst; i++)
			{
				Vector3 vector4 = normalized * (Rand.Value * verbProps.beamMaxDeviation) - normalized / 2f;
				Vector3 vector5 = Mathf.Sin(((float)i / (float)ShotsPerBurst + 0.5f) * (float)Math.PI * 57.29578f) * verbProps.beamCurvature * -normalized - normalized * verbProps.beamMaxDeviation / 2f;
				path.Add(vector3 + (vector4 + vector5) * num);
				vector3 += vector2 * num2;
			}
			if (verbProps.beamMoteDef != null)
			{
				mote = MoteMaker.MakeInteractionOverlay(verbProps.beamMoteDef, caster, new TargetInfo(path[0].ToIntVec3(), caster.Map));
			}
			TryCastNextBurstShot();
			ticksToNextPathStep = verbProps.ticksBetweenBurstShots;
			endEffecter?.Cleanup();
			if (verbProps.soundCastBeam != null)
			{
				sustainer = verbProps.soundCastBeam.TrySpawnSustainer(SoundInfo.InMap(caster, MaintenanceType.PerTick));
			}
		}

		private bool CanHit(Thing thing)
		{
			if (!thing.Spawned)
			{
				return false;
			}
			return !CoverUtility.ThingCovered(thing, caster.Map);
		}

		private void HitCell(IntVec3 cell)
		{
			ApplyDamage(VerbUtility.ThingsToHit(cell, caster.Map, CanHit).RandomElementWithFallback());
		}

		private void ApplyDamage(Thing thing)
		{
			IntVec3 intVec = InterpolatedPosition.Yto0().ToIntVec3();
			IntVec3 intVec2 = GenSight.LastPointOnLineOfSight(caster.Position, intVec, (IntVec3 c) => c.CanBeSeenOverFast(caster.Map), skipFirstCell: true);
			if (intVec2.IsValid)
			{
				intVec = intVec2;
			}
			Map map = caster.Map;
			if (thing == null || verbProps.beamDamageDef == null)
			{
				return;
			}
			float angleFlat = (currentTarget.Cell - caster.Position).AngleFlat;
			BattleLogEntry_RangedImpact log = new BattleLogEntry_RangedImpact(caster, thing, currentTarget.Thing, base.EquipmentSource.def, null, null);
			DamageInfo dinfo = new DamageInfo(verbProps.beamDamageDef, verbProps.beamDamageDef.defaultDamage, verbProps.beamDamageDef.defaultArmorPenetration, angleFlat, caster, null, base.EquipmentSource.def, DamageInfo.SourceCategory.ThingOrUnknown, currentTarget.Thing);
			thing.TakeDamage(dinfo).AssociateWithLog(log);
			if (thing.CanEverAttachFire())
			{
				if (Rand.Chance(verbProps.beamChanceToAttachFire))
				{
					thing.TryAttachFire(verbProps.beamFireSizeRange.RandomInRange);
				}
			}
			else if (Rand.Chance(verbProps.beamChanceToStartFire))
			{
				FireUtility.TryStartFireIn(intVec, map, verbProps.beamFireSizeRange.RandomInRange);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref path, "path", LookMode.Value);
			Scribe_Values.Look(ref ticksToNextPathStep, "ticksToNextPathStep", 0);
			Scribe_Values.Look(ref initialTargetPosition, "initialTargetPosition");
			if (Scribe.mode == LoadSaveMode.PostLoadInit && path == null)
			{
				path = new List<Vector3>();
			}
		}
	}
}
