using UWE;
using UnityEngine;

public class DisableWaterRenderingVolume : MonoBehaviour
{
	public enum WaterStates
	{
		Disable = 0,
		Enable = 1
	}

	public WaterStates changeOnEnter = WaterStates.Enable;

	private void OnTriggerEnter(Collider col)
	{
		GameObject entityRoot = UWE.Utils.GetEntityRoot(col.gameObject);
		if (!entityRoot)
		{
			entityRoot = col.gameObject;
		}
		if (UWE.Utils.GetComponentInHierarchy<Player>(entityRoot) != null)
		{
			Player.main.SetDisplaySurfaceWater(changeOnEnter == WaterStates.Enable);
		}
	}
}
