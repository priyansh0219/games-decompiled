using UnityEngine;

public class SpawnOnKill : MonoBehaviour
{
	[AssertNotNull]
	public GameObject prefabToSpawn;

	public bool randomPush;

	public void OnKill()
	{
		GameObject gameObject = Object.Instantiate(prefabToSpawn, base.transform.position, base.transform.rotation);
		if (randomPush)
		{
			Rigidbody component = gameObject.GetComponent<Rigidbody>();
			if ((bool)component)
			{
				component.AddForce(Random.onUnitSphere * 1.4f, ForceMode.Impulse);
			}
		}
	}
}
