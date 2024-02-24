using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndCreditsManager : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	private enum Phase
	{
		None = 0,
		Logo = 1,
		Credits = 2,
		Easter = 3,
		End = 4
	}

	private const GameInput.Button skipButton = GameInput.Button.UIMenu;

	[AssertLocalization(1)]
	private const string skipCreditsKey = "ExitCredits";

	private const ManagedUpdate.Queue queueLayoutComplete = ManagedUpdate.Queue.UILayoutComplete;

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.LateUpdate;

	public static bool showEaster;

	[AssertNotNull]
	public Image logo;

	[AssertNotNull]
	public TextMeshProUGUI textField;

	[AssertNotNull]
	public FMOD_CustomEmitter endMusic;

	[AssertNotNull]
	public GameObject easterEggHolder;

	[AssertNotNull]
	public FMODAsset easterEggVO;

	[AssertNotNull]
	public TextMeshProUGUI skipText;

	public AnimationCurve fadeCurve;

	public float scrollSpeed;

	public float scrollStep;

	[AssertNotNull]
	public TextAsset creditsTextAsset;

	[AssertNotNull]
	public TextAsset ps4CreditsTextAsset;

	[AssertNotNull]
	public TextAsset switchCreditsTextAsset;

	private RectTransform rt;

	private Phase phase;

	private float phaseStartTime;

	private float contentHeight;

	private float hintStartTime;

	private void Start()
	{
		rt = textField.rectTransform;
		TextAsset textAsset = creditsTextAsset;
		textField.SetText(textAsset.text);
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UILayoutComplete, OnLayoutComplete);
		UpdateBindings();
		GameInput.OnBindingsChanged += UpdateBindings;
		phase = Phase.Logo;
		phaseStartTime = (hintStartTime = Time.unscaledTime);
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdate, OnLateUpdate);
	}

	private void OnDestroy()
	{
		GameInput.OnBindingsChanged -= UpdateBindings;
	}

	private void OnLateUpdate()
	{
		float unscaledTime = Time.unscaledTime;
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		switch (phase)
		{
		case Phase.Logo:
		{
			float num2 = unscaledTime - phaseStartTime;
			float time = fadeCurve[fadeCurve.length - 1].time;
			float alpha = fadeCurve.Evaluate(num2);
			logo.canvasRenderer.SetAlpha(alpha);
			if (num2 > 0.5f && !endMusic.playing)
			{
				endMusic.Play();
			}
			if (num2 >= time)
			{
				logo.canvasRenderer.SetAlpha(0f);
				phase = Phase.Credits;
				rt.anchoredPosition = Vector2.zero;
			}
			break;
		}
		case Phase.Credits:
		{
			Vector2 anchoredPosition = rt.anchoredPosition;
			anchoredPosition.y += scrollSpeed * unscaledDeltaTime;
			rt.anchoredPosition = anchoredPosition;
			if (anchoredPosition.y >= GetScreenHeight() + contentHeight)
			{
				phase = Phase.Easter;
				phaseStartTime = unscaledTime;
			}
			break;
		}
		case Phase.Easter:
			if (showEaster)
			{
				showEaster = false;
				float num = (float)FMODExtensions.GetLength(easterEggVO.path) / 1000f + 1f;
				phaseStartTime = unscaledTime + num;
				easterEggHolder.SetActive(value: true);
				RuntimeManager.PlayOneShot(easterEggVO.path);
			}
			if (unscaledTime - phaseStartTime >= 0f)
			{
				phase = Phase.End;
				phaseStartTime = unscaledTime;
			}
			break;
		case Phase.End:
			phase = Phase.None;
			ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.LateUpdate, OnLateUpdate);
			AddressablesUtility.LoadScene("Cleaner", LoadSceneMode.Single);
			break;
		}
		if (phase != 0)
		{
			if (GameInput.AnyKeyDown)
			{
				hintStartTime = unscaledTime - 3.2f;
			}
			float t = unscaledTime - hintStartTime;
			float a = MathExtensions.Trapezoid(3f, 0.2f, 3f, 2f, t, wrap: false);
			Color color = skipText.color;
			color.a = a;
			skipText.color = color;
			if (phase != Phase.End && GameInput.GetButtonHeld(GameInput.Button.UIMenu) && GameInput.GetButtonHeldTime(GameInput.Button.UIMenu) > 1f)
			{
				phase = Phase.End;
			}
		}
	}

	private void OnLayoutComplete()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UILayoutComplete, OnLayoutComplete);
		contentHeight = textField.preferredHeight;
	}

	private float GetScreenHeight()
	{
		return ((RectTransform)rt.parent).rect.height;
	}

	private void UpdateBindings()
	{
		string buttonFormat = LanguageCache.GetButtonFormat("ExitCredits", GameInput.Button.UIMenu);
		skipText.SetText(buttonFormat);
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (phase == Phase.Credits)
		{
			float num = Mathf.Sign(eventData.scrollDelta.y) * scrollStep;
			Vector2 anchoredPosition = rt.anchoredPosition;
			anchoredPosition.y = Mathf.Clamp(anchoredPosition.y - num, 0f, GetScreenHeight() + contentHeight);
			rt.anchoredPosition = anchoredPosition;
		}
	}
}
