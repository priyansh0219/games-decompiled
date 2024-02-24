using UnityEngine;

public class BleederAttachTarget : MonoBehaviour
{
	public Player player;

	public bool occupied;

	public bool attached;

	public Bleeder bleeder;

	private void OnKill()
	{
		if (bleeder != null)
		{
			bleeder.SendMessage("OnTargetKilled");
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
		if (occupied)
		{
			Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
		}
		if (attached)
		{
			Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
		}
		Gizmos.DrawSphere(base.transform.position, 0.1f);
	}
}
