using System.Collections.Generic;
using UnityEngine;

public class PowerEffectManager : MonoBehaviour
{
	private PowerRelay powerRelay;

	private bool prevPowerRelayState;

	public GameObject[] affectedModels;

	public float duration = 2f;

	public AnimationCurve powerAnim;

	private List<Renderer> renderers = new List<Renderer>();

	private MaterialPropertyBlock block;

	private bool powerGoDown;

	private float powerScalar = 1f;

	private float prevPowerScalar = 1f;

	private void Start()
	{
		block = new MaterialPropertyBlock();
		powerRelay = GetComponent<PowerRelay>();
		prevPowerRelayState = powerRelay.IsPowered();
		GameObject[] array = affectedModels;
		for (int i = 0; i < array.Length; i++)
		{
			Renderer[] componentsInChildren = array[i].GetComponentsInChildren<Renderer>(includeInactive: true);
			foreach (Renderer item in componentsInChildren)
			{
				renderers.Add(item);
			}
		}
		Debug.Log("found " + renderers.Count + " renderers ");
	}

	private void UpdateMaterialsAndLights()
	{
		if (prevPowerScalar == powerScalar)
		{
			return;
		}
		float value = 1f - powerScalar;
		if (powerAnim != null)
		{
			value = powerAnim.Evaluate(Mathf.Clamp(powerScalar, 0f, 1f));
		}
		foreach (Renderer renderer in renderers)
		{
			block.Clear();
			renderer.GetPropertyBlock(block);
			block.SetFloat(ShaderPropertyID._UwePowerLoss, value);
			renderer.SetPropertyBlock(block);
		}
		prevPowerScalar = powerScalar;
	}

	private bool CheckIsPowered()
	{
		if (powerRelay != null)
		{
			return powerRelay.IsPowered();
		}
		return false;
	}

	private void Update()
	{
		bool flag = CheckIsPowered();
		if (prevPowerRelayState && !flag)
		{
			powerGoDown = true;
		}
		else if (!prevPowerRelayState && flag)
		{
			powerGoDown = false;
		}
		prevPowerRelayState = flag;
		if (powerGoDown && powerScalar > 0f)
		{
			powerScalar -= Time.deltaTime * (1f / duration);
		}
		else if (!powerGoDown && powerScalar < 1f)
		{
			powerScalar += Time.deltaTime * (1f / duration);
		}
		UpdateMaterialsAndLights();
	}

	public bool GetPowerState()
	{
		return powerGoDown;
	}

	public void ForcePowerState(bool state)
	{
		powerGoDown = state;
	}
}
