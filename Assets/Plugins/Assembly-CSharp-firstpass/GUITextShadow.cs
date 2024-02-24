using UnityEngine;

public class GUITextShadow : MonoBehaviour
{
	public Vector2 offsetPixels = new Vector2(1f, -1f);

	public Color shadowColor = Color.black;

	public GameObject shadow;

	public GUIText shadowText;

	public void Start()
	{
		if (shadow == null)
		{
			shadow = new GameObject(base.gameObject.name + "-textshadow");
			shadow.transform.parent = base.transform;
			shadowText = shadow.AddComponent<GUIText>();
			shadowText.material = GetComponent<GUIText>().material;
		}
	}

	public void LateUpdate()
	{
		if (shadow != null)
		{
			if (!GetComponent<GUIText>().enabled)
			{
				shadowText.enabled = false;
				return;
			}
			shadowText.enabled = true;
			shadowText.alignment = GetComponent<GUIText>().alignment;
			shadowText.anchor = GetComponent<GUIText>().anchor;
			Color color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, GetComponent<GUIText>().color.a);
			shadowText.color = color;
			shadowText.font = GetComponent<GUIText>().font;
			shadowText.fontSize = GetComponent<GUIText>().fontSize;
			shadowText.fontStyle = GetComponent<GUIText>().fontStyle;
			shadowText.lineSpacing = GetComponent<GUIText>().lineSpacing;
			shadowText.pixelOffset = GetComponent<GUIText>().pixelOffset + offsetPixels;
			shadowText.richText = GetComponent<GUIText>().richText;
			shadowText.tabSize = GetComponent<GUIText>().tabSize;
			shadowText.text = GetComponent<GUIText>().text;
			shadowText.gameObject.layer = base.gameObject.layer;
			shadow.transform.localPosition = new Vector3(0f, 0f, -0.01f);
		}
	}
}
