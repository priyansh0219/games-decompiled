using UnityEngine;

public class uGUI_AdaptiveBrightness : MonoBehaviour
{
	[Range(0f, 1f)]
	public float dayBrightness = 0.9f;

	[Range(0f, 1f)]
	public float nightBrightness = 0.2f;

	private CanvasGroup canvasGroup;

	private float lightness = 1f;

	private float lerpSpeed = 5f;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		if (canvasGroup == null)
		{
			canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}
		canvasGroup.alpha = 0f;
	}

	private void LateUpdate()
	{
		lightness = Mathf.Lerp(lightness, GetLightness(), Time.deltaTime * lerpSpeed);
		canvasGroup.alpha = Mathf.Lerp(nightBrightness, dayBrightness, lightness);
	}

	public static float GetLightness()
	{
		Player main = Player.main;
		if (main == null || !main.IsInsideWalkable())
		{
			DayNightCycle main2 = DayNightCycle.main;
			if (main2 != null)
			{
				return main2.GetLocalLightScalar();
			}
		}
		return 1f;
	}
}
