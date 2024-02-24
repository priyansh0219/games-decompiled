using UnityEngine;

public class SignalPreview : MonoBehaviour
{
	public SignalInfo info;

	public string description;

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.DrawIcon(base.transform.position, "signal.png", allowScaling: false);
		Gizmos.matrix = Matrix4x4.identity;
	}
}
