using UWE;
using UnityEngine;

public class HardnessMixin : MonoBehaviour
{
	[Range(0f, 1f)]
	public float hardness;

	public static float GetHardness(GameObject go)
	{
		HardnessMixin componentInHierarchy = UWE.Utils.GetComponentInHierarchy<HardnessMixin>(go);
		if (componentInHierarchy != null)
		{
			return componentInHierarchy.hardness;
		}
		return 0f;
	}
}
