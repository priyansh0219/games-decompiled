using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class uGUI_BindingText : MonoBehaviour
{
	protected TextMeshProUGUI text;

	private string format;

	private bool translate;

	public GameInput.Button button;

	protected virtual void Awake()
	{
		text = GetComponent<TextMeshProUGUI>();
		format = text.text;
		translate = true;
	}

	protected virtual void Start()
	{
		RefreshText();
	}

	private void OnEnable()
	{
		GameInput.OnBindingsChanged += RefreshText;
	}

	private void OnDisable()
	{
		GameInput.OnBindingsChanged -= RefreshText;
	}

	public void RefreshText()
	{
		string text = ((!translate) ? string.Format(format, GameInput.FormatButton(button)) : LanguageCache.GetButtonFormat(format, button));
		this.text.text = text;
	}

	public void SetText(string format, bool translate = false)
	{
		this.format = format;
		this.translate = translate;
		RefreshText();
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
