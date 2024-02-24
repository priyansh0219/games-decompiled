using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Eatable : MonoBehaviour, ISecondaryTooltip
{
	[NonSerialized]
	[ProtoMember(1)]
	public float timeDecayStart;

	public float foodValue;

	public float waterValue;

	public float stomachVolume;

	public bool decomposes;

	public bool despawns = true;

	public bool allowOverfill;

	public float kDecayRate;

	public float despawnDelay = 300f;

	private bool wasActive;

	private float timeDespawnStart;

	private void Awake()
	{
		if (despawns && decomposes)
		{
			StartDespawnInvoke();
		}
		SetDecomposes(decomposes);
	}

	public float GetFoodValue()
	{
		float result = foodValue;
		if (decomposes)
		{
			result = Mathf.Max(foodValue - (DayNightCycle.main.timePassedAsFloat - timeDecayStart) * kDecayRate, -25f);
		}
		return result;
	}

	public float GetWaterValue()
	{
		float result = waterValue;
		if (decomposes)
		{
			result = Mathf.Max(waterValue - (DayNightCycle.main.timePassedAsFloat - timeDecayStart) * kDecayRate, -25f);
		}
		return result;
	}

	public float GetStomachVolume()
	{
		return stomachVolume;
	}

	public string GetSecondaryTooltip()
	{
		if (!GameModeUtils.RequiresSurvival())
		{
			return string.Empty;
		}
		float num = GetFoodValue();
		float num2 = GetWaterValue();
		string result = string.Empty;
		if (decomposes)
		{
			if (num < 0f && num2 < 0f)
			{
				result = Language.main.Get("Rotting");
			}
			else if (num < 0.5f * foodValue)
			{
				result = Language.main.Get("Ripe");
			}
			else if (num < 0.9f * foodValue)
			{
				result = Language.main.Get("Decomposing");
			}
		}
		return result;
	}

	public bool IsRotten()
	{
		if (GetFoodValue() < 0f)
		{
			return GetWaterValue() < 0f;
		}
		return false;
	}

	public void SetDecomposes(bool value)
	{
		if (!value)
		{
			timeDecayStart = 0f;
		}
		else if (timeDecayStart == 0f)
		{
			timeDecayStart = DayNightCycle.main.timePassedAsFloat;
		}
		decomposes = value;
		CancelInvoke();
		if (value)
		{
			StartDespawnInvoke();
		}
	}

	private void StartDespawnInvoke()
	{
		float time = UnityEngine.Random.Range(0f, 5f);
		InvokeRepeating("IterateDespawn", time, 5f);
	}

	private void IterateDespawn()
	{
		if (!IsRotten())
		{
			return;
		}
		if (!base.gameObject.activeSelf)
		{
			wasActive = false;
			return;
		}
		if (!wasActive)
		{
			timeDespawnStart = DayNightCycle.main.timePassedAsFloat;
		}
		wasActive = true;
		if (DayNightCycle.main.timePassedAsFloat - timeDespawnStart > despawnDelay)
		{
			CancelInvoke();
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
