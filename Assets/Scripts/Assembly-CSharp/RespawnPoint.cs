using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
	public bool IsInGhostBase()
	{
		Base componentInParent = GetComponentInParent<Base>();
		if (componentInParent != null)
		{
			return componentInParent.isGhost;
		}
		return false;
	}

	public Vector3 GetSpawnPosition()
	{
		return base.transform.position;
	}
}
