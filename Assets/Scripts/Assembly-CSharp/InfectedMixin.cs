using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class InfectedMixin : MonoBehaviour
{
	private const string uwe_infection = "UWE_INFECTION";

	private const string uwe_playerinfection = "UWE_PLAYERINFECTION";

	[ProtoMember(1)]
	public float infectedAmount;

	private const int currentVersion = 4;

	[NonSerialized]
	[ProtoMember(2)]
	public int version = 4;

	public Player player;

	[AssertNotNull]
	public Renderer[] renderers;

	private List<Material> materials;

	private float prevInfectedAmount;

	private string shaderKeyWord = "UWE_INFECTION";

	private void Awake()
	{
		if (player != null)
		{
			shaderKeyWord = "UWE_PLAYERINFECTION";
		}
		UpdateInfectionShading();
	}

	private void Start()
	{
		UpdateInfectionShading();
	}

	private void OnDestroy()
	{
		if (materials != null)
		{
			for (int i = 0; i < materials.Count; i++)
			{
				UnityEngine.Object.Destroy(materials[i]);
			}
		}
	}

	public bool IsInfected()
	{
		return infectedAmount > 0f;
	}

	public bool IsHealedByPeeper()
	{
		if (player == null && infectedAmount > 0f)
		{
			return infectedAmount < 0.15f;
		}
		return false;
	}

	public float GetInfectedAmount()
	{
		return infectedAmount;
	}

	public float GetPrevInfectedAmount()
	{
		return prevInfectedAmount;
	}

	public bool SetInfectedAmount(float amount)
	{
		bool result = false;
		if (!Mathf.Approximately(amount, infectedAmount))
		{
			infectedAmount = Mathf.Clamp01(amount);
			UpdateInfectionShading();
			result = true;
		}
		return result;
	}

	public void IncreaseInfectedAmount(float amount)
	{
		if (infectedAmount > 0f)
		{
			infectedAmount = Mathf.Max(infectedAmount, amount);
		}
	}

	public void RemoveInfection()
	{
		SetInfectedAmount(0f);
	}

	public void AlleviateSymptoms()
	{
		if (infectedAmount > 0f)
		{
			SetInfectedAmount(Mathf.Min(infectedAmount, 0.1f));
		}
	}

	public void Heal(float amount)
	{
		if (infectedAmount > 0.1f)
		{
			SetInfectedAmount(Mathf.Max(0.1f, infectedAmount - amount));
		}
	}

	public void UpdateInfectionShading()
	{
		if (renderers == null)
		{
			return;
		}
		float num = ((player != null) ? player.GetInfectionAmount() : infectedAmount);
		if (num == prevInfectedAmount)
		{
			return;
		}
		if (materials == null)
		{
			materials = new List<Material>();
			for (int i = 0; i < renderers.Length; i++)
			{
				if (renderers[i] != null)
				{
					materials.AddRange(renderers[i].materials);
				}
			}
		}
		bool flag = !(prevInfectedAmount > 0f);
		bool flag2 = prevInfectedAmount > 0f && num == 0f;
		for (int j = 0; j < materials.Count; j++)
		{
			Material material = materials[j];
			if (material != null)
			{
				material.SetFloat(ShaderPropertyID._InfectionAmount, num);
				material.SetVector(ShaderPropertyID._ModelScale, base.transform.localScale);
				if (flag)
				{
					material.EnableKeyword(shaderKeyWord);
				}
				else if (flag2)
				{
					material.DisableKeyword(shaderKeyWord);
				}
			}
		}
		prevInfectedAmount = num;
	}
}
