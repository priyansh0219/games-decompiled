using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Flare : PlayerTool
{
	[ProtoMember(1)]
	public float energyLeft = 10f;

	public Light light;

	public FMOD_CustomLoopingEmitter loopingSound;

	public FMOD_StudioEventEmitter throwSound;

	public VFXController fxControl;

	public MeshRenderer capRenderer;

	public Rigidbody useRigidbody;

	private float flickerInterval = 0.25f;

	private float dropTorqueAmount = 5f;

	private float throwForceAmount = 100f;

	private float dropForceAmount = 1f;

	private float throwDuration = 0.5f;

	[NonSerialized]
	[ProtoMember(2)]
	public bool flareActiveState;

	[NonSerialized]
	[ProtoMember(3)]
	public bool hasBeenThrown;

	[NonSerialized]
	[ProtoMember(4)]
	public float flareActivateTime;

	private bool fxIsPlaying;

	private bool isThrowing;

	private bool isLightFadinfIn;

	private float originalIntensity;

	private float originalrange;

	private Sequence sequence;

	public override void Awake()
	{
		base.Awake();
		originalIntensity = light.intensity;
		originalrange = light.range;
		light.intensity = 0f;
		light.range = 0f;
		sequence = new Sequence();
		if (hasBeenThrown)
		{
			capRenderer.enabled = true;
			if ((bool)fxControl && !fxIsPlaying)
			{
				fxControl.Play(1);
				fxIsPlaying = true;
				light.enabled = true;
			}
		}
	}

	private void UpdateLight()
	{
		float num = (float)(DayNightCycle.main.timePassed - (double)flareActivateTime);
		if (num > 0.1f)
		{
			float num2 = num / flickerInterval;
			float num3 = 0.45f + 0.55f * Mathf.PerlinNoise(num2, 0f);
			float num4 = originalIntensity * num3;
			float num5 = originalrange * 0.65f + 0.35f * Mathf.Sin(num2);
			if (num < 0.43f)
			{
				float t = num * 3f - 0.1f;
				float intensity = Mathf.Lerp(0f, num4, t);
				FlashingLightHelpers.SafeIntensityChangePerFrame(light, intensity);
				float range = Mathf.Lerp(0f, num5, t);
				FlashingLightHelpers.SafeRangeChangePreFrame(light, range);
			}
			else
			{
				FlashingLightHelpers.SafeIntensityChangePerFrame(light, num4);
				FlashingLightHelpers.SafeRangeChangePreFrame(light, num5);
			}
		}
	}

	private void Update()
	{
		if (flareActiveState)
		{
			sequence.Update();
			UpdateLight();
			energyLeft = Mathf.Max(energyLeft - Time.deltaTime, 0f);
		}
		else
		{
			light.intensity = 0f;
		}
		if (fxIsPlaying && energyLeft < 3f)
		{
			fxControl.StopAndDestroy(1, 2f);
			fxControl.Play(2);
			fxIsPlaying = false;
		}
		if (energyLeft == 0f)
		{
			loopingSound.Stop();
			UnityEngine.Object.Destroy(base.gameObject, 2f);
		}
	}

	public override void OnHolster()
	{
		base.OnHolster();
		if (!isThrowing)
		{
			loopingSound.Stop();
			if (fxIsPlaying)
			{
				fxControl.StopAndDestroy(1, 0f);
				fxIsPlaying = false;
			}
			SetFlareActiveState(newFlareActiveState: false);
		}
	}

	public override void OnDraw(Player p)
	{
		base.OnDraw(p);
		if (hasBeenThrown)
		{
			SetFlareActiveState(newFlareActiveState: true);
			if ((bool)fxControl && !fxIsPlaying)
			{
				fxControl.Play(1);
				fxIsPlaying = true;
			}
		}
	}

	public override bool OnRightHandDown()
	{
		if (Player.main.IsBleederAttached())
		{
			return true;
		}
		_isInUse = true;
		return true;
	}

	public override void OnToolUseAnim(GUIHand hand)
	{
		if (!isThrowing)
		{
			SetFlareActiveState(newFlareActiveState: true);
			sequence.Set(throwDuration, target: true, Throw);
			isThrowing = true;
		}
	}

	public override bool GetAltUsedToolThisFrame()
	{
		if (_isInUse)
		{
			return hasBeenThrown;
		}
		return false;
	}

	private void Throw()
	{
		_isInUse = false;
		pickupable.Drop(base.transform.position);
		base.transform.GetComponent<WorldForces>().enabled = true;
		throwSound.StartEvent();
	}

	private void OnDrop()
	{
		float num = (isThrowing ? throwForceAmount : dropForceAmount);
		useRigidbody.AddForce(MainCamera.camera.transform.forward * num);
		useRigidbody.AddTorque(base.transform.right * dropTorqueAmount);
		if (isThrowing)
		{
			useRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		}
		if (hasBeenThrown)
		{
			SetFlareActiveState(newFlareActiveState: true);
			if ((bool)fxControl && !fxIsPlaying)
			{
				fxControl.Play(1);
			}
			fxIsPlaying = true;
		}
		isThrowing = false;
	}

	private void SetFlareActiveState(bool newFlareActiveState)
	{
		if (flareActiveState == newFlareActiveState)
		{
			return;
		}
		if (newFlareActiveState)
		{
			loopingSound.Play();
			if ((bool)fxControl)
			{
				fxControl.Play(0);
			}
			capRenderer.enabled = false;
			light.enabled = true;
			isLightFadinfIn = true;
			hasBeenThrown = true;
			flareActivateTime = DayNightCycle.main.timePassedAsFloat;
		}
		flareActiveState = newFlareActiveState;
	}

	private void OnDisable()
	{
		if (fxIsPlaying)
		{
			fxControl.StopAndDestroy(1, 0f);
			fxIsPlaying = false;
		}
	}

	protected override void OnDestroy()
	{
		fxControl.StopAndDestroy(2f);
		base.OnDestroy();
	}
}
