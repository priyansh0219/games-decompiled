using System;
using System.Collections.Generic;
using UnityEngine;

public class EnergyEffect : MonoBehaviour
{
	public GameObject[] modelsWithEmissive;

	public GameObject[] toDisableOnPowerDown;

	public float duration = 2f;

	[AssertNotNull]
	public AnimationCurve powerAnim;

	private EnergyMixin energy;

	private readonly List<Renderer> renderers = new List<Renderer>();

	private MaterialPropertyBlock block;

	[NonSerialized]
	public Utils.MonitoredValue<bool> powerDown = new Utils.MonitoredValue<bool>();

	[NonSerialized]
	public Utils.MonitoredValue<float> powerScalar = new Utils.MonitoredValue<float>();

	private void Start()
	{
		block = new MaterialPropertyBlock();
		energy = GetComponent<EnergyMixin>();
		for (int i = 0; i < modelsWithEmissive.Length; i++)
		{
			if (modelsWithEmissive[i] != null)
			{
				Renderer[] componentsInChildren = modelsWithEmissive[i].GetComponentsInChildren<Renderer>(includeInactive: true);
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					renderers.Add(componentsInChildren[j]);
				}
			}
		}
		powerDown.Update(newValue: false);
		powerScalar.Update(1f);
		powerDown.changedEvent.AddHandler(this, OnPowerDownChanged);
		powerScalar.changedEvent.AddHandler(this, OnPowerScalarChanged);
	}

	private void OnPowerScalarChanged(Utils.MonitoredValue<float> powerScalar)
	{
		float value = 1f - powerScalar.value;
		if (powerAnim != null)
		{
			value = 1f - powerAnim.Evaluate(Mathf.Clamp(powerScalar.value, 0f, 1f));
		}
		for (int i = 0; i < renderers.Count; i++)
		{
			renderers[i].GetPropertyBlock(block);
			block.SetFloat(ShaderPropertyID._UwePowerLoss, value);
			renderers[i].SetPropertyBlock(block);
		}
	}

	private void OnPowerDownChanged(Utils.MonitoredValue<bool> powerDown)
	{
		for (int i = 0; i < toDisableOnPowerDown.Length; i++)
		{
			toDisableOnPowerDown[i].SetActive(!powerDown.value);
		}
	}

	private void Update()
	{
		if (energy != null)
		{
			powerDown.Update(energy.IsDepleted());
		}
		if (powerDown.value && powerScalar.value > 0f)
		{
			powerScalar.Update(Mathf.Clamp01(powerScalar.value - Time.deltaTime * (1f / duration)));
		}
		else if (!powerDown.value && powerScalar.value < 1f)
		{
			powerScalar.Update(Mathf.Clamp01(powerScalar.value + Time.deltaTime * (1f / duration)));
		}
	}
}
