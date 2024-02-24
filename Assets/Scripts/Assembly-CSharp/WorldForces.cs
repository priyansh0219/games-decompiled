using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class WorldForces : MonoBehaviour
{
	private class Explosion
	{
		public Vector3 position;

		public double startTime;

		public double endTime;

		public float magnitude;

		public float radius;
	}

	public class Current
	{
		public Vector3 position;

		public float radius;

		public double startTime;

		public double endTime;

		public Vector3 direction;

		public float startSpeed;
	}

	public bool handleGravity = true;

	public float aboveWaterGravity = 9.81f;

	public float underwaterGravity = 1f;

	public bool handleDrag = true;

	public float aboveWaterDrag = 0.1f;

	public float underwaterDrag = 1f;

	[NonSerialized]
	public bool lockInterpolation;

	[NonSerialized]
	public float waterDepth;

	[HideInInspector]
	public bool aboveWaterOverride;

	[AssertNotNull]
	public Rigidbody useRigidbody;

	public bool moveWithPlatform;

	private bool detectionState;

	private const float kExplosionTravelSpeed = 500f;

	private const float kExplosionEdgeDuration = 0.03f;

	private static List<Explosion> explosionList = new List<Explosion>();

	private static List<Current> currentsList = new List<Current>();

	[NonSerialized]
	public int updaterIndex = -1;

	private bool been_disabled;

	private bool was_above_water;

	private void OnEnable()
	{
		RegisterWorldForces();
	}

	private void OnDisable()
	{
		if (updaterIndex != -1)
		{
			WorldForcesManager.Instance.RemoveWorldForces(this);
		}
	}

	public static void AddExplosion(Vector3 position, double time, float magnitude, float radius)
	{
		Explosion explosion = new Explosion();
		explosion.position = position;
		explosion.startTime = time;
		explosion.endTime = time + (double)(radius / 500f);
		explosion.magnitude = magnitude;
		explosion.radius = radius;
		explosionList.Add(explosion);
	}

	public static void AddCurrent(Vector3 position, double time, float radius, Vector3 direction, float startSpeed, float lifeTime)
	{
		AddCurrent(new Current
		{
			position = position,
			radius = radius,
			direction = direction,
			startSpeed = startSpeed,
			startTime = time,
			endTime = time + (double)lifeTime
		});
	}

	public static void AddCurrent(Current current)
	{
		if (current != null && !currentsList.Contains(current))
		{
			currentsList.Add(current);
		}
	}

	public static void RemoveCurrent(Current current)
	{
		if (current != null)
		{
			currentsList.Remove(current);
		}
	}

	private void Start()
	{
		try
		{
			if ((bool)useRigidbody && handleDrag)
			{
				bool flag = IsAboveWater();
				if (!flag)
				{
					useRigidbody.drag = underwaterDrag;
				}
				else
				{
					useRigidbody.drag = aboveWaterDrag;
				}
				was_above_water = flag;
				UpdateInterpolation();
			}
		}
		finally
		{
		}
	}

	public bool IsAboveWater()
	{
		if (aboveWaterOverride)
		{
			return true;
		}
		return base.transform.position.y >= waterDepth;
	}

	public void RegisterWorldForces()
	{
		if (useRigidbody != null)
		{
			WorldForcesManager.Instance.AddWorldForces(this);
		}
	}

	public Vector3 GetGravityAtHeight(float y)
	{
		if (aboveWaterOverride)
		{
			return new Vector3(0f, 0f - aboveWaterGravity, 0f);
		}
		float t = (0f - (y - waterDepth)) * 10f;
		float num = Mathf.Lerp(aboveWaterGravity, underwaterGravity, t);
		return new Vector3(0f, 0f - num, 0f);
	}

	public float GetDragAtHeight(float y)
	{
		if (y >= waterDepth || aboveWaterOverride)
		{
			return aboveWaterDrag;
		}
		return underwaterDrag;
	}

	private void UpdateInterpolation()
	{
		if ((bool)useRigidbody && !lockInterpolation)
		{
			useRigidbody.interpolation = ((!useRigidbody.isKinematic) ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None);
		}
		if (!detectionState && (bool)useRigidbody && !useRigidbody.isKinematic)
		{
			if (useRigidbody.collisionDetectionMode == CollisionDetectionMode.Discrete)
			{
				useRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
			}
			detectionState = true;
		}
	}

	public void DoFixedUpdate()
	{
		UpdateInterpolation();
		if (useRigidbody == null || useRigidbody.isKinematic)
		{
			return;
		}
		Vector3 position = base.transform.position;
		if (handleGravity && useRigidbody != null)
		{
			Vector3 gravityAtHeight = GetGravityAtHeight(position.y);
			useRigidbody.AddForce(gravityAtHeight, ForceMode.Acceleration);
		}
		bool flag = aboveWaterOverride || position.y >= waterDepth;
		if (handleDrag && useRigidbody != null)
		{
			if (was_above_water && !flag)
			{
				useRigidbody.drag = underwaterDrag;
			}
			else if (!was_above_water && flag)
			{
				useRigidbody.drag = aboveWaterDrag;
			}
			was_above_water = flag;
		}
		for (int i = 0; i < explosionList.Count; i++)
		{
			Explosion explosion = explosionList[i];
			if (DayNightCycle.main.timePassed > explosion.endTime)
			{
				explosionList[i] = explosionList[explosionList.Count - 1];
				explosionList.RemoveAt(explosionList.Count - 1);
				i--;
				continue;
			}
			double startTime = explosion.startTime;
			float magnitude = (explosion.position - position).magnitude;
			double num = startTime + (double)(magnitude / 500f);
			if (DayNightCycle.main.timePassed >= num && DayNightCycle.main.timePassed <= num + 0.029999999329447746 && useRigidbody != null)
			{
				Vector3 vector = position - explosion.position;
				vector.Normalize();
				float num2 = Mathf.Max(explosion.magnitude - magnitude / 500f, 1f);
				Vector3 vector2 = vector * (num2 * (0.5f + UnityEngine.Random.value * 0.5f));
				useRigidbody.AddForce(vector2, ForceMode.Impulse);
				Debug.DrawLine(position, position + vector2, Color.yellow, 0.1f);
			}
		}
		Vector3 vector3 = Vector3.zero;
		float num3 = 0f;
		for (int j = 0; j < currentsList.Count; j++)
		{
			Current current = currentsList[j];
			if (current == null || DayNightCycle.main.timePassed > current.endTime)
			{
				currentsList[j] = currentsList[currentsList.Count - 1];
				currentsList.RemoveAt(currentsList.Count - 1);
				j--;
			}
			else if ((position - current.position).sqrMagnitude < current.radius * current.radius)
			{
				float num4 = current.startSpeed;
				if (!double.IsInfinity(current.endTime))
				{
					float b = (float)(current.endTime - current.startTime);
					float value = (float)(DayNightCycle.main.timePassed - current.startTime);
					float t = Mathf.InverseLerp(0f, b, value);
					num4 = Mathf.Lerp(current.startSpeed, 0f, t);
				}
				if (num4 > num3)
				{
					num3 = num4;
					vector3 = current.direction;
				}
			}
		}
		if (num3 > 0f && useRigidbody != null)
		{
			useRigidbody.AddForce(num3 * vector3, ForceMode.Impulse);
		}
	}

	public string CompileTimeCheck()
	{
		if (!useRigidbody)
		{
			return "Missing rigidbody";
		}
		if (handleGravity && !useRigidbody.isKinematic)
		{
			if (underwaterGravity > 0f)
			{
				return "Entities that sink under water must be set kinematic to prevent them from falling through the floor.";
			}
			if (underwaterGravity < 0f)
			{
				return "Entities that float to the surface must be set kinematic to prevent them from rising through the ceiling.";
			}
		}
		return null;
	}
}
