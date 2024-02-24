using UnityEngine;

public class WaterTemperatureSimulation : MonoBehaviour
{
	[HideInInspector]
	public static WaterTemperatureSimulation main;

	[SerializeField]
	private AnimationCurve depthModifier;

	[SerializeField]
	private AnimationCurve dayNightModifier;

	[SerializeField]
	private AnimationCurve dayNightDepthMultiplier;

	[SerializeField]
	private float perlinFrequency = 2f;

	[SerializeField]
	private float perlinAmplitude = 2f;

	private void Start()
	{
		main = this;
	}

	public float GetTemperature(Vector3 wsPos)
	{
		float num = 20f;
		WaterBiomeManager waterBiomeManager = WaterBiomeManager.main;
		if ((bool)waterBiomeManager && waterBiomeManager.GetSettings(wsPos, onlyAffectsVisuals: false, out var settings))
		{
			num = settings.temperature;
		}
		EcoRegionManager ecoRegionManager = EcoRegionManager.main;
		if (ecoRegionManager != null)
		{
			IEcoTarget ecoTarget = ecoRegionManager.FindNearestTarget(EcoTargetType.HeatArea, wsPos, null, 3);
			if (ecoTarget != null)
			{
				float magnitude = (ecoTarget.GetPosition() - wsPos).magnitude;
				float num2 = Mathf.Clamp(60f - magnitude, 0f, 60f);
				num += num2;
				Debug.DrawLine(wsPos, ecoTarget.GetPosition(), Color.red, 5f);
			}
		}
		DayNightCycle dayNightCycle = DayNightCycle.main;
		float dayScalar = ((dayNightCycle != null) ? dayNightCycle.GetDayScalar() : 0.5f);
		return GetTemperature(num, wsPos, dayScalar);
	}

	public float GetTemperature(float baseTemperature, Vector3 wsPos, float dayScalar)
	{
		return baseTemperature + depthModifier.Evaluate(0f - wsPos.y) + dayNightModifier.Evaluate(dayScalar) * dayNightDepthMultiplier.Evaluate(0f - wsPos.y) + (Mathf.PerlinNoise(wsPos.x * perlinFrequency, wsPos.z * perlinFrequency) * perlinAmplitude - 0.5f * perlinAmplitude);
	}
}
