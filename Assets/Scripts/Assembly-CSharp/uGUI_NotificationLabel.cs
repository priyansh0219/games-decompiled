using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class uGUI_NotificationLabel : MonoBehaviour, ILayoutIgnorer
{
	private RectTransform _rectTransform;

	[AssertNotNull]
	public Image background;

	[AssertNotNull]
	public TextMeshProUGUI text;

	public RectTransform rectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	public bool ignoreLayout => true;

	public static uGUI_NotificationLabel CreateInstance(RectTransform parent)
	{
		uGUI_NotificationLabel result = null;
		uGUI main = uGUI.main;
		GameObject gameObject = ((main != null) ? main.prefabNotificationLabel : null);
		if (gameObject != null)
		{
			result = Object.Instantiate(gameObject).GetComponent<uGUI_NotificationLabel>();
			result.rectTransform.SetParent(parent, worldPositionStays: false);
			return result;
		}
		Debug.LogError("Failed to get reference to NotificationLabel prefab from uGUI.main.prefabNotificationLabel!");
		return result;
	}

	public void SetAnchor(UIAnchor anchor)
	{
		RectTransform obj = rectTransform;
		Vector2 anchorMin = (rectTransform.anchorMax = RectTransformExtensions.GetAnchorPoint(anchor));
		obj.anchorMin = anchorMin;
	}

	public void SetOffset(Vector2 offset)
	{
		rectTransform.anchoredPosition = offset;
	}

	public void SetText(string text)
	{
		this.text.text = text;
	}

	public void SetBackgroundColor(Color color)
	{
		background.color = color;
	}

	public void SetAlpha(float alpha)
	{
		alpha = Mathf.Clamp01(alpha);
		Color color = background.color;
		color.a = alpha;
		background.color = color;
		color = text.color;
		color.a = alpha;
		text.color = color;
	}
}
