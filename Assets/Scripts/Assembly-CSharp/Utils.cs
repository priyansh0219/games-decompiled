using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FMOD.Studio;
using FMODUnity;
using UWE;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class Utils
{
	public class MonitoredValue<T> where T : IEquatable<T>
	{
		public Event<MonitoredValue<T>> changedEvent = new Event<MonitoredValue<T>>();

		public T value { get; private set; }

		public T prevValue { get; private set; }

		public void Update(T newValue)
		{
			prevValue = value;
			value = newValue;
			if (!prevValue.Equals(newValue))
			{
				changedEvent.Trigger(this);
			}
		}

		public bool UsedToBe(T oldValue)
		{
			if (prevValue.Equals(oldValue))
			{
				return !value.Equals(oldValue);
			}
			return false;
		}
	}

	public class ScalarMonitor
	{
		public float prevValue { get; private set; }

		public float currValue { get; private set; }

		public float Get()
		{
			return currValue;
		}

		public ScalarMonitor(float initValue)
		{
			Init(initValue);
		}

		public void Init(float initValue)
		{
			prevValue = initValue;
			currValue = initValue;
		}

		public void Update(float currValue)
		{
			prevValue = this.currValue;
			this.currValue = currValue;
		}

		public void Add(float delta)
		{
			Update(Get() + delta);
		}

		public void AddClamp01(float delta)
		{
			float num = Mathf.Clamp01(Get() + delta);
			Update(num);
		}

		public bool MoveTowards(float target, float maxAbsDelta)
		{
			float f = target - currValue;
			if (Mathf.Abs(f) < maxAbsDelta)
			{
				Update(target);
				return true;
			}
			Update(currValue + Mathf.Sign(f) * maxAbsDelta);
			return false;
		}

		public int GetNumIntervalsPassed(float intervalSize, float offset = 0f)
		{
			int num = Mathf.FloorToInt((prevValue + offset) / intervalSize);
			return Mathf.FloorToInt((currValue + offset) / intervalSize) - num;
		}

		public bool DidChangeInterval(float intervalSize, float offset = 0f)
		{
			return Mathf.Abs(GetNumIntervalsPassed(intervalSize, offset)) > 0;
		}

		public bool JustDroppedBelow(float thresh)
		{
			if (prevValue >= thresh)
			{
				return currValue < thresh;
			}
			return false;
		}

		public bool JustWentAbove(float thresh)
		{
			if (prevValue <= thresh)
			{
				return currValue > thresh;
			}
			return false;
		}
	}

	public struct SystemStopWatch
	{
		private enum State
		{
			Stopped = 0,
			Paused = 1,
			Running = 2
		}

		private double runTime;

		private double elapsed;

		private State state;

		public void Reset()
		{
			runTime = GetSystemTime();
			elapsed = 0.0;
			state = State.Running;
		}

		public void Pause()
		{
			elapsed += GetSystemTime() - runTime;
			state = State.Paused;
		}

		public void Resume()
		{
			runTime = GetSystemTime();
			state = State.Running;
		}

		public void Stop()
		{
			if (state == State.Running)
			{
				elapsed += GetSystemTime() - runTime;
			}
			state = State.Stopped;
		}

		public double GetElapsed()
		{
			if (state == State.Running)
			{
				return elapsed + GetSystemTime() - runTime;
			}
			return elapsed;
		}

		public double GetElapsedSeconds()
		{
			return GetElapsed() / 1000.0;
		}
	}

	public class GraphNode<T>
	{
		private readonly HashSet<GraphNode<T>> neighbors = new HashSet<GraphNode<T>>();

		public T data { get; private set; }

		public GraphNode(T data)
		{
			this.data = data;
		}

		public int GetNumNeighbors()
		{
			return neighbors.Count;
		}

		public bool IsNeighbor(GraphNode<T> nbor)
		{
			return neighbors.Contains(nbor);
		}

		public ICollection<GraphNode<T>> GetNeighbors()
		{
			return neighbors;
		}

		public static bool Connect(GraphNode<T> a, GraphNode<T> b)
		{
			a.neighbors.Add(b);
			return b.neighbors.Add(a);
		}
	}

	public class HierarchyNode<T>
	{
		private readonly HashSet<HierarchyNode<T>> children = new HashSet<HierarchyNode<T>>();

		public HierarchyNode<T> parent { get; private set; }

		public T data { get; private set; }

		public HierarchyNode(T data)
		{
			this.data = data;
			parent = null;
		}

		public bool IsChild(HierarchyNode<T> child)
		{
			return children.Contains(child);
		}

		public static bool Connect(HierarchyNode<T> child, HierarchyNode<T> parent)
		{
			child.parent = parent;
			return parent.children.Add(child);
		}
	}

	public delegate bool Grid3DFunc<T>(Int3 pos, T cell);

	public class SimpleWatch
	{
		private float _elapsed;

		public float _duration;

		private bool _isRunning;

		public bool isComplete => _elapsed >= _duration;

		public bool isRunning => _isRunning;

		public float elapsed => _elapsed;

		public float duration
		{
			get
			{
				return _duration;
			}
			set
			{
				_duration = value;
			}
		}

		public float fraction => Mathf.Clamp01(_elapsed / _duration);

		public SimpleWatch()
		{
		}

		public SimpleWatch(float duration)
		{
			_duration = duration;
		}

		public void Restart(float duration)
		{
			_elapsed = 0f;
			_duration = duration;
			_isRunning = true;
		}

		public void Restart()
		{
			_elapsed = 0f;
			_isRunning = true;
		}

		public void Stop()
		{
			_isRunning = false;
		}

		public void Resume()
		{
			if (!isComplete)
			{
				_isRunning = true;
			}
		}

		public void Tick(float deltaTime)
		{
			if (_isRunning && !isComplete)
			{
				_elapsed += deltaTime;
			}
		}
	}

	public static int[] FacingDX = new int[4] { 0, 1, 0, -1 };

	public static int[] FacingDY = new int[4] { 1, 0, -1, 0 };

	public static Facing[] FacingValues = new Facing[4]
	{
		Facing.North,
		Facing.East,
		Facing.South,
		Facing.West
	};

	public static string[] FacingStrings = new string[4] { "North", "East", "South", "West" };

	private static GameMode legacyGameMode;

	private static bool continueMode = false;

	private const string lootCubePath = "WorldEntities/Natural/LootCube.prefab";

	private static GameObject _genericLootPrefab;

	public static GameObject genericLootPrefab => _genericLootPrefab;

	public static GameObject FindChild(GameObject obj, string childName)
	{
		GameObject gameObject = null;
		foreach (Transform item in obj.transform)
		{
			gameObject = FindChild(item.gameObject, childName);
			if (gameObject == null)
			{
				if (item.gameObject.name == childName)
				{
					return item.gameObject;
				}
				continue;
			}
			break;
		}
		return gameObject;
	}

	public static SubRoot GetSubRoot()
	{
		SubRoot result = null;
		GameObject localPlayer = GetLocalPlayer();
		if (localPlayer != null)
		{
			result = localPlayer.GetComponent<Player>().GetCurrentSub();
		}
		return result;
	}

	public static bool IsAncestorOf(GameObject ancestor, GameObject obj)
	{
		while (true)
		{
			if (ancestor == obj)
			{
				return true;
			}
			if (obj.transform.parent == null)
			{
				break;
			}
			obj = obj.transform.parent.gameObject;
		}
		return false;
	}

	public static C FindAncestorWithComponent<C>(GameObject go) where C : Component
	{
		return go.FindAncestor<C>();
	}

	public static C FindEnabledAncestorWithComponent<C>(GameObject go) where C : Behaviour
	{
		return go.FindEnabledAncestor<C>();
	}

	public static Transform FindAncestorWithInterface<C>(this GameObject go)
	{
		Transform transform = go.transform;
		while (transform != null && transform.gameObject.GetComponent(typeof(C)) == null)
		{
			transform = transform.parent;
		}
		return transform;
	}

	public static VFXSurfaceTypes GetObjectSurfaceType(GameObject obj, VFXSurfaceTypes defaultSurfaceType = VFXSurfaceTypes.none)
	{
		VFXSurfaceTypes result = defaultSurfaceType;
		if ((bool)obj)
		{
			VFXSurface component = obj.GetComponent<VFXSurface>();
			if ((bool)component)
			{
				result = component.surfaceType;
			}
		}
		return result;
	}

	public static VFXSurfaceTypes GetTerrainSurfaceType(Vector3 position, Vector3 normal, VFXSurfaceTypes defaultSurfaceType = VFXSurfaceTypes.none)
	{
		VFXSurfaceTypes result = defaultSurfaceType;
		string terrainMaterial = MaterialDatabase.GetTerrainMaterial(position, normal);
		if (!string.IsNullOrEmpty(terrainMaterial))
		{
			try
			{
				result = (VFXSurfaceTypes)Enum.Parse(typeof(VFXSurfaceTypes), terrainMaterial, ignoreCase: true);
			}
			catch
			{
			}
		}
		return result;
	}

	public static bool NearlyEqual(float a, float b, float epsilon = float.Epsilon)
	{
		try
		{
			float num = Mathf.Abs(a);
			float num2 = Mathf.Abs(b);
			float num3 = Mathf.Abs(a - b);
			if (a == b)
			{
				return true;
			}
			if (a == 0f || b == 0f || num3 < float.MinValue)
			{
				return num3 < epsilon * float.MinValue;
			}
			return num3 / (num + num2) < epsilon;
		}
		finally
		{
		}
	}

	public static bool EqualWithinDelta(int a, int b, int delta)
	{
		if (a == b)
		{
			return true;
		}
		return Mathf.Abs(a - b) < delta;
	}

	public static float Gaussian2D(Vector2 p, Vector2 sigma)
	{
		return Mathf.Exp(-1f * (p.x * p.x / (2f * sigma.x * sigma.x) + p.y * p.y / (2f * sigma.y * sigma.y)));
	}

	public static float GaussianSample2D(float[,] array, int xSize, int ySize, int x, int y, int radius)
	{
		if (radius == 0)
		{
			return array[x, y];
		}
		float num = (float)radius / 3f;
		Vector2 sigma = new Vector2(num, num);
		float num2 = 0f;
		float num3 = 0f;
		for (int i = -radius; i <= radius; i++)
		{
			for (int j = -radius; j <= radius; j++)
			{
				int num4 = x + i;
				int num5 = y + j;
				if (num4 >= 0 && num4 < xSize && num5 >= 0 && num5 < ySize)
				{
					float num6 = Gaussian2D(new Vector2(i, j), sigma);
					num2 += num6 * array[num4, num5];
					num3 += num6;
				}
			}
		}
		return num2 / num3;
	}

	public static Vector3 XProjection(Vector3 v)
	{
		return new Vector3(v.x, 0f, 0f);
	}

	public static Vector3 YProjection(Vector3 v)
	{
		return new Vector3(0f, v.y, 0f);
	}

	public static Vector3 ZProjection(Vector3 v)
	{
		return new Vector3(0f, 0f, v.z);
	}

	public static Vector3 SampleDiscXZ(float maxLength)
	{
		float f = UnityEngine.Random.value * 2f * (float)System.Math.PI;
		float num = UnityEngine.Random.value + UnityEngine.Random.value;
		float num2 = ((num > 1f) ? (2f - num) : num);
		return new Vector3(num2 * Mathf.Cos(f), 0f, num2 * Mathf.Sin(f)) * maxLength;
	}

	public static Vector2 SampleDisc2D(float maxLength)
	{
		float f = UnityEngine.Random.value * 2f * (float)System.Math.PI;
		float num = UnityEngine.Random.value + UnityEngine.Random.value;
		float num2 = ((num > 1f) ? (2f - num) : num);
		return new Vector2(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f)) * maxLength;
	}

	public static Vector2 SampleCircle2D(float radius)
	{
		return PolarToVec2(UnityEngine.Random.value * 2f * (float)System.Math.PI, radius);
	}

	public static Vector3 SampleXZCircle(float radius)
	{
		Vector2 vector = SampleCircle2D(radius);
		return new Vector3(vector.x, 0f, vector.y);
	}

	public static Vector2 PolarToVec2(float angle, float radius)
	{
		return new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
	}

	public static Vector2 SampleAnnulus(float minRadius, float maxRadius)
	{
		float angle = UnityEngine.Random.value * 2f * (float)System.Math.PI;
		float radius = Mathf.Sqrt(UnityEngine.Random.value * (minRadius * minRadius - maxRadius * maxRadius) + maxRadius * maxRadius);
		return PolarToVec2(angle, radius);
	}

	public static Quaternion GetRandomYawQuat()
	{
		return Quaternion.AngleAxis(UnityEngine.Random.Range(-180, 180), Vector3.up);
	}

	public static List<float> GetRandomValues(int num)
	{
		List<float> list = new List<float>();
		for (int i = 0; i < num; i++)
		{
			list.Add(UnityEngine.Random.value);
		}
		return list;
	}

	public static float GetRandomValue(float val, float variance)
	{
		return val + UnityEngine.Random.Range(0f - variance, variance);
	}

	public static float GetRandomValueClamped(float val, float variance, float min, float max)
	{
		return Mathf.Clamp(val + UnityEngine.Random.Range(0f - variance, variance), min, max);
	}

	public static Quaternion GetRandomSpinAbout(Vector3 dir)
	{
		return Quaternion.AngleAxis(Mathf.Lerp(0f, 360f, UnityEngine.Random.value), dir) * Quaternion.FromToRotation(Vector3.up, dir);
	}

	public static Quaternion GetRandomLimitedRotation(float maxDegs)
	{
		Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
		return Quaternion.AngleAxis(maxDegs * UnityEngine.Random.value, onUnitSphere);
	}

	public static Vector3 SampleSphere(float radius)
	{
		Vector3 result = default(Vector3);
		do
		{
			result.x = Mathf.Lerp(0f - radius, radius, UnityEngine.Random.value);
			result.y = Mathf.Lerp(0f - radius, radius, UnityEngine.Random.value);
			result.z = Mathf.Lerp(0f - radius, radius, UnityEngine.Random.value);
		}
		while (result.magnitude > radius);
		return result;
	}

	public static bool CompareTechType(GameObject go1, GameObject go2)
	{
		TechType techType = CraftData.GetTechType(go1);
		TechType techType2 = CraftData.GetTechType(go2);
		if (techType != 0 && techType2 != 0)
		{
			return techType == techType2;
		}
		return false;
	}

	public static GameObject GetLocalPlayer()
	{
		if (Player.main == null)
		{
			return null;
		}
		return Player.main.gameObject;
	}

	public static Player GetLocalPlayerComp()
	{
		return Player.main;
	}

	public static Vector3 GetLocalPlayerPos()
	{
		if (Player.main == null)
		{
			return Vector3.zero;
		}
		return Player.main.transform.position;
	}

	public static void SetLayerRecursively(GameObject obj, int newLayer, bool renderersOnly = true, int excludeLayer = -1)
	{
		if ((!renderersOnly || obj.GetComponent<Renderer>() != null || obj.GetComponent<Canvas>() != null) && obj.layer != excludeLayer)
		{
			obj.layer = newLayer;
		}
		foreach (Transform item in obj.transform)
		{
			SetLayerRecursively(item.gameObject, newLayer, renderersOnly, excludeLayer);
		}
	}

	public static GameObject SpawnFromPrefab(GameObject prefab, Transform parent)
	{
		GameObject gameObject = UWE.Utils.InstantiateWrap(prefab, prefab.transform.position, prefab.transform.rotation);
		gameObject.transform.parent = parent;
		return gameObject;
	}

	public static GameObject SpawnPrefabAt(GameObject prefab, Transform parent, Vector3 pos)
	{
		GameObject gameObject = SpawnFromPrefab(prefab, parent);
		gameObject.transform.position = pos;
		return gameObject;
	}

	public static GameObject SpawnZeroedAt(GameObject prefab, Transform parent, bool keepScale = false)
	{
		GameObject gameObject = SpawnFromPrefab(prefab, parent);
		if (keepScale)
		{
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
		}
		else
		{
			ZeroTransform(gameObject.transform);
		}
		return gameObject;
	}

	public static void ZeroTransform(Transform t)
	{
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;
		t.localScale = new Vector3(1f, 1f, 1f);
	}

	public static void CreateNPrefabs(GameObject prefab, float maxDist = 12f, int number = 1)
	{
		for (int i = 0; i < number; i++)
		{
			CreatePrefab(prefab, maxDist, i > 0);
		}
	}

	public static GameObject CreatePrefab(GameObject prefab, float maxDist = 12f, bool randomizeDirection = false)
	{
		Transform transform = MainCamera.camera.transform;
		Vector3 forward = transform.forward;
		Vector3 position = transform.position;
		Vector3 vector = forward;
		if (randomizeDirection)
		{
			float num = 0.5f;
			vector = (forward + transform.right * (UnityEngine.Random.value - 0.5f) * num / 2f + transform.up * (UnityEngine.Random.value - 0.5f) * num / 2f).normalized;
		}
		Vector3 vector2 = position + maxDist * vector;
		Vector3 toDirection = Vector3.up;
		Vector3 vector3 = position + vector;
		if (Physics.Raycast(vector3, vector, out var hitInfo, maxDist))
		{
			vector2 = hitInfo.point;
			toDirection = hitInfo.normal;
		}
		Debug.DrawLine(vector3, vector2, Color.green, 10f);
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab, vector2, Quaternion.FromToRotation(Vector3.up, toDirection));
		gameObject.SetActive(value: true);
		return gameObject;
	}

	public static IEnumerator CreateNPrefabs(TechType techType, float maxDist = 12f, int number = 1)
	{
		CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
		yield return request;
		GameObject result = request.GetResult();
		if (result == null)
		{
			ErrorMessage.AddDebug("Could not find prefab for tech type = " + techType);
		}
		else
		{
			CreateNPrefabs(result, maxDist, number);
		}
	}

	public static void DebugDrawCircleGrid(Vector3 center, float radius, int radialSegs, int circSegs, Color c, float time = 0f, bool depthTest = true)
	{
		for (int i = 0; i < circSegs; i++)
		{
			float f = (float)System.Math.PI * 2f / (float)circSegs * (float)i;
			float f2 = (float)System.Math.PI * 2f / (float)circSegs * (float)(i + 1);
			Debug.DrawLine(center, center + new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f)) * radius, c, time, depthTest);
			for (int j = 0; j < radialSegs; j++)
			{
				float num = radius / (float)radialSegs * (float)(j + 1);
				Debug.DrawLine(center + new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f)) * num, center + new Vector3(Mathf.Cos(f2), 0f, Mathf.Sin(f2)) * num, c, time, depthTest);
			}
		}
	}

	public static void DebugDrawAxis(Transform trans, float scale)
	{
		Vector3 position = trans.position;
		Debug.DrawLine(position, position + scale * trans.right, Color.red);
		Debug.DrawLine(position, position + scale * trans.up, Color.green);
		Debug.DrawLine(position, position + scale * trans.forward, Color.blue);
	}

	public static void DebugDrawStar(Vector3 pos, float radius, Color c, float time = 0f)
	{
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				for (int k = -1; k <= 1; k++)
				{
					if (i != 0 || j != 0 || k != 0)
					{
						DebugRay(pos, new Vector3(i, j, k).normalized, radius, c, time);
					}
				}
			}
		}
	}

	public static void DebugDrawGrid(Vector3 origin, Vector3 topLeft, Vector3 bottomRight, int res, Color c, float time = 0f, bool depthTest = true)
	{
		Vector3 vector = topLeft - origin;
		for (int i = 0; i < res + 1; i++)
		{
			Vector3 start = origin + i * vector / res;
			Vector3 end = bottomRight + i * vector / res;
			Debug.DrawLine(start, end, c, time, depthTest);
		}
		Vector3 vector2 = bottomRight - origin;
		for (int j = 0; j < res + 1; j++)
		{
			Vector3 start2 = origin + j * vector2 / res;
			Vector3 end2 = topLeft + j * vector2 / res;
			Debug.DrawLine(start2, end2, c, time, depthTest);
		}
	}

	public static void DebugDrawBox(Vector3 lsMin, Vector3 lsMax, int res, Transform transform, Color c, float time = 0f, bool depthTest = true)
	{
		Vector3 v = lsMax - lsMin;
		Vector3 lossyScale = transform.lossyScale;
		Vector3 vector = lossyScale.x * transform.TransformDirection(XProjection(v));
		Vector3 vector2 = lossyScale.y * transform.TransformDirection(YProjection(v));
		Vector3 vector3 = lossyScale.z * transform.TransformDirection(ZProjection(v));
		Vector3 vector4 = transform.TransformPoint(lsMin);
		Vector3 vector5 = transform.TransformPoint(lsMax);
		DebugDrawGrid(vector4, vector4 + vector2, vector4 + vector, res, c, time, depthTest);
		DebugDrawGrid(vector4, vector4 + vector3, vector4 + vector, res, c, time, depthTest);
		DebugDrawGrid(vector4, vector4 + vector2, vector4 + vector3, res, c, time, depthTest);
		DebugDrawGrid(vector5, vector5 - vector2, vector5 - vector, res, c, time, depthTest);
		DebugDrawGrid(vector5, vector5 - vector3, vector5 - vector, res, c, time, depthTest);
		DebugDrawGrid(vector5, vector5 - vector2, vector5 - vector3, res, c, time, depthTest);
	}

	public static void DebugDrawAABB(Vector3 min, Vector3 max, int res, Color c, float time = 0f, bool depthTest = true)
	{
		Vector3 v = max - min;
		Vector3 vector = XProjection(v);
		Vector3 vector2 = YProjection(v);
		Vector3 vector3 = ZProjection(v);
		DebugDrawGrid(min, min + vector2, min + vector, res, c, time, depthTest);
		DebugDrawGrid(min, min + vector3, min + vector, res, c, time, depthTest);
		DebugDrawGrid(min, min + vector2, min + vector3, res, c, time, depthTest);
		DebugDrawGrid(max, max - vector2, max - vector, res, c, time, depthTest);
		DebugDrawGrid(max, max - vector3, max - vector, res, c, time, depthTest);
		DebugDrawGrid(max, max - vector2, max - vector3, res, c, time, depthTest);
	}

	public static void DebugDrawOBounds(OBounds bounds, Color c, float time = 0f, bool depthTest = true)
	{
		Vector3 vector = bounds.extents.x * bounds.xAxis;
		Vector3 vector2 = bounds.extents.y * bounds.yAxis;
		Vector3 vector3 = bounds.extents.z * bounds.zAxis;
		Vector3 vector4 = bounds.center - vector - vector2 - vector3;
		Vector3 vector5 = bounds.center + vector - vector2 - vector3;
		Vector3 vector6 = bounds.center + vector - vector2 + vector3;
		Vector3 vector7 = bounds.center - vector - vector2 + vector3;
		Vector3 vector8 = bounds.center - vector + vector2 - vector3;
		Vector3 vector9 = bounds.center + vector + vector2 - vector3;
		Vector3 vector10 = bounds.center + vector + vector2 + vector3;
		Vector3 vector11 = bounds.center - vector + vector2 + vector3;
		Debug.DrawLine(vector4, vector5, c, time, depthTest);
		Debug.DrawLine(vector5, vector6, c, time, depthTest);
		Debug.DrawLine(vector6, vector7, c, time, depthTest);
		Debug.DrawLine(vector7, vector4, c, time, depthTest);
		Debug.DrawLine(vector8, vector9, c, time, depthTest);
		Debug.DrawLine(vector9, vector10, c, time, depthTest);
		Debug.DrawLine(vector10, vector11, c, time, depthTest);
		Debug.DrawLine(vector11, vector8, c, time, depthTest);
		Debug.DrawLine(vector4, vector8, c, time, depthTest);
		Debug.DrawLine(vector5, vector9, c, time, depthTest);
		Debug.DrawLine(vector6, vector10, c, time, depthTest);
		Debug.DrawLine(vector7, vector11, c, time, depthTest);
	}

	public static void DebugRay(Vector3 origin, Vector3 dir, float length, Color c, float time = 0f)
	{
		Debug.DrawLine(origin, origin + length * dir.normalized, c, time);
	}

	public static void FlipX(GameObject obj)
	{
		Vector3 localScale = obj.transform.localScale;
		localScale.x = -1f;
		obj.transform.localScale = localScale;
	}

	public static void FlipZ(GameObject obj)
	{
		Vector3 localScale = obj.transform.localScale;
		localScale.z = -1f;
		obj.transform.localScale = localScale;
	}

	public static string RemoveClonedName(string objName)
	{
		string input = new Regex("[^a-zA-Z0-9]").Replace(objName, "");
		return new Regex("(Clone)").Replace(input, "");
	}

	public static float Vector3InverseLerp(Vector3 a, Vector3 b, Vector3 value)
	{
		Vector3 vector = b - a;
		return Vector3.Dot(value - a, vector) / Vector3.Dot(vector, vector);
	}

	public static Facing GetOppositeFacing(Facing f)
	{
		return (Facing)((int)(f + 2) % 4);
	}

	public static Facing GetNextFacingCW(Facing f)
	{
		return (Facing)((int)(f + 1) % 4);
	}

	public static float GetDegreesFromNorth(Facing f)
	{
		switch (f)
		{
		case Facing.North:
			return 0f;
		case Facing.East:
			return 90f;
		case Facing.South:
			return 180f;
		default:
			return -90f;
		}
	}

	public static Int2 RotatedSize(Int2 northFacingSize, Facing newFacing)
	{
		Int2 result = northFacingSize;
		switch (newFacing)
		{
		case Facing.North:
		case Facing.South:
			return result;
		case Facing.East:
		case Facing.West:
			return new Int2(result.y, result.x);
		default:
			return result;
		}
	}

	public static Facing WorldToLocalFacing(Facing myFacing, Facing globalFacing)
	{
		return (Facing)(((int)globalFacing + (int)myFacing) % 4);
	}

	public static T ChooseRandom<T>(T[] items)
	{
		return items[UnityEngine.Random.Range(0, items.Length)];
	}

	public static T ChooseRandom<T>(List<T> items)
	{
		return items[UnityEngine.Random.Range(0, items.Count)];
	}

	public static float AddAngleWithinCCWBounds(float startDegs, float deltaDegs, float minDegs, float maxDegs)
	{
		MakeAngleInCCWBounds(ref startDegs, minDegs, maxDegs);
		return Mathf.Clamp(startDegs + deltaDegs, minDegs, maxDegs);
	}

	public static bool MakeAngleInCCWBounds(ref float degs, float minDegs, float maxDegs)
	{
		MakeCCWBounds(ref minDegs, ref maxDegs);
		while (degs < minDegs)
		{
			if ((double)Mathf.Abs(degs - minDegs) < 0.0001)
			{
				degs = minDegs;
			}
			else
			{
				degs += 360f;
			}
		}
		while (degs > maxDegs)
		{
			if ((double)Mathf.Abs(degs - maxDegs) < 0.0001)
			{
				degs = maxDegs;
			}
			else
			{
				degs -= 360f;
			}
		}
		if (degs <= maxDegs)
		{
			return degs >= minDegs;
		}
		return false;
	}

	public static void MakeCCWBounds(ref float minDegs, ref float maxDegs)
	{
		while (maxDegs < minDegs)
		{
			maxDegs += 360f;
		}
		while (minDegs + 360f <= maxDegs)
		{
			minDegs += 360f;
		}
	}

	public static float AddClamp01(float x, float delta)
	{
		return Mathf.Clamp01(x + delta);
	}

	public static void AddClamp01(ref float x, float delta)
	{
		x = Mathf.Clamp01(x + delta);
	}

	public static Vector3 TerrainToWorldPoint(Terrain terrain, float u, float v)
	{
		Vector3 position = terrain.GetPosition();
		Vector3 size = terrain.terrainData.size;
		float x = position.x + size.x * v;
		float z = position.z + size.z * u;
		Vector3 vector = new Vector3(x, position.y + size.y, z);
		vector.y = position.y + terrain.SampleHeight(vector);
		return vector;
	}

	public static Vector2 WorldToTerrainUV(Terrain terrain, float x, float z)
	{
		Vector3 position = terrain.GetPosition();
		Vector3 size = terrain.terrainData.size;
		float y = (x - position.x) / size.x;
		return new Vector2((z - position.z) / size.z, y);
	}

	public static void GUIDrawTextureCentered(Texture tex, Vector2 ssCenter, float fractionOfHeight)
	{
		int num = Mathf.FloorToInt((float)Screen.height * fractionOfHeight);
		GUI.DrawTexture(new Rect(ssCenter.x - (float)(num / 2), ssCenter.y - (float)(num / 2), num, num), tex, ScaleMode.ScaleToFit, alphaBlend: true, 0f);
	}

	public static void GUIDrawTextureInCorner(Texture tex, float fractionOfHeight, bool isLeft, bool isTop, Vector2 relOffset)
	{
		int width = Screen.width;
		int height = Screen.height;
		int num = Mathf.FloorToInt((float)height * fractionOfHeight);
		GUI.DrawTexture(new Rect((float)((!isLeft) ? (width - num) : 0) + relOffset.x * (float)num, (float)((!isTop) ? (height - num) : 0) + relOffset.y * (float)num, num, num), tex, ScaleMode.ScaleToFit, alphaBlend: true, 0f);
	}

	public static Vector2 GetPointInRect(Rect r, Vector2 uv)
	{
		return new Vector2(r.x + uv.x * r.width, r.y + uv.y * r.height);
	}

	public static Rect MakeCenteredRect(Vector2 center, float size)
	{
		return new Rect(center.x - size / 2f, center.y - size / 2f, size, size);
	}

	public static void SetBoundsMax(Renderer render, Vector3 wsMax)
	{
		Vector3 position = render.gameObject.transform.position;
		Vector3 vector = render.bounds.max - position;
		render.gameObject.transform.position = wsMax - vector;
	}

	public static void Sandwich(MeshFilter mesh, float yMin, float yMax)
	{
		if (!(yMin > yMax))
		{
			Vector3 localPosition = mesh.transform.localPosition;
			Vector3 localScale = mesh.transform.localScale;
			localPosition.y = (yMin + yMax) / 2f;
			localScale.y = (yMax - yMin) / mesh.mesh.bounds.size.y;
			mesh.transform.localPosition = localPosition;
			mesh.transform.localScale = localScale;
		}
	}

	public static float AngleXZ(Vector3 a, Vector3 b)
	{
		Vector3 from = new Vector3(a.x, 0f, a.z);
		Vector3 to = new Vector3(b.x, 0f, b.z);
		return Vector3.Angle(from, to);
	}

	public static void DrawOutline(Rect rect, string text, GUIStyle style, Color outColor, Color inColor, float size)
	{
		float num = size * 0.5f;
		GUIStyle gUIStyle = new GUIStyle(style);
		Color color = GUI.color;
		style.normal.textColor = outColor;
		GUI.color = outColor;
		rect.x -= num;
		GUI.Label(rect, text, style);
		rect.x += size;
		GUI.Label(rect, text, style);
		rect.x -= num;
		rect.y -= num;
		GUI.Label(rect, text, style);
		rect.y += size;
		GUI.Label(rect, text, style);
		rect.y -= num;
		style.normal.textColor = inColor;
		GUI.color = color;
		GUI.Label(rect, text, style);
		style = gUIStyle;
	}

	public static void DrawShadow(Rect rect, GUIContent content, GUIStyle style, Color txtColor, Color shadowColor, Vector2 direction)
	{
		GUIStyle gUIStyle = style;
		style.normal.textColor = shadowColor;
		rect.x += direction.x;
		rect.y += direction.y;
		GUI.Label(rect, content, style);
		style.normal.textColor = txtColor;
		rect.x -= direction.x;
		rect.y -= direction.y;
		GUI.Label(rect, content, style);
		style = gUIStyle;
	}

	public static void DrawLayoutShadow(GUIContent content, GUIStyle style, Color txtColor, Color shadowColor, Vector2 direction, params GUILayoutOption[] options)
	{
		DrawShadow(GUILayoutUtility.GetRect(content, style, options), content, style, txtColor, shadowColor, direction);
	}

	public static bool DrawButtonWithShadow(Rect r, GUIContent content, GUIStyle style, float shadowAlpha, Vector2 direction)
	{
		GUIStyle gUIStyle = new GUIStyle(style);
		gUIStyle.normal.background = null;
		gUIStyle.hover.background = null;
		gUIStyle.active.background = null;
		bool result = GUI.Button(r, content, style);
		DrawShadow(txtColor: r.Contains(Event.current.mousePosition) ? gUIStyle.hover.textColor : gUIStyle.normal.textColor, rect: r, content: content, style: gUIStyle, shadowColor: new Color(0f, 0f, 0f, shadowAlpha), direction: direction);
		return result;
	}

	public static bool DrawLayoutButtonWithShadow(GUIContent content, GUIStyle style, float shadowAlpha, Vector2 direction, params GUILayoutOption[] options)
	{
		return DrawButtonWithShadow(GUILayoutUtility.GetRect(content, style, options), content, style, shadowAlpha, direction);
	}

	public static double GetSystemTime()
	{
		return UWE.Utils.GetSystemTime();
	}

	public static float GetPitchDegs(Vector3 dir)
	{
		return (0f - Mathf.Asin(Mathf.Clamp(dir.y, -1f, 1f))) * 57.29578f;
	}

	public static float GetYawDegs(Vector3 dir)
	{
		if ((double)Mathf.Abs(dir.x) < 0.001 && (double)Mathf.Abs(dir.z) < 0.001)
		{
			return 0f;
		}
		float num = Mathf.Atan2(dir.x, dir.z);
		if (num < 0f)
		{
			num += (float)System.Math.PI * 2f;
		}
		return num * 57.29578f;
	}

	public static void SetForwardNoRoll(Transform trans, Vector3 dir)
	{
		trans.eulerAngles = new Vector3(GetPitchDegs(dir), GetYawDegs(dir), 0f);
	}

	public static int FindMax(int[] ints)
	{
		int num = -1;
		for (int i = 0; i < ints.Length; i++)
		{
			if (num == -1 || ints[i] > ints[num])
			{
				num = i;
			}
		}
		return num;
	}

	public static List<int> CreateRange(int start, int cap)
	{
		List<int> list = new List<int>();
		for (int i = start; i < cap; i++)
		{
			list.Add(i);
		}
		return list;
	}

	public static bool Read(StreamReader reader, IList<int> list)
	{
		string[] array = reader.ReadLine().Split(' ');
		int num = Convert.ToInt32(array[0]);
		if (array.Length != num + 2)
		{
			Debug.LogError("Mismatch between int-list count and actual number of written items");
			return false;
		}
		list.Clear();
		for (int i = 0; i < num; i++)
		{
			list.Add(Convert.ToInt32(array[i + 1]));
		}
		return true;
	}

	public static void Write<T>(StreamWriter writer, IList<T> list)
	{
		string text = list.Count + " ";
		for (int i = 0; i < list.Count; i++)
		{
			text = string.Concat(text, list[i], " ");
		}
		writer.WriteLine(text);
	}

	public static bool Save(string path, IList<int> list)
	{
		StreamWriter streamWriter = FileUtils.CreateTextFile(path);
		if (streamWriter == null)
		{
			return false;
		}
		Write(streamWriter, list);
		streamWriter.Close();
		return true;
	}

	public static bool Load(string path, IList<int> list)
	{
		if (!File.Exists(path))
		{
			return false;
		}
		StreamReader streamReader = FileUtils.ReadTextFile(path);
		if (streamReader == null)
		{
			return false;
		}
		bool result = Read(streamReader, list);
		streamReader.Close();
		return result;
	}

	public static bool SetLegacyGameMode(GameMode gameMode)
	{
		legacyGameMode = gameMode;
		GameModeOption mode = GameModeOption.None;
		switch (legacyGameMode)
		{
		case GameMode.Survival:
			mode = GameModeOption.None;
			break;
		case GameMode.Freedom:
			mode = GameModeOption.NoSurvival;
			break;
		case GameMode.Hardcore:
			mode = GameModeOption.Hardcore;
			break;
		case GameMode.Creative:
			mode = GameModeOption.Creative;
			break;
		}
		return GameModeUtils.SetGameMode(mode, GameModeOption.None);
	}

	public static GameMode GetLegacyGameMode()
	{
		return legacyGameMode;
	}

	public static void SetContinueMode(bool mode)
	{
		continueMode = mode;
	}

	public static bool GetContinueMode()
	{
		return continueMode;
	}

	public static string GetIP()
	{
		IPAddress[] addressList = Dns.GetHostEntry(GetHostName()).AddressList;
		return addressList[addressList.Length - 1].ToString();
	}

	public static string GetHostName()
	{
		return Dns.GetHostName();
	}

	public static string GetUniqueClientId()
	{
		return Md5Sum(GetHostName() + ":" + GetIP());
	}

	public static string Md5Sum(string strToEncrypt)
	{
		byte[] bytes = new UTF8Encoding().GetBytes(strToEncrypt);
		byte[] array = new MD5CryptoServiceProvider().ComputeHash(bytes);
		string text = "";
		for (int i = 0; i < array.Length; i++)
		{
			text += Convert.ToString(array[i], 16).PadLeft(2, '0');
		}
		return text.PadLeft(32, '0');
	}

	public static void ForGrid3D<T>(Array3<T> grid, Grid3DFunc<T> func)
	{
		for (int i = 0; i < grid.sizeX; i++)
		{
			for (int j = 0; j < grid.sizeY; j++)
			{
				for (int k = 0; k < grid.sizeZ; k++)
				{
					if (!func(new Int3(i, j, k), grid[i, j, k]))
					{
						return;
					}
				}
			}
		}
	}

	public static Vector3 GetRandomPosInView(float distribution = 10f)
	{
		return MainCamera.camera.transform.position + MainCamera.camera.transform.forward * UnityEngine.Random.Range(1f, distribution) + UnityEngine.Random.Range(-1f, 1f) * MainCamera.camera.transform.right * distribution / 2f;
	}

	public static void PlayOneShotPS(GameObject particleSystemPrefab, Vector3 position, Quaternion orientation, Transform effectParent = null)
	{
		GameObject gameObject = UWE.Utils.InstantiateWrap(particleSystemPrefab, position, orientation);
		if (effectParent != null)
		{
			gameObject.transform.parent = effectParent;
		}
		PostSpawnPlayOneShotPS(gameObject);
	}

	public static IEnumerator PlayOneShotPSAsync(string key, Vector3 position, Quaternion orientation, Transform effectParent = null, float scale = 1f)
	{
		CoroutineTask<GameObject> task = AddressablesUtility.InstantiateAsync(key, effectParent, position, orientation);
		yield return task;
		GameObject result = task.GetResult();
		if (result != null)
		{
			result.transform.localScale = new Vector3(scale, scale, scale);
			PostSpawnPlayOneShotPS(result);
		}
		else
		{
			Debug.Log("PlayOneShotPS() - Couldn't load particle system from \"" + key + "\"");
		}
	}

	private static void PostSpawnPlayOneShotPS(GameObject instance)
	{
		ParticleSystem component = instance.GetComponent<ParticleSystem>();
		component.Play();
		UWE.Utils.DestroyWrap(instance, component.main.duration);
	}

	public static Color HexStringToColor(string hexString)
	{
		int num = int.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier);
		int num2 = int.Parse(hexString.Substring(2, 2), NumberStyles.AllowHexSpecifier);
		int num3 = int.Parse(hexString.Substring(4, 2), NumberStyles.AllowHexSpecifier);
		int num4 = 255;
		if (hexString.Length == 8)
		{
			num4 = int.Parse(hexString.Substring(6, 2), NumberStyles.AllowHexSpecifier);
		}
		return new Color(num, num2, num3, num4);
	}

	public static string ColorToHexString(Color32 color)
	{
		return color.r.ToString("X2") + color.g.ToString("X2") + color.g.ToString("X2");
	}

	public static void AddMaterial(Renderer renderer, Material m)
	{
		int num = renderer.sharedMaterials.Length;
		Material[] array = new Material[num + 1];
		Array.Copy(renderer.sharedMaterials, array, num);
		Debug.Log("     Adding material to " + renderer.gameObject.name);
		array[num] = m;
		renderer.sharedMaterials = array;
	}

	public static void RemoveMaterial(Renderer renderer, Material m)
	{
		int num = renderer.sharedMaterials.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (renderer.sharedMaterials[i] != m)
			{
				num2++;
			}
		}
		if (num == num2)
		{
			return;
		}
		Material[] array = new Material[num2];
		int num3 = 0;
		for (int j = 0; j < num; j++)
		{
			if (renderer.sharedMaterials[j] != m)
			{
				array[num3++] = renderer.sharedMaterials[j];
			}
		}
		renderer.sharedMaterials = array;
	}

	public static bool ModifyMaterial(GameObject obj, string materialName, bool addShader = true)
	{
		Material material = (Material)Resources.Load(materialName);
		if (!material)
		{
			return false;
		}
		MeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer renderer in componentsInChildren)
		{
			if (addShader)
			{
				AddMaterial(renderer, material);
			}
			else
			{
				RemoveMaterial(renderer, material);
			}
		}
		SkinnedMeshRenderer[] componentsInChildren2 = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer renderer2 in componentsInChildren2)
		{
			if (addShader)
			{
				AddMaterial(renderer2, material);
			}
			else
			{
				RemoveMaterial(renderer2, material);
			}
		}
		return true;
	}

	public static bool IsTerrain(GameObject go)
	{
		if (!go.CompareTag("Terrain"))
		{
			return go.GetComponentInParent<Voxeland>() != null;
		}
		return true;
	}

	[Obsolete("Use PlayFMODAsset instead")]
	public static void PlayEnvSound(string soundName, Vector3 position, float soundRadiusObsolete = 10f)
	{
		PlayEnvSound(soundName, null, position, soundRadiusObsolete);
	}

	[Obsolete("Use PlayFMODAsset instead")]
	public static void PlayEnvSound(string soundName, GameObject sourceObsolete, Vector3 position, float soundRadiusObsolete = 10f)
	{
		FMODUWE.PlayOneShot(soundName, position);
	}

	public static void PlayEnvSound(FMOD_StudioEventEmitter eventEmitter, Vector3 position = default(Vector3), float soundRadiusObsolete = 20f)
	{
		Vector3 position2 = ((position == default(Vector3)) ? eventEmitter.gameObject.transform.position : position);
		float volume = 1f;
		eventEmitter.PlayOneShotNoWorld(position2, volume);
	}

	public static void PlayEnvSound(FMOD_CustomEmitter eventEmitter, Vector3 positionObsolete = default(Vector3), float soundRadiusObsolete = 20f)
	{
		eventEmitter.Play();
	}

	public static void PlayFMODAsset(FMODAsset asset, Transform t, float soundRadiusObsolete = 20f)
	{
		PlayFMODAsset(asset, t.gameObject, t.position, soundRadiusObsolete);
	}

	public static void PlayFMODAsset(FMODAsset asset, Vector3 position, float soundRadiusObsolete = 20f)
	{
		PlayFMODAsset(asset, null, position, soundRadiusObsolete);
	}

	public static void PlayFMODAsset(FMODAsset asset, GameObject sourceObsolete, Vector3 position, float soundRadiusObsolete = 20f)
	{
		if (asset != null)
		{
			FMODUWE.PlayOneShot(asset, position);
		}
	}

	public static EventInstance GetFMODEvent(FMODAsset asset, Transform transform)
	{
		EventInstance @event = FMODUWE.GetEvent(asset);
		if (@event.hasHandle())
		{
			@event.set3DAttributes(transform.To3DAttributes());
		}
		return @event;
	}

	public static EventInstance GetFMODEvent(FMODAsset asset, Vector3 position)
	{
		EventInstance @event = FMODUWE.GetEvent(asset);
		if (@event.hasHandle())
		{
			@event.set3DAttributes(position.To3DAttributes());
		}
		return @event;
	}

	public static PARAMETER_ID GetFMODParamIndex(FMODAsset asset, string paramName)
	{
		PARAMETER_ID result = FMODUWE.invalidParameterId;
		EventInstance @event = FMODUWE.GetEvent(asset);
		if (@event.hasHandle())
		{
			result = FMODUWE.GetEventInstanceParameterIndex(@event, paramName);
			@event.release();
		}
		return result;
	}

	public static void PlayFMODAsset(FMODAsset asset)
	{
		if (asset != null && asset.path != "")
		{
			EventInstance @event = FMODUWE.GetEvent(asset);
			if (@event.hasHandle())
			{
				@event.start();
			}
		}
	}

	public static float Dist(Transform t1, Transform t2)
	{
		return (t1.position - t2.position).magnitude;
	}

	public static string ToString(GameObject obj)
	{
		if (!(obj != null))
		{
			return "<null";
		}
		return obj.name;
	}

	public static IEnumerator EnsureLootCubeCreated()
	{
		if (_genericLootPrefab == null)
		{
			AsyncOperationHandle<GameObject> request = AddressablesUtility.LoadAsync<GameObject>("WorldEntities/Natural/LootCube.prefab");
			yield return request;
			_genericLootPrefab = request.Result;
		}
	}

	public static GameObject CreateGenericLoot(TechType techType)
	{
		GameObject gameObject = SpawnFromPrefab(genericLootPrefab, null);
		gameObject.GetComponent<Pickupable>().SetTechTypeOverride(techType, lootCube: true);
		return gameObject;
	}

	public static void StopAllFMODEvents(GameObject root, bool allowFadeOut = false)
	{
		if (!(root == null))
		{
			FMOD_StudioEventEmitter[] componentsInChildren = root.GetComponentsInChildren<FMOD_StudioEventEmitter>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Stop(allowFadeOut);
			}
			FMOD_CustomLoopingEmitter[] componentsInChildren2 = root.GetComponentsInChildren<FMOD_CustomLoopingEmitter>(includeInactive: true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].Stop();
			}
		}
	}

	public static void StartAllFMODEvents(GameObject root)
	{
		if (!(root == null))
		{
			FMOD_StudioEventEmitter[] componentsInChildren = root.GetComponentsInChildren<FMOD_StudioEventEmitter>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].StartEvent();
			}
			FMOD_CustomLoopingEmitter[] componentsInChildren2 = root.GetComponentsInChildren<FMOD_CustomLoopingEmitter>(includeInactive: true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].Play();
			}
		}
	}

	public static bool CheckObjectInFront(Transform object1, Transform object2, float passAngle = 90f)
	{
		passAngle *= (float)System.Math.PI / 180f;
		passAngle = Mathf.Cos(passAngle);
		passAngle *= 57.29578f;
		if (Vector3.Dot(object1.forward, (object2.position - object1.position).normalized) > passAngle)
		{
			return true;
		}
		return false;
	}

	public static bool CheckObjectOnTop(Transform object1, Transform object2, float passAngle = 90f)
	{
		passAngle *= (float)System.Math.PI / 180f;
		passAngle = Mathf.Cos(passAngle);
		passAngle *= 57.29578f;
		if (Vector3.Dot(object1.up, (object2.position - object1.position).normalized) > passAngle)
		{
			return true;
		}
		return false;
	}

	public static string PrettifyDate(long dateTicks)
	{
		DateTime arg = new DateTime(dateTicks);
		return Language.main.GetFormat("DateFormat", arg);
	}

	public static string PrettifyTime(int totalSeconds)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
		int minutes = timeSpan.Minutes;
		int hours = timeSpan.Hours;
		int days = timeSpan.Days;
		if (days > 0)
		{
			return Language.main.GetFormat("TimeFormatDaysHoursMinutes", days, hours, minutes);
		}
		if (hours > 0)
		{
			return Language.main.GetFormat("TimeFormatHoursMinutes", hours, minutes);
		}
		return Language.main.GetFormat("TimeFormatMinutes", minutes);
	}
}
