using UnityEngine;

public class MovePoint : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(base.transform.position, 0.25f);
	}
}
