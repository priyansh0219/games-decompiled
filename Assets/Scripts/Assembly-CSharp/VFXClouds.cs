using UnityEngine;

public class VFXClouds : MonoBehaviour
{
	private float seaLevel = -0.4f;

	private void LateUpdate()
	{
		Vector3 localPlayerPos = Utils.GetLocalPlayerPos();
		localPlayerPos.y = seaLevel;
		base.transform.position = localPlayerPos;
	}
}
