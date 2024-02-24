using System.Collections.Generic;
using UWE;
using UnityEngine;

public class EcoRegionManager
{
	private static EcoRegionManager _main;

	private const int kNumXZRegions = 256;

	private const int kNumYRegions = 128;

	private Int3 regionBoundsMax = new Int3(255, 127, 255);

	private readonly float kRegionSize = 16f;

	private readonly float kMaxAboveWaterHeight = 100f;

	private Bounds ecoRegionsBounds;

	private readonly Dictionary<Int3, EcoRegion> regionMap = new Dictionary<Int3, EcoRegion>();

	private EcoRegion cameraRegion;

	private Int3 cameraRegionIndices;

	private readonly EcoRegionPool regionPool;

	public static EcoRegionManager main
	{
		get
		{
			if (_main == null)
			{
				_main = new EcoRegionManager();
			}
			return _main;
		}
	}

	public static void Deinitialize()
	{
		_main = null;
	}

	private EcoRegionManager()
	{
		float num = 256f * kRegionSize * 0.5f;
		float num2 = 128f * kRegionSize * 0.5f;
		ecoRegionsBounds = new Bounds
		{
			center = new Vector3(0f, kMaxAboveWaterHeight - num2, 0f),
			extents = new Vector3(num, num2, num)
		};
		regionPool = new EcoRegionPool();
	}

	private EcoRegion CreateRegion(Int3 pos)
	{
		EcoRegion ecoRegion = regionPool.Get();
		Vector3 cornerPos = new Vector3(pos.x, pos.y, pos.z) * kRegionSize + ecoRegionsBounds.min;
		ecoRegion.Initialize(cornerPos, kRegionSize, pos);
		return ecoRegion;
	}

	private bool CheckBounds(Int3 index)
	{
		if (index.x >= 0 && index.x < 256 && index.y >= 0 && index.y < 128 && index.z >= 0)
		{
			return index.z < 256;
		}
		return false;
	}

	private EcoRegion GetInitializedRegion(Int3 index)
	{
		try
		{
			if (!CheckBounds(index))
			{
				return null;
			}
			if (!regionMap.TryGetValue(index, out var value))
			{
				value = CreateRegion(index);
				regionMap.Add(index, value);
			}
			return value;
		}
		finally
		{
		}
	}

	private bool GetRegionXYZ(Vector3 pos, out Int3 index)
	{
		bool result = false;
		if (ecoRegionsBounds.Contains(pos))
		{
			index.x = (int)((pos.x - ecoRegionsBounds.min.x) / kRegionSize);
			index.y = (int)((pos.y - ecoRegionsBounds.min.y) / kRegionSize);
			index.z = (int)((pos.z - ecoRegionsBounds.min.z) / kRegionSize);
			index = index.Clamp(Int3.zero, regionBoundsMax);
			result = true;
		}
		else
		{
			index = Int3.zero;
		}
		return result;
	}

	public EcoRegion GetRegionIfExists(Int3 index)
	{
		EcoRegion value = null;
		if (CheckBounds(index))
		{
			regionMap.TryGetValue(index, out value);
		}
		return value;
	}

	public EcoRegion GetRegion(Vector3 pos, EcoRegion currentRegion = null)
	{
		EcoRegion result = null;
		if (GetRegionXYZ(pos, out var index))
		{
			if (currentRegion != null && index == currentRegion.listIndices)
			{
				return currentRegion;
			}
			result = GetInitializedRegion(index);
		}
		return result;
	}

	private void UpdateCameraRegion()
	{
		if (GetRegionXYZ(MainCamera.camera.transform.position, out var index) && index != cameraRegionIndices)
		{
			EcoRegion initializedRegion = GetInitializedRegion(index);
			if (initializedRegion != null)
			{
				cameraRegion = initializedRegion;
				cameraRegionIndices = index;
				cameraRegion.DrawDebug(Color.green, Time.deltaTime, depthTest: false);
			}
		}
	}

	public void EcoUpdate()
	{
		UpdateCameraRegion();
		regionPool.Update();
	}

	public IEcoTarget FindNearestTargetPhysicsQuery(EcoTargetType type, Vector3 position, float radius, EcoRegion.TargetFilter filter, ref float outTargetDistanceSqr, ref Collider outCollider)
	{
		int num = UWE.Utils.OverlapSphereIntoSharedBuffer(position, radius);
		IEcoTarget result = null;
		float num2 = float.MaxValue;
		for (int i = 0; i < num; i++)
		{
			IEcoTarget component = UWE.Utils.sharedColliderBuffer[i].gameObject.GetComponent<IEcoTarget>();
			if (component != null && component.GetTargetType() == type && (filter == null || filter(component)))
			{
				float sqrMagnitude = (UWE.Utils.sharedColliderBuffer[i].transform.position - position).sqrMagnitude;
				if (sqrMagnitude < num2)
				{
					result = component;
					num2 = (outTargetDistanceSqr = sqrMagnitude);
					outCollider = UWE.Utils.sharedColliderBuffer[i];
				}
			}
		}
		return result;
	}

	public IEcoTarget FindNearestTarget(EcoTargetType type, Vector3 wsPos, EcoRegion.TargetFilter isTargetValid = null, int maxRings = 1)
	{
		float distance;
		return FindNearestTarget(type, wsPos, out distance, isTargetValid, maxRings);
	}

	public IEcoTarget FindNearestTarget(EcoTargetType type, Vector3 wsPos, out float distance, EcoRegion.TargetFilter isTargetValid, int maxRings)
	{
		try
		{
			if (!GetRegionXYZ(wsPos, out var index))
			{
				distance = -1f;
				return null;
			}
			float num = float.MaxValue;
			IEcoTarget ecoTarget = null;
			int num2 = 0;
			for (int i = 0; i <= maxRings && (ecoTarget == null || i <= num2 + 1); i++)
			{
				RingWalker3D ringWalker3D = new RingWalker3D(i);
				while (ringWalker3D.MoveNext())
				{
					Int3 current = ringWalker3D.Current;
					Int3 index2 = index + current;
					EcoRegion regionIfExists = GetRegionIfExists(index2);
					if (regionIfExists != null)
					{
						float bestDist = float.MaxValue;
						IEcoTarget best = null;
						regionIfExists.FindNearestTargetSqr(type, wsPos, isTargetValid, ref bestDist, ref best);
						if (bestDist < num)
						{
							num = bestDist;
							ecoTarget = best;
							num2 = i;
						}
					}
				}
			}
			distance = num;
			return ecoTarget;
		}
		finally
		{
		}
	}
}
