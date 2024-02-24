using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class TridentHole : MonoBehaviour
{
	public TridentHome tridentHome;

	private void Start()
	{
	}

	private void OnTriggerStay(Collider other)
	{
		if ((bool)other.gameObject.GetComponent<Player>())
		{
			tridentHome.AwakeNearbyTrident(this);
		}
	}
}
