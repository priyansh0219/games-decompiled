using System;
using System.Collections.Generic;
using Gendarme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidLackOfCohesionOfMethodsRule")]
public class uGUI_Tooltip : MonoBehaviour
{
	private static readonly Vector2 margin = new Vector2(10f, 20f);

	private const float paddingX = 20f;

	private const float paddingY = 20f;

	private const float spacing = 10f;

	private const float iconWidth = 110f;

	private const int iconsInRow = 3;

	private const float gap = 0.9f;

	private const float maxWidth = 469f;

	private const int poolChunkSize = 3;

	private const ManagedUpdate.Queue queueUpdate = ManagedUpdate.Queue.PreCanvasTooltip;

	private const ManagedUpdate.Queue queueLayout = ManagedUpdate.Queue.UILayout;

	private const ManagedUpdate.Queue queueLayoutComplete = ManagedUpdate.Queue.UILayoutComplete;

	private static uGUI_Tooltip main;

	private static string prefix;

	private static string postfix;

	private static TooltipData tooltipData = new TooltipData();

	private static bool visible = true;

	public float scaleFactor = 0.00116f;

	[AssertNotNull]
	public Canvas canvas;

	[AssertNotNull]
	public uGUI_TooltipIcon prefabIconEntry;

	[AssertNotNull]
	public RectTransform rectTransform;

	[AssertNotNull]
	public CanvasGroup canvasGroup;

	[AssertNotNull]
	public Image background;

	[AssertNotNull]
	public TextMeshProUGUI textPrefix;

	[AssertNotNull]
	public TextMeshProUGUI textPostfix;

	[AssertNotNull]
	public FlexibleGridLayout iconCanvas;

	private RectTransform backgroundRect;

	private RectTransform textRectPrefix;

	private RectTransform textRectPostfix;

	private Matrix4x4 worldToLocalMatrix;

	private Matrix4x4 localToWorldMatrix;

	private Vector3 position;

	private Quaternion rotation;

	private Vector3 scale;

	private Rect rect;

	private Vector3 aimingPosition;

	private Vector3 aimingForward;

	private int layer;

	private int defaultSortingLayerID;

	private int defaultSortingOrder;

	private int sortingLayerID;

	private int sortingOrder;

	private float alpha;

	private int cachedTooltipIconsHash = -1;

	private List<uGUI_TooltipIcon> icons = new List<uGUI_TooltipIcon>();

	private void Awake()
	{
		if (main != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		main = this;
		defaultSortingLayerID = canvas.sortingLayerID;
		defaultSortingOrder = canvas.sortingOrder;
		sortingLayerID = defaultSortingLayerID;
		sortingOrder = defaultSortingOrder;
		backgroundRect = background.rectTransform;
		textRectPrefix = textPrefix.rectTransform;
		textRectPostfix = textPostfix.rectTransform;
		layer = LayerID.UI;
		canvasGroup.alpha = 0f;
	}

	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.PreCanvasTooltip, OnUpdate);
	}

	private void OnDisable()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.PreCanvasTooltip, OnUpdate);
	}

	private void OnUpdate()
	{
		if ((string.IsNullOrEmpty(prefix) && tooltipData.icons.Count == 0 && string.IsNullOrEmpty(postfix)) || !ExtractParams())
		{
			Clear();
		}
		if (visible)
		{
			canvasGroup.alpha = alpha;
			base.gameObject.layer = layer;
			canvas.sortingLayerID = sortingLayerID;
			canvas.sortingOrder = sortingOrder;
			bool flag = false;
			if (textPrefix.text != prefix)
			{
				textPrefix.text = prefix;
				flag = true;
			}
			if (textPostfix.text != postfix)
			{
				textPostfix.text = postfix;
				flag = true;
			}
			int num = 5351;
			List<TooltipIcon> list = tooltipData.icons;
			for (int i = 0; i < list.Count; i++)
			{
				num = 31 * num + list[i].GetHashCode();
			}
			if (num != cachedTooltipIconsHash)
			{
				cachedTooltipIconsHash = num;
				flag = true;
			}
			if (flag)
			{
				int count = list.Count;
				EnsureIconsCount(count);
				for (int j = 0; j < icons.Count; j++)
				{
					uGUI_TooltipIcon uGUI_TooltipIcon2 = icons[j];
					if (j < count)
					{
						TooltipIcon tooltipIcon = list[j];
						uGUI_TooltipIcon2.gameObject.SetActive(value: true);
						uGUI_TooltipIcon2.SetIcon(tooltipIcon.sprite);
						uGUI_TooltipIcon2.SetText(tooltipIcon.text);
					}
					else
					{
						uGUI_TooltipIcon2.gameObject.SetActive(value: false);
					}
				}
				ManagedUpdate.Subscribe(ManagedUpdate.Queue.UILayout, OnLayout);
				ManagedUpdate.Subscribe(ManagedUpdate.Queue.UILayoutComplete, OnLayoutComplete);
			}
			else
			{
				UpdatePosition();
			}
		}
		else
		{
			canvasGroup.alpha = 0f;
		}
	}

	private bool ExtractParams()
	{
		RectTransform rt = null;
		if (CursorManager.GetPointerInfo(ref rt, out var _, out position, out var aimingTransform, out alpha))
		{
			worldToLocalMatrix = rt.worldToLocalMatrix;
			localToWorldMatrix = rt.localToWorldMatrix;
			rotation = rt.rotation;
			scale = rt.lossyScale;
			rect = rt.rect;
			aimingPosition = aimingTransform.position;
			aimingForward = aimingTransform.forward;
			layer = rt.gameObject.layer;
			Canvas canvas = RectTransformExtensions.GetCanvas(rt.gameObject);
			if (canvas != null)
			{
				sortingLayerID = canvas.sortingLayerID;
				sortingOrder = canvas.sortingOrder + 2;
			}
			else
			{
				sortingLayerID = defaultSortingLayerID;
				sortingOrder = defaultSortingOrder;
			}
			return true;
		}
		return false;
	}

	private void UpdatePosition()
	{
		float num = Vector3.Dot(aimingForward, position - aimingPosition) * scaleFactor * uGUI_CanvasScaler.uiScale;
		rectTransform.localScale = new Vector3(num, num, num);
		rectTransform.rotation = rotation;
		textPrefix.SetScaleDirty();
		textPostfix.SetScaleDirty();
		for (int i = 0; i < icons.Count; i++)
		{
			uGUI_TooltipIcon uGUI_TooltipIcon2 = icons[i];
			if (uGUI_TooltipIcon2.gameObject.activeSelf)
			{
				uGUI_TooltipIcon2.title.SetScaleDirty();
			}
		}
		Vector2 b = new Vector2(num / scale.x, num / scale.y);
		Vector2 vector = Vector2.Scale(backgroundRect.rect.size, b);
		Vector2 vector2 = Vector2.Scale(margin, b);
		Vector3 vector3 = worldToLocalMatrix.MultiplyPoint3x4(position);
		Vector3 point = new Vector3(0f, 0f, vector3.z);
		Vector2 vector4 = rect.position;
		Vector2 vector5 = rect.position + rect.size;
		float num2 = Mathf.Max(0f, vector3.x + vector2.x + vector.x - vector5.x);
		float num3 = 0f - Mathf.Min(0f, vector3.x - vector2.x - vector.x - vector4.x);
		float num4 = Mathf.Max(0f, vector3.y + vector2.y + vector.y - vector5.y);
		float num5 = 0f - Mathf.Min(0f, vector3.y - vector2.y - vector.y - vector4.y);
		if (num2 > 0f)
		{
			if (num3 > 0f)
			{
				point.x = ((num2 > num3) ? vector4.x : (vector5.x - vector.x));
			}
			else
			{
				point.x = vector3.x - vector2.x - vector.x;
			}
		}
		else
		{
			point.x = vector3.x + vector2.x;
		}
		if (num5 > 0f)
		{
			if (num4 > 0f)
			{
				point.y = ((num5 > num4) ? vector5.y : (vector4.y + vector.y));
			}
			else
			{
				point.y = vector3.y + vector2.y + vector.y;
			}
		}
		else
		{
			point.y = vector3.y - vector2.y;
		}
		rectTransform.position = localToWorldMatrix.MultiplyPoint3x4(point);
	}

	private void EnsureIconsCount(int count)
	{
		if (icons.Count < count)
		{
			int num = Math.Max(3, count - icons.Count);
			for (int i = 0; i < num; i++)
			{
				uGUI_TooltipIcon component = UnityEngine.Object.Instantiate(prefabIconEntry.gameObject).GetComponent<uGUI_TooltipIcon>();
				component.rectTransform.SetParent(iconCanvas.transform, worldPositionStays: false);
				VerticalLayoutGroup layoutGroup = component.layoutGroup;
				int num2 = layoutGroup.padding.left + layoutGroup.padding.right;
				float num3 = 110f - (float)num2;
				component.SetSize(num3, num3);
				component.gameObject.SetActive(value: false);
				icons.Add(component);
			}
		}
	}

	[SuppressMessage("Gendarme.Rules.Smells", "AvoidLongMethodsRule")]
	private void OnLayout()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UILayout, OnLayout);
		RectTransform rectTransform = iconCanvas.rectTransform;
		_ = textPrefix.pixelsPerUnit;
		for (int i = 0; i < icons.Count; i++)
		{
			icons[i].layoutGroup.CalculateLayoutInputHorizontal();
		}
		float x = textPrefix.GetPreferredValues(prefix).x;
		iconCanvas.CalculateLayoutInputHorizontal();
		float preferredWidth = iconCanvas.preferredWidth;
		float x2 = textPostfix.GetPreferredValues(postfix).x;
		float a = Mathf.Max(Mathf.Max(x, x2), preferredWidth);
		a = Mathf.Min(a, 429f);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(preferredWidth, a));
		textRectPrefix.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, a);
		textRectPostfix.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, a);
		iconCanvas.SetLayoutHorizontal();
		for (int j = 0; j < icons.Count; j++)
		{
			icons[j].layoutGroup.SetLayoutHorizontal();
		}
		for (int k = 0; k < icons.Count; k++)
		{
			icons[k].layoutGroup.CalculateLayoutInputVertical();
		}
		float y = textPrefix.GetPreferredValues(prefix, a, 0f).y;
		iconCanvas.CalculateLayoutInputVertical();
		float preferredHeight = iconCanvas.preferredHeight;
		float y2 = textPostfix.GetPreferredValues(postfix, a, 0f).y;
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
		textRectPrefix.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y);
		textRectPostfix.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y2);
		iconCanvas.SetLayoutVertical();
		for (int l = 0; l < icons.Count; l++)
		{
			icons[l].layoutGroup.SetLayoutVertical();
		}
		float num = 20f;
		textRectPrefix.anchoredPosition = new Vector2(20f, 0f - num);
		num += y;
		rectTransform.anchoredPosition = new Vector2(20f + 0.5f * (a - rectTransform.rect.width), 0f - num);
		num += preferredHeight;
		if (y2 > 0f)
		{
			num += 10f;
		}
		textRectPostfix.anchoredPosition = new Vector2(20f, 0f - num);
		num += y2;
		num += 20f;
		this.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, a + 40f);
		this.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num);
	}

	private void OnLayoutComplete()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UILayoutComplete, OnLayoutComplete);
		UpdatePosition();
	}

	public static void Set(ITooltip tooltip)
	{
		if (tooltip != null)
		{
			tooltipData.Reset();
			tooltip.GetTooltip(tooltipData);
			prefix = tooltipData.prefix.ToString();
			postfix = tooltipData.postfix.ToString();
			visible = true;
		}
	}

	public static void Clear()
	{
		visible = false;
	}
}
