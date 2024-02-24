using UnityEngine;

public class LockToPlayer : MonoBehaviour
{
	public bool lockX = true;

	public bool lockY = true;

	public bool lockZ = true;

	private void Update()
	{
		GameObject localPlayer = Utils.GetLocalPlayer();
		if ((bool)localPlayer && (lockX || lockY || lockZ))
		{
			Vector3 position = new Vector3(lockX ? localPlayer.transform.position.x : base.gameObject.transform.position.x, lockY ? localPlayer.transform.position.y : base.gameObject.transform.position.y, lockZ ? localPlayer.transform.position.z : base.gameObject.transform.position.z);
			base.transform.position = position;
		}
	}
}
