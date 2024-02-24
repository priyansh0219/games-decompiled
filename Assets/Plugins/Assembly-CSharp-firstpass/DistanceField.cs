using UnityEngine;

public class DistanceField : ScriptableObject
{
	public const float valueScale = 5f;

	[ReadOnly]
	public Texture3D texture;

	[ReadOnly]
	public Vector3 min;

	[ReadOnly]
	public Vector3 max;

	[ReadOnly]
	public Vector3 meshBoundsSize;
}
