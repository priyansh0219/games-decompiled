using UnityEngine;

[RequireComponent(typeof(EnergyMixin))]
public class Seaglide : PlayerTool
{
	[AssertNotNull]
	public ToggleLights toggleLights;

	[AssertNotNull]
	public ParticleSystem bubbles;

	[AssertNotNull]
	public ParticleSystem trailFX;

	[AssertNotNull]
	public GameObject screenEffectModel;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public EngineRpmSFXManager engineRPMManager;

	public Gradient gradientInner;

	public Gradient gradientOuter;

	public float powerGlideForce;

	public bool powerGlideActive;

	public float powerGlideParam;

	private bool activeState;

	private float timeSinceUse;

	private float propSpeed;

	private float spinUpSpeed = 200f;

	private float spinDownSpeed = 150f;

	private float maxSpinSpeed = 400f;

	private float _smoothedMoveSpeed;

	private Vector3 steerAngle = Vector3.zero;

	private float propSpeedParam;

	private float propSpeedVel;

	private int fmodIndexPowerglide = -1;

	private Color lightColor = Color.red;

	private string cachedPrimaryUseText;

	private string cachedAltUseText;

	private string cachedUseText;

	private Material screenEffectMat1;

	private Material screenEffectMat2;

	[AssertLocalization]
	private const string lightsButtonFormat = "SeaglideLightsTooltip";

	[AssertLocalization]
	private const string mapButtonFormat = "SeaglideMapToolip";

	private void Start()
	{
		toggleLights.lightsCallback += onLightsToggled;
		toggleLights.SetLightsActive(isActive: false);
		lightColor = toggleLights.FindLightColor();
		if (screenEffectModel != null)
		{
			screenEffectMat1 = screenEffectModel.GetComponent<Renderer>().materials[0];
			screenEffectMat2 = screenEffectModel.GetComponent<Renderer>().materials[1];
		}
	}

	private void SetVFXActive(bool state)
	{
		if (state)
		{
			bubbles.Play();
			trailFX.Play();
		}
		else
		{
			bubbles.Stop();
			trailFX.Stop();
		}
	}

	private void UpdateActiveState()
	{
		bool num = activeState;
		activeState = false;
		if (energyMixin.charge > 0f)
		{
			if (screenEffectModel != null)
			{
				screenEffectModel.SetActive(usingPlayer != null);
			}
			if (usingPlayer != null && usingPlayer.IsSwimming())
			{
				Vector3 moveDirection = GameInput.GetMoveDirection();
				activeState = moveDirection.x != 0f || moveDirection.z != 0f;
			}
			if (powerGlideActive)
			{
				activeState = true;
			}
		}
		if (num != activeState)
		{
			SetVFXActive(activeState);
		}
	}

	private void UpdatePropeller()
	{
		if (activeState)
		{
			propSpeed += Time.deltaTime * spinUpSpeed;
		}
		else
		{
			propSpeed -= Time.deltaTime * spinDownSpeed;
		}
		propSpeed = Mathf.Clamp(propSpeed, 0f, maxSpinSpeed);
		if (powerGlideActive)
		{
			propSpeed *= 1.5f;
		}
		if (animator.gameObject.activeInHierarchy)
		{
			SafeAnimator.SetBool(animator, "moving", activeState);
		}
	}

	public float GetActiveScalar()
	{
		return propSpeed / maxSpinSpeed;
	}

	public bool HasEnergy()
	{
		return !energyMixin.IsDepleted();
	}

	public void UpdateEnergy()
	{
		if (activeState)
		{
			timeSinceUse += Time.deltaTime;
			if (timeSinceUse >= 1f)
			{
				energyMixin.ConsumeEnergy(0.1f);
				timeSinceUse -= 1f;
			}
		}
		if (powerGlideActive)
		{
			float num = 1f * Time.deltaTime;
			if (energyMixin.charge >= num)
			{
				energyMixin.ConsumeEnergy(num);
			}
		}
		powerGlideParam = Mathf.Lerp(powerGlideParam, powerGlideActive ? 1f : 0f, Time.deltaTime * 3f);
		powerGlideForce = Mathf.Lerp(powerGlideForce, powerGlideActive ? 50000f : 0f, Time.deltaTime * 20000f);
	}

	private void UpdatePropFX()
	{
		float target = 0f;
		if (usingPlayer != null && activeState)
		{
			target = Mathf.Clamp(GetComponent<Rigidbody>().velocity.magnitude / 5f, 0f, 1f);
			_ = GetComponent<Rigidbody>().velocity.magnitude;
		}
		_smoothedMoveSpeed = Mathf.MoveTowards(_smoothedMoveSpeed, target, Time.deltaTime);
		if (screenEffectMat1 != null)
		{
			screenEffectMat1.SetColor(ShaderPropertyID._Color, gradientInner.Evaluate(_smoothedMoveSpeed));
		}
		if (screenEffectMat2 != null)
		{
			screenEffectMat2.SetColor(ShaderPropertyID._Color, gradientOuter.Evaluate(_smoothedMoveSpeed));
		}
	}

	private void Update()
	{
		UpdateActiveState();
		UpdatePropeller();
		UpdatePropFX();
		UpdateEnergy();
		if (usingPlayer != null && toggleLights != null)
		{
			toggleLights.CheckLightToggle();
		}
		if (activeState)
		{
			engineRPMManager.AccelerateInput();
		}
	}

	private void FixedUpdate()
	{
		if (powerGlideActive)
		{
			Player.main.gameObject.GetComponent<Rigidbody>().AddForce(base.gameObject.transform.forward * powerGlideForce, ForceMode.Force);
		}
	}

	public override bool OnRightHandUp()
	{
		if (CraftData.GetTechType(base.gameObject) == TechType.PowerGlide)
		{
			powerGlideActive = false;
		}
		return false;
	}

	public override bool OnRightHandHeld()
	{
		if (CraftData.GetTechType(base.gameObject) == TechType.PowerGlide && energyMixin.charge > 0f)
		{
			powerGlideActive = true;
		}
		return false;
	}

	public void onLightsToggled(bool active)
	{
	}

	public override void OnDraw(Player p)
	{
		onLightsToggled(toggleLights.lightsActive);
		base.OnDraw(p);
	}

	public override void OnHolster()
	{
		onLightsToggled(active: false);
		OnToolAnimHolster();
		base.OnHolster();
	}

	public override string GetCustomUseText()
	{
		string buttonFormat = LanguageCache.GetButtonFormat("SeaglideLightsTooltip", GameInput.Button.RightHand);
		string buttonFormat2 = LanguageCache.GetButtonFormat("SeaglideMapToolip", GameInput.Button.AltTool);
		if (cachedPrimaryUseText != buttonFormat || cachedAltUseText != buttonFormat2)
		{
			cachedPrimaryUseText = buttonFormat;
			cachedAltUseText = buttonFormat2;
			cachedUseText = $"{buttonFormat}, {buttonFormat2}";
		}
		return cachedUseText;
	}
}
