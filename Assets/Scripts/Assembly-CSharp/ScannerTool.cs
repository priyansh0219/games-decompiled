using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScannerTool : PlayerTool
{
	public enum ScreenState
	{
		None = 0,
		Default = 1,
		Ready = 2,
		Scanning = 3,
		Unpowered = 4,
		NotInfected = 5,
		Infected = 6
	}

	public enum ScanState
	{
		None = 0,
		Scan = 1,
		SelfScan = 2
	}

	private const float infectionDisplayTime = 3f;

	private const float newScanDelay = 0.5f;

	private const float speedForEpileptics = 0.1f;

	public float powerConsumption = 0.2f;

	public const float scanDistance = 8f;

	public GameObject scanBeam;

	public float nozzleRotationSpeed = 10f;

	[Range(0.01f, 5f)]
	public float pointSwitchTimeMin = 0.1f;

	[Range(0.01f, 5f)]
	public float pointSwitchTimeMax = 1f;

	public Animator animator;

	public FMOD_CustomLoopingEmitter scanSound;

	public FMODAsset completeSound;

	public GameObject screenDefault;

	public GameObject screenProgress;

	public TextMeshProUGUI screenDefaultText;

	public TextMeshProUGUI screenProgressText;

	public SimpleAnimation screenAnimator;

	public Image screenProgressImage;

	public TextMeshProUGUI screenProgressValueText;

	public Texture2D scanCircuitTex;

	public Color scanCircuitColor = Color.white;

	public Texture2D scanOrganicTex;

	public Color scanOrganicColor = Color.white;

	public VFXController fxControl;

	private ScanState stateLast;

	private ScanState stateCurrent;

	private float idleTimer;

	private Material scanMaterialCircuitFX;

	private Material scanMaterialOrganicFX;

	private VFXOverlayMaterial scanFX;

	private Vector3 leftScanPoint;

	private Vector3 rightScanPoint;

	public override void Awake()
	{
		base.Awake();
		UpdateScreen(ScreenState.Default);
	}

	private void Start()
	{
		SetFXActive(state: false);
		Shader scannerToolScanning = ShaderManager.preloadedShaders.scannerToolScanning;
		if (scannerToolScanning != null)
		{
			scanMaterialCircuitFX = new Material(scannerToolScanning);
			scanMaterialCircuitFX.hideFlags = HideFlags.HideAndDontSave;
			scanMaterialCircuitFX.SetTexture(ShaderPropertyID._MainTex, scanCircuitTex);
			scanMaterialCircuitFX.SetColor(ShaderPropertyID._Color, scanCircuitColor);
			scanMaterialOrganicFX = new Material(scannerToolScanning);
			scanMaterialOrganicFX.hideFlags = HideFlags.HideAndDontSave;
			scanMaterialOrganicFX.SetTexture(ShaderPropertyID._MainTex, scanOrganicTex);
			scanMaterialOrganicFX.SetColor(ShaderPropertyID._Color, scanOrganicColor);
		}
	}

	private void OnDisable()
	{
		scanSound.Stop();
	}

	private void Update()
	{
		if (base.isDrawn)
		{
			if (idleTimer > 0f)
			{
				idleTimer = Mathf.Max(0f, idleTimer - Time.deltaTime);
			}
			string buttonFormat = LanguageCache.GetButtonFormat("ScannerSelfScanFormat", GameInput.Button.AltTool);
			HandReticle.main.SetTextRaw(HandReticle.TextType.Use, buttonFormat);
			HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, string.Empty);
		}
	}

	private void LateUpdate()
	{
		if (base.isDrawn)
		{
			bool flag = stateCurrent == ScanState.Scan;
			bool flag2 = stateCurrent == ScanState.SelfScan;
			if (idleTimer <= 0f)
			{
				OnHover();
			}
			SetFXActive(flag || flag2);
			if (animator != null && animator.isActiveAndEnabled)
			{
				SafeAnimator.SetBool(animator, "using_tool", flag);
				SafeAnimator.SetBool(animator, "using_tool_alt", flag2);
			}
			if (flag || flag2)
			{
				scanSound.Play();
			}
			else
			{
				scanSound.Stop();
			}
			stateLast = stateCurrent;
			stateCurrent = ScanState.None;
		}
	}

	public override bool OnRightHandDown()
	{
		if (Player.main.IsBleederAttached())
		{
			return true;
		}
		switch (Scan())
		{
		case PDAScanner.Result.Processed:
			ErrorMessage.AddDebug(Language.main.Get("ScannerInstanceKnown"));
			break;
		case PDAScanner.Result.Known:
			ErrorMessage.AddDebug(Language.main.GetFormat("ScannerEntityKnown", Language.main.Get(PDAScanner.scanTarget.techType.AsString())));
			break;
		}
		return true;
	}

	public override bool OnRightHandHeld()
	{
		Scan();
		return true;
	}

	public override bool OnAltDown()
	{
		Scan();
		return true;
	}

	public override bool OnAltHeld()
	{
		Scan();
		return true;
	}

	public override bool OnAltUp()
	{
		return true;
	}

	private PDAScanner.Result Scan()
	{
		if (stateCurrent != 0)
		{
			return PDAScanner.Result.None;
		}
		if (idleTimer > 0f)
		{
			return PDAScanner.Result.None;
		}
		PDAScanner.Result result = PDAScanner.Result.None;
		PDAScanner.ScanTarget scanTarget = PDAScanner.scanTarget;
		if (scanTarget.isValid && energyMixin.charge > 0f)
		{
			result = PDAScanner.Scan();
			switch (result)
			{
			case PDAScanner.Result.Scan:
			{
				float amount = powerConsumption * Time.deltaTime;
				energyMixin.ConsumeEnergy(amount);
				stateCurrent = ((!scanTarget.isPlayer) ? ScanState.Scan : ScanState.SelfScan);
				break;
			}
			case PDAScanner.Result.Done:
			case PDAScanner.Result.Researched:
				UpdateScreen(ScreenState.Default);
				idleTimer = 0.5f;
				PDASounds.queue.PlayIfFree(completeSound);
				if (fxControl != null)
				{
					fxControl.Play(0);
				}
				break;
			case PDAScanner.Result.NotInfected:
			case PDAScanner.Result.Infected:
				UpdateScreen((result == PDAScanner.Result.Infected) ? ScreenState.Infected : ScreenState.NotInfected);
				idleTimer = 3f;
				PDASounds.queue.PlayIfFree(completeSound);
				if (fxControl != null)
				{
					fxControl.Play(0);
				}
				break;
			}
		}
		return result;
	}

	private void OnHover()
	{
		if (energyMixin.charge <= 0f)
		{
			UpdateScreen(ScreenState.Unpowered);
			return;
		}
		PDAScanner.ScanTarget scanTarget = PDAScanner.scanTarget;
		if (!scanTarget.isValid)
		{
			UpdateScreen(ScreenState.Default);
		}
		else if (PDAScanner.CanScan(scanTarget) == PDAScanner.Result.Scan)
		{
			HandReticle main = HandReticle.main;
			if (stateCurrent != ScanState.SelfScan)
			{
				main.SetText(HandReticle.TextType.Hand, scanTarget.techType.AsString(), translate: true, GameInput.Button.RightHand);
				main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			}
			if (stateCurrent == ScanState.Scan || stateCurrent == ScanState.SelfScan)
			{
				UpdateScreen(ScreenState.Scanning, scanTarget.progress);
				return;
			}
			main.SetIcon(HandReticle.IconType.Scan, 1.5f);
			UpdateScreen(ScreenState.Ready);
		}
		else
		{
			UpdateScreen(ScreenState.Default);
		}
	}

	public override bool GetUsedToolThisFrame()
	{
		return stateCurrent == ScanState.Scan;
	}

	public override bool GetAltUsedToolThisFrame()
	{
		return stateCurrent == ScanState.SelfScan;
	}

	public override void OnHolster()
	{
		base.OnHolster();
		stateLast = ScanState.None;
		stateCurrent = ScanState.None;
		idleTimer = 0f;
		UpdateScreen(ScreenState.None);
		SetFXActive(state: false);
	}

	private void SetFXActive(bool state)
	{
		scanBeam.gameObject.SetActive(state);
		if (state && PDAScanner.scanTarget.isValid)
		{
			PlayScanFX();
		}
		else
		{
			StopScanFX();
		}
	}

	private void PlayScanFX()
	{
		PDAScanner.ScanTarget scanTarget = PDAScanner.scanTarget;
		if (!scanTarget.isValid)
		{
			return;
		}
		if (scanFX != null)
		{
			if (scanFX.gameObject != scanTarget.gameObject)
			{
				StopScanFX();
				scanFX = scanTarget.gameObject.AddComponent<VFXOverlayMaterial>();
				if (scanTarget.gameObject.GetComponent<Creature>() != null)
				{
					scanFX.ApplyOverlay(scanMaterialOrganicFX, "VFXOverlay: Scanning", instantiateMaterial: false);
				}
				else
				{
					scanFX.ApplyOverlay(scanMaterialCircuitFX, "VFXOverlay: Scanning", instantiateMaterial: false);
				}
			}
		}
		else
		{
			scanFX = scanTarget.gameObject.AddComponent<VFXOverlayMaterial>();
			if (scanTarget.gameObject.GetComponent<Creature>() != null)
			{
				scanFX.ApplyOverlay(scanMaterialOrganicFX, "VFXOverlay: Scanning", instantiateMaterial: false);
			}
			else
			{
				scanFX.ApplyOverlay(scanMaterialCircuitFX, "VFXOverlay: Scanning", instantiateMaterial: false);
			}
		}
		float value = 1f;
		if (!MiscSettings.flashes)
		{
			value = 0.1f;
		}
		scanMaterialCircuitFX.SetFloat(ShaderPropertyID._TimeScale, value);
		scanMaterialOrganicFX.SetFloat(ShaderPropertyID._TimeScale, value);
	}

	private void StopScanFX()
	{
		if (scanFX != null)
		{
			scanFX.RemoveOverlay();
		}
	}

	protected override void OnDestroy()
	{
		if (scanFX != null)
		{
			StopScanFX();
		}
		base.OnDestroy();
	}

	private void UpdateScreen(ScreenState state, float progress = 0f)
	{
		bool flag = state == ScreenState.Scanning;
		screenDefault.SetActive(!flag);
		screenProgress.SetActive(flag);
		screenAnimator.Off();
		switch (state)
		{
		case ScreenState.Default:
			screenDefaultText.text = Language.main.Get("ScannerScreenDefault");
			screenDefaultText.color = new Color32(159, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			break;
		case ScreenState.Ready:
			screenDefaultText.text = Language.main.Get("ScannerScreenReady");
			screenDefaultText.color = new Color32(159, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			screenAnimator.pulse = true;
			screenAnimator.pulseFrequency = 5f;
			screenAnimator.pulseMin = 0.1f;
			screenAnimator.pulseMax = 1f;
			break;
		case ScreenState.Scanning:
			screenProgressText.text = Language.main.Get("ScannerScreenScanning");
			screenProgressText.color = new Color32(159, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			screenProgressImage.fillAmount = Mathf.Clamp01(progress);
			screenProgressValueText.text = Mathf.RoundToInt(progress * 100f) + "%";
			break;
		case ScreenState.Unpowered:
			screenDefaultText.text = Language.main.Get("ScannerScreenUnpowered");
			screenDefaultText.color = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);
			screenAnimator.pulse = true;
			screenAnimator.pulseFrequency = 5f;
			screenAnimator.pulseMin = 0.1f;
			screenAnimator.pulseMax = 1f;
			break;
		case ScreenState.NotInfected:
			screenDefaultText.text = Language.main.Get("ScannerScreenNotInfected");
			screenDefaultText.color = new Color(0f, 255f, 0f, 255f);
			screenAnimator.pulse = true;
			screenAnimator.pulseFrequency = 5f;
			screenAnimator.pulseMin = 0.1f;
			screenAnimator.pulseMax = 1f;
			break;
		case ScreenState.Infected:
			screenDefaultText.text = Language.main.Get("ScannerScreenInfected");
			screenDefaultText.color = new Color(255f, 0f, 0f, 255f);
			screenAnimator.pulse = true;
			screenAnimator.pulseFrequency = 5f;
			screenAnimator.pulseMin = 0.1f;
			screenAnimator.pulseMax = 1f;
			break;
		}
	}
}
