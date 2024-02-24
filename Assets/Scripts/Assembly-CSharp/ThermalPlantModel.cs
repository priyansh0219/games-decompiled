using UnityEngine;

public class ThermalPlantModel : MonoBehaviour, IBuilderGhostModel
{
	void IBuilderGhostModel.UpdateGhostModelColor(bool allowed, ref Color color)
	{
		float num = 0f;
		WaterTemperatureSimulation main = WaterTemperatureSimulation.main;
		if ((bool)main)
		{
			num = main.GetTemperature(base.transform.position);
		}
		if (allowed && num < 25f)
		{
			color = Color.yellow;
		}
	}
}
