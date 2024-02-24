using UnityEngine;

[ExecuteInEditMode]
public class VFXWaterFall : MonoBehaviour
{
	public float topHeight = 1f;

	public float botHeight = 1f;

	public Transform topTransform;

	public Transform fallTransform;

	public Transform botTransform;

	private Vector3 topScale = Vector3.one;

	private Vector3 botScale = Vector3.one;

	private void Awake()
	{
		ApplyScaling();
	}

	public void ApplyScaling()
	{
		topScale.y = topHeight / base.transform.localScale.y;
		botScale.y = botHeight / base.transform.localScale.y;
		topTransform.localScale = topScale;
		botTransform.localScale = botScale;
	}
}
