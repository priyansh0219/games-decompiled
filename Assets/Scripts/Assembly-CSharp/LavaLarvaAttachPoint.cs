using UnityEngine;

public class LavaLarvaAttachPoint : MonoBehaviour
{
	public bool occupied;

	public bool attached;

	public GameObject lavaLarva;

	public void Clear()
	{
		occupied = false;
		attached = false;
		lavaLarva = null;
	}
}
