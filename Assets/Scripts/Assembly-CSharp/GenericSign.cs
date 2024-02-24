using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GenericSign : MonoBehaviour
{
	public string key = "LABEL";

	public Color color = Color.white;

	public float scale = 1f;

	public bool showBackground;

	public bool showLeftArrow;

	public bool showRightArrow;

	public bool showUpArrow;

	public bool showDownArrow;

	[AssertNotNull]
	public TextMeshProUGUI text;

	[AssertNotNull]
	public Image background;

	[AssertNotNull]
	public Image leftArrow;

	[AssertNotNull]
	public Image rightArrow;

	[AssertNotNull]
	public Image upArrow;

	[AssertNotNull]
	public Image downArrow;

	[AssertNotNull]
	public RectTransform rect;

	private void Start()
	{
		UpdateCanvas();
	}

	public void UpdateCanvas()
	{
		rect.localScale = new Vector3(scale, scale, 1f);
		Language main = Language.main;
		text.text = ((main != null) ? main.Get(key) : key);
		text.color = color;
		leftArrow.color = color;
		rightArrow.color = color;
		upArrow.color = color;
		downArrow.color = color;
		background.gameObject.SetActive(showBackground);
		leftArrow.gameObject.SetActive(showLeftArrow);
		rightArrow.gameObject.SetActive(showRightArrow);
		upArrow.gameObject.SetActive(showUpArrow);
		downArrow.gameObject.SetActive(showDownArrow);
	}
}
