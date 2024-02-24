using UWE;
using UnityEngine;

public class BaseUtils
{
	public static bool IsBaseGhost(GameObject obj)
	{
		Transform transform = obj.transform;
		Base @base = null;
		while ((bool)transform)
		{
			@base = transform.GetComponent<Base>();
			if ((bool)@base)
			{
				return @base.isGhost;
			}
			transform = transform.parent;
		}
		return false;
	}

	private static bool IsDestroyed(GameObject gameObject)
	{
		return gameObject.tag == "ToDestroy";
	}

	private static bool IsOtherBasePiece(GameObject other, GameObject ignoreObject)
	{
		if (other.GetComponent<Base>() != null || other.GetComponentInParent<Base>() != null)
		{
			return !UWE.Utils.SharingHierarchy(other, ignoreObject);
		}
		return false;
	}

	public static float GetFloorDistance(Vector3 startPoint, Vector3 direction, float maxLength, GameObject ignoreObject = null)
	{
		float num = -1f;
		float num2 = -1f;
		bool flag = false;
		bool flag2 = false;
		int num3 = UWE.Utils.RaycastIntoSharedBuffer(startPoint, direction, maxLength, -1, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < num3; i++)
		{
			RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[i];
			if (raycastHit.collider.gameObject.layer == LayerID.Player || IsBaseGhost(raycastHit.collider.gameObject) || IsDestroyed(raycastHit.collider.gameObject))
			{
				continue;
			}
			if (IsOtherBasePiece(raycastHit.collider.gameObject, ignoreObject))
			{
				if (num == -1f || num > raycastHit.distance)
				{
					num = raycastHit.distance;
				}
				flag = true;
			}
			else
			{
				if (num2 == -1f || num2 > raycastHit.distance)
				{
					num2 = raycastHit.distance;
				}
				flag2 = true;
			}
		}
		if (flag2 && flag && num2 - num > 0.3f)
		{
			return -1f;
		}
		return num2;
	}
}
