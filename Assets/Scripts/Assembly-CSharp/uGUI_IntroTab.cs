using Gendarme;
using TMPro;
using UnityEngine;

public class uGUI_IntroTab : uGUI_PDATab
{
	private enum Phase
	{
		None = 0,
		Delay = 1,
		Begin = 2,
		Loop = 3,
		End = 4
	}

	private const float timeUntilButtonPress = 3.9f;

	private const float timeLoading = 9f;

	[AssertLocalization]
	private const string pdaBootingEmergencyKey = "PDABootingEmergency";

	[AssertLocalization(1)]
	private const string pdaBootingLoadPercentFormat = "PDABootLoadingPercent";

	[AssertNotNull]
	public Animation animation;

	[AssertNotNull]
	public TextMeshProUGUI textBooting;

	[AssertNotNull]
	public TextMeshProUGUI textLoading;

	[AssertNotNull]
	public uGUI_CircularBar progressIndicator;

	[AssertNotNull]
	public AnimationClip clipBegin;

	[AssertNotNull]
	public AnimationClip clipLoop;

	[AssertNotNull]
	public AnimationClip clipEnd;

	[AssertNotNull]
	public FMODAsset soundFirstUse;

	private Phase phase;

	private Sequence sequence = new Sequence(initialState: false);

	private AnimationState begin;

	private AnimationState loop;

	private AnimationState end;

	private float f;

	public override int notificationsCount => 0;

	protected override void Awake()
	{
		animation.playAutomatically = false;
		animation.Stop();
		animation.AddClip(clipBegin, "begin");
		animation.AddClip(clipLoop, "loop");
		animation.AddClip(clipEnd, "end");
		begin = animation["begin"];
		loop = animation["loop"];
		end = animation["end"];
		PrepareState(begin);
		PrepareState(loop);
		PrepareState(end);
		loop.wrapMode = WrapMode.Loop;
		progressIndicator.value = 0f;
	}

	public override void Open()
	{
		if (phase == Phase.None)
		{
			pda.soundQueue.PlayImmediately(soundFirstUse);
			Callback();
			animation.Sample();
		}
		base.gameObject.SetActive(value: true);
	}

	public override void Close()
	{
		phase = Phase.None;
		UpdateAnimationState();
		sequence.ForceState(state: false);
		pda.SetBackgroundAlpha(1f);
		base.gameObject.SetActive(value: false);
	}

	public override void OnUpdate(bool isOpen)
	{
		if (!isOpen)
		{
			return;
		}
		sequence.Update(PDA.deltaTime);
		if (!sequence.active)
		{
			return;
		}
		float t = sequence.t;
		switch (phase)
		{
		case Phase.Begin:
			begin.normalizedTime = t;
			break;
		case Phase.Loop:
			loop.normalizedTime = t * 9f % 1f;
			if (t >= f)
			{
				f = t + Random.Range(0.01f, 0.2f);
				if (f > 1f)
				{
					f = 1f;
				}
			}
			UpdateLoadingText(f);
			break;
		case Phase.End:
			end.normalizedTime = t;
			break;
		}
	}

	public override void OnLanguageChanged()
	{
		textBooting.text = Language.main.Get("PDABootingEmergency");
		UpdateLoadingText(progressIndicator.value);
	}

	public override uGUI_INavigableIconGrid GetInitialGrid()
	{
		return null;
	}

	[SuppressMessage("Gendarme.Rules.Naming", "AvoidRedundancyInMethodNameRule")]
	public override bool OnButtonDown(GameInput.Button button)
	{
		if (button == GameInput.Button.UINextTab || button == GameInput.Button.UIPrevTab || button == GameInput.Button.UICancel)
		{
			return true;
		}
		return false;
	}

	private void PrepareState(AnimationState state)
	{
		state.blendMode = AnimationBlendMode.Blend;
		state.enabled = false;
		state.layer = 0;
		state.speed = 0f;
		state.weight = 0f;
		state.wrapMode = WrapMode.Once;
	}

	private void UpdateLoadingText(float value)
	{
		progressIndicator.value = value;
		textLoading.text = Language.main.GetFormat("PDABootLoadingPercent", progressIndicator.value);
	}

	private void Callback()
	{
		begin.normalizedTime = 0f;
		loop.normalizedTime = 0f;
		end.normalizedTime = 0f;
		switch (phase)
		{
		case Phase.None:
			phase = Phase.Delay;
			UpdateAnimationState();
			pda.SetBackgroundAlpha(0f);
			sequence.Set(3.9f, current: false, target: true, Callback);
			break;
		case Phase.Delay:
			phase = Phase.Begin;
			UpdateAnimationState();
			f = 0f;
			UpdateLoadingText(0f);
			pda.RevealBackground();
			sequence.Set(begin.length, current: false, target: true, Callback);
			break;
		case Phase.Begin:
			phase = Phase.Loop;
			UpdateAnimationState();
			sequence.Set(9f, current: false, target: true, Callback);
			break;
		case Phase.Loop:
			phase = Phase.End;
			UpdateAnimationState();
			sequence.Set(end.length, current: false, target: true, Callback);
			break;
		case Phase.End:
			phase = Phase.None;
			UpdateAnimationState();
			sequence.ForceState(state: false);
			pda.OpenTab(PDATab.Inventory);
			pda.RevealContent();
			break;
		}
	}

	private void UpdateAnimationState()
	{
		bool flag = phase == Phase.Delay || phase == Phase.Begin;
		bool flag2 = phase == Phase.Loop;
		bool flag3 = phase == Phase.End;
		begin.enabled = flag;
		loop.enabled = flag2;
		end.enabled = flag3;
		begin.weight = (flag ? 1f : 0f);
		loop.weight = (flag2 ? 1f : 0f);
		end.weight = (flag3 ? 1f : 0f);
	}
}
