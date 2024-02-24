using System;
using System.Collections.Generic;
using System.Text;
using Gendarme;
using UnityEngine;
using UnityEngine.UI;

[SuppressMessage("Gendarme.Rules.Performance", "AvoidUnneededFieldInitializationRule")]
public class FlexibleGridLayout : LayoutGroup
{
	public enum HorizontalAlignment
	{
		Left = 0,
		Center = 1,
		Right = 2
	}

	protected struct SizeData
	{
		public float min;

		public float preferred;

		[SuppressMessage("Gendarme.Rules.Maintainability", "VariableNamesShouldNotMatchFieldNamesRule")]
		public SizeData(float min, float preferred)
		{
			this.min = min;
			this.preferred = preferred;
		}
	}

	public HorizontalAlignment horizontalAlignment;

	public bool childForceExpandHorizontal;

	public bool clampMaxWidth = true;

	public bool useOnlyMinWidth = true;

	[NonSerialized]
	protected List<SizeData> _widths = new List<SizeData>();

	[NonSerialized]
	protected List<int> _rowColumns = new List<int>();

	[NonSerialized]
	protected List<float> _heights = new List<float>();

	public new RectTransform rectTransform => base.rectTransform;

	protected FlexibleGridLayout()
	{
	}

	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();
		CalcAlongAxis(0);
	}

	public override void SetLayoutHorizontal()
	{
		SetChildrenAlongAxis(0);
	}

	public override void CalculateLayoutInputVertical()
	{
		CalcAlongAxis(1);
	}

	public override void SetLayoutVertical()
	{
		SetChildrenAlongAxis(1);
	}

	private void CalcAlongAxis(int axis)
	{
		int count = base.rectChildren.Count;
		if (count == 0)
		{
			SetLayoutInputForAxis(0f, 0f, 0f, axis);
			return;
		}
		if (axis == 0)
		{
			_widths.Clear();
			if (_widths.Capacity < count)
			{
				_widths.Capacity = count;
			}
			float totalMin = 0f;
			float num = 0f;
			for (int i = 0; i < count; i++)
			{
				RectTransform child = base.rectChildren[i];
				GetChildSize(child, axis, controlSize: true, out var min, out var preferred);
				_widths.Add(new SizeData(min, preferred));
				num += preferred;
			}
			SetLayoutInputForAxis(totalMin, num, 0f, axis);
			return;
		}
		_heights.Clear();
		int count2 = _rowColumns.Count;
		if (_heights.Capacity < count2)
		{
			_heights.Capacity = count2;
		}
		float num2 = 0f;
		int num3 = 0;
		for (int j = 0; j < count2; j++)
		{
			int num4 = _rowColumns[j];
			float num5 = float.MinValue;
			for (int k = 0; k < num4; k++)
			{
				RectTransform child2 = base.rectChildren[num3];
				GetChildSize(child2, axis, controlSize: true, out var min2, out var preferred2);
				float num6 = Mathf.Max(min2, preferred2);
				if (num6 > num5)
				{
					num5 = num6;
				}
				num3++;
			}
			_heights.Add(num5);
			num2 += num5;
		}
		SetLayoutInputForAxis(num2, num2, 0f, axis);
	}

	[SuppressMessage("Gendarme.Rules.Smells", "AvoidLongMethodsRule")]
	private void SetChildrenAlongAxis(int axis)
	{
		int count = base.rectChildren.Count;
		if (count == 0)
		{
			return;
		}
		float num = rectTransform.rect.size[0];
		if (axis == 0)
		{
			_rowColumns.Clear();
			if (_rowColumns.Capacity < count)
			{
				_rowColumns.Capacity = count;
			}
			float num2 = 0f;
			float num3 = 0f;
			int num4 = 0;
			int i;
			for (i = 0; i < count; i++)
			{
				_ = base.rectChildren[i];
				SizeData sizeData = _widths[i];
				float min = sizeData.min;
				float preferred = sizeData.preferred;
				if (num2 + min > num)
				{
					if (num4 == i)
					{
						LayoutColumns(num4, i + 1, min, preferred, num);
						_rowColumns.Add(1);
						num4 = i + 1;
						num2 = 0f;
						num3 = 0f;
					}
					else
					{
						LayoutColumns(num4, i, num2, num3, num);
						int item = i - num4;
						_rowColumns.Add(item);
						num4 = i;
						num2 = min;
						num3 = preferred;
					}
				}
				else
				{
					num2 += min;
					num3 += preferred;
				}
			}
			int num5 = i - num4;
			if (num5 > 0)
			{
				LayoutColumns(num4, i, num2, num3, num);
				_rowColumns.Add(num5);
			}
			return;
		}
		float num6 = 0f;
		int num7 = 0;
		for (int j = 0; j < _rowColumns.Count; j++)
		{
			int num8 = _rowColumns[j];
			float num9 = _heights[j];
			for (int k = 0; k < num8; k++)
			{
				RectTransform rect = base.rectChildren[num7];
				SetChildAlongAxis(rect, 1, num6, num9);
				num7++;
			}
			num6 += num9;
		}
	}

	private void GetChildSize(RectTransform child, int axis, bool controlSize, out float min, out float preferred)
	{
		if (controlSize)
		{
			min = LayoutUtility.GetMinSize(child, axis);
			preferred = LayoutUtility.GetPreferredSize(child, axis);
		}
		else
		{
			min = child.sizeDelta[axis];
			preferred = min;
		}
	}

	private void LayoutColumns(int start, int end, float totalMinWidth, float totaPreferredWidth, float parentWidth)
	{
		if (useOnlyMinWidth)
		{
			LayoutColumnsMin(start, end, totalMinWidth, parentWidth);
		}
		else
		{
			LayoutColumnsPreferred(start, end, totalMinWidth, totaPreferredWidth, parentWidth);
		}
	}

	private void LayoutColumnsMin(int start, int end, float totalMinWidth, float parentWidth)
	{
		float num = totalMinWidth;
		if (clampMaxWidth && num > parentWidth)
		{
			num = parentWidth;
		}
		if (childForceExpandHorizontal && num < parentWidth)
		{
			num = parentWidth;
		}
		float num2 = GetInitialPosition(num, parentWidth);
		for (int i = start; i < end; i++)
		{
			RectTransform rect = base.rectChildren[i];
			float min = _widths[i].min;
			float num3 = num * (min / totalMinWidth);
			SetChildAlongAxis(rect, 0, num2, num3);
			num2 += num3;
		}
	}

	private void LayoutColumnsPreferred(int start, int end, float totalMinWidth, float totalPreferredWidth, float parentWidth)
	{
		float num = totalPreferredWidth;
		if (num > parentWidth)
		{
			num = parentWidth;
		}
		if (!clampMaxWidth && num < totalMinWidth)
		{
			num = totalMinWidth;
		}
		if (childForceExpandHorizontal && num < parentWidth)
		{
			num = parentWidth;
		}
		float num2 = GetInitialPosition(num, parentWidth);
		for (int i = start; i < end; i++)
		{
			RectTransform rect = base.rectChildren[i];
			SizeData sizeData = _widths[i];
			float min = sizeData.min;
			float preferred = sizeData.preferred;
			float num3 = 0f;
			if (num > totalPreferredWidth)
			{
				num3 = preferred + (num - totalPreferredWidth) * (preferred / totalPreferredWidth);
			}
			else if (num < totalMinWidth)
			{
				num3 = num * (min / totalMinWidth);
			}
			else
			{
				num3 = min;
				if (totalPreferredWidth > totalMinWidth)
				{
					num3 += (num - totalMinWidth) * ((preferred - min) / (totalPreferredWidth - totalMinWidth));
				}
			}
			SetChildAlongAxis(rect, 0, num2, num3);
			num2 += num3;
		}
	}

	private float GetInitialPosition(float width, float parentWidth)
	{
		float result = 0f;
		switch (horizontalAlignment)
		{
		case HorizontalAlignment.Left:
			result = 0f;
			break;
		case HorizontalAlignment.Center:
			result = 0.5f * (parentWidth - width);
			break;
		case HorizontalAlignment.Right:
			result = parentWidth - width;
			break;
		}
		return result;
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public string Log()
	{
		int count = base.rectChildren.Count;
		float num = rectTransform.rect.size[0];
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("l: {0} rows: {1}\n", count, _rowColumns.Count);
		int num2 = 0;
		for (int i = 0; i < _rowColumns.Count; i++)
		{
			int num3 = _rowColumns[i];
			stringBuilder.AppendFormat("row: {0}, columns: {1}", i, num3);
			stringBuilder.AppendFormat(", widths: [");
			float num4 = 0f;
			float num5 = 0f;
			for (int j = 0; j < num3; j++)
			{
				SizeData sizeData = _widths[num2];
				num4 += sizeData.min;
				num5 += sizeData.preferred;
				if (j > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.AppendFormat("min: {0}, preferred: {1}", sizeData.min, sizeData.preferred);
				num2++;
			}
			stringBuilder.AppendFormat("] = {0} < {1}\n", num4, num);
		}
		return stringBuilder.ToString();
	}
}
