using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class HandReticle : MonoBehaviour
{
	public enum IconType
	{
		None = 0,
		Default = 1,
		Hand = 2,
		HandDeny = 3,
		Scan = 4,
		Progress = 5,
		Info = 6,
		Drill = 7,
		PackUp = 8,
		Rename = 9,
		Interact = 10
	}

	public enum TextType
	{
		Hand = 0,
		HandSubscript = 1,
		Use = 2,
		UseSubscript = 3,
		Count = 4
	}

	public static HandReticle main;

	public RectTransform iconCanvas;

	public List<uGUI_HandReticleIcon> icons = new List<uGUI_HandReticleIcon>();

	public float iconScaleSpeed = 10f;

	public TextMeshProUGUI compTextHand;

	public TextMeshProUGUI compTextHandSubscript;

	public TextMeshProUGUI compTextUse;

	public TextMeshProUGUI compTextUseSubscript;

	public Image progressImage;

	public TextMeshProUGUI progressText;

	private IconType iconType;

	private IconType desiredIconType;

	private float desiredIconScale = 1f;

	private Dictionary<IconType, uGUI_HandReticleIcon> _icons;

	private Dictionary<GameInput.Button, Dictionary<string, string>> textCache = new Dictionary<GameInput.Button, Dictionary<string, string>>();

	private int hideCount;

	private bool hideForScreenshots;

	private string textHand;

	private string textHandSubscript;

	private string textUse;

	private string textUseSubscript;

	private float progress;

	private float targetDistance;

	private Dictionary<GameInput.Button, string> bindingCache = new Dictionary<GameInput.Button, string>();

	private float cachedProgress = float.MinValue;

	[AssertLocalization(2)]
	private const string handReticleAddButtonFormatKey = "HandReticleAddButtonFormat";

	[AssertLocalization(1)]
	private const string handReticleProgressPercentFormatKey = "HandReticleProgressPercentFormat";

	public IconType CurrentIconType => iconType;

	private void Awake()
	{
		_icons = new Dictionary<IconType, uGUI_HandReticleIcon>();
		int i = 0;
		for (int count = icons.Count; i < count; i++)
		{
			uGUI_HandReticleIcon uGUI_HandReticleIcon2 = icons[i];
			IconType type = uGUI_HandReticleIcon2.type;
			uGUI_HandReticleIcon2.SetActive(active: false);
			if (type == IconType.None)
			{
				Debug.LogError("HandReticle : Awake() : It is not allowed to explicitly assign IconType.None!");
				continue;
			}
			if (!_icons.ContainsKey(type))
			{
				_icons.Add(type, uGUI_HandReticleIcon2);
				continue;
			}
			Debug.LogError(string.Concat("HandReticle : Awake() : Duplicate icon type '", type, "' found at index '", i, "' in the iconData list!"));
		}
		progressImage.type = Image.Type.Filled;
		progressImage.fillMethod = Image.FillMethod.Radial360;
		progressImage.fillOrigin = 2;
		progressImage.fillClockwise = true;
		UpdateProgress();
	}

	private void Start()
	{
		main = this;
		iconType = IconType.None;
		for (int i = 0; i < 4; i++)
		{
			SetTextRaw((TextType)i, string.Empty);
		}
	}

	private void OnEnable()
	{
		GameInput.OnBindingsChanged += OnBindingsChanged;
		Language.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDisable()
	{
		GameInput.OnBindingsChanged -= OnBindingsChanged;
		Language.OnLanguageChanged -= OnLanguageChanged;
	}

	private void LateUpdate()
	{
		UpdateText(compTextHand, textHand);
		UpdateText(compTextHandSubscript, textHandSubscript);
		UpdateText(compTextUse, textUse);
		UpdateText(compTextUseSubscript, textUseSubscript);
		if (hideCount > 0 || AvatarInputHandler.main == null || !AvatarInputHandler.main.IsEnabled())
		{
			desiredIconType = IconType.None;
		}
		SetIconInternal(desiredIconType);
		UpdateScale();
		UpdateProgress();
		int i = 0;
		for (int count = icons.Count; i < count; i++)
		{
			icons[i].UpdateIcon();
		}
		if (XRSettings.enabled)
		{
			float num = targetDistance;
			if (num == 0f)
			{
				num = 1000f;
			}
			Matrix4x4 worldToLocalMatrix = base.transform.parent.worldToLocalMatrix;
			Matrix4x4 localToWorldMatrix = ((SNCameraRoot.main != null) ? SNCameraRoot.main.guiCam : MainCamera.camera).transform.localToWorldMatrix;
			float z = (worldToLocalMatrix * localToWorldMatrix).MultiplyPoint3x4(new Vector3(0f, 0f, num)).z;
			Vector3 localPosition = base.transform.localPosition;
			localPosition.z = z;
			base.transform.localPosition = localPosition;
		}
		desiredIconScale = 1f;
		desiredIconType = IconType.Default;
		progress = 0f;
		textHand = string.Empty;
		textHandSubscript = string.Empty;
		textUse = string.Empty;
		textUseSubscript = string.Empty;
	}

	public string GetText(string text, bool translate, GameInput.Button button = GameInput.Button.None)
	{
		try
		{
			Dictionary<string, string> value = null;
			if (!textCache.TryGetValue(button, out value))
			{
				value = new Dictionary<string, string>();
				textCache.Add(button, value);
			}
			string value2;
			if (string.IsNullOrEmpty(text) && button == GameInput.Button.None)
			{
				value2 = text;
			}
			else if (!value.TryGetValue(text, out value2))
			{
				value2 = (translate ? Language.main.Get(text) : text);
				if (button != GameInput.Button.None)
				{
					if (!bindingCache.TryGetValue(button, out var value3))
					{
						value3 = GameInput.FormatButton(button);
						bindingCache.Add(button, value3);
					}
					value2 = Language.main.GetFormat("HandReticleAddButtonFormat", value2, value3);
				}
				value.Add(text, value2);
			}
			return value2;
		}
		finally
		{
		}
	}

	public void SetText(TextType type, string text, bool translate, GameInput.Button button = GameInput.Button.None)
	{
		try
		{
			string text2 = GetText(text, translate, button);
			SetTextRaw(type, text2);
		}
		finally
		{
		}
	}

	public void SetTextRaw(TextType type, string text)
	{
		switch (type)
		{
		case TextType.Hand:
			textHand = text;
			break;
		case TextType.HandSubscript:
			textHandSubscript = text;
			break;
		case TextType.Use:
			textUse = text;
			break;
		case TextType.UseSubscript:
			textUseSubscript = text;
			break;
		}
	}

	public void SetIcon(IconType type, float size = 1f)
	{
		desiredIconType = type;
		desiredIconScale = size;
	}

	public void RequestCrosshairHide()
	{
		hideCount++;
	}

	public void UnrequestCrosshairHide()
	{
		hideCount--;
	}

	public void SetTargetDistance(float distance)
	{
		targetDistance = distance;
	}

	public void SetProgress(float progress)
	{
		this.progress = progress;
	}

	private bool ShouldHideAll()
	{
		if (!hideForScreenshots)
		{
			return AvatarInputHandler.main == null;
		}
		return true;
	}

	private void OnBindingsChanged()
	{
		ResetCache();
	}

	private void OnLanguageChanged()
	{
		ResetCache();
	}

	private void ResetCache()
	{
		foreach (KeyValuePair<GameInput.Button, Dictionary<string, string>> item in textCache)
		{
			item.Value.Clear();
		}
		bindingCache.Clear();
	}

	private void UpdateText(TextMeshProUGUI comp, string text)
	{
		comp.text = text;
		comp.gameObject.SetActive(!ShouldHideAll() && !string.IsNullOrEmpty(text));
	}

	private void SetIconInternal(IconType newIconType)
	{
		if (iconType != newIconType)
		{
			float duration = ((newIconType == IconType.None) ? 0f : 0.1f);
			if (iconType != 0 && _icons.TryGetValue(iconType, out var value))
			{
				value.SetActive(active: false, duration);
			}
			iconType = newIconType;
			if (iconType != 0 && _icons.TryGetValue(iconType, out value))
			{
				value.SetActive(active: true, duration);
			}
		}
	}

	private void UpdateScale()
	{
		Vector3 localScale = iconCanvas.localScale;
		localScale.x = (localScale.y = Mathf.Lerp(localScale.x, Mathf.Clamp(desiredIconScale, 0f, 2f), Time.deltaTime * iconScaleSpeed));
		iconCanvas.localScale = localScale;
	}

	private void UpdateProgress()
	{
		progress = Mathf.Clamp01(progress);
		if (cachedProgress != progress)
		{
			cachedProgress = progress;
			progressImage.fillAmount = progress;
			progressText.text = Language.main.GetFormat("HandReticleProgressPercentFormat", progress);
		}
	}

	private void HideForScreenshots()
	{
		hideForScreenshots = true;
	}

	private void UnhideForScreenshots()
	{
		hideForScreenshots = false;
	}
}
