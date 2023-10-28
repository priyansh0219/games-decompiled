using UnityEngine;

namespace Verse
{
	public abstract class SubEffecter_Sprayer : SubEffecter
	{
		private Mote mote;

		private Vector3? lastOffset;

		public SubEffecter_Sprayer(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		protected void MakeMote(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1)
		{
			Vector3 vector = Vector3.zero;
			switch (def.spawnLocType)
			{
			case MoteSpawnLocType.OnSource:
				vector = A.CenterVector3;
				break;
			case MoteSpawnLocType.OnTarget:
				vector = B.CenterVector3;
				break;
			case MoteSpawnLocType.BetweenPositions:
			{
				Vector3 vector5 = (A.HasThing ? A.Thing.DrawPos : A.Cell.ToVector3Shifted());
				Vector3 vector6 = (B.HasThing ? B.Thing.DrawPos : B.Cell.ToVector3Shifted());
				vector = ((A.HasThing && !A.Thing.Spawned) ? vector6 : ((!B.HasThing || B.Thing.Spawned) ? (vector5 * def.positionLerpFactor + vector6 * (1f - def.positionLerpFactor)) : vector5));
				break;
			}
			case MoteSpawnLocType.RandomCellOnTarget:
				vector = ((!B.HasThing) ? CellRect.CenteredOn(B.Cell, 0) : B.Thing.OccupiedRect()).RandomCell.ToVector3Shifted();
				break;
			case MoteSpawnLocType.RandomDrawPosOnTarget:
				if (B.Thing.DrawSize != Vector2.one && B.Thing.DrawSize != Vector2.zero)
				{
					Vector2 vector2 = B.Thing.DrawSize.RotatedBy(B.Thing.Rotation);
					Vector3 vector3 = new Vector3(vector2.x * Rand.Value, 0f, vector2.y * Rand.Value);
					vector = B.CenterVector3 + vector3 - new Vector3(vector2.x / 2f, 0f, vector2.y / 2f);
				}
				else
				{
					Vector3 vector4 = new Vector3(Rand.Value, 0f, Rand.Value);
					vector = B.CenterVector3 + vector4 - new Vector3(0.5f, 0f, 0.5f);
				}
				break;
			case MoteSpawnLocType.BetweenTouchingCells:
				vector = A.Cell.ToVector3Shifted() + (B.Cell - A.Cell).ToVector3().normalized * 0.5f;
				break;
			}
			if (parent != null)
			{
				Rand.PushState(parent.GetHashCode());
				if (A.CenterVector3 != B.CenterVector3)
				{
					vector += (B.CenterVector3 - A.CenterVector3).normalized * parent.def.offsetTowardsTarget.RandomInRange;
				}
				Vector3 vector7 = Gen.RandomHorizontalVector(parent.def.positionRadius);
				Rand.PopState();
				if (def.positionDimensions.HasValue)
				{
					vector7 += Gen.Random2DVector(def.positionDimensions.Value);
				}
				vector += vector7 + parent.offset;
			}
			Map map = A.Map ?? B.Map;
			float num = (def.absoluteAngle ? 0f : ((def.useTargetAInitialRotation && A.HasThing) ? A.Thing.Rotation.AsAngle : ((!def.useTargetBInitialRotation || !B.HasThing) ? (B.Cell - A.Cell).AngleFlat : B.Thing.Rotation.AsAngle)));
			float num2 = ((parent != null) ? parent.scale : 1f);
			if (map == null)
			{
				return;
			}
			int randomInRange = def.burstCount.RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				Vector3 vector8 = def.positionOffset;
				if (def.useTargetAInitialRotation && A.HasThing)
				{
					vector8 = vector8.RotatedBy(A.Thing.Rotation);
				}
				else if (def.useTargetBInitialRotation && B.HasThing)
				{
					vector8 = vector8.RotatedBy(B.Thing.Rotation);
				}
				if (!def.perRotationOffsets.NullOrEmpty())
				{
					vector8 += def.perRotationOffsets[((def.spawnLocType == MoteSpawnLocType.OnSource) ? A.Thing.Rotation : B.Thing.Rotation).AsInt];
				}
				for (int j = 0; j < 5; j++)
				{
					vector8 = vector8 * num2 + Rand.InsideAnnulusVector3(def.positionRadiusMin, def.positionRadius) * num2;
					if (def.avoidLastPositionRadius < float.Epsilon || !lastOffset.HasValue || (vector8 - lastOffset.Value).MagnitudeHorizontal() > def.avoidLastPositionRadius)
					{
						break;
					}
				}
				lastOffset = vector8;
				Vector3 vector9 = vector + vector8;
				if (def.rotateTowardsTargetCenter && B.HasThing)
				{
					num = (vector9 - B.CenterVector3).AngleFlat();
				}
				if (def.moteDef != null && vector.ShouldSpawnMotesAt(map, def.moteDef.drawOffscreen))
				{
					mote = (Mote)ThingMaker.MakeThing(def.moteDef);
					GenSpawn.Spawn(mote, vector.ToIntVec3(), map);
					mote.Scale = def.scale.RandomInRange * num2;
					mote.exactPosition = vector9;
					mote.rotationRate = def.rotationRate.RandomInRange;
					mote.exactRotation = def.rotation.RandomInRange + num;
					mote.instanceColor = base.EffectiveColor;
					if (overrideSpawnTick != -1)
					{
						mote.ForceSpawnTick(overrideSpawnTick);
					}
					if (mote is MoteThrown moteThrown)
					{
						moteThrown.airTimeLeft = def.airTime.RandomInRange;
						moteThrown.SetVelocity(def.angle.RandomInRange + num, def.speed.RandomInRange);
					}
					if (def.attachToSpawnThing && mote is MoteAttached moteAttached)
					{
						if (def.spawnLocType == MoteSpawnLocType.OnSource && A.HasThing)
						{
							moteAttached.Attach(A, vector8);
						}
						else if (def.spawnLocType == MoteSpawnLocType.OnTarget && B.HasThing)
						{
							moteAttached.Attach(B, vector8);
						}
					}
					mote.Maintain();
				}
				else if (def.fleckDef != null && vector9.ShouldSpawnMotesAt(map, def.fleckDef.drawOffscreen))
				{
					float velocityAngle = (def.fleckUsesAngleForVelocity ? (def.angle.RandomInRange + num) : 0f);
					map.flecks.CreateFleck(new FleckCreationData
					{
						def = def.fleckDef,
						scale = def.scale.RandomInRange * num2,
						spawnPosition = vector9,
						rotationRate = def.rotationRate.RandomInRange,
						rotation = def.rotation.RandomInRange + num,
						instanceColor = base.EffectiveColor,
						velocitySpeed = def.speed.RandomInRange,
						velocityAngle = velocityAngle,
						ageTicksOverride = overrideSpawnTick
					});
				}
			}
		}

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			base.SubEffectTick(A, B);
			if (mote != null && mote.def.mote.needsMaintenance)
			{
				mote.Maintain();
			}
		}
	}
}
