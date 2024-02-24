using UnityEngine;

public class BaseWaterTransition : MonoBehaviour
{
	public Base.Face face;

	private float waterLevel1;

	private float waterLevel2;

	private bool flowing;

	public void SetWaterLevels(float _waterLevel1, float _waterLevel2, bool _flowing)
	{
		waterLevel1 = _waterLevel1;
		waterLevel2 = _waterLevel2;
		flowing = _flowing;
		Debug.Log(string.Format("Water levels set to {0} and {1} ({2})", waterLevel1, waterLevel2, flowing ? "flowing" : "not flowing"));
	}
}
