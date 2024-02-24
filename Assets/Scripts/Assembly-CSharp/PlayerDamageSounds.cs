using FMODUnity;
using UnityEngine;

public class PlayerDamageSounds : MonoBehaviour, IOnTakeDamage
{
	[AssertNotNull]
	public FMOD_StudioEventEmitter painWithTank;

	[AssertNotNull]
	public FMOD_StudioEventEmitter painWithoutTank;

	[AssertNotNull]
	public StudioEventEmitter painWithoutTankLight;

	[AssertNotNull]
	public FMOD_StudioEventEmitter painWithoutMask;

	[AssertNotNull]
	public FMOD_CustomEmitter painSmoke;

	public void OnTakeDamage(DamageInfo damageInfo)
	{
		if (!(damageInfo.damage > 0f) || damageInfo.type == DamageType.Radiation)
		{
			return;
		}
		Player main = Player.main;
		if (damageInfo.type == DamageType.Smoke)
		{
			painSmoke.Play();
		}
		else if ((bool)main && !main.isUnderwater.value)
		{
			if (!painWithoutMask.GetIsPlaying())
			{
				painWithoutMask.StartEvent();
			}
		}
		else if (Inventory.Get().GetPickupCount(TechType.Tank) != 0)
		{
			if (!painWithTank.GetIsPlaying())
			{
				painWithTank.StartEvent();
			}
		}
		else if (damageInfo.damage > 8f)
		{
			if (!painWithoutTank.GetIsPlaying())
			{
				painWithoutTank.StartEvent();
			}
		}
		else if (!painWithoutTankLight.IsPlaying())
		{
			painWithoutTankLight.Play();
		}
	}
}
