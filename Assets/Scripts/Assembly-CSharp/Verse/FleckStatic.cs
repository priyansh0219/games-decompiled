using UnityEngine;

namespace Verse
{
	public struct FleckStatic : IFleck
	{
		public FleckDef def;

		public Map map;

		public Vector3 exactPosition;

		public float exactRotation;

		public Vector3 exactScale;

		public Color instanceColor;

		public float solidTimeOverride;

		public float ageSecs;

		public int ageTicks;

		public int setupTick;

		public Vector3 spawnPosition;

		public float skidSpeedMultiplierPerTick;

		public float Scale
		{
			set
			{
				exactScale = new Vector3(value, 1f, value);
			}
		}

		public float SolidTime
		{
			get
			{
				if (!(solidTimeOverride < 0f))
				{
					return solidTimeOverride;
				}
				return def.solidTime;
			}
		}

		public Vector3 DrawPos => exactPosition + def.unattachedDrawOffset;

		public bool EndOfLife => ageSecs >= def.Lifespan;

		public float Alpha
		{
			get
			{
				float num = ageSecs;
				if (num <= def.fadeInTime)
				{
					if (def.fadeInTime > 0f)
					{
						return num / def.fadeInTime;
					}
					return 1f;
				}
				if (num <= def.fadeInTime + SolidTime)
				{
					return 1f;
				}
				if (def.fadeOutTime > 0f)
				{
					return 1f - Mathf.InverseLerp(def.fadeInTime + SolidTime, def.fadeInTime + SolidTime + def.fadeOutTime, num);
				}
				return 1f;
			}
		}

		public void Setup(FleckCreationData creationData)
		{
			def = creationData.def;
			exactScale = Vector3.one;
			instanceColor = creationData.instanceColor ?? Color.white;
			solidTimeOverride = creationData.solidTimeOverride ?? (-1f);
			skidSpeedMultiplierPerTick = Rand.Range(0.3f, 0.95f);
			ageSecs = 0f;
			if (creationData.exactScale.HasValue)
			{
				exactScale = creationData.exactScale.Value;
			}
			else
			{
				Scale = creationData.scale;
			}
			exactPosition = creationData.spawnPosition;
			spawnPosition = creationData.spawnPosition;
			exactRotation = creationData.rotation;
			setupTick = Find.TickManager.TicksGame;
			if (creationData.ageTicksOverride != -1)
			{
				ForceSpawnTick(creationData.ageTicksOverride);
			}
		}

		public bool TimeInterval(float deltaTime, Map map)
		{
			if (EndOfLife)
			{
				return true;
			}
			ageSecs += deltaTime;
			ageTicks++;
			if (def.growthRate != 0f)
			{
				float num = Mathf.Sign(exactScale.x);
				float num2 = Mathf.Sign(exactScale.z);
				exactScale = new Vector3(exactScale.x + num * (def.growthRate * deltaTime), exactScale.y, exactScale.z + num2 * (def.growthRate * deltaTime));
				exactScale.x = ((num > 0f) ? Mathf.Max(exactScale.x, 0.0001f) : Mathf.Min(exactScale.x, -0.0001f));
				exactScale.z = ((num2 > 0f) ? Mathf.Max(exactScale.z, 0.0001f) : Mathf.Min(exactScale.z, -0.0001f));
			}
			return false;
		}

		public void Draw(DrawBatch batch)
		{
			Draw(def.altitudeLayer.AltitudeFor(def.altitudeLayerIncOffset), batch);
		}

		public void Draw(float altitude, DrawBatch batch)
		{
			exactPosition.y = altitude;
			int num = setupTick + spawnPosition.GetHashCode();
			((Graphic_Fleck)def.GetGraphicData(num).Graphic).DrawFleck(new FleckDrawData
			{
				alpha = Alpha,
				color = instanceColor,
				drawLayer = 0,
				pos = DrawPos,
				rotation = exactRotation,
				scale = exactScale,
				ageSecs = ageSecs,
				id = num
			}, batch);
		}

		public void ForceSpawnTick(int tick)
		{
			ageTicks = Find.TickManager.TicksGame - tick;
			ageSecs = ageTicks.TicksToSeconds();
		}
	}
}
