using System;
using UWE;
using UnityEngine;

public abstract class Landscape : MonoBehaviour
{
	public struct Projected
	{
		public Vector3 position;

		public Vector3 normal;

		public bool hitAnything;
	}

	public static Landscape main;

	public Event<Landscape> builtEvent = new Event<Landscape>();

	public Event<Landscape> startBuildingEvent = new Event<Landscape>();

	public Event<Landscape> lootSpawned = new Event<Landscape>();

	[NonSerialized]
	public string currentStepLabel;

	public abstract bool IsTerrainReady();

	public abstract bool GetAreAgentsReady();

	public virtual bool IsReady()
	{
		if (IsTerrainReady())
		{
			return GetAreAgentsReady();
		}
		return false;
	}

	public abstract Vector3 UVToWorldPoint(Vector2 uv);

	public abstract Vector2 WorldPointToUV(Vector3 wsPos);

	public virtual float GetTopographicHeight(Vector2 uv)
	{
		return UVToWorldPoint(uv).y;
	}

	public abstract Projected Project(Vector3 wsPos, float distance);

	public abstract Bounds GetBounds();

	public abstract Vector3 GetBoundsMin();

	public abstract Vector3 GetBoundsMax();

	public virtual bool IsDefinitelyEmpty(Vector3 wsPos)
	{
		return false;
	}

	public bool IsInside(Vector3 wsPos)
	{
		return GetBounds().Contains(wsPos);
	}

	public abstract bool IsMeshBuiltAt(Vector3 wsPos);
}
