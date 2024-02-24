using UnityEngine;

public class GenericLoot : MonoBehaviour
{
	public static bool IsInSub(GameObject go)
	{
		if (go != null && go.GetComponentInParent<SubRoot>() != null)
		{
			return true;
		}
		return Player.main.IsInSub();
	}
}
