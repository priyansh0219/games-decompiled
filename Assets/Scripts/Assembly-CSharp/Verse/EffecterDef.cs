using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class EffecterDef : Def
	{
		public List<SubEffecterDef> children;

		public float positionRadius;

		public FloatRange offsetTowardsTarget;

		public Effecter Spawn()
		{
			return new Effecter(this);
		}

		public Effecter Spawn(IntVec3 target, Map map, float scale = 1f)
		{
			Effecter effecter = new Effecter(this);
			TargetInfo targetInfo = new TargetInfo(target, map);
			effecter.scale = scale;
			effecter.Trigger(targetInfo, targetInfo);
			return effecter;
		}

		public Effecter Spawn(IntVec3 targetA, IntVec3 targetB, Map map, float scale = 1f)
		{
			Effecter effecter = new Effecter(this);
			TargetInfo a = new TargetInfo(targetA, map);
			effecter.scale = scale;
			effecter.Trigger(a, new TargetInfo(targetB, map));
			return effecter;
		}

		public Effecter Spawn(IntVec3 target, Map map, Vector3 offset, float scale = 1f)
		{
			Effecter effecter = new Effecter(this);
			TargetInfo targetInfo = new TargetInfo(target, map);
			effecter.scale = scale;
			effecter.offset = offset;
			effecter.Trigger(targetInfo, targetInfo);
			return effecter;
		}

		public Effecter Spawn(Thing target, Map map, float scale = 1f)
		{
			Effecter effecter = new Effecter(this);
			effecter.offset = target.TrueCenter() - target.Position.ToVector3Shifted();
			effecter.scale = scale;
			TargetInfo targetInfo = new TargetInfo(target.Position, map);
			effecter.Trigger(targetInfo, targetInfo);
			return effecter;
		}

		public Effecter SpawnAttached(Thing target, Map map, float scale = 1f)
		{
			Effecter effecter = new Effecter(this);
			effecter.offset = target.TrueCenter() - target.Position.ToVector3Shifted();
			effecter.scale = scale;
			effecter.Trigger(target, target);
			return effecter;
		}

		public Effecter Spawn(Thing target, Map map, Vector3 offset)
		{
			Effecter effecter = new Effecter(this);
			effecter.offset = offset;
			TargetInfo targetInfo = new TargetInfo(target.Position, map);
			effecter.Trigger(targetInfo, targetInfo);
			return effecter;
		}
	}
}
