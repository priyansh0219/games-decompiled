using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerPusher : MonoBehaviour
{
	public List<Vector3> forceList = new List<Vector3>();

	public void ApplyPlayerForce(Vector3 forceVector)
	{
		forceList.Add(forceVector);
	}

	private void LateUpdate()
	{
		base.gameObject.GetComponent<Player>();
		CharacterController component = GetComponent<CharacterController>();
		for (int i = 0; i < forceList.Count; i++)
		{
			Vector3 vector = forceList[i] * Time.deltaTime;
			Debug.Log("PlayerPush.LateUpdate(): Moving controller by delta: " + vector);
			component.Move(vector);
		}
		forceList.Clear();
	}
}
