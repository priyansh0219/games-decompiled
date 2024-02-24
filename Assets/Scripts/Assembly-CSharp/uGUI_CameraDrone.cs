using System;
using Gendarme;
using TMPro;
using UWE;
using UnityEngine;
using UnityEngine.UI;

[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
public class uGUI_CameraDrone : MonoBehaviour
{
	public static uGUI_CameraDrone main;

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.PreCanvasPing;

	[AssertLocalization]
	private const string distanceLabelKey = "CameraDroneDistance";

	[AssertLocalization]
	private const string meterSuffixKey = "MeterSuffix";

	[AssertLocalization]
	private const string noSignalLabelKey = "MapRoomCameraNoSignal";

	[AssertLocalization(1)]
	private const string titleFormat = "CameraDroneTitle";

	[AssertLocalization(4)]
	private const string controlsFormat = "MapRoomCameraControls";

	[AssertNotNull]
	public GameObject content;

	[AssertNotNull]
	public GameObject connecting;

	[AssertNotNull]
	public GameObject noSignal;

	[AssertNotNull]
	public TextMeshProUGUI textTitle;

	[AssertNotNull]
	public TextMeshProUGUI textHealth;

	[AssertNotNull]
	public TextMeshProUGUI textPower;

	[AssertNotNull]
	public TextMeshProUGUI textDistance;

	[AssertNotNull]
	public TextMeshProUGUI textNoSignal;

	[AssertNotNull]
	public FMODAsset changeCameraSound;

	[AssertNotNull]
	public Image fader;

	[AssertNotNull]
	public RectTransform pingCanvas;

	[AssertNotNull]
	public Sprite pingSprite;

	public float fadeOutTime = 0.3f;

	private MapRoomCamera activeCamera;

	private MapRoomScreen activeScreen;

	private bool waitForCamera = true;

	private string stringDistance;

	private string stringMeterSuffix;

	private string stringControls;

	private int health = int.MinValue;

	private int power = int.MinValue;

	private int distance = int.MinValue;

	private uGUI_Icon ping;

	private Sequence faderSequence = new Sequence();

	private float _noise;

	private Vector2 safeZone = new Vector2(0.1f, 0.1f);

	public float noise => _noise;

	private void Awake()
	{
		if (main != null)
		{
			UWE.Utils.DestroyWrap(this);
			return;
		}
		main = this;
		GameObject gameObject = new GameObject("Ping");
		ping = gameObject.AddComponent<uGUI_Icon>();
		ping.sprite = pingSprite;
		ping.rectTransform.SetParent(pingCanvas, worldPositionStays: false);
		ping.rectTransform.anchorMin = Vector2.zero;
		ping.rectTransform.anchorMax = Vector2.zero;
		Material material = new Material(uGUI_ItemIcon.iconMaterial);
		ping.material = material;
		MaterialExtensions.SetKeyword(material, "NOTIFICATION", state: true);
		material.SetFloat(ShaderPropertyID._NotificationStrength, 1f);
		material.SetVector(ShaderPropertyID._Size, pingSprite.rect.size);
		content.SetActive(value: false);
		faderSequence.ForceState(state: false);
	}

	private void OnEnable()
	{
		UpdateTexts();
		GameInput.OnBindingsChanged += OnBindingsChanged;
		Language.OnLanguageChanged += OnLanguageChanged;
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.PreCanvasPing, UpdatePing);
	}

	private void OnDisable()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.PreCanvasPing, UpdatePing);
		GameInput.OnBindingsChanged -= OnBindingsChanged;
		Language.OnLanguageChanged -= OnLanguageChanged;
	}

	private void LateUpdate()
	{
		content.SetActive(activeCamera != null);
		float a = 0f;
		float a2 = (faderSequence.target ? 1f : 0f);
		if (faderSequence.active)
		{
			faderSequence.Update();
			a2 = (a = 0.5f * (1f - Mathf.Cos((float)System.Math.PI * faderSequence.t)));
		}
		float b = ((activeCamera != null) ? (Mathf.Max(0f, activeCamera.GetScreenDistance() - 250f) / 250f) : 0f);
		_noise = Mathf.Max(a, b);
		float b2 = ((activeCamera != null) ? Mathf.Clamp((activeCamera.GetScreenDistance() - 520f) / 100f, 0f, 0.99f) : 0f);
		Color color = fader.color;
		color.a = Mathf.Max(a2, b2);
		fader.color = color;
		if (activeCamera == null)
		{
			return;
		}
		if (activeCamera.CanBeControlled())
		{
			if (waitForCamera)
			{
				if (activeCamera.IsReady())
				{
					connecting.SetActive(value: false);
					waitForCamera = false;
					faderSequence.Set(1f, target: false);
				}
			}
			else
			{
				UpdateDistanceText(GetDistanceToCamera());
				int num = -1;
				LiveMixin liveMixin = activeCamera.liveMixin;
				if (liveMixin != null)
				{
					num = Mathf.RoundToInt(100f * (liveMixin.health / liveMixin.maxHealth));
				}
				if (health != num)
				{
					health = num;
					textHealth.text = IntStringCache.GetStringForInt(health);
				}
				int num2 = -1;
				EnergyMixin energyMixin = activeCamera.energyMixin;
				if (energyMixin != null)
				{
					num2 = Mathf.RoundToInt(100f * (energyMixin.charge / energyMixin.capacity));
				}
				if (power != num2)
				{
					power = num2;
					textPower.text = IntStringCache.GetStringForInt(power);
				}
			}
		}
		else
		{
			UpdateDistanceText(-1);
			faderSequence.ForceState(state: true);
			noSignal.SetActive(value: true);
			connecting.SetActive(value: false);
		}
		HandReticle.main.SetTextRaw(HandReticle.TextType.Use, stringControls);
		HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, string.Empty);
	}

	public void SetScreen(MapRoomScreen screen)
	{
		activeScreen = screen;
	}

	public MapRoomScreen GetScreen()
	{
		return activeScreen;
	}

	public void SetCamera(MapRoomCamera camera)
	{
		if (!(activeCamera == camera))
		{
			activeCamera = camera;
			if (activeCamera != null)
			{
				waitForCamera = true;
				UpdateCameraTitle();
				UpdateDistanceText(-1);
				Utils.PlayFMODAsset(changeCameraSound);
				faderSequence.ForceState(state: true);
				connecting.SetActive(value: true);
			}
			else
			{
				faderSequence.ForceState(state: false);
			}
			noSignal.SetActive(value: false);
			ping.enabled = false;
		}
	}

	public MapRoomCamera GetCamera()
	{
		return activeCamera;
	}

	private void OnBindingsChanged()
	{
		UpdateBindings();
	}

	private void OnLanguageChanged()
	{
		UpdateTexts();
	}

	private void UpdateTexts()
	{
		UpdateCameraTitle();
		UpdateDistanceText(GetDistanceToCamera());
		UpdateBindings();
		Language language = Language.main;
		stringDistance = language.Get("CameraDroneDistance");
		stringMeterSuffix = language.Get("MeterSuffix");
		textNoSignal.text = language.Get("MapRoomCameraNoSignal");
	}

	private void UpdateCameraTitle()
	{
		string text = string.Empty;
		if (activeCamera != null)
		{
			text = Language.main.GetFormat("CameraDroneTitle", activeCamera.GetCameraNumber());
		}
		textTitle.text = text;
	}

	private void UpdateDistanceText(int newDistance)
	{
		if (distance != newDistance)
		{
			distance = newDistance;
			textDistance.text = string.Format("<color=#6EFEFFFF>{0}</color> <size=26>{1} {2}</size>", stringDistance, (distance >= 0) ? IntStringCache.GetStringForInt(distance) : "--", stringMeterSuffix);
		}
	}

	private void UpdateBindings()
	{
		string arg = GameInput.FormatButton(GameInput.Button.CyclePrev);
		string arg2 = GameInput.FormatButton(GameInput.Button.CycleNext);
		string arg3 = GameInput.FormatButton(GameInput.Button.RightHand);
		string arg4 = GameInput.FormatButton(GameInput.Button.Exit);
		stringControls = Language.main.GetFormat("MapRoomCameraControls", arg, arg2, arg3, arg4);
	}

	private void UpdatePing()
	{
		if (activeCamera == null || !activeCamera.CanBeControlled() || waitForCamera)
		{
			return;
		}
		Camera camera = MainCamera.camera;
		bool flag = false;
		Vector2 anchoredPosition = Vector2.zero;
		if (camera != null && activeScreen != null && activeCamera != null)
		{
			Matrix4x4 worldToLocalMatrix = camera.transform.worldToLocalMatrix;
			float aspect = camera.aspect;
			float num = Mathf.Tan(camera.fieldOfView * 0.5f * ((float)System.Math.PI / 180f));
			Rect rect = pingCanvas.rect;
			float width = rect.width;
			float height = rect.height;
			Vector3 position = activeScreen.transform.position;
			Vector3 vector = new Vector3(worldToLocalMatrix.m00 * position.x + worldToLocalMatrix.m01 * position.y + worldToLocalMatrix.m02 * position.z + worldToLocalMatrix.m03, worldToLocalMatrix.m10 * position.x + worldToLocalMatrix.m11 * position.y + worldToLocalMatrix.m12 * position.z + worldToLocalMatrix.m13, worldToLocalMatrix.m20 * position.x + worldToLocalMatrix.m21 * position.y + worldToLocalMatrix.m22 * position.z + worldToLocalMatrix.m23);
			float num2 = vector.z * num;
			float num3 = num2 * aspect;
			if (vector.z > 3f && Mathf.Abs(vector.x) < num3 * (1f + safeZone.x) && Mathf.Abs(vector.y) < num2 * (1f + safeZone.y))
			{
				Vector2 vector2 = new Vector2(vector.x / num3, vector.y / num2);
				anchoredPosition = new Vector2((vector2.x * 0.5f + 0.5f) * width, (vector2.y * 0.5f + 0.5f) * height);
				flag = true;
			}
		}
		if (flag)
		{
			ping.rectTransform.anchoredPosition = anchoredPosition;
		}
		ping.enabled = flag;
	}

	private int GetDistanceToCamera()
	{
		if (activeCamera == null)
		{
			return -1;
		}
		if (!activeCamera.CanBeControlled())
		{
			return -1;
		}
		if (waitForCamera)
		{
			return -1;
		}
		return Mathf.FloorToInt(activeCamera.GetScreenDistance());
	}
}
