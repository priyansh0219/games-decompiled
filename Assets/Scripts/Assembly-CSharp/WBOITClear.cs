using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WBOITClear : MonoBehaviour
{
	private void Start()
	{
		if (GraphicsUtil.IsOpenGL())
		{
			base.gameObject.SetActive(value: true);
			GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
