using UnityEngine;

public class MainMenuCornerCurve : MonoBehaviour
{
	public enum Corner
	{
		upperleft = 0,
		upperright = 1,
		lowerleft = 2,
		lowerright = 3
	}

	public GameObject cornerCurve;

	public Camera cam;

	public Corner corner;

	public float animationSpeed = 0.5f;

	private bool isExpanding;

	private bool isRetracting;

	private Vector3 beginPos;

	private Vector3 targetPos;

	private void Start()
	{
		Vector3 position = base.gameObject.transform.position;
		Vector3 position2 = new Vector3(0f, 0f, 0f);
		base.gameObject.transform.position = position2;
		MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
		CombineInstance[] array = new CombineInstance[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			array[i].mesh = componentsInChildren[i].sharedMesh;
			array[i].transform = componentsInChildren[i].transform.localToWorldMatrix;
			componentsInChildren[i].gameObject.active = false;
		}
		base.transform.GetComponent<MeshFilter>().mesh = new Mesh();
		base.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(array, mergeSubMeshes: true, useMatrices: true);
		base.transform.gameObject.SetActive(value: true);
		base.transform.GetComponent<MeshCollider>().sharedMesh = base.transform.GetComponent<MeshFilter>().mesh;
		base.gameObject.transform.position = position;
		switch (corner)
		{
		case Corner.upperleft:
			setTarget(0f, 1f);
			break;
		case Corner.upperright:
			setTarget(1f, 1f);
			break;
		case Corner.lowerleft:
			setTarget(0f, 0f);
			break;
		case Corner.lowerright:
			setTarget(1f, 0f);
			break;
		}
		beginPos = cornerCurve.transform.position;
	}

	private void setTarget(float x, float y)
	{
		targetPos = Vector3.Lerp(new Vector3(cornerCurve.transform.position.x, cornerCurve.transform.position.y, cam.nearClipPlane), cam.ViewportToWorldPoint(new Vector3(x, y, cam.nearClipPlane)), 0.5f);
	}

	private void Update()
	{
		if (isExpanding)
		{
			animate(targetPos);
		}
		if (isRetracting)
		{
			animate(beginPos);
		}
	}

	private void animate(Vector3 target)
	{
		cornerCurve.transform.position = Vector3.Lerp(cornerCurve.transform.position, target, animationSpeed * Time.deltaTime);
	}

	private void OnMouseEnter()
	{
		isExpanding = true;
		isRetracting = false;
	}

	private void OnMouseExit()
	{
		isRetracting = true;
		isExpanding = false;
	}
}
