using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class uGUI_ItemIcon : Graphic, ITooltip, uGUI_ILockable, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IDragHoverHandler, IPointerDownHandler, IPointerUpHandler, INotificationTarget, ISelectable, ISelectHandler, IDeselectHandler
{
	private static readonly Vector2 vectorCenter = new Vector2(0.5f, 0.5f);

	protected const float fadeTime = 0.1f;

	public const float radiusCoef = -0.1464466f;

	public const float invSquareRoot = 0.7071068f;

	public const string keywordNotification = "NOTIFICATION";

	public const string keywordGrayscale = "GRAYSCALE";

	public const string keywordFillRadial = "FILL_RADIAL";

	public const string keywordFillHorizontal = "FILL_HORIZONTAL";

	public const string keywordFillVertical = "FILL_VERTICAL";

	public const string keywordSlice9Grid = "SLICE_9_GRID";

	private static Texture2D _overlayTex;

	private static Material _iconMaterial;

	public uGUI_IIconManager manager;

	protected bool hovered;

	protected bool pressed;

	protected uGUI_Icon background;

	protected uGUI_Icon foreground;

	protected Image bar;

	protected Text label;

	protected uGUI_NotificationLabel notification;

	protected Vector2 foregroundSize = new Vector2(-1f, -1f);

	protected Vector2 backgroundSize = new Vector2(-1f, -1f);

	protected Vector2 barSize = new Vector2(-1f, -1f);

	protected float barRadius = -1f;

	protected Color foregroundColorNormal = new Color(1f, 1f, 1f, 1f);

	protected Color foregroundColorHovered = new Color(1f, 1f, 1f, 1f);

	protected Color foregroundColorPressed = new Color(1f, 1f, 1f, 1f);

	protected Color backgroundColorNormal = new Color(1f, 1f, 1f, 1f);

	protected Color backgroundColorHovered = new Color(1f, 1f, 1f, 1f);

	protected Color backgroundColorPressed = new Color(1f, 1f, 1f, 1f);

	protected float foregroundChroma = 1f;

	protected float backgroundChroma = 1f;

	protected float godRaysIntensity;

	protected int notificationNumber = int.MinValue;

	protected CoroutineTween punchTween;

	protected float punchSeed;

	protected float punchFrequency;

	protected float punchAmplitude;

	protected float punchDuration;

	protected float punchDelay;

	private FillMethod previousFillMethod;

	private float previousFillProgress;

	public static Material iconMaterial
	{
		get
		{
			if (_iconMaterial == null)
			{
				_iconMaterial = new Material(ShaderManager.preloadedShaders.UIIcon);
				_overlayTex = Resources.Load<Texture2D>("Sprites/NotificationOverlay");
				if (_overlayTex != null)
				{
					_iconMaterial.SetTexture(ShaderPropertyID._NotificationOverlayTex, _overlayTex);
				}
				else
				{
					Debug.LogError("NotificationOverlay texture is not found!");
				}
			}
			return _iconMaterial;
		}
	}

	protected float barThickness
	{
		get
		{
			if (!(bar != null))
			{
				return 0f;
			}
			return bar.material.GetFloat(ShaderPropertyID._Width) - 1f;
		}
	}

	public bool showTooltipOnDrag => false;

	protected override void Awake()
	{
		base.Awake();
		punchTween = new CoroutineTween(this)
		{
			deltaTimeProvider = PDA.GetDeltaTime,
			mode = CoroutineTween.Mode.Once,
			onStart = OnPunchStart,
			onUpdate = OnPunchUpdate,
			onStop = OnPunchStop
		};
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateColor();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		hovered = false;
		pressed = false;
		if (punchTween != null)
		{
			punchTween.Stop();
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
	}

	public void Init(uGUI_IIconManager manager, Transform parent, Vector2 anchor, Vector2 pivot)
	{
		this.manager = manager;
		RectTransformExtensions.SetParams(base.rectTransform, anchor, pivot, parent);
	}

	public void SetForegroundSprite(Sprite sprite)
	{
		if (sprite == null)
		{
			if (foreground != null)
			{
				foreground.sprite = null;
				foreground.enabled = false;
			}
		}
		else
		{
			CreateForeground();
			foreground.sprite = sprite;
			foreground.enabled = true;
			SetForegroundSize(foregroundSize.x, foregroundSize.y);
		}
	}

	public void SetBackgroundSprite(Sprite sprite)
	{
		if (sprite == null)
		{
			if (background != null)
			{
				background.sprite = null;
				background.enabled = false;
			}
			return;
		}
		CreateBackground();
		background.sprite = sprite;
		background.enabled = true;
		SetBackgroundSize(backgroundSize.x, backgroundSize.y);
		Material material = background.material;
		bool proceduralSlice9Grid = SpriteManager.GetProceduralSlice9Grid(sprite);
		MaterialExtensions.SetKeyword(material, "SLICE_9_GRID", proceduralSlice9Grid);
		if (proceduralSlice9Grid)
		{
			material.SetVector(ShaderPropertyID._Size, backgroundSize);
		}
		UpdateColor();
	}

	public void SetBackgroundRadius(float radius)
	{
		if (background != null)
		{
			background.material.SetFloat(ShaderPropertyID._Radius, radius);
		}
	}

	public void SetForegroundBlending(Blending blending)
	{
		if (!(foreground == null))
		{
			MaterialExtensions.SetBlending(foreground.material, blending, blending == Blending.Additive);
		}
	}

	public void SetBackgroundBlending(Blending blending)
	{
		if (!(background == null))
		{
			MaterialExtensions.SetBlending(background.material, blending, blending == Blending.Additive);
		}
	}

	public void SetLabelFont(Font font, int fontSize)
	{
		if (!(font == null))
		{
			if (label == null)
			{
				CreateLabel("Label", base.rectTransform, vectorCenter, vectorCenter, base.gameObject.layer, font, fontSize, out label);
				RearrangeElements();
			}
			else
			{
				label.font = font;
				label.fontSize = fontSize;
			}
		}
	}

	public void SetLabelText(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			if (label != null)
			{
				label.text = string.Empty;
				label.enabled = false;
			}
		}
		else if (label != null)
		{
			label.text = text;
			label.enabled = true;
		}
		else
		{
			Debug.LogError("uGUI_ItemIcon : SetLabelText() : Attempt to set text to an uninitialized label. Call SetLabelFont() once first.");
		}
	}

	public void SetColors(Color normal, Color hovered, Color pressed)
	{
		SetForegroundColors(normal, hovered, pressed);
		SetBackgroundColors(normal, hovered, pressed);
	}

	public void SetForegroundColors(Color normal, Color hovered, Color pressed)
	{
		foregroundColorNormal = normal;
		foregroundColorHovered = hovered;
		foregroundColorPressed = pressed;
		UpdateColor();
	}

	public void SetBackgroundColors(Color normal, Color hovered, Color pressed)
	{
		backgroundColorNormal = normal;
		backgroundColorHovered = hovered;
		backgroundColorPressed = pressed;
		UpdateColor();
	}

	public void SetChroma(float chroma)
	{
		SetForegroundChroma(chroma);
		SetBackgroundChroma(chroma);
	}

	public void SetForegroundChroma(float chroma)
	{
		chroma = Mathf.Clamp01(chroma);
		if (foregroundChroma != chroma)
		{
			if (foreground != null)
			{
				Material obj = foreground.material;
				MaterialExtensions.SetKeyword(obj, "GRAYSCALE", chroma < 1f);
				obj.SetFloat(ShaderPropertyID._Chroma, chroma);
			}
			foregroundChroma = chroma;
		}
	}

	public void SetBackgroundChroma(float chroma)
	{
		chroma = Mathf.Clamp01(chroma);
		if (backgroundChroma != chroma)
		{
			if (background != null)
			{
				Material obj = background.material;
				MaterialExtensions.SetKeyword(obj, "GRAYSCALE", chroma < 1f);
				obj.SetFloat(ShaderPropertyID._Chroma, chroma);
			}
			backgroundChroma = chroma;
		}
	}

	public void SetAlpha(float normal, float hovered, float pressed)
	{
		SetForegroundAlpha(normal, hovered, pressed);
		SetBackgroundAlpha(normal, hovered, pressed);
	}

	public void SetForegroundAlpha(float alpha)
	{
		SetForegroundAlpha(alpha, alpha, alpha);
	}

	public void SetForegroundAlpha(float normal, float hovered, float pressed)
	{
		foregroundColorNormal.a = normal;
		foregroundColorHovered.a = hovered;
		foregroundColorPressed.a = pressed;
		UpdateColor();
	}

	public void SetBackgroundAlpha(float alpha)
	{
		SetBackgroundAlpha(alpha, alpha, alpha);
	}

	public void SetBackgroundAlpha(float normal, float hovered, float pressed)
	{
		backgroundColorNormal.a = normal;
		backgroundColorHovered.a = hovered;
		backgroundColorPressed.a = pressed;
		UpdateColor();
	}

	public void SetBarValue(float value)
	{
		if (value < 0f)
		{
			if (bar != null)
			{
				UnityEngine.Object.Destroy(bar.material);
				UnityEngine.Object.Destroy(bar.gameObject);
			}
			return;
		}
		if (bar == null)
		{
			GameObject gameObject = new GameObject("Bar");
			gameObject.layer = base.gameObject.layer;
			bar = gameObject.AddComponent<Image>();
			Shader uIIconBar = ShaderManager.preloadedShaders.UIIconBar;
			bar.material = new Material(uIIconBar);
			bar.raycastTarget = false;
			RectTransformExtensions.SetParams(bar.rectTransform, vectorCenter, vectorCenter, base.rectTransform);
			SetBarSize(barSize.x, barSize.y);
			SetBarRadius(barRadius);
			RearrangeElements();
			bar.rectTransform.anchoredPosition = Vector2.zero;
		}
		bar.material.SetFloat(ShaderPropertyID._Value, value);
	}

	public void SetSize(Vector2 size)
	{
		SetSize(size.x, size.y);
	}

	public void SetSize(float width, float height)
	{
		SetActiveSize(width, height);
		SetBackgroundSize(width, height);
		SetForegroundSize(width, height);
		SetBarSize(width, height);
	}

	public void SetActiveSize(Vector2 size)
	{
		SetActiveSize(size.x, size.y);
	}

	public void SetActiveSize(float width, float height)
	{
		RectTransformExtensions.SetSize(base.rectTransform, width, height);
		if (foreground != null)
		{
			foreground.rectTransform.anchoredPosition = Vector2.zero;
		}
		if (background != null)
		{
			background.rectTransform.anchoredPosition = Vector2.zero;
		}
		if (label != null)
		{
			label.rectTransform.anchoredPosition = Vector2.zero;
		}
		if (bar != null)
		{
			bar.rectTransform.anchoredPosition = Vector2.zero;
		}
	}

	public void SetGraphicSize(Vector2 size)
	{
		SetBackgroundSize(size.x, size.y);
		SetForegroundSize(size.x, size.y);
		SetBarSize(size.x, size.y);
	}

	public void SetForegroundSize(Vector2 size)
	{
		SetForegroundSize(size.x, size.y);
	}

	public void SetBackgroundSize(Vector2 size)
	{
		SetBackgroundSize(size.x, size.y);
	}

	public void SetBarSize(Vector2 size)
	{
		SetBarSize(size.x, size.y);
	}

	public void SetForegroundSize(float width, float height, bool keepAspect = true)
	{
		foregroundSize.Set(width, height);
		if (foreground != null)
		{
			Sprite sprite = foreground.sprite;
			if (sprite != null)
			{
				Vector2 size = sprite.rect.size;
				RectTransformExtensions.Fit(foreground.rectTransform, foregroundSize.x, foregroundSize.y, size.x, size.y, keepAspect);
			}
		}
		UpdateFillProperties();
	}

	public void SetBackgroundSize(float width, float height, bool keepAspect = false)
	{
		backgroundSize.Set(width, height);
		if (background != null)
		{
			Sprite sprite = background.sprite;
			if (sprite != null)
			{
				Vector2 size = sprite.rect.size;
				RectTransformExtensions.Fit(background.rectTransform, backgroundSize.x, backgroundSize.y, size.x, size.y, keepAspect);
			}
			background.material.SetVector(ShaderPropertyID._Size, backgroundSize);
		}
		UpdateFillProperties();
	}

	public void SetBarSize(float width, float height)
	{
		barSize.Set(width, height);
		if (bar != null)
		{
			float num = barThickness;
			Vector2 vector = new Vector2(width + num, height + num);
			RectTransformExtensions.SetSize(bar.rectTransform, vector.x, vector.y);
			bar.material.SetVector(ShaderPropertyID._Size, vector);
		}
	}

	public void SetBarRadius(float radius)
	{
		barRadius = radius;
		if (bar != null)
		{
			bar.material.SetFloat(ShaderPropertyID._Radius, barRadius + barThickness * 0.5f);
		}
	}

	private void UpdateFillProperties()
	{
		Vector2 vector = Vector2.Max(backgroundSize, foregroundSize);
		Vector4 value = new Vector4(-0.5f * vector.x, -0.5f * vector.y, vector.x, vector.y);
		if (background != null)
		{
			background.material.SetVector(ShaderPropertyID._FillRect, value);
		}
		if (foreground != null)
		{
			foreground.material.SetVector(ShaderPropertyID._FillRect, value);
		}
	}

	public void SetPosition(float x, float y)
	{
		SetPosition(new Vector2(x, y));
	}

	public void SetPosition(Vector2 position)
	{
		base.rectTransform.anchoredPosition = position;
	}

	public void SetAsLastSibling()
	{
		base.rectTransform.SetAsLastSibling();
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}

	public void SetScale(float x, float y)
	{
		base.rectTransform.localScale = new Vector3(x, y, 1f);
	}

	public void SetProgress(float progress, FillMethod fillMethod = FillMethod.Radial)
	{
		if (progress < 0f)
		{
			progress = 0f;
		}
		if (progress >= 1f)
		{
			progress = 1f;
			fillMethod = FillMethod.None;
		}
		if (fillMethod != previousFillMethod)
		{
			if (foreground != null)
			{
				Material obj = foreground.material;
				MaterialExtensions.SetKeyword(obj, "FILL_RADIAL", fillMethod == FillMethod.Radial);
				MaterialExtensions.SetKeyword(obj, "FILL_HORIZONTAL", fillMethod == FillMethod.Horizontal);
				MaterialExtensions.SetKeyword(obj, "FILL_VERTICAL", fillMethod == FillMethod.Vertical);
			}
			if (background != null)
			{
				Material obj2 = background.material;
				MaterialExtensions.SetKeyword(obj2, "FILL_RADIAL", fillMethod == FillMethod.Radial);
				MaterialExtensions.SetKeyword(obj2, "FILL_HORIZONTAL", fillMethod == FillMethod.Horizontal);
				MaterialExtensions.SetKeyword(obj2, "FILL_VERTICAL", fillMethod == FillMethod.Vertical);
			}
			previousFillMethod = fillMethod;
		}
		if (previousFillProgress != progress)
		{
			if (foreground != null)
			{
				foreground.material.SetFloat(ShaderPropertyID._FillValue, progress);
			}
			if (background != null)
			{
				background.material.SetFloat(ShaderPropertyID._FillValue, progress);
			}
			previousFillProgress = progress;
		}
	}

	public void SetGodRaysIntensity(float intensity)
	{
		intensity = Mathf.Clamp01(intensity);
		if (godRaysIntensity != intensity)
		{
			godRaysIntensity = intensity;
			if (foreground != null)
			{
				Material obj = foreground.material;
				MaterialExtensions.SetKeyword(obj, "NOTIFICATION", intensity > 0f);
				obj.SetFloat(ShaderPropertyID._NotificationStrength, intensity);
			}
			if (background != null)
			{
				Material obj2 = background.material;
				MaterialExtensions.SetKeyword(obj2, "NOTIFICATION", intensity > 0f);
				obj2.SetFloat(ShaderPropertyID._NotificationStrength, intensity);
			}
		}
	}

	public bool SetNotificationProgress(float progress)
	{
		float notificationAlpha = Mathf.Sin(progress * ((float)Math.PI / 2f));
		SetGodRaysIntensity(notificationAlpha);
		bool num = SetNotificationAlpha(notificationAlpha);
		if (num)
		{
			SetNotificationBackgroundColor(NotificationManager.notificationColor);
			SetNotificationAnchor(UIAnchor.UpperRight);
			SetNotificationOffset(new Vector2(-10f, -10f));
		}
		SetNotificationText("+");
		return num;
	}

	public bool SetNotificationAlpha(float alpha)
	{
		bool result = false;
		alpha = Mathf.Clamp01(alpha);
		if (alpha > 0f)
		{
			if (notification == null)
			{
				notification = uGUI_NotificationLabel.CreateInstance(base.rectTransform);
				RearrangeElements();
				result = true;
			}
			notification.SetAlpha(alpha);
		}
		else if (notification != null)
		{
			notificationNumber = int.MinValue;
			UnityEngine.Object.Destroy(notification.gameObject);
		}
		return result;
	}

	public void SetNotificationAnchor(UIAnchor anchor)
	{
		if (notification != null)
		{
			notification.SetAnchor(anchor);
		}
	}

	public void SetNotificationOffset(Vector2 offset)
	{
		if (notification != null)
		{
			notification.SetOffset(offset);
		}
	}

	public void SetNotificationBackgroundColor(Color color)
	{
		if (notification != null)
		{
			notification.SetBackgroundColor(color);
		}
	}

	public void SetNotificationNumber(int number)
	{
		if (notification != null && notificationNumber != number)
		{
			notificationNumber = number;
			notification.SetText(IntStringCache.GetStringForInt(notificationNumber));
		}
	}

	public void SetNotificationText(string text)
	{
		if (notification != null)
		{
			notificationNumber = int.MinValue;
			notification.SetText(text);
		}
	}

	public void PunchScale()
	{
		PunchScale(5f, 0.5f, 1f + UnityEngine.Random.Range(-0.2f, 0.2f));
	}

	public void PunchScale(float frequency, float amplitude, float duration, float delay = 0f)
	{
		if (!(duration <= 0f))
		{
			if (delay < 0f)
			{
				delay = 0f;
			}
			punchFrequency = frequency;
			punchAmplitude = amplitude;
			punchDuration = duration;
			punchDelay = delay;
			punchTween.duration = punchDuration + punchDelay;
			punchTween.Start();
		}
	}

	protected void OnPunchStart()
	{
		punchSeed = UnityEngine.Random.value;
	}

	protected void OnPunchUpdate(float scalar)
	{
		float reduction = 1f / (0.01f * punchDuration);
		float num = punchDelay / (punchDelay + punchDuration);
		MathExtensions.Oscillation(t: (scalar - num) / (1f - num), reduction: reduction, frequency: punchFrequency, seed: punchSeed, o: out var o, o1: out var o2);
		if (foreground != null)
		{
			foreground.rectTransform.localScale = new Vector3(1f + punchAmplitude * o, 1f + punchAmplitude * o2, 1f);
		}
		if (background != null)
		{
			background.rectTransform.localScale = new Vector3(1f + punchAmplitude * o, 1f + punchAmplitude * o2, 1f);
		}
	}

	protected void OnPunchStop()
	{
		if (foreground != null)
		{
			foreground.rectTransform.localScale = new Vector3(1f, 1f, 1f);
		}
		if (background != null)
		{
			background.rectTransform.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	protected void CreateForeground()
	{
		if (!(foreground != null))
		{
			CreateIcon("Foreground", base.rectTransform, vectorCenter, vectorCenter, base.gameObject.layer, out foreground);
			RearrangeElements();
			Material material = new Material(iconMaterial);
			UpdateFillProperties();
			MaterialExtensions.SetKeyword(material, "GRAYSCALE", foregroundChroma < 1f);
			material.SetFloat(ShaderPropertyID._Chroma, foregroundChroma);
			MaterialExtensions.SetKeyword(material, "NOTIFICATION", godRaysIntensity > 0f);
			material.SetFloat(ShaderPropertyID._NotificationStrength, godRaysIntensity);
			foreground.material = material;
		}
	}

	protected void CreateBackground()
	{
		if (!(background != null))
		{
			CreateIcon("Background", base.rectTransform, vectorCenter, vectorCenter, base.gameObject.layer, out background);
			RearrangeElements();
			Material material = new Material(iconMaterial);
			UpdateFillProperties();
			MaterialExtensions.SetKeyword(material, "GRAYSCALE", backgroundChroma < 1f);
			material.SetFloat(ShaderPropertyID._Chroma, backgroundChroma);
			MaterialExtensions.SetKeyword(material, "NOTIFICATION", godRaysIntensity > 0f);
			material.SetFloat(ShaderPropertyID._NotificationStrength, godRaysIntensity);
			background.material = material;
		}
	}

	public static void CreateIcon(string name, Transform parent, Vector2 anchor, Vector2 pivot, int layer, out uGUI_Icon image)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.layer = layer;
		image = gameObject.AddComponent<uGUI_Icon>();
		image.raycastTarget = false;
		RectTransformExtensions.SetParams(image.rectTransform, anchor, pivot, parent);
		image.enabled = false;
	}

	public static void CreateLabel(string name, Transform parent, Vector2 anchor, Vector2 pivot, int layer, Font font, int fontSize, out Text label)
	{
		GameObject gameObject = new GameObject("Label");
		gameObject.layer = layer;
		label = gameObject.AddComponent<Text>();
		RectTransformExtensions.SetParams(label.rectTransform, anchor, pivot, parent);
		label.alignment = TextAnchor.MiddleCenter;
		label.horizontalOverflow = HorizontalWrapMode.Overflow;
		label.verticalOverflow = VerticalWrapMode.Overflow;
		label.font = font;
		label.fontSize = fontSize;
		label.supportRichText = false;
		label.text = string.Empty;
		label.enabled = false;
	}

	private void RearrangeElements()
	{
		int num = 0;
		if (background != null)
		{
			background.rectTransform.SetSiblingIndex(num);
			num++;
		}
		if (foreground != null)
		{
			foreground.rectTransform.SetSiblingIndex(num);
			num++;
		}
		if (label != null)
		{
			label.rectTransform.SetSiblingIndex(num);
			num++;
		}
		if (bar != null)
		{
			bar.rectTransform.SetSiblingIndex(num);
			num++;
		}
		if (notification != null)
		{
			notification.rectTransform.SetSiblingIndex(num);
			num++;
		}
	}

	private void UpdateColor()
	{
		if (foreground != null && foreground.enabled)
		{
			Color color = foregroundColorNormal;
			color = (hovered ? ((!pressed) ? foregroundColorHovered : foregroundColorPressed) : ((!pressed) ? foregroundColorNormal : foregroundColorPressed));
			CanvasRenderer canvasRenderer = foreground.canvasRenderer;
			if (canvasRenderer != null)
			{
				canvasRenderer.SetColor(color);
			}
		}
		if (background != null && background.enabled)
		{
			Color color2 = backgroundColorNormal;
			color2 = (hovered ? ((!pressed) ? backgroundColorHovered : backgroundColorPressed) : ((!pressed) ? backgroundColorNormal : backgroundColorPressed));
			CanvasRenderer canvasRenderer2 = background.canvasRenderer;
			if (canvasRenderer2 != null)
			{
				canvasRenderer2.SetColor(color2);
			}
		}
		if (bar != null && bar.enabled)
		{
			Color color3 = backgroundColorNormal;
			color3 = (hovered ? ((!pressed) ? backgroundColorHovered : backgroundColorPressed) : ((!pressed) ? backgroundColorNormal : backgroundColorPressed));
			CanvasRenderer canvasRenderer3 = bar.canvasRenderer;
			if (canvasRenderer3 != null)
			{
				canvasRenderer3.SetColor(color3);
			}
		}
	}

	private void OnPointerEnter()
	{
		hovered = true;
		UpdateColor();
		if (manager != null)
		{
			manager.OnPointerEnter(this);
		}
	}

	private void OnPointerExit()
	{
		hovered = false;
		UpdateColor();
		if (manager != null)
		{
			manager.OnPointerExit(this);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnPointerEnter();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnPointerExit();
	}

	void ISelectHandler.OnSelect(BaseEventData eventData)
	{
		OnPointerEnter();
	}

	void IDeselectHandler.OnDeselect(BaseEventData eventData)
	{
		OnPointerExit();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		pressed = true;
		UpdateColor();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		pressed = false;
		UpdateColor();
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
		if (manager == null || num == -1 || !manager.OnPointerClick(this, num))
		{
			FPSInputModule.BubbleEvent(base.gameObject, eventData, ExecuteEvents.pointerClickHandler);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (manager == null || !manager.OnBeginDrag(this))
		{
			GameObject pointerDrag = FPSInputModule.BubbleEvent(base.gameObject, eventData, ExecuteEvents.beginDragHandler);
			eventData.pointerDrag = pointerDrag;
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.OnEndDrag(this);
		}
	}

	public void OnDrop(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.OnDrop(this);
		}
	}

	public void OnDragHoverEnter(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.OnDragHoverEnter(this);
		}
	}

	public void OnDragHoverStay(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.OnDragHoverStay(this);
		}
	}

	public void OnDragHoverExit(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.OnDragHoverExit(this);
		}
	}

	bool INotificationTarget.IsVisible()
	{
		if (this != null && base.isActiveAndEnabled)
		{
			if (foreground != null && foreground.isActiveAndEnabled)
			{
				CanvasRenderer canvasRenderer = foreground.canvasRenderer;
				if (canvasRenderer != null && !canvasRenderer.cull)
				{
					return canvasRenderer.GetInheritedAlpha() > 0f;
				}
			}
			if (background != null && background.isActiveAndEnabled)
			{
				CanvasRenderer canvasRenderer = background.canvasRenderer;
				if (canvasRenderer != null && !canvasRenderer.cull)
				{
					return canvasRenderer.GetInheritedAlpha() > 0f;
				}
			}
			if (label != null && label.isActiveAndEnabled)
			{
				CanvasRenderer canvasRenderer = background.canvasRenderer;
				if (canvasRenderer != null && !canvasRenderer.cull)
				{
					return canvasRenderer.GetInheritedAlpha() > 0f;
				}
			}
		}
		return false;
	}

	bool INotificationTarget.IsDestroyed()
	{
		return this == null;
	}

	void INotificationTarget.Progress(float progress)
	{
		SetNotificationProgress(progress);
	}

	public void OnLock()
	{
		float normal = 0.3f;
		SetAlpha(normal, normal, normal);
	}

	public void OnUnlock()
	{
		float normal = 1f;
		SetAlpha(normal, normal, normal);
	}

	void ITooltip.GetTooltip(TooltipData data)
	{
		if (manager != null)
		{
			manager.GetTooltip(this, data);
			return;
		}
		Transform parent = base.transform.parent;
		if (parent != null)
		{
			parent.GetComponentInParent<ITooltip>()?.GetTooltip(data);
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
		return base.rectTransform;
	}

	bool ISelectable.OnButtonDown(GameInput.Button button)
	{
		if (manager == null)
		{
			return false;
		}
		if (button == GameInput.button0)
		{
			manager.OnPointerClick(this, 0);
			return true;
		}
		if (button == GameInput.button1)
		{
			manager.OnPointerClick(this, 1);
			return true;
		}
		if (button == GameInput.button2)
		{
			manager.OnPointerClick(this, 2);
			return true;
		}
		if (button == GameInput.button3)
		{
			manager.OnPointerClick(this, 3);
			return true;
		}
		return manager.OnButtonDown(this, button);
	}
}
