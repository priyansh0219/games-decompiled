using UnityEngine;

public class CrabsnakeMushroom : MonoBehaviour
{
	[AssertNotNull]
	public Transform crabsnakeSpawn;

	public bool occupied;

	public Vector3 GetCrabsnakePosition()
	{
		return crabsnakeSpawn.position;
	}

	public Quaternion GetCrabsnakeRotation()
	{
		return crabsnakeSpawn.rotation;
	}

	public Vector3 GetUpDirection()
	{
		return crabsnakeSpawn.up;
	}
}
