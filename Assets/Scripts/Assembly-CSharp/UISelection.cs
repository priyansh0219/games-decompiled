using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class UISelection
{
	private static ISelectable _selected;

	public static readonly List<ISelectable> sSelectables = new List<ISelectable>();

	public static bool HasSelection
	{
		get
		{
			if (_selected != null && !_selected.Equals(null))
			{
				return _selected.IsValid();
			}
			return false;
		}
	}

	public static ISelectable selected
	{
		get
		{
			return _selected;
		}
		set
		{
			_selected = value;
		}
	}

	public static ISelectable FindSelectable(RectTransform canvas, Vector2 canvasDir, ISelectable current, List<ISelectable> candidates, bool fromEdge)
	{
		if (current == null)
		{
			return null;
		}
		RectTransform rect = current.GetRect();
		if (rect == null)
		{
			return null;
		}
		Vector3 vector = canvas.TransformDirection(canvasDir);
		Vector2 vector3;
		if (fromEdge)
		{
			Vector3 vector2 = Quaternion.Inverse(rect.rotation) * vector;
			vector3 = RectTransformExtensions.GetPointOnRectEdge(rect, vector2);
		}
		else
		{
			vector3 = rect.rect.center;
		}
		return FindSelectable(rect.TransformPoint(vector3), vector, candidates, current);
	}

	public static ISelectable FindSelectable(RectTransform canvas, Vector3 worldPos, List<ISelectable> candidates)
	{
		if (canvas == null || candidates == null)
		{
			return null;
		}
		ISelectable result = null;
		float num = float.PositiveInfinity;
		for (int i = 0; i < candidates.Count; i++)
		{
			ISelectable selectable = candidates[i];
			if (selectable == null || !selectable.IsValid())
			{
				continue;
			}
			RectTransform rect = selectable.GetRect();
			if (!(rect == null))
			{
				float sqrMagnitude = (rect.ClosestPoint(worldPos) - worldPos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = selectable;
				}
			}
		}
		return result;
	}

	public static ISelectable FindSelectable(RectTransform canvas, Vector3 canvasPos, Vector2 canvasDir, List<ISelectable> candidates, ISelectable ignoreSelectable)
	{
		if (canvas == null)
		{
			return null;
		}
		Vector3 worldPos = canvas.TransformPoint(canvasPos);
		Vector3 worldDir = canvas.TransformDirection(canvasDir);
		return FindSelectable(worldPos, worldDir, candidates, ignoreSelectable);
	}

	public static ISelectable FindSelectable(Vector3 worldPos, Vector3 worldDir, List<ISelectable> candidates, ISelectable ignoreSelectable = null)
	{
		if (worldDir == Vector3.zero || candidates == null)
		{
			return null;
		}
		worldDir.Normalize();
		ISelectable result = null;
		float num = float.NegativeInfinity;
		for (int i = 0; i < candidates.Count; i++)
		{
			ISelectable selectable = candidates[i];
			if (selectable == null || selectable == ignoreSelectable || !selectable.IsValid())
			{
				continue;
			}
			RectTransform rect = selectable.GetRect();
			if (rect == null)
			{
				continue;
			}
			Vector3 vector = rect.ClosestPoint(worldPos) - worldPos;
			float magnitude = vector.magnitude;
			Vector3 rhs = vector / magnitude;
			float num2 = Vector3.Dot(worldDir, rhs);
			if (num2 < 0.08715574f)
			{
				Dbg.HighlightRect(rect, Color.red, 2f);
				continue;
			}
			Dbg.HighlightRect(rect, Color.green, 2f);
			float num3 = num2 / magnitude;
			if (num3 > num)
			{
				num = num3;
				result = selectable;
			}
		}
		return result;
	}

	public static void Scroll(this ScrollRect scrollRect, float scrollDelta, float speedMultiplier)
	{
		RectTransform viewport = scrollRect.viewport;
		RectTransform content = scrollRect.content;
		if (viewport == null || content == null)
		{
			return;
		}
		float height = content.rect.height;
		float height2 = viewport.rect.height;
		float num = height - height2;
		if (num > 0f)
		{
			float num2 = scrollDelta * 600f * speedMultiplier * Time.unscaledDeltaTime / num;
			float num3 = scrollRect.verticalNormalizedPosition + num2;
			if (num3 < 0f)
			{
				num3 = Mathf.Max(num3, 0f - Mathf.Abs(scrollDelta) * 100f / num);
			}
			else if (num3 > 1f)
			{
				num3 = Mathf.Min(num3, 1f + Mathf.Abs(scrollDelta) * 100f / num);
			}
			scrollRect.verticalNormalizedPosition = num3;
		}
	}

	public static void ScrollTo(this ScrollRect scrollRect, RectTransform selectedRect, bool xRight, bool yUp, Vector4 padding)
	{
		if (selectedRect == null)
		{
			return;
		}
		RectTransform viewport = scrollRect.viewport;
		if (viewport == null)
		{
			return;
		}
		RectTransform content = scrollRect.content;
		if (content == null)
		{
			return;
		}
		Vector2 size = viewport.rect.size;
		Vector2 size2 = content.rect.size;
		Vector2 vector = new Vector2((size.x < size2.x) ? (size.x / size2.x) : 1f, (size.y < size2.y) ? (size.y / size2.y) : 1f);
		Rect rect = selectedRect.rect;
		float x = rect.x - padding.x;
		float y = rect.y - padding.y;
		float x2 = rect.xMax + padding.z;
		float y2 = rect.yMax + padding.w;
		Vector3[] array = new Vector3[4]
		{
			new Vector3(x, y, 0f),
			new Vector3(x, y2, 0f),
			new Vector3(x2, y2, 0f),
			new Vector3(x2, y, 0f)
		};
		float num = float.MaxValue;
		float num2 = float.MinValue;
		float num3 = float.MaxValue;
		float num4 = float.MinValue;
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector2 = content.InverseTransformPoint(selectedRect.TransformPoint(array[i]));
			float num5 = vector2.x / size2.x;
			float num6 = vector2.y / size2.y;
			if (!xRight)
			{
				num5 = 0f - num5;
			}
			if (!yUp)
			{
				num6 = 0f - num6;
			}
			if (num5 < num)
			{
				num = num5;
			}
			if (num5 > num2)
			{
				num2 = num5;
			}
			if (num6 < num3)
			{
				num3 = num6;
			}
			if (num6 > num4)
			{
				num4 = num6;
			}
		}
		Vector2 normalizedPosition = scrollRect.normalizedPosition;
		normalizedPosition.x = Adjust(scrollRect.horizontalNormalizedPosition, vector.x, num, num2, xRight);
		normalizedPosition.y = Adjust(scrollRect.verticalNormalizedPosition, vector.y, num3, num4, yUp);
		scrollRect.normalizedPosition = normalizedPosition;
	}

	private static float Adjust(float position, float size, float min, float max, bool positiveAxis)
	{
		float result = position;
		if (size != 1f)
		{
			if (positiveAxis)
			{
				float num = (1f - size) * position;
				float num2 = size + (1f - size) * position;
				if (num > min)
				{
					result = min / (1f - size);
				}
				else if (num2 < max)
				{
					result = (max - size) / (1f - size);
				}
			}
			else
			{
				float num3 = 1f - size + (size - 1f) * position;
				float num4 = 1f + (size - 1f) * position;
				if (num3 > min)
				{
					result = (min - 1f + size) / (size - 1f);
				}
				else if (num4 < max)
				{
					result = (max - 1f) / (size - 1f);
				}
			}
		}
		return result;
	}
}
