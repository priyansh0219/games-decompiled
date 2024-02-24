using UnityEngine;

public class BasePowerRelay : PowerRelay
{
	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public Base baseComp;

	[AssertNotNull]
	public VoiceNotification powerUpInside;

	[AssertNotNull]
	public VoiceNotification powerUpOutside;

	[AssertNotNull]
	public VoiceNotification powerDownInside;

	[AssertNotNull]
	public VoiceNotification powerDownOutside;

	public override void Start()
	{
		base.Start();
		powerUpEvent.AddHandler(base.gameObject, PowerUpEvent);
		powerDownEvent.AddHandler(base.gameObject, PowerDownEvent);
	}

	public override Vector3 GetConnectPoint(Vector3 fromPosition)
	{
		return baseComp.GetClosestPoint(fromPosition);
	}

	private void PlayPowerDownIfUnpowered()
	{
		if (!IsPowered())
		{
			if (Player.main.GetCurrentSub() == subRoot)
			{
				powerDownInside.Play();
			}
			else
			{
				powerDownOutside.Play();
			}
		}
	}

	private void PowerUpEvent(PowerRelay relay)
	{
		Invoke("PlayPowerUpIfPowered", 0.5f);
	}

	private void PowerDownEvent(PowerRelay relay)
	{
		Invoke("PlayPowerDownIfUnpowered", 0.5f);
	}

	private void PlayPowerUpIfPowered()
	{
		if (IsPowered())
		{
			if (Player.main.GetCurrentSub() == subRoot)
			{
				powerUpInside.Play();
			}
			else
			{
				powerUpOutside.Play();
			}
		}
	}
}
