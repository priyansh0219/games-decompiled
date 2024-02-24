using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class uGUI_TooltipIcon : MonoBehaviour, ILayoutElement
{
	[AssertNotNull]
	public uGUI_ItemIcon icon;

	[AssertNotNull]
	public TextMeshProUGUI title;

	[AssertNotNull]
	public VerticalLayoutGroup layoutGroup;

	[AssertNotNull]
	public LayoutElement iconLayout;

	[SerializeField]
	private float _minWidth = -1f;

	private RectTransform _rectTransform;

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

	public float minWidth => _minWidth;

	public float minHeight => -1f;

	public float preferredWidth => -1f;

	public float preferredHeight => -1f;

	public float flexibleWidth => -1f;

	public float flexibleHeight => -1f;

	public int layoutPriority => 1;

	public void CalculateLayoutInputHorizontal()
	{
	}

	public void CalculateLayoutInputVertical()
	{
	}

	private void OnEnable()
	{
		SetDirty();
	}

	private void OnDisable()
	{
		SetDirty();
	}

	private void OnTransformParentChanged()
	{
		SetDirty();
	}

	private void OnDidApplyAnimationProperties()
	{
		SetDirty();
	}

	private void OnBeforeTransformParentChanged()
	{
		SetDirty();
	}

	public void SetIcon(Sprite sprite)
	{
		icon.SetForegroundSprite(sprite);
	}

	public void SetText(string text)
	{
		title.text = text;
	}

	public void SetSize(float width, float height)
	{
		icon.SetSize(width, height);
		iconLayout.minWidth = width;
		iconLayout.minHeight = height;
	}

	private void SetDirty()
	{
		if (base.isActiveAndEnabled)
		{
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
	}
}
