using UnityEngine;

public class VoxelandHelper : MonoBehaviour
{
	public float diagScale = 0.5f;

	public float diagBias = 0.51f;

	public float amp = 5f;

	public float bias = 5f;

	public float period = 8f;

	public float simpMaxError = 0.2f;

	public float antiSliverWeight = 0.01f;

	public bool simplifyLowMesh;

	public bool skipRandomPhase;

	public Vector3 dir = Vector3.right;

	public GameObject debugPoint;

	public bool debugGizmos = true;

	public Int3 debugBlock;

	public bool debugMeshingFaceCenters;

	private Voxeland land => GetComponent<Voxeland>();
}
