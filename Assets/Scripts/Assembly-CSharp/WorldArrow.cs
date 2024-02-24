using TMPro;
using UnityEngine;

public class WorldArrow : MonoBehaviour
{
	public GameObject upArrow;

	public GameObject downArrow;

	public TextMeshProUGUI text;

	public GameObject children;

	private Transform parentTransform;

	private Vector3 offset;

	private GameObject textCanvasGO;

	private void Awake()
	{
		textCanvasGO = text.canvas.gameObject;
	}

	private void OnEnable()
	{
		PointDown();
	}

	private void Update()
	{
		text.SetScaleDirty();
	}

	public void PointNone()
	{
		upArrow.SetActive(value: false);
		downArrow.SetActive(value: false);
		children.transform.localPosition = Vector3.zero;
	}

	public void PointUp()
	{
		upArrow.SetActive(value: true);
		downArrow.SetActive(value: false);
		children.transform.localPosition = new Vector3(0f, -0.6f, 0f);
	}

	public void PointDown()
	{
		upArrow.SetActive(value: false);
		downArrow.SetActive(value: true);
		children.transform.localPosition = new Vector3(0f, 0.3f, 0f);
	}

	public void SetText(string message)
	{
		text.text = message;
	}

	public void SetPosition(Transform t, Vector3 localOffset)
	{
		parentTransform = t;
		offset = localOffset;
	}
}
