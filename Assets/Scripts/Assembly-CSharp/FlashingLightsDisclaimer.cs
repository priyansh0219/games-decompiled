using System.Collections;
using TMPro;
using UnityEngine;

public class FlashingLightsDisclaimer : MonoBehaviour
{
	private static bool isFirstRun = true;

	public const float fadeDuration = 0.6f;

	[SerializeField]
	[AssertNotNull]
	private CanvasGroup canvasGroup;

	[SerializeField]
	[AssertNotNull]
	private GameObject textObject;

	[SerializeField]
	[AssertNotNull]
	private TextMeshProUGUI text;

	private float beginFade = -1f;

	[AssertLocalization]
	private const string flashingLightsDisclaimer = "FlashingLightsWarning";

	private float _showStartTime = -1f;

	private static void GameStarted()
	{
		isFirstRun = false;
	}

	public static bool CanShow()
	{
		return isFirstRun;
	}

	public void TryToShow()
	{
		bool flag = isFirstRun;
		base.gameObject.SetActive(flag);
		GameStarted();
		if (flag)
		{
			StartCoroutine(TrackShowStartTime());
		}
	}

	public bool IsShown()
	{
		return base.gameObject.activeSelf;
	}

	public void StartHidingDisclaimer()
	{
		GameStarted();
		if (base.gameObject.activeSelf && beginFade < 0f)
		{
			beginFade = Time.unscaledTime;
		}
	}

	private void OnEnable()
	{
		Language.OnLanguageChanged += SetText;
	}

	private void OnDisable()
	{
		Language.OnLanguageChanged -= SetText;
	}

	private void Update()
	{
		if (!(beginFade < 0f))
		{
			float unscaledTime = Time.unscaledTime;
			float num = 1f - (unscaledTime - beginFade) / 0.6f;
			canvasGroup.alpha = MathExtensions.EaseInOutSine(Mathf.Clamp01(num));
			if (num < 0f)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}

	private void Start()
	{
		SetText();
		textObject.SetActive(value: true);
	}

	private void SetText()
	{
		text.text = Language.main.Get("FlashingLightsWarning");
	}

	public float GetShowTime()
	{
		if (!(_showStartTime >= 0f))
		{
			return 0f;
		}
		return Time.realtimeSinceStartup - _showStartTime;
	}

	private IEnumerator TrackShowStartTime()
	{
		yield return new WaitForEndOfFrame();
		_showStartTime = Time.realtimeSinceStartup;
	}
}
