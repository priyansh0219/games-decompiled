using UnityEngine;

public class CyclopsBatteryHandTargetWrapper : MonoBehaviour
{
	[AssertNotNull]
	public LiveMixin subLiveMixin;

	[AssertNotNull]
	public BatterySource batterySource;

	public void OnHandHover()
	{
		if (subLiveMixin.IsAlive())
		{
			batterySource.HandHover();
		}
		else if (batterySource.HasItem())
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "CyclopsRemovePowerCell", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnHandClick()
	{
		if (subLiveMixin.IsAlive())
		{
			batterySource.InitiateReload();
		}
		else
		{
			batterySource.Select(null);
		}
	}
}
