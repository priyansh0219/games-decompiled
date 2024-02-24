using UWE;
using UnityEngine;

public class DebugDrawAABB : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		Bounds encapsulatedAABB = UWE.Utils.GetEncapsulatedAABB(base.gameObject);
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(encapsulatedAABB.center, 0.1f);
		Gizmos.DrawWireCube(encapsulatedAABB.center, encapsulatedAABB.size);
	}
}
