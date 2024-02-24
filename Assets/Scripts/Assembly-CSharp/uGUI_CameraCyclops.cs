using System;
using Gendarme;
using TMPro;
using UWE;
using UnityEngine;
using UnityEngine.UI;

[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
public class uGUI_CameraCyclops : MonoBehaviour
{
	public static uGUI_CameraCyclops main;

	[AssertLocalization]
	private const string keyCamera1 = "CyclopsExternalCam1";

	[AssertLocalization]
	private const string keyCamera2 = "CyclopsExternalCam2";

	[AssertLocalization]
	private const string keyCamera3 = "CyclopsExternalCam3";

	[AssertLocalization(4)]
	private const string keyCameraControls = "CyclopsExternalCamControls";

	[AssertNotNull]
	public GameObject content;

	[AssertNotNull]
	public Image fader;

	[AssertNotNull]
	public TextMeshProUGUI textTitle;

	[AssertNotNull]
	public RectTransform arrow;

	public float transitionEffectDuration = 0.2f;

	private string[] cameraNames = new string[3];

	private string stringControls;

	private int cameraIndex = -1;

	private Sequence sequence = new Sequence();

	private void Awake()
	{
		if (main != null)
		{
			UWE.Utils.DestroyWrap(this);
			return;
		}
		main = this;
		sequence.ForceState(state: false);
		content.SetActive(value: false);
	}

	private void OnEnable()
	{
		UpdateTexts();
		GameInput.OnBindingsChanged += OnBindingsChanged;
		Language.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDisable()
	{
		GameInput.OnBindingsChanged -= OnBindingsChanged;
		Language.OnLanguageChanged -= OnLanguageChanged;
	}

	private void Update()
	{
		if (sequence.active)
		{
			sequence.Update();
			if (Player.main != null)
			{
				float a = 0.5f * (1f - Mathf.Cos((float)System.Math.PI * sequence.t));
				Color color = fader.color;
				color.a = a;
				fader.color = color;
			}
		}
		if (content.activeSelf)
		{
			HandReticle.main.SetTextRaw(HandReticle.TextType.Use, stringControls);
			HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, string.Empty);
		}
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
		UpdateBindings();
		cameraNames[0] = Language.main.Get("CyclopsExternalCam1");
		cameraNames[1] = Language.main.Get("CyclopsExternalCam2");
		cameraNames[2] = Language.main.Get("CyclopsExternalCam3");
	}

	private void UpdateBindings()
	{
		string arg = GameInput.FormatButton(GameInput.Button.CyclePrev);
		string arg2 = GameInput.FormatButton(GameInput.Button.CycleNext);
		string arg3 = GameInput.FormatButton(GameInput.Button.LeftHand);
		string arg4 = GameInput.FormatButton(CyclopsExternalCams.buttonsExit[0]);
		stringControls = Language.main.GetFormat("CyclopsExternalCamControls", arg, arg2, arg3, arg4);
	}

	public void SetCamera(int index)
	{
		if (cameraIndex != index)
		{
			cameraIndex = index;
			if (cameraIndex >= 0 && cameraIndex < cameraNames.Length)
			{
				textTitle.text = cameraNames[cameraIndex];
			}
			else
			{
				textTitle.text = string.Empty;
			}
			bool flag = cameraIndex >= 0;
			if (flag)
			{
				sequence.ForceState(state: true);
				sequence.Set(transitionEffectDuration, target: false);
			}
			else
			{
				sequence.ForceState(state: false);
			}
			content.SetActive(flag);
		}
	}

	public void SetDirection(float angle)
	{
		arrow.localRotation = Quaternion.Euler(0f, 0f, 0f - angle);
	}
}
