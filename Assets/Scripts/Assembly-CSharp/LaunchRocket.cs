using System;
using System.Collections;
using Gendarme;
using Story;
using UnityEngine;
using UnityEngine.SceneManagement;
using mset;

[SkipProtoContractCheck]
public class LaunchRocket : HandTarget, IHandTarget
{
	[Serializable]
	public struct Subtitle
	{
		public string name;

		public float delay;

		[HideInInspector]
		public bool played;
	}

	[AssertNotNull]
	public RocketPreflightCheckManager preflightCheckManager;

	[AssertNotNull]
	public CanvasGroup panelCanvasFader;

	[AssertNotNull]
	public PDANotification gunNotDisabled;

	[Header("Animation")]
	[AssertNotNull]
	public PlayerCinematicController cinematicController;

	[AssertNotNull]
	public Transform rootTransform;

	[AssertNotNull]
	public Transform rocketTrajectoryHelper;

	[AssertNotNull]
	public Animator primarySceneAnimator;

	[Header("Main")]
	[AssertNotNull]
	public GameObject endSequenceGo;

	[AssertNotNull]
	public StoryGoal launchRocketGoal;

	[Header("Time Capsule")]
	[AssertNotNull]
	public TimeCapsuleHandScanner timeCapsuleHandScanner;

	[Header("Planet & Sun")]
	[AssertNotNull]
	public Transform endSequencePlanetHelper;

	[AssertNotNull]
	public Transform endSequenceSunHelper;

	[AssertNotNull]
	public Vector3 sunStartPos = new Vector3(263977f, 64154f, 0f);

	[AssertNotNull]
	public Vector3 sunEndPos = new Vector3(-469064f / (float)Math.PI, -226952f, 0f);

	[AssertNotNull]
	public AnimationCurve sunRotationCurve;

	[Header("Clouds & Sky")]
	[AssertNotNull]
	public GameObject planetGo;

	[AssertNotNull]
	public GameObject cloudsDomeGo;

	[AssertNotNull]
	public AnimationCurve spaceTransitionCurve;

	[AssertNotNull]
	public AnimationCurve skyTransitionCurve;

	[Header("Lighting")]
	public float dayNightTime = 0.4f;

	[AssertNotNull]
	public Sky spaceSky;

	[AssertNotNull]
	public SkyApplier spaceSkyApp;

	[AssertNotNull]
	public Sky interiorSky;

	[AssertNotNull]
	public SkyApplier skyApp;

	[AssertNotNull]
	public Sky exteriorSky;

	[AssertNotNull]
	public SkyApplier skyAppExterior;

	[AssertNotNull]
	public AnimationCurve powerLossCurve;

	[AssertNotNull]
	public AnimationCurve exteriorPowerLossCurve;

	[AssertNotNull]
	public GameObject godRaysGo;

	[Header("Sound")]
	[AssertNotNull]
	public FMOD_CustomEmitter endSFX;

	[AssertNotNull]
	public FMOD_CustomEmitter endMusic;

	[Header("FX")]
	public float sequenceDuration = 103f;

	public float speedFxDelay = 25f;

	public float debrisImpactDelay = 51f;

	public float planetRevealDelay = 65f;

	public float godRaysDelay = 80f;

	public float warpDelay = 95f;

	[AssertNotNull]
	public VFXController fxControl;

	[AssertNotNull]
	public AnimationCurve lensFlareBrightnessCurve;

	[AssertNotNull]
	public AnimationCurve radialBlurCurve;

	[Header("Story")]
	public Subtitle[] subtitles;

	[Header("Rocket shield")]
	[AssertNotNull]
	public Renderer shieldRenderer;

	[AssertNotNull]
	public Transform shieldImpactHelper;

	[AssertNotNull]
	public AnimationCurve shieldIntensityCurve;

	private float shieldImpactIntensity;

	private float shieldIntensity;

	private float shieldGoToIntensity;

	[Header("Final Outro")]
	[AssertNotNull]
	public GameObject splashScreenPrefab;

	public float fadeToWhiteDelay = 101f;

	public float telepathyDelay = 6f;

	public float finalOutroSequenceDuration = 100f;

	private RadialBlurScreenFXController radialBlurControl;

	private MaterialPropertyBlock block;

	private Color fxColor = Color.white;

	private float animTime = -1f;

	private static bool launchStarted;

	private bool forcedRocketReady;

	private float timePassed;

	public static bool isLaunching => launchStarted;

	public override void Awake()
	{
		block = new MaterialPropertyBlock();
		radialBlurControl = MainCamera.camera.GetComponent<RadialBlurScreenFXController>();
		DevConsole.RegisterConsoleCommand(this, "forcerocketready");
	}

	public void OnConsoleCommand_forcerocketready()
	{
		forcedRocketReady = true;
	}

	private void PlaySpeedFX()
	{
		fxControl.Play(0);
	}

	private void PlayImpactFX()
	{
		fxControl.Play(1);
		shieldImpactIntensity = 1f;
	}

	private void PlayRedLightsAlarmFX()
	{
		fxControl.Play(4);
	}

	private void PlaySecondImpactFX()
	{
		shieldImpactIntensity = 1f;
	}

	private void RevealPlanet()
	{
		planetGo.SetActive(value: true);
	}

	private void StartGodRays()
	{
		godRaysGo.SetActive(value: true);
	}

	private void Warp()
	{
		fxControl.Play(3);
	}

	private void FinalSplashScreen()
	{
		Utils.SpawnPrefabAt(splashScreenPrefab, MainCamera.camera.transform, MainCamera.camera.transform.position);
	}

	private void PlayTelepathyFX()
	{
		MainCamera.camera.GetComponent<TelepathyScreenFXController>().StartFinalTelepathy();
	}

	private void EndCredits()
	{
		MainCamera.camera.GetComponent<TelepathyScreenFXController>().StopTelepathy();
		uSkyManager.main.RevertRocketLaunch();
		cinematicController.OnPlayerCinematicModeEnd();
		EndCreditsManager.showEaster = true;
		AddressablesUtility.LoadSceneAsync("EndCreditsSceneCleaner", LoadSceneMode.Single);
	}

	private void HideCrashedShip()
	{
		SceneManager.UnloadSceneAsync("Aurora");
	}

	private void Update()
	{
		if (launchStarted)
		{
			timePassed += Time.deltaTime;
			for (int i = 0; i < subtitles.Length; i++)
			{
				if (!subtitles[i].played && timePassed > subtitles[i].delay)
				{
					subtitles[i].played = true;
					Subtitles.Add(subtitles[i].name);
					break;
				}
			}
			animTime += Time.deltaTime / sequenceDuration;
			float time = Mathf.Clamp01(animTime);
			endSequenceSunHelper.localPosition = Vector3.Lerp(sunStartPos, sunEndPos, sunRotationCurve.Evaluate(time));
			uSkyManager.main.SetEndSequenceVariables(skyTransitionCurve.Evaluate(time), lensFlareBrightnessCurve.Evaluate(time));
			radialBlurControl.SetAmount(radialBlurCurve.Evaluate(time));
			uSkyManager.main.spaceTransition = spaceTransitionCurve.Evaluate(time);
			shieldImpactIntensity = Mathf.MoveTowards(shieldImpactIntensity, 0f, Time.deltaTime / 4f);
			shieldIntensity = shieldIntensityCurve.Evaluate(time);
			shieldRenderer.gameObject.SetActive(shieldIntensity > 0f);
			shieldRenderer.material.SetFloat(ShaderPropertyID._Intensity, shieldIntensity);
			shieldRenderer.material.SetFloat(ShaderPropertyID._ImpactIntensity, shieldImpactIntensity);
			shieldRenderer.material.SetVector(ShaderPropertyID._ImpactPosition, shieldImpactHelper.position);
			for (int j = 0; j < skyApp.renderers.Length; j++)
			{
				Renderer obj = skyApp.renderers[j];
				block.Clear();
				obj.GetPropertyBlock(block);
				float num = powerLossCurve.Evaluate(time);
				block.SetFloat(ShaderPropertyID._UwePowerLoss, num);
				Vector4 vector = block.GetVector(ShaderPropertyID._ExposureIBL);
				vector.x = Mathf.Lerp(interiorSky.DiffIntensity, 0f, num);
				vector.y = Mathf.Lerp(interiorSky.SpecIntensity, 0f, num);
				block.SetVector(ShaderPropertyID._ExposureIBL, vector);
				obj.SetPropertyBlock(block);
			}
			for (int k = 0; k < skyAppExterior.renderers.Length; k++)
			{
				Renderer obj2 = skyAppExterior.renderers[k];
				block.Clear();
				obj2.GetPropertyBlock(block);
				float num2 = exteriorPowerLossCurve.Evaluate(time);
				block.SetFloat(ShaderPropertyID._UwePowerLoss, num2);
				Vector4 vector2 = block.GetVector(ShaderPropertyID._ExposureIBL);
				vector2.x = Mathf.Lerp(exteriorSky.DiffIntensity, 0f, num2);
				vector2.y = Mathf.Lerp(exteriorSky.SpecIntensity, 0f, num2);
				block.SetVector(ShaderPropertyID._ExposureIBL, vector2);
				obj2.SetPropertyBlock(block);
			}
			for (int l = 0; l < spaceSkyApp.renderers.Length; l++)
			{
				Renderer target = spaceSkyApp.renderers[l];
				spaceSky.ApplyFast(target, 0);
			}
		}
		if (launchStarted && panelCanvasFader.alpha > 0f)
		{
			panelCanvasFader.alpha = Mathf.MoveTowards(panelCanvasFader.alpha, 0f, Time.deltaTime * 2f);
		}
	}

	private bool IsRocketReady()
	{
		if (!preflightCheckManager.ReturnRocketReadyForLaunch(useDelay: true))
		{
			return forcedRocketReady;
		}
		return true;
	}

	public void OnHandHover(GUIHand hand)
	{
		bool flag = IsRocketReady();
		if (flag)
		{
			flag = !WaitScreen.IsWaiting;
		}
		string text = (flag ? "Launch_Rocket" : "Rocket_NotReady");
		HandReticle.main.SetText(HandReticle.TextType.Hand, text, translate: true);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Interact);
	}

	private static void SetLaunchStarted()
	{
		launchStarted = true;
	}

	public void OnHandClick(GUIHand hand)
	{
		if (IsRocketReady() && !launchStarted)
		{
			if (!StoryGoalCustomEventHandler.main.gunDisabled && !forcedRocketReady)
			{
				gunNotDisabled.Play();
				return;
			}
			SetLaunchStarted();
			PlayerTimeCapsule.main.Submit(null);
			StartCoroutine(StartEndCinematic());
			HandReticle.main.RequestCrosshairHide();
		}
	}

	private IEnumerator StartEndCinematic()
	{
		timePassed = 0f;
		NoDamageConsoleCommand.main.SetNoDamageCheat(state: true);
		endSequenceGo.SetActive(value: true);
		cloudsDomeGo.SetActive(value: true);
		DayNightCycle.main.SetDayNightTime(dayNightTime);
		DayNightCycle.main.Pause();
		uSkyManager.main.LaunchRocket(rocketTrajectoryHelper, endSequenceSunHelper, endSequencePlanetHelper);
		Inventory.main.quickSlots.DeselectImmediate();
		launchRocketGoal.Trigger();
		timeCapsuleHandScanner.LaunchTimeCapsule();
		fxControl.Play(2);
		yield return null;
		StoryGoalScheduler.main.Pause();
		Invoke("HideCrashedShip", 1f);
		Invoke("PlaySpeedFX", speedFxDelay);
		Invoke("PlayImpactFX", debrisImpactDelay);
		Invoke("PlayRedLightsAlarmFX", debrisImpactDelay - 3f);
		Invoke("PlaySecondImpactFX", debrisImpactDelay + 3f);
		Invoke("RevealPlanet", planetRevealDelay);
		Invoke("StartGodRays", godRaysDelay);
		Invoke("Warp", warpDelay);
		Invoke("FinalSplashScreen", fadeToWhiteDelay);
		Invoke("PlayTelepathyFX", fadeToWhiteDelay + telepathyDelay);
		Invoke("EndCredits", fadeToWhiteDelay + finalOutroSequenceDuration);
		animTime = 0f;
		primarySceneAnimator.SetBool("ending_sequence", value: true);
		cinematicController.StartCinematicMode(Player.main);
		endSFX.Play();
		endMusic.Play();
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private void OnDestroy()
	{
		launchStarted = false;
	}
}
