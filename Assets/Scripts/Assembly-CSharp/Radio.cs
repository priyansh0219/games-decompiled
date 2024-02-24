using Story;
using UnityEngine;

public class Radio : HandTarget, IHandTarget
{
	public delegate void CancelIcon();

	[AssertNotNull]
	public ParticleSystem flare;

	[AssertNotNull]
	public Constructable constructable;

	[AssertNotNull]
	public FMODASRPlayer radioSound;

	[AssertNotNull]
	public FMOD_CustomEmitter playSound;

	[AssertNotNull]
	public LiveMixin liveMixin;

	[AssertNotNull]
	public VoiceNotification repairNotification;

	private bool hasMessage;

	[AssertLocalization]
	private const string damagedRadioHandText = "DamagedRadio";

	[AssertLocalization]
	private const string weldToFixHandText = "WeldToFix";

	[AssertLocalization]
	private const string radioHasMessageHandText = "Radio";

	[AssertLocalization]
	private const string radioPlayHandText = "Radio_Play";

	public static event CancelIcon CancelIconEvent;

	private void OnEnable()
	{
		StoryGoalManager.PendingMessageEvent -= NewRadioMessage;
		StoryGoalManager.PendingMessageEvent += NewRadioMessage;
		StoryGoalManager.main.PulsePendingMessages();
	}

	private void OnDisable()
	{
		StoryGoalManager.PendingMessageEvent -= NewRadioMessage;
	}

	private void NewRadioMessage(bool newMessages)
	{
		if (IsFullHealth())
		{
			ToggleBlink(newMessages);
		}
		hasMessage = newMessages;
	}

	public void OnRepair()
	{
		StoryGoalManager.main.PulsePendingMessages();
		Invoke("PlayRadioRepairVO", 2f);
	}

	private void PlayRadioRepairVO()
	{
		repairNotification.Play();
	}

	public void OnHandHover(GUIHand hand)
	{
		bool flag = IsFullHealth();
		if (!flag)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "DamagedRadio", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "WeldToFix", translate: true);
		}
		else if (hasMessage && flag)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "Radio", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "Radio_Play", translate: true);
			HandReticle.main.SetIcon(HandReticle.IconType.Interact);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (hasMessage && constructable.constructed && IsFullHealth() && !IsInvoking("PlayRadioMessage"))
		{
			playSound.Play();
			Invoke("PlayRadioMessage", 1.25f);
			if (Radio.CancelIconEvent != null)
			{
				Radio.CancelIconEvent();
			}
		}
	}

	private void PlayRadioMessage()
	{
		StoryGoalManager.main.ExecutePendingRadioMessage();
	}

	private void ToggleBlink(bool on)
	{
		if ((bool)flare)
		{
			flare.EnableEmission(on);
		}
		if (on)
		{
			radioSound.Play();
		}
		else
		{
			radioSound.Stop();
		}
	}

	private bool IsFullHealth()
	{
		return Mathf.Approximately(liveMixin.GetHealthFraction(), 1f);
	}
}
