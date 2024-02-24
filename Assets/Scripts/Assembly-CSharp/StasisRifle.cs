using UnityEngine;

[RequireComponent(typeof(EnergyMixin))]
public class StasisRifle : PlayerTool
{
	public float energyCost = 5f;

	public float chargeDuration = 3f;

	public Animator animator;

	[AssertNotNull]
	public Transform muzzle;

	public GameObject effectSpherePrefab;

	[AssertNotNull]
	public FMOD_StudioEventEmitter chargeBegin;

	[AssertNotNull]
	public FMOD_StudioEventEmitter chargeLoop;

	[AssertNotNull]
	public FMODAsset fireSound;

	[AssertNotNull]
	public VFXController fxControl;

	public Renderer bar;

	public int barMaterialID = 1;

	private Transform tr;

	private float chargeAmount;

	private bool isCharging;

	private static StasisSphere sphere;

	private float chargeNormalized => chargeAmount / energyCost;

	public override void Awake()
	{
		base.Awake();
		tr = GetComponent<Transform>();
		if (sphere == null)
		{
			sphere = Object.Instantiate(effectSpherePrefab, tr.position, Quaternion.identity).GetComponent<StasisSphere>();
			if (sphere == null)
			{
				Debug.LogError("Component of type StasisSphere is not found on effectSpherePrefab!");
			}
		}
		if (bar == null)
		{
			Debug.LogError("Bar renderer is not assigned", this);
		}
		UpdateBar();
	}

	public override bool OnRightHandDown()
	{
		BeginCharge();
		Charge();
		return isCharging;
	}

	public override bool OnRightHandHeld()
	{
		Charge();
		return true;
	}

	public override bool OnRightHandUp()
	{
		EndCharge();
		Fire();
		return true;
	}

	public override void OnHolster()
	{
		base.OnHolster();
		EndCharge();
		chargeAmount = 0f;
		UpdateBar();
	}

	public override bool GetUsedToolThisFrame()
	{
		return isCharging;
	}

	private void BeginCharge()
	{
		if (!isCharging)
		{
			if (energyMixin.charge <= 0f)
			{
				ErrorMessage.AddError(Language.main.Get("BatteryDepleted"));
				return;
			}
			isCharging = true;
			fxControl.Play(0);
			chargeBegin.StartEvent();
			chargeLoop.StartEvent();
			Animate(state: true);
		}
	}

	private void EndCharge()
	{
		if (isCharging)
		{
			isCharging = false;
			fxControl.StopAndDestroy(0, 0f);
			if (chargeBegin.GetIsStartingOrPlaying())
			{
				chargeBegin.Stop(allowFadeout: false);
			}
			if (chargeLoop.GetIsStartingOrPlaying())
			{
				chargeLoop.Stop(allowFadeout: false);
			}
			Animate(state: false);
		}
	}

	private void Charge()
	{
		if (isCharging)
		{
			float num = energyCost * Time.deltaTime / chargeDuration;
			float charge = energyMixin.charge;
			bool flag = false;
			if (num >= charge)
			{
				num = charge;
				flag = true;
			}
			else if (chargeAmount + num >= energyCost)
			{
				num = energyCost - chargeAmount;
				flag = true;
			}
			energyMixin.ConsumeEnergy(num);
			chargeAmount += num;
			UpdateBar();
			SafeAnimator.SetBool(Utils.GetLocalPlayerComp().armsController.gameObject.GetComponent<Animator>(), "charged_stasisrifle", value: true);
			if (flag)
			{
				EndCharge();
				Fire();
			}
		}
	}

	private void Fire()
	{
		if (!(chargeAmount <= 0f))
		{
			fxControl.Play(1);
			FMODUWE.PlayOneShot(fireSound, tr.position, Mathf.Lerp(0.3f, 1f, chargeNormalized));
			float speed = 25f;
			float lifeTime = 3f;
			sphere.Shoot(muzzle.position, Player.main.camRoot.GetAimingTransform().rotation, speed, lifeTime, chargeNormalized);
			chargeAmount = 0f;
			UpdateBar();
		}
	}

	private void UpdateBar()
	{
		if (!(bar == null))
		{
			bar.materials[barMaterialID].SetFloat(ShaderPropertyID._Amount, chargeNormalized);
		}
	}

	private void Animate(bool state)
	{
		if (!(animator == null) && animator.isActiveAndEnabled)
		{
			SafeAnimator.SetBool(animator, "using_tool", state);
		}
	}
}
