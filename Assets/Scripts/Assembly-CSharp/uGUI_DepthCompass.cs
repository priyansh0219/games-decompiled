using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_DepthCompass : MonoBehaviour, ILocalizationCheckable
{
	private enum DepthMode
	{
		Undefined = 0,
		None = 1,
		Player = 2,
		Submersible = 3,
		MapRoomCamera = 4
	}

	[Header("Submersible Depth Indicator")]
	[AssertNotNull]
	public Image submersibleBackground;

	[AssertNotNull]
	public TextMeshProUGUI submersibleDepth;

	[AssertNotNull]
	public TextMeshProUGUI submersibleDepthSuffix;

	[AssertNotNull]
	public TextMeshProUGUI submersibleCrushDepth;

	[Header("Player Depth Indicator")]
	[AssertNotNull]
	public Image shadow;

	[AssertNotNull]
	public Image halfMoon;

	[AssertNotNull]
	public TextMeshProUGUI depthText;

	[AssertNotNull]
	public TextMeshProUGUI suffixText;

	[Space(10f)]
	[AssertNotNull]
	public Sprite shadowNormal;

	[AssertNotNull]
	public Sprite shadowDanger;

	[Space(10f)]
	public Color32 textColorNormal;

	public Color32 textColorDanger;

	[Space(10f)]
	[AssertNotNull]
	public Sprite halfMoonNormal;

	[AssertNotNull]
	public Sprite halfMoonDanger;

	[AssertNotNull]
	public Sprite halfMoonCompassNormal;

	[AssertNotNull]
	public Sprite halfMoonCompassDanger;

	[Space(10f)]
	[AssertNotNull]
	public uGUI_Compass compass;

	private bool _initialized;

	private DepthMode _depthMode;

	private string _meterSuffix = "m";

	private string[] _compassDirectionKeys;

	private int _playerDepthValue = int.MinValue;

	private int _submersibleDepthValue = int.MinValue;

	private int _submersibleCrushDepthValue = int.MinValue;

	private Ocean.DepthClass _cachedDepthClass = Ocean.DepthClass.Safe;

	private int _cachedCompassEnabled = -1;

	[AssertLocalization]
	private const string meterSuffixKey = "MeterSuffix";

	private const string compassDirectionFormat = "CompassDirection{0}";

	private void Start()
	{
		compass.material = new Material(compass.material);
		shadow.material = new Material(shadow.material);
	}

	private void OnDisable()
	{
		Deinitialize();
	}

	private void LateUpdate()
	{
		Initialize();
		UpdateDepth();
		UpdateCompass();
	}

	private void Initialize()
	{
		if (_initialized)
		{
			return;
		}
		Player main = Player.main;
		if (main == null)
		{
			return;
		}
		_initialized = true;
		_compassDirectionKeys = new string[compass.labels.Length];
		for (int i = 0; i < _compassDirectionKeys.Length; i++)
		{
			uGUI_Compass.Label label = compass.labels[i];
			if (label != null && label.text != null)
			{
				_compassDirectionKeys[i] = $"CompassDirection{label.name}";
			}
		}
		OnLanguageChanged();
		OnDepthClassChanged(main.depthClass);
		main.depthClass.changedEvent.AddHandler(base.gameObject, OnDepthClassChanged);
		Language.OnLanguageChanged += OnLanguageChanged;
	}

	private void Deinitialize()
	{
		if (_initialized)
		{
			_initialized = false;
			Language.OnLanguageChanged -= OnLanguageChanged;
			Player main = Player.main;
			if (main != null)
			{
				main.depthClass.changedEvent.RemoveHandler(base.gameObject, OnDepthClassChanged);
			}
		}
	}

	private void OnDepthClassChanged(Utils.MonitoredValue<int> value)
	{
		Ocean.DepthClass value2 = (Ocean.DepthClass)value.value;
		value2 = Ocean.DepthClass.Safe;
		if (_cachedDepthClass != value2)
		{
			_cachedDepthClass = value2;
			if (_depthMode == DepthMode.Player)
			{
				UpdateHalfMoonSprite();
			}
		}
		if ((uint)value2 > 1u && (uint)(value2 - 2) <= 1u)
		{
			shadow.sprite = shadowDanger;
			MaterialExtensions.SetBlending(shadow.material, Blending.AlphaBlend, alphaPremultiply: false);
			depthText.color = textColorDanger;
			suffixText.color = textColorDanger;
		}
		else
		{
			shadow.sprite = shadowNormal;
			MaterialExtensions.SetBlending(shadow.material, Blending.Multiplicative, alphaPremultiply: true);
			depthText.color = textColorNormal;
			suffixText.color = textColorNormal;
		}
	}

	private void OnLanguageChanged()
	{
		Language main = Language.main;
		if (main == null)
		{
			return;
		}
		_meterSuffix = main.Get("MeterSuffix");
		submersibleDepthSuffix.text = _meterSuffix;
		uGUI_Compass.Label[] labels = compass.labels;
		int i = 0;
		for (int num = labels.Length; i < num; i++)
		{
			uGUI_Compass.Label label = labels[i];
			if (label != null)
			{
				TextMeshProUGUI text = label.text;
				if (!(text == null))
				{
					text.text = main.Get(_compassDirectionKeys[i]);
				}
			}
		}
	}

	private void UpdateDepth()
	{
		int depth;
		int crushDepth;
		DepthMode depthInfo = GetDepthInfo(out depth, out crushDepth);
		if (_depthMode != depthInfo)
		{
			_depthMode = depthInfo;
			bool flag = _depthMode == DepthMode.Player;
			bool flag2 = _depthMode == DepthMode.Submersible;
			bool flag3 = _depthMode == DepthMode.MapRoomCamera;
			shadow.enabled = flag;
			halfMoon.enabled = flag;
			depthText.enabled = flag;
			suffixText.enabled = flag;
			if (flag)
			{
				UpdateHalfMoonSprite();
			}
			submersibleDepth.enabled = flag2 || flag3;
			submersibleDepthSuffix.enabled = flag2 || flag3;
			submersibleCrushDepth.enabled = flag2;
			submersibleBackground.enabled = flag2 || flag3;
		}
		if (_depthMode == DepthMode.Player)
		{
			if (_playerDepthValue != depth)
			{
				_playerDepthValue = depth;
				depthText.text = IntStringCache.GetStringForInt(depth);
				suffixText.text = _meterSuffix;
			}
		}
		else if (_depthMode == DepthMode.Submersible)
		{
			if (_submersibleDepthValue != depth)
			{
				_submersibleDepthValue = depth;
				submersibleDepth.text = IntStringCache.GetStringForInt(_submersibleDepthValue);
			}
			if (_submersibleCrushDepthValue != crushDepth)
			{
				_submersibleCrushDepthValue = crushDepth;
				submersibleCrushDepth.text = $"   {IntStringCache.GetStringForInt(_submersibleCrushDepthValue)}";
			}
		}
		else if (_depthMode == DepthMode.MapRoomCamera && _submersibleDepthValue != depth)
		{
			_submersibleDepthValue = depth;
			submersibleDepth.text = IntStringCache.GetStringForInt(_submersibleDepthValue);
		}
	}

	private void UpdateCompass()
	{
		bool flag = IsCompassEnabled();
		int num = (flag ? 1 : 0);
		if (_cachedCompassEnabled != num)
		{
			_cachedCompassEnabled = num;
			if (_depthMode == DepthMode.Player)
			{
				UpdateHalfMoonSprite();
			}
		}
		compass.SetVisible(flag);
		if (flag)
		{
			compass.direction = MainCamera.camera.transform.eulerAngles.y / 360f;
		}
	}

	private void UpdateHalfMoonSprite()
	{
		Ocean.DepthClass cachedDepthClass = _cachedDepthClass;
		if ((uint)cachedDepthClass > 1u && (uint)(cachedDepthClass - 2) <= 1u)
		{
			halfMoon.sprite = ((_cachedCompassEnabled == 1) ? halfMoonCompassDanger : halfMoonDanger);
		}
		else
		{
			halfMoon.sprite = ((_cachedCompassEnabled == 1) ? halfMoonCompassNormal : halfMoonNormal);
		}
	}

	private DepthMode GetDepthInfo(out int depth, out int crushDepth)
	{
		depth = 0;
		crushDepth = 0;
		if (!_initialized)
		{
			return DepthMode.None;
		}
		if (!uGUI.isMainLevel)
		{
			return DepthMode.None;
		}
		if (uGUI.isIntro)
		{
			return DepthMode.None;
		}
		if (LaunchRocket.isLaunching)
		{
			return DepthMode.None;
		}
		Player main = Player.main;
		if (main == null || main.cinematicModeActive)
		{
			return DepthMode.None;
		}
		PDA pDA = main.GetPDA();
		if (pDA != null && pDA.isInUse)
		{
			return DepthMode.None;
		}
		Vehicle vehicle = main.GetVehicle();
		if (vehicle != null)
		{
			vehicle.GetDepth(out depth, out crushDepth);
			return DepthMode.Submersible;
		}
		uGUI_CameraDrone main2 = uGUI_CameraDrone.main;
		if (main2 != null)
		{
			MapRoomCamera camera = main2.GetCamera();
			if (camera != null)
			{
				depth = Mathf.FloorToInt(camera.GetDepth());
				return DepthMode.MapRoomCamera;
			}
		}
		if (main.GetMode() == Player.Mode.Piloting)
		{
			return DepthMode.None;
		}
		depth = Mathf.FloorToInt(Ocean.GetDepthOf(main.gameObject));
		return DepthMode.Player;
	}

	private bool IsCompassEnabled()
	{
		if (!_initialized)
		{
			return false;
		}
		if (!uGUI.isMainLevel)
		{
			return false;
		}
		if (LaunchRocket.isLaunching)
		{
			return false;
		}
		if (uGUI.isIntro)
		{
			return false;
		}
		Player main = Player.main;
		if (main == null)
		{
			return false;
		}
		PDA pDA = main.GetPDA();
		if (pDA != null && pDA.isInUse)
		{
			return false;
		}
		if (main.GetMode() == Player.Mode.Piloting || main.cinematicModeActive)
		{
			return false;
		}
		Inventory main2 = Inventory.main;
		if (main2 != null && main2.equipment != null && main2.equipment.GetCount(TechType.Compass) > 0)
		{
			return true;
		}
		uGUI_CameraDrone main3 = uGUI_CameraDrone.main;
		if (main3 != null && main3.GetCamera() != null)
		{
			return true;
		}
		return false;
	}

	public string CompileTimeCheck(ILanguage language)
	{
		for (int i = 0; i < compass.labels.Length; i++)
		{
			uGUI_Compass.Label label = compass.labels[i];
			if (label != null && label.text != null)
			{
				string text = language.CheckKey($"CompassDirection{label.name}");
				if (text != null)
				{
					return text;
				}
			}
		}
		return null;
	}
}
