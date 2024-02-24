using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(LensWater))]
public class LensWaterController : MonoBehaviour
{
	private bool inWaterEnvironment;

	private bool belowWaterSurface;

	public float waterLevel;

	private LensWater fx;

	private void Start()
	{
		inWaterEnvironment = false;
		belowWaterSurface = false;
		fx = GetComponent<LensWater>();
		if (Utils.GetLocalPlayer() == null)
		{
			Object.Destroy(this);
			Object.Destroy(GetComponent<LensWater>());
			return;
		}
		Player component = Utils.GetLocalPlayer().GetComponent<Player>();
		component.isUnderwater.changedEvent.AddHandler(this, OnPlayerEnvironmentChanged);
		inWaterEnvironment = component.isUnderwater.value;
		if (inWaterEnvironment)
		{
			belowWaterSurface = IsBelowWaterSurface();
		}
	}

	private bool IsBelowWaterSurface()
	{
		return base.transform.position.y < waterLevel;
	}

	public void OnPlayerEnvironmentChanged(Utils.MonitoredValue<bool> isUnderwater)
	{
		inWaterEnvironment = isUnderwater.value;
		if (!isUnderwater.value)
		{
			fx.CreateSplash();
		}
		else
		{
			belowWaterSurface = IsBelowWaterSurface();
		}
	}

	private void Update()
	{
		if (inWaterEnvironment)
		{
			bool flag = IsBelowWaterSurface();
			if (belowWaterSurface && !flag)
			{
				fx.CreateSplash();
			}
			belowWaterSurface = flag;
		}
	}
}
