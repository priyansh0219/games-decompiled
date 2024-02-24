using UnityEngine;

public class LockPlayerForTime : MonoBehaviour
{
	public bool lockInEditor;

	public float secondsToLock = 5f;

	private Player player;

	private void Start()
	{
		player = GetComponent<Player>();
		LockForSeconds(5f);
	}

	private void LeaveLocked()
	{
		player.ExitLockedMode(respawn: false, findNewPosition: false);
	}

	private void LockForSeconds(float seconds)
	{
		player.EnterLockedMode(null);
		Invoke("LeaveLocked", seconds);
	}
}
