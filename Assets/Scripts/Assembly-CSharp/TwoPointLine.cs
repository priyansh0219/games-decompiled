using UnityEngine;

public class TwoPointLine : MonoBehaviour
{
	private Color c1 = new Color(0f, 1f, 1f, 1f);

	private Color c2 = new Color(1f, 0f, 0f, 1f);

	private float w1;

	private float w2 = 1f;

	private Transform t1;

	private Transform t2;

	private Vector3 p1 = Vector3.zero;

	private Vector3 p2 = Vector3.one;

	private LineRenderer lineRenderer;

	private void Awake()
	{
		lineRenderer = base.gameObject.AddComponent<LineRenderer>();
		lineRenderer.useWorldSpace = true;
		lineRenderer.SetVertexCount(2);
		lineRenderer.SetWidth(w1, w2);
		lineRenderer.SetColors(c1, c2);
	}

	public void Initialize(Material mat, Transform t1, Transform t2, float w1, float w2, float duration)
	{
		this.t1 = t1;
		this.t2 = t2;
		this.w1 = w1;
		this.w2 = w2;
		lineRenderer.materials = new Material[1]
		{
			new Material(mat)
		};
		Invoke("Destroy", duration);
	}

	private void Update()
	{
		if (t1 != null)
		{
			p1 = t1.position;
		}
		if (t2 != null)
		{
			p2 = t2.position;
		}
		lineRenderer.SetPosition(0, p1);
		lineRenderer.SetPosition(1, p2);
	}

	private void Destroy()
	{
		Object.Destroy(base.gameObject);
	}
}
