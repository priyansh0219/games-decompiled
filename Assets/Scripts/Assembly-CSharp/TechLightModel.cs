using UnityEngine;

public class TechLightModel : MonoBehaviour, IBuilderGhostModel
{
	void IBuilderGhostModel.UpdateGhostModelColor(bool allowed, ref Color color)
	{
		if (allowed && TechLight.GetNearestValidRelay(base.gameObject) == null)
		{
			color = Color.yellow;
		}
	}
}
