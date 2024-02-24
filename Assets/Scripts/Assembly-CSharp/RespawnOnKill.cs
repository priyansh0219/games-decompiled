using UnityEngine;

public class RespawnOnKill : MonoBehaviour
{
	private void OnKill()
	{
		Vector3 vector = new Vector3((Random.value - 0.5f) * 50f, Random.value * 25f, (Random.value - 0.5f) * 50f);
		base.transform.position = base.transform.position + vector;
	}
}
