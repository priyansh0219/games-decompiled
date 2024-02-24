using System.Collections.Generic;
using UWE;
using UnityEngine;

public class StasisSphere : Bullet
{
	private const float exitRadiusMargin = 2f;

	public float maxRadius = 10f;

	public float minRadius = 1f;

	public float maxTime = 20f;

	public float minTime = 4f;

	public LayerMask fieldLayerMask = -1;

	[AssertNotNull]
	public FMOD_StudioEventEmitter soundActivate;

	[AssertNotNull]
	public FMODAsset soundDeactivate;

	[AssertNotNull]
	public FMODAsset soundEnter;

	public GameObject vfxFreeze;

	public GameObject vfxUnfreeze;

	public VFXController fxControl;

	public GameObject fxElecSphere;

	private float radius;

	private float time;

	private bool fieldEnabled;

	private float fieldEnergy;

	private List<Rigidbody> targets;

	private MeshRenderer meshRenderer;

	private Color startColor;

	private List<FlashingLightHelpers.ShaderVector4ScalerToken> textureSpeedTokens;

	private float fieldRadius => Mathf.Max(radius * MathExtensions.SmoothValue(fieldEnergy, 0.9f), 0.01f);

	protected override void Awake()
	{
		base.Awake();
		meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.enabled = false;
		startColor = GetComponent<Renderer>().materials[0].GetColor(ShaderPropertyID._Color);
		textureSpeedTokens = FlashingLightHelpers.CreateUberShaderVector4ScalerTokens(meshRenderer.materials[0], meshRenderer.materials[1]);
		targets = new List<Rigidbody>();
		CancelAll();
		MiscSettings.isFlashesEnabled.changedEvent.AddHandler(this, OnFlashesEnabled);
		UpdateSpeed();
	}

	private void OnDestroy()
	{
		MiscSettings.isFlashesEnabled.changedEvent.RemoveHandler(this, OnFlashesEnabled);
	}

	private void LateUpdate()
	{
		if (!fieldEnabled)
		{
			return;
		}
		fieldEnergy -= Time.deltaTime / time;
		float num = fieldRadius;
		float num2 = num * num + 4f;
		if (fieldEnergy <= 0f)
		{
			fieldEnergy = 0f;
			CancelAll();
			FMODUWE.PlayOneShot(soundDeactivate, base.tr.position);
			return;
		}
		Rigidbody target = null;
		List<Rigidbody> list = new List<Rigidbody>();
		int num3 = UWE.Utils.OverlapSphereIntoSharedBuffer(base.tr.position, num, fieldLayerMask);
		for (int i = 0; i < num3; i++)
		{
			Collider other = UWE.Utils.sharedColliderBuffer[i];
			if (Freeze(other, ref target))
			{
				list.Add(target);
			}
		}
		for (int num4 = targets.Count - 1; num4 >= 0; num4--)
		{
			target = targets[num4];
			if (target == null || !target.gameObject.activeSelf)
			{
				targets.RemoveAt(num4);
			}
			else
			{
				Pickupable componentInParent = target.GetComponentInParent<Pickupable>();
				if (componentInParent != null && componentInParent.attached)
				{
					targets.RemoveAt(num4);
				}
				else if (!list.Contains(target))
				{
					Vector3 vector = target.ClosestPointOnBounds(base.tr.position);
					Vector3 vector2 = vector - base.tr.position;
					Debug.DrawLine(base.tr.position, vector, Color.red);
					if (vector2.sqrMagnitude > num2)
					{
						Unfreeze(target);
						targets.RemoveAt(num4);
					}
				}
			}
		}
		base.tr.localScale = (2f * num + 2f) * Vector3.one;
		UpdateMaterials();
	}

	public void Shoot(Vector3 position, Quaternion rotation, float speed, float lifeTime, float chargeNormalized)
	{
		CancelAll();
		radius = Mathf.Lerp(minRadius, maxRadius, chargeNormalized);
		time = Mathf.Lerp(minTime, maxTime, chargeNormalized);
		fieldEnergy = 1f;
		base.Shoot(position, rotation, speed, lifeTime);
	}

	protected override void OnMadeVisible()
	{
		fxControl.Play(0);
	}

	protected override void OnHit(RaycastHit hitInfo)
	{
		EnableField();
	}

	protected override void OnEnergyDepleted()
	{
		if (base.visible)
		{
			fxControl.StopAndDestroy(0, 1f);
		}
	}

	private void EnableField()
	{
		if (!fieldEnabled)
		{
			fieldEnabled = true;
			soundActivate.StartEvent();
			meshRenderer.enabled = true;
			if (base.visible)
			{
				fxControl.StopAndDestroy(0, 1f);
			}
			fxControl.Play(1);
			Utils.SpawnZeroedAt(fxElecSphere, base.transform);
		}
	}

	private void CancelAll()
	{
		if (base.visible)
		{
			fxControl.StopAndDestroy(0, 0f);
		}
		if (fieldEnabled)
		{
			fieldEnabled = false;
			UnfreezeAll();
			if (soundActivate.GetIsStartingOrPlaying())
			{
				soundActivate.Stop(allowFadeout: false);
			}
			meshRenderer.enabled = false;
		}
	}

	private bool Freeze(Collider other, ref Rigidbody target)
	{
		target = other.GetComponentInParent<Rigidbody>();
		if (target == null)
		{
			return false;
		}
		if (targets.Contains(target))
		{
			return true;
		}
		if (target.isKinematic)
		{
			return false;
		}
		if ((bool)other.GetComponentInParent<Player>())
		{
			return false;
		}
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(target, isKinematic: true);
		targets.Add(target);
		Utils.PlayOneShotPS(vfxFreeze, target.GetComponent<Transform>().position, Quaternion.identity);
		FMODUWE.PlayOneShot(soundEnter, base.tr.position);
		target.SendMessage("OnFreezeByStasisSphere", SendMessageOptions.DontRequireReceiver);
		return true;
	}

	private void Unfreeze(Rigidbody target)
	{
		if (!(target == null))
		{
			Utils.PlayOneShotPS(vfxUnfreeze, target.GetComponent<Transform>().position, Quaternion.identity);
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(target, isKinematic: false);
			target.SendMessage("OnUnfreezeByStasisSphere", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void UnfreezeAll()
	{
		for (int num = targets.Count - 1; num >= 0; num--)
		{
			Unfreeze(targets[num]);
			targets.RemoveAt(num);
		}
	}

	private void UpdateMaterials()
	{
		float t = MathExtensions.SmoothValue(fieldEnergy, 0.9f);
		Color value = Color.Lerp(Color.clear, startColor, t);
		GetComponent<Renderer>().materials[0].SetColor(ShaderPropertyID._Color, value);
		GetComponent<Renderer>().materials[1].SetColor(ShaderPropertyID._Color, value);
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
			textureSpeedTokens.SetScale(0.1f);
		}
	}
}
