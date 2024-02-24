using UnityEngine;

public class SpikePlant : PlantBehaviour
{
	private void OnGrown()
	{
		if (GetComponentInParent<WaterPark>() != null)
		{
			Object.Destroy(GetComponent<RangeAttacker>());
			Object.Destroy(GetComponent<RangeTargeter>());
		}
	}
}
