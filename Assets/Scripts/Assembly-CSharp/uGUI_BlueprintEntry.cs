using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class uGUI_BlueprintEntry : MonoBehaviour, ILayoutElement, INotificationTarget, ISelectable, ITooltip, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler, ICompileTimeCheckable
{
	[AssertNotNull]
	public uGUI_ItemIcon icon;

	[AssertNotNull]
	public TextMeshProUGUI title;

	[AssertNotNull]
	public GameObject progressPlaceholder;

	[AssertNotNull]
	public GameObject prefabProgress;

	public Vector2 iconSize = new Vector2(106f, 106f);

	public RectTransform pin;

	[SerializeField]
	private float _minWidth = -1f;

	private RectTransform _rectTransform;

	private uGUI_BlueprintProgress _progress;

	private uGUI_BlueprintsTab _manager;

	private const uint _showProgressAboveTotal = 1u;

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

	public bool showTooltipOnDrag => false;

	public void CalculateLayoutInputHorizontal()
	{
	}

	public void CalculateLayoutInputVertical()
	{
	}

	private void Awake()
	{
		icon.SetSize(iconSize);
		progressPlaceholder.SetActive(value: false);
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

	public void Initialize(uGUI_BlueprintsTab manager)
	{
		_manager = manager;
	}

	public void SetIcon(TechType techType)
	{
		icon.SetForegroundSprite(SpriteManager.Get(techType));
		icon.SetBackgroundSprite(SpriteManager.GetBackground(techType));
		float num = Mathf.Min(iconSize.x, iconSize.y) * 0.5f;
		icon.SetBackgroundRadius(num);
		icon.SetBarRadius(num);
	}

	public void SetText(string text)
	{
		title.text = text;
	}

	public void SetValue(int unlocked, int total)
	{
		if (_progress == null)
		{
			if ((long)total > 1L)
			{
				_progress = Object.Instantiate(prefabProgress).GetComponent<uGUI_BlueprintProgress>();
				RectTransform component = _progress.GetComponent<RectTransform>();
				component.SetParent(rectTransform, worldPositionStays: false);
				component.SetAsLastSibling();
				progressPlaceholder.SetActive(value: true);
			}
		}
		else
		{
			if ((long)total > 1L)
			{
				_progress.gameObject.SetActive(value: true);
			}
			if ((long)total <= 1L)
			{
				_progress.gameObject.SetActive(value: false);
				progressPlaceholder.SetActive(value: false);
			}
		}
		if (_progress != null)
		{
			_progress.SetValue(unlocked, total);
		}
		icon.SetChroma((unlocked < total) ? 0f : 1f);
	}

	private void SetDirty()
	{
		if (base.isActiveAndEnabled)
		{
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (_manager != null)
		{
			_manager.OnPointerEnter(this);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (_manager != null)
		{
			_manager.OnPointerExit(this);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		int num = -1;
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Left:
			num = 0;
			break;
		case PointerEventData.InputButton.Right:
			num = 1;
			break;
		case PointerEventData.InputButton.Middle:
			num = 2;
			break;
		}
		if (_manager == null || num == -1 || !_manager.OnPointerClick(this, num))
		{
			FPSInputModule.BubbleEvent(base.gameObject, eventData, ExecuteEvents.pointerClickHandler);
		}
	}

	public void GetTooltip(TooltipData data)
	{
		if (_manager != null)
		{
			_manager.GetTooltip(this, data);
		}
	}

	bool ISelectable.IsValid()
	{
		if (this != null)
		{
			return base.isActiveAndEnabled;
		}
		return false;
	}

	RectTransform ISelectable.GetRect()
	{
		return rectTransform;
	}

	bool ISelectable.OnButtonDown(GameInput.Button button)
	{
		return false;
	}

	bool INotificationTarget.IsVisible()
	{
		return ((INotificationTarget)icon).IsVisible();
	}

	bool INotificationTarget.IsDestroyed()
	{
		return this == null;
	}

	void INotificationTarget.Progress(float progress)
	{
		if (icon.SetNotificationProgress(progress))
		{
			icon.SetNotificationBackgroundColor(NotificationManager.notificationColor);
			icon.SetNotificationOffset(iconSize * -0.1464466f);
		}
	}

	public string CompileTimeCheck()
	{
		if (prefabProgress.GetComponent<uGUI_BlueprintProgress>() == null)
		{
			return $"uGUI_BlueprintProgress component is expected on {prefabProgress.name} prefab assigned to prefabProgress field\n";
		}
		return null;
	}
}
