using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class uGUI_Text : MonoBehaviour
{
	protected TextMeshProUGUI text;

	protected virtual void Awake()
	{
		text = GetComponent<TextMeshProUGUI>();
	}

	protected virtual void Start()
	{
		SetText(text.text, translate: true);
	}

	public void SetText(string s, bool translate = false)
	{
		if (translate)
		{
			s = Language.main.Get(s);
		}
		text.text = s;
	}

	public void SetAlignment(TextAlignmentOptions anchor)
	{
		text.alignment = anchor;
	}

	public void SetColor(Color color)
	{
		text.color = new Color(color.r, color.g, color.b, text.color.a);
	}
}
