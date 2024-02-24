using UnityEngine;
using UnityEngine.UI;

public static class UIUtils
{
	public static void ScrollToShowItemInCenter(Transform target)
	{
		ScrollRect componentInParent = target.GetComponentInParent<ScrollRect>();
		if (componentInParent != null)
		{
			RectTransform component = target.GetComponent<RectTransform>();
			RectTransform content = componentInParent.content;
			Vector2 vector = componentInParent.viewport.GetComponent<RectTransform>().rect.size * 0.5f;
			Vector2 size = content.rect.size;
			Vector3 vector2 = content.InverseTransformPoint(component.position);
			vector2 += new Vector3(component.rect.size.x, component.rect.size.y, 0f) * 0.25f;
			Vector2 normalizedPosition = new Vector2(Mathf.Clamp01(vector2.x / (size.x - vector.x)), 1f - Mathf.Clamp01(vector2.y / (0f - (size.y - vector.y))));
			Vector2 vector3 = new Vector2(vector.x / size.x, vector.y / size.y);
			normalizedPosition.x -= (1f - normalizedPosition.x) * vector3.x;
			normalizedPosition.y += normalizedPosition.y * vector3.y;
			normalizedPosition.x = Mathf.Clamp01(normalizedPosition.x);
			normalizedPosition.y = Mathf.Clamp01(normalizedPosition.y);
			componentInParent.normalizedPosition = normalizedPosition;
		}
	}

	public static void ScrollToShowItem(Transform item)
	{
		ScrollRect componentInParent = item.GetComponentInParent<ScrollRect>();
		if (componentInParent != null)
		{
			RectTransform content = componentInParent.content;
			RectTransform component = componentInParent.GetComponent<RectTransform>();
			RectTransform component2 = item.GetComponent<RectTransform>();
			Vector2 vector = content.InverseTransformPoint(component2.position);
			Vector2 vector2 = new Vector2(component2.rect.width, component2.rect.height);
			vector2 *= 1.3f;
			float num = Mathf.Max(0f, content.sizeDelta.y - component.sizeDelta.y);
			float num2 = 1f - Mathf.Clamp01((0f - vector.y + 0.5f * vector2.y - component.sizeDelta.y) / num);
			float num3 = 1f - Mathf.Clamp01((0f - vector.y - 0.5f * vector2.y) / num);
			if (componentInParent.verticalNormalizedPosition < num3)
			{
				componentInParent.verticalNormalizedPosition = num3;
			}
			else if (componentInParent.verticalNormalizedPosition > num2)
			{
				componentInParent.verticalNormalizedPosition = num2;
			}
		}
	}
}
