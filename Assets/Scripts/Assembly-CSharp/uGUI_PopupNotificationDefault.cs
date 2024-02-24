using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_PopupNotificationDefault : uGUI_PopupNotificationSkin
{
	[AssertNotNull]
	public CanvasGroup canvasGroup;

	public float ox = 20f;

	public float oy = 20f;

	[AssertNotNull]
	public TextMeshProUGUI title;

	[AssertNotNull]
	public TextMeshProUGUI text;

	[AssertNotNull]
	public TextMeshProUGUI controls;

	public Image image;

	[AssertNotNull]
	public Sprite defaultSprite;

	public override void SetVisible(bool visible)
	{
		canvasGroup.alpha = (visible ? 1f : 0f);
	}

	public override void OnShow(uGUI_PopupNotification.Entry current)
	{
		title.text = current.title;
		if (image != null)
		{
			image.sprite = ((current.sprite != null) ? current.sprite : defaultSprite);
		}
		text.text = current.text;
		controls.text = current.controls;
	}

	public override void SetTransition(float value)
	{
		PlatformUtils.main.GetServices();
		TextAnchor anchor = TextAnchor.MiddleLeft;
		RectTransform component = GetComponent<RectTransform>();
		Rect rect = component.parent.GetComponent<RectTransform>().rect;
		Rect rect2 = component.rect;
		RectTransformExtensions.GetRectPositions(rect, rect2, anchor, ox, oy, out var p, out var p2);
		component.anchoredPosition = Vector2.Lerp(t: MathExtensions.EaseOutSine(value), a: p, b: p2);
	}
}
