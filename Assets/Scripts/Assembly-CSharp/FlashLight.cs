using UnityEngine;

[RequireComponent(typeof(EnergyMixin))]
public class FlashLight : PlayerTool
{
	public Light flashLight;

	public float useInterval = 5f;

	private float timeSinceUse;

	[AssertNotNull]
	public ToggleLights toggleLights;

	private Color lightColor;

	public override void OnDraw(Player p)
	{
		onLightsToggled(toggleLights.lightsActive);
		base.OnDraw(p);
	}

	public override void OnHolster()
	{
		onLightsToggled(active: false);
		base.OnHolster();
	}

	private void Start()
	{
		lightColor = toggleLights.FindLightColor();
		toggleLights.lightsCallback += onLightsToggled;
	}

	public void onLightsToggled(bool active)
	{
	}

	public override void OnToolUseAnim(GUIHand hand)
	{
		toggleLights.SetLightsActive(!toggleLights.GetLightsActive());
	}
}
