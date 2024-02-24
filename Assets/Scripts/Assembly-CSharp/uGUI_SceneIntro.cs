using System;
using System.Collections;
using System.Text;
using Story;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(FMOD_StudioEventEmitter))]
public class uGUI_SceneIntro : uGUI_Scene, IInputHandler
{
	private delegate bool Condition();

	private const GameInput.Button skipButton = GameInput.Button.UIMenu;

	[AssertLocalization(1)]
	private const string skipIntroKey = "SkipIntro";

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.Update;

	private const float hintDelay = 0f;

	private const float hintIn = 1f;

	private const float hintDuration = 3f;

	private const float hintOut = 2f;

	private const float skipTimeout = 1.5f;

	[AssertNotNull]
	public uGUI_Fader fader;

	[AssertNotNull]
	public uGUI_TextFade mainText;

	[AssertNotNull]
	public TextMeshProUGUI skipText;

	[AssertNotNull]
	public uGUI_CircularBar skipProgress;

	public float timeBeforeCinematicStart;

	public float timeLaunching;

	public float timeBlackout;

	public float blackoutDuration;

	[AssertNotNull]
	public FMOD_StudioEventEmitter emitter;

	public FMODAsset unmuteEvent;

	public string launchingText;

	private bool moveNext;

	private Coroutine coroutine;

	private Action onIntroDone;

	private float skipHintStartTime = -1f;

	public bool showing => coroutine != null;

	private void Start()
	{
		fader.SetState(base.enabled);
		fader.FadeOut();
		mainText.SetState(enabled: false);
		skipText.SetAlpha(0f);
		skipProgress.value = 0f;
		UpdateBindings();
		GameInput.OnBindingsChanged += OnBindingsChanged;
	}

	private void OnDestroy()
	{
		GameInput.OnBindingsChanged -= OnBindingsChanged;
	}

	private void OnBindingsChanged()
	{
		UpdateBindings();
	}

	private void UpdateBindings()
	{
		Language main = Language.main;
		skipText.SetText(main.GetFormat("SkipIntro", GameInput.FormatButton(GameInput.Button.UIMenu)));
	}

	public void Play(Action callback)
	{
		if (!showing)
		{
			onIntroDone = callback;
			coroutine = StartCoroutine(IntroSequence());
			ManagedUpdate.Subscribe(ManagedUpdate.Queue.Update, OnUpdate);
			InputHandlerStack.main.Push(this);
			BiomeGoalTracker.main.enabled = false;
		}
	}

	public void Stop(bool isInterrupted)
	{
		ResumeGameTime();
		MainMenuMusic.Stop();
		VRLoadingOverlay.Hide();
		BiomeGoalTracker.main.enabled = true;
		if (showing)
		{
			StopCoroutine(coroutine);
			coroutine = null;
			skipText.SetAlpha(0f);
			skipProgress.value = 0f;
			ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.Update, OnUpdate);
			mainText.SetState(enabled: false);
			fader.SetState(enabled: false);
			emitter.Stop();
			Utils.PlayFMODAsset(unmuteEvent);
			EscapePod.main.StopIntroCinematic(isInterrupted);
			skipHintStartTime = -1f;
			if (onIntroDone != null)
			{
				Action action = onIntroDone;
				onIntroDone = null;
				action();
			}
		}
	}

	private void OnUpdate()
	{
		if (skipHintStartTime >= 0f)
		{
			float time = Time.time;
			if (GameInput.AnyKeyDown)
			{
				skipHintStartTime = time - 1f;
			}
			float buttonHeldTime = GameInput.GetButtonHeldTime(GameInput.Button.UIMenu);
			if (buttonHeldTime >= 1.5f)
			{
				Stop(isInterrupted: true);
				return;
			}
			float t = time - skipHintStartTime;
			float value = MathExtensions.Trapezoid(0f, 1f, 3f, 2f, t, wrap: false);
			value = MathExtensions.EaseInOutSine(value);
			skipText.SetAlpha(value);
			skipProgress.value = Mathf.Clamp01(buttonHeldTime / 1.5f);
		}
	}

	private IEnumerator IntroSequence()
	{
		fader.SetState(enabled: true);
		PauseGameTime();
		yield return new WaitForSecondsRealtime(0.5f);
		mainText.SetText("");
		yield return new WaitForSecondsRealtime(2f);
		while (!LargeWorldStreamer.main.IsWorldSettled())
		{
			yield return new WaitForSecondsRealtime(1f);
		}
		mainText.SetText(Language.main.Get("PressAnyButton"));
		mainText.SetState(enabled: true);
		VRLoadingOverlay.Hide();
		while (!GameInput.AnyKeyDown)
		{
			yield return null;
		}
		moveNext = false;
		mainText.FadeOut(0.2f, Callback);
		while (!moveNext)
		{
			yield return null;
		}
		emitter.StartEvent();
		float timeFootStepSoundStart = Time.time;
		MainMenuMusic.Stop();
		EscapePod.main.TriggerIntroCinematic();
		uGUI_EscapePod.main.SetHeader(Language.main.Get("IntroEscapePod1Header"), new Color32(243, 94, 63, byte.MaxValue), 4f);
		uGUI_EscapePod.main.SetContent(Language.main.Get("IntroEscapePod1Content"), new Color32(233, 63, 27, byte.MaxValue));
		uGUI_EscapePod.main.SetPower(Language.main.Get("IntroEscapePod1Power"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		skipHintStartTime = Time.time;
		mainText.SetText(Language.main.Get("IntroUWEPresents"), translate: true);
		moveNext = false;
		mainText.FadeIn(1f, Callback);
		while (!moveNext)
		{
			yield return null;
		}
		yield return new WaitForSecondsRealtime(3f);
		mainText.FadeOut(1f, Callback);
		while (Time.time < timeFootStepSoundStart + timeBeforeCinematicStart)
		{
			yield return null;
		}
		moveNext = false;
		fader.FadeOut(3f, Callback);
		if (XRSettings.enabled && VROptions.skipIntro)
		{
			Stop(isInterrupted: true);
			GameObject gameObject = GameObject.Find("fire_extinguisher_01_tp");
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			GameObject gameObject2 = GameObject.Find("IntroFireExtinugisherPickup");
			if (gameObject2 != null)
			{
				UnityEngine.Object.Destroy(gameObject2);
			}
		}
		while (Time.time < timeFootStepSoundStart + timeLaunching)
		{
			yield return null;
		}
		if (!string.IsNullOrEmpty(launchingText))
		{
			Subtitles.Add(launchingText);
		}
		while (Time.time < timeFootStepSoundStart + timeBlackout)
		{
			yield return null;
		}
		fader.SetState(enabled: true);
		yield return new WaitForSecondsRealtime(blackoutDuration);
		uGUI_EscapePod.main.SetHeader(Language.main.Get("IntroEscapePod2Header"), new Color32(243, 94, 63, byte.MaxValue), 4f);
		uGUI_EscapePod.main.SetContent(Language.main.Get("IntroEscapePod2Content"), new Color32(233, 63, 27, byte.MaxValue));
		uGUI_EscapePod.main.SetPower(Language.main.Get("IntroEscapePod2Power"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		fader.FadeOut(4f, Callback);
		while (EscapePod.main.IsPlayingIntroCinematic())
		{
			yield return null;
		}
		Stop(isInterrupted: false);
		StartCoroutine(ControlsHints());
	}

	private void ResumeGameTime()
	{
		DayNightCycle main = DayNightCycle.main;
		if (main == null)
		{
			return;
		}
		main.Resume();
		Player main2 = Player.main;
		if (!(main2 == null))
		{
			Survival component = main2.GetComponent<Survival>();
			if (!(component == null))
			{
				component.freezeStats = false;
			}
		}
	}

	private void PauseGameTime()
	{
		DayNightCycle main = DayNightCycle.main;
		if (main == null)
		{
			return;
		}
		main.Pause();
		Player main2 = Player.main;
		if (!(main2 == null))
		{
			Survival component = main2.GetComponent<Survival>();
			if (!(component == null))
			{
				component.freezeStats = true;
			}
		}
	}

	private void Callback()
	{
		moveNext = true;
	}

	bool IInputHandler.HandleInput()
	{
		if (!showing)
		{
			return false;
		}
		if (skipHintStartTime >= 0f)
		{
			float time = Time.time;
			if (GameInput.AnyKeyDown)
			{
				skipHintStartTime = time - 1f;
			}
			float buttonHeldTime = GameInput.GetButtonHeldTime(GameInput.Button.UIMenu);
			if (buttonHeldTime >= 1.5f)
			{
				Stop(isInterrupted: true);
				return false;
			}
			skipProgress.value = Mathf.Clamp01(buttonHeldTime / 1.5f);
		}
		return true;
	}

	bool IInputHandler.HandleLateInput()
	{
		return showing;
	}

	public void OnFocusChanged(InputFocusMode mode)
	{
	}

	private IEnumerator ControlsHints()
	{
		Hint.main.message.anchor = TextAnchor.UpperCenter;
		Player player = Player.main;
		yield return new WaitForSeconds(1f);
		Condition condition = () => GameInput.GetLookDelta().sqrMagnitude > 0f;
		bool gamepad = GameInput.PrimaryDevice == GameInput.Device.Controller;
		string format = Language.main.GetFormat("HintLook", GameInput.FormatButton(GameInput.Button.Look));
		yield return HintRoutine(10f, condition, format, Language.main.Get("HintSuccess"), usePDATime: false);
		condition = () => GameInput.GetMoveDirection().sqrMagnitude > 0f;
		if (gamepad)
		{
			format = Language.main.GetFormat("HintMove", GameInput.FormatButton(GameInput.Button.Move));
		}
		else
		{
			using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
			{
				StringBuilder sb = stringBuilderPool.sb;
				string value = Language.main.Get("InputSeparator");
				sb.AppendFormat(GameInput.FormatButton(GameInput.Button.MoveForward)).Append(value).AppendFormat(GameInput.FormatButton(GameInput.Button.MoveBackward))
					.Append(value)
					.AppendFormat(GameInput.FormatButton(GameInput.Button.MoveLeft))
					.Append(value)
					.AppendFormat(GameInput.FormatButton(GameInput.Button.MoveRight));
				format = Language.main.GetFormat("HintMove", sb.ToString());
			}
		}
		yield return HintRoutine(5f, condition, format, Language.main.Get("HintSuccess"), usePDATime: false);
		condition = () => Inventory.main.container.Contains(TechType.FireExtinguisher);
		format = Language.main.GetFormat("HintPickupFireExtinguisher", GameInput.FormatButton(GameInput.Button.LeftHand));
		yield return HintRoutine(5f, condition, format, Language.main.Get("HintSuccess"), usePDATime: false);
		condition = delegate
		{
			Pickupable held = Inventory.main.GetHeld();
			return held != null && held.GetTechType() == TechType.FireExtinguisher && GameInput.GetButtonHeld(GameInput.Button.RightHand);
		};
		format = Language.main.GetFormat("HintUseFireExtinguisher", GameInput.FormatButton(GameInput.Button.RightHand));
		yield return HintRoutine(1f, condition, format, Language.main.Get("HintSuccess"), usePDATime: false);
		PDA pda = player.GetPDA();
		while (pda.state != 0)
		{
			yield return null;
		}
		condition = () => pda.state == PDA.State.Closing;
		format = Language.main.GetFormat("HintOpenClosePDA", GameInput.FormatButton(GameInput.Button.PDA));
		yield return HintRoutine(30f, condition, format, Language.main.Get("HintSuccess"), usePDATime: true);
		while (pda.state != PDA.State.Closed)
		{
			yield return null;
		}
		QuickSlots quickSlots = Inventory.main.quickSlots;
		yield return null;
		int slot = quickSlots.GetActiveSlotID();
		condition = () => slot != quickSlots.GetActiveSlotID();
		format = ((!gamepad) ? Language.main.GetFormat("HintKeyboardQuickslots", GameInput.FormatButton(GameInput.Button.Slot1), GameInput.FormatButton(GameInput.Button.Slot2), GameInput.FormatButton(GameInput.Button.Slot3), GameInput.FormatButton(GameInput.Button.Slot4), GameInput.FormatButton(GameInput.Button.Slot5)) : Language.main.GetFormat("HintGamepadQuickslots", GameInput.FormatButton(GameInput.Button.CyclePrev), GameInput.FormatButton(GameInput.Button.CycleNext)));
		yield return HintRoutine(1f, condition, format, Language.main.Get("HintSuccess"), usePDATime: false);
	}

	private IEnumerator HintRoutine(float timeout, Condition condition, string message, string success, bool usePDATime)
	{
		uGUI_PopupMessage hint = Hint.main.message;
		while (!condition())
		{
			timeout -= (usePDATime ? PDA.deltaTime : Time.deltaTime);
			if (timeout > 0f)
			{
				yield return null;
				continue;
			}
			hint.SetText(message, TextAnchor.MiddleCenter);
			timeout = 60f;
			hint.Show(timeout);
			while (!condition())
			{
				if (timeout > 0f)
				{
					yield return null;
					timeout -= (usePDATime ? PDA.deltaTime : Time.deltaTime);
					continue;
				}
				yield break;
			}
			hint.SetText(success, TextAnchor.MiddleCenter);
			hint.Show(1f);
			break;
		}
	}
}
