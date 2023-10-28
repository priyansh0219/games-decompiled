using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public struct FleckThrown : IFleck
	{
		public FleckStatic baseData;

		public float airTimeLeft;

		public Vector3 velocity;

		public float rotationRate;

		public FleckAttachLink link;

		private Vector3 attacheeLastPosition;

		public const float MinSpeed = 0.02f;

		public bool Flying => airTimeLeft > 0f;

		public bool Skidding
		{
			get
			{
				if (!Flying)
				{
					return Speed > 0.01f;
				}
				return false;
			}
		}

		public Vector3 Velocity
		{
			get
			{
				return velocity;
			}
			set
			{
				velocity = value;
			}
		}

		public float MoveAngle
		{
			get
			{
				return velocity.AngleFlat();
			}
			set
			{
				SetVelocity(value, Speed);
			}
		}

		public float Speed
		{
			get
			{
				return velocity.MagnitudeHorizontal();
			}
			set
			{
				if (value == 0f)
				{
					velocity = Vector3.zero;
				}
				else if (velocity == Vector3.zero)
				{
					velocity = new Vector3(value, 0f, 0f);
				}
				else
				{
					velocity = velocity.normalized * value;
				}
			}
		}

		public void Setup(FleckCreationData creationData)
		{
			baseData = default(FleckStatic);
			baseData.Setup(creationData);
			airTimeLeft = creationData.airTimeLeft ?? 999999f;
			attacheeLastPosition = new Vector3(-1000f, -1000f, -1000f);
			link = creationData.link;
			if (link.Linked)
			{
				attacheeLastPosition = link.LastDrawPos;
			}
			baseData.exactPosition += creationData.def.attachedDrawOffset;
			rotationRate = creationData.rotationRate;
			SetVelocity(creationData.velocityAngle, creationData.velocitySpeed);
			if (creationData.velocity.HasValue)
			{
				velocity += creationData.velocity.Value;
			}
		}

		public bool TimeInterval(float deltaTime, Map map)
		{
			if (baseData.TimeInterval(deltaTime, map))
			{
				return true;
			}
			if (!Flying && !Skidding)
			{
				return false;
			}
			Vector3 vector = NextExactPosition(deltaTime);
			IntVec3 intVec = new IntVec3(vector);
			if (intVec != new IntVec3(baseData.exactPosition))
			{
				if (!intVec.InBounds(map))
				{
					return true;
				}
				if (baseData.def.collide && intVec.Filled(map))
				{
					WallHit();
					return false;
				}
			}
			baseData.exactPosition = vector;
			if (baseData.def.rotateTowardsMoveDirection && velocity != default(Vector3))
			{
				baseData.exactRotation = velocity.AngleFlat() + baseData.def.rotateTowardsMoveDirectionExtraAngle;
			}
			else
			{
				baseData.exactRotation += rotationRate * deltaTime;
			}
			velocity += baseData.def.acceleration * deltaTime;
			if (baseData.def.speedPerTime != FloatRange.Zero)
			{
				Speed = Mathf.Max(Speed + baseData.def.speedPerTime.RandomInRange * deltaTime, 0f);
			}
			if (airTimeLeft > 0f)
			{
				airTimeLeft -= deltaTime;
				if (airTimeLeft < 0f)
				{
					airTimeLeft = 0f;
				}
				if (airTimeLeft <= 0f && !baseData.def.landSound.NullOrUndefined())
				{
					baseData.def.landSound.PlayOneShot(new TargetInfo(new IntVec3(baseData.exactPosition), map));
				}
			}
			if (Skidding)
			{
				Speed *= baseData.skidSpeedMultiplierPerTick;
				rotationRate *= baseData.skidSpeedMultiplierPerTick;
				if (Speed < 0.02f)
				{
					Speed = 0f;
				}
			}
			return false;
		}

		private Vector3 NextExactPosition(float deltaTime)
		{
			Vector3 result = baseData.exactPosition + velocity * deltaTime;
			if (link.Linked)
			{
				bool flag = link.detachAfterTicks == -1 || baseData.ageTicks < link.detachAfterTicks;
				if (!link.Target.ThingDestroyed && flag)
				{
					link.UpdateDrawPos();
				}
				Vector3 vector = baseData.def.attachedDrawOffset;
				if (baseData.def.attachedToHead && link.Target.Thing is Pawn pawn && pawn.story != null)
				{
					vector = pawn.Drawer.renderer.BaseHeadOffsetAt((pawn.GetPosture() == PawnPosture.Standing) ? Rot4.North : pawn.Drawer.renderer.LayingFacing()).RotatedBy(pawn.Drawer.renderer.BodyAngle());
				}
				Vector3 vector2 = link.LastDrawPos - attacheeLastPosition;
				result += vector2;
				result += vector;
				result.y = AltitudeLayer.MoteOverhead.AltitudeFor();
				attacheeLastPosition = link.LastDrawPos;
			}
			return result;
		}

		public void SetVelocity(float angle, float speed)
		{
			velocity = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * speed;
		}

		public void Draw(DrawBatch batch)
		{
			baseData.Draw(batch);
		}

		private void WallHit()
		{
			airTimeLeft = 0f;
			Speed = 0f;
			rotationRate = 0f;
		}
	}
}
