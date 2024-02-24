using System;
using System.Collections.Generic;
using UnityEngine;

public class EMPBlast : MonoBehaviour
{
	public float lifeTime = 4f;

	public AnimationCurve blastRadius = AnimationCurve.Linear(0f, 0f, 1f, 100f);

	public AnimationCurve blastHeight = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[AssertNotNull]
	public Material empedMaterial;

	[NonSerialized]
	public EMPAttack source;

	[AssertNotNull]
	[SerializeField]
	private Renderer renderer;

	private float startTime;

	private float currentBlastRadius;

	private float currentBlastHeight;

	private List<FlashingLightHelpers.ShaderVector4ScalerToken> textureSpeedTokens;

	public float disableElectronicsTime = 2f;

	private void Awake()
	{
		textureSpeedTokens = FlashingLightHelpers.CreateUberShaderVector4ScalerTokens(renderer.materials[0], renderer.materials[1], empedMaterial);
		MiscSettings.isFlashesEnabled.changedEvent.AddHandler(this, OnFlashesEnabled);
		UpdateSpeed();
	}

	private void OnDestroy()
	{
		MiscSettings.isFlashesEnabled.changedEvent.RemoveHandler(this, OnFlashesEnabled);
	}

	private void Start()
	{
		startTime = Time.time;
		SetProgress(0f);
	}

	private void Update()
	{
		float value = Mathf.InverseLerp(startTime, startTime + lifeTime, Time.time);
		value = Mathf.Clamp01(value);
		SetProgress(value);
		if (value == 1f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void SetProgress(float progress)
	{
		currentBlastRadius = blastRadius.Evaluate(progress);
		currentBlastHeight = blastHeight.Evaluate(progress);
		base.transform.localScale = new Vector3(currentBlastRadius, currentBlastHeight, currentBlastRadius);
	}

	public void OnTouch(Collider collider)
	{
		GameObject gameObject = null;
		if (collider.attachedRigidbody != null)
		{
			gameObject = collider.attachedRigidbody.gameObject;
		}
		if (!(gameObject != null) || !isValidTarget(gameObject))
		{
			return;
		}
		if (gameObject.CompareTag("Submarine"))
		{
			PowerRelay component = gameObject.GetComponent<PowerRelay>();
			if (component != null)
			{
				component.DisableElectronicsForTime(disableElectronicsTime);
			}
			if ((bool)source)
			{
				source.OnCyclopsHit();
			}
			return;
		}
		if (gameObject.CompareTag("Player"))
		{
			gameObject = Inventory.main.GetHeldObject();
			if (gameObject == null)
			{
				return;
			}
		}
		EnergyInterface component2 = gameObject.GetComponent<EnergyInterface>();
		if (component2 != null)
		{
			component2.DisableElectronicsForTime(disableElectronicsTime);
			ApplyAndForgetOverlayFX(gameObject);
			return;
		}
		EnergyMixin component3 = gameObject.GetComponent<EnergyMixin>();
		if (component3 != null)
		{
			component3.DisableElectronicsForTime(disableElectronicsTime);
			ApplyAndForgetOverlayFX(gameObject);
		}
	}

	private bool isValidTarget(GameObject target)
	{
		if (target == base.gameObject)
		{
			return false;
		}
		return true;
	}

	private void ApplyAndForgetOverlayFX(GameObject targetObj)
	{
		targetObj.AddComponent<VFXOverlayMaterial>().ApplyAndForgetOverlay(empedMaterial, "VFXOverlay: EMPed", Color.clear, lifeTime);
	}

	private void OnFlashesEnabled(Utils.MonitoredValue<bool> isFlashesEnabled)
	{
		UpdateSpeed();
	}

	private void UpdateSpeed()
	{
		if (MiscSettings.flashes)
		{
			textureSpeedTokens.RestoreScale();
		}
		else
		{
			textureSpeedTokens.SetScale(0.01f);
		}
	}
}
