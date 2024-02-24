using UWE;
using UnityEngine;

[RequireComponent(typeof(EnergyMixin))]
public class LaserCutter : PlayerTool
{
	public FMODASRPlayer laserCutSound;

	public VFXController fxControl;

	public float laserEnergyCost = 1f;

	private float healthPerWeld = 25f;

	private bool usedThisFrame;

	private Sealed activeCuttingTarget;

	private bool fxIsPlaying;

	public Light fxLight;

	private float lightIntensity;

	private AimIKTarget playerIKTarget;

	private float totalTimeActive;

	private Color lightbarColor = Color.black;

	private void Start()
	{
		playerIKTarget = Player.main.armsController.lookTargetTransform.GetComponent<AimIKTarget>();
	}

	private void OnDisable()
	{
		activeCuttingTarget = null;
		if (playerIKTarget != null)
		{
			playerIKTarget.enabled = true;
		}
	}

	public override void OnToolUseAnim(GUIHand hand)
	{
		LaserCut();
	}

	public override void OnHolster()
	{
		base.OnHolster();
		StopLaserCuttingFX();
		fxLight.intensity = lightIntensity;
	}

	private void LaserCut()
	{
		bool flag = true;
		if ((bool)activeCuttingTarget && activeCuttingTarget.requireOpenFromFront && !Utils.CheckObjectInFront(activeCuttingTarget.transform, Player.main.transform))
		{
			flag = false;
		}
		if (energyMixin.IsDepleted())
		{
			flag = false;
		}
		if (!flag || !(activeCuttingTarget != null))
		{
			return;
		}
		bool flag2 = false;
		activeCuttingTarget.Weld(healthPerWeld);
		if (activeCuttingTarget.openedAmount < activeCuttingTarget.maxOpenedAmount)
		{
			flag2 = true;
		}
		if (flag2)
		{
			if ((bool)activeCuttingTarget.GetComponent<LaserCutObject>())
			{
				activeCuttingTarget.GetComponent<LaserCutObject>().ActivateFX();
			}
			if (playerIKTarget != null)
			{
				playerIKTarget.enabled = false;
			}
			StartLaserCuttingFX();
			energyMixin.ConsumeEnergy(laserEnergyCost);
		}
		else if (playerIKTarget != null)
		{
			playerIKTarget.enabled = true;
		}
	}

	private void UpdateTarget()
	{
		activeCuttingTarget = null;
		if (!(usingPlayer != null))
		{
			return;
		}
		Vector3 position = default(Vector3);
		GameObject closestObj = null;
		UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, 2f, ref closestObj, ref position);
		if (closestObj == null)
		{
			InteractionVolumeUser component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
			if (component != null && component.GetMostRecent() != null)
			{
				closestObj = component.GetMostRecent().gameObject;
			}
		}
		if ((bool)closestObj)
		{
			Sealed @sealed = closestObj.FindAncestor<Sealed>();
			if ((bool)@sealed)
			{
				activeCuttingTarget = @sealed;
			}
		}
	}

	private void StartLaserCuttingFX()
	{
		if (fxControl != null && !fxIsPlaying)
		{
			int i = (Player.main.IsUnderwater() ? 1 : 0);
			if (MiscSettings.flashes)
			{
				fxControl.Play(i);
				fxLight.enabled = true;
			}
			fxIsPlaying = true;
			InvokeRepeating("RandomizeIntensity", 0f, 0.05f);
			totalTimeActive = 0f;
		}
	}

	private void StopLaserCuttingFX()
	{
		laserCutSound.Stop();
		if (playerIKTarget != null)
		{
			playerIKTarget.enabled = true;
		}
		if (fxControl != null && fxIsPlaying)
		{
			fxControl.StopAndDestroy(0f);
			fxIsPlaying = false;
			CancelInvoke("RandomizeIntensity");
			fxLight.enabled = false;
		}
	}

	private void Update()
	{
		usedThisFrame = false;
		if (base.isDrawn)
		{
			if (AvatarInputHandler.main.IsEnabled() && Player.main.IsAlive() && GameInput.GetButtonHeld(GameInput.Button.RightHand) && !Player.main.IsBleederAttached())
			{
				usedThisFrame = true;
			}
			if (usedThisFrame)
			{
				laserCutSound.Play();
			}
			else if (fxIsPlaying)
			{
				StopLaserCuttingFX();
			}
			else
			{
				laserCutSound.Stop();
			}
			if (fxIsPlaying)
			{
				fxLight.intensity = Mathf.MoveTowards(fxLight.intensity, lightIntensity, Time.deltaTime * 25f);
			}
			UpdateTarget();
		}
	}

	public override void OnDraw(Player p)
	{
		base.OnDraw(p);
		if (firstUseAnimationStarted)
		{
			fxControl.Play(2);
		}
	}

	protected override void OnFirstUseAnimationStop()
	{
		base.OnFirstUseAnimationStop();
		fxControl.StopAndDestroy(2, 0f);
	}

	private void RandomizeIntensity()
	{
		lightIntensity = Random.Range(0f, 4f);
	}

	public override bool GetUsedToolThisFrame()
	{
		return usedThisFrame;
	}

	private void UpdateLightbar()
	{
		totalTimeActive += Time.deltaTime;
		float num = UWE.Utils.SineWaveNegOneToOne(totalTimeActive * 0.5f) * 100f;
		int value = Mathf.FloorToInt(150f + num);
		value = Mathf.Clamp(value, 13, 255);
		lightbarColor.r = value;
		PlatformUtils.SetLightbarColor(lightbarColor);
	}
}
