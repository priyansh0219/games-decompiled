using UnityEngine;

public class StartScreenFade : MonoBehaviour
{
	public string menuEnviornmentWaterScapeName;

	public Texture splashImageOnFirstEntry;

	public Texture splashImageOnSubsequentEntries;

	private Texture currentSplashScreen;

	private Texture2D singlePixel;

	private bool fadeInStarted;

	private float elapsedFadeTime;

	private float fadeTimeInv;

	private bool fadingInDone;

	private float overlayFadeValue;

	private Camera camera;

	private void Awake()
	{
		base.useGUILayout = false;
	}

	private void Start()
	{
		if (currentSplashScreen == null)
		{
			currentSplashScreen = splashImageOnFirstEntry;
		}
		else
		{
			currentSplashScreen = splashImageOnSubsequentEntries;
		}
		camera = MainCamera.camera;
		singlePixel = new Texture2D(1, 1, TextureFormat.RGB24, mipChain: false);
	}

	public void StartToFade(float fadeTime)
	{
		if (!fadeInStarted)
		{
			fadeInStarted = true;
			elapsedFadeTime = 0f;
			fadeTimeInv = 1f / fadeTime;
		}
	}

	private void Update()
	{
		if (fadeInStarted && !fadingInDone)
		{
			elapsedFadeTime += Time.unscaledDeltaTime;
			overlayFadeValue = Mathf.Lerp(0f, 1f, elapsedFadeTime * fadeTimeInv);
			if (overlayFadeValue == 1f)
			{
				fadingInDone = true;
			}
		}
	}

	private void OnGUI()
	{
		if (!fadingInDone)
		{
			Color color = GUI.color;
			GUI.color = new Color(0f, 0f, 0f, 1f - overlayFadeValue);
			GUI.DrawTexture(new Rect(0f, 0f, camera.pixelWidth, camera.pixelHeight), singlePixel, ScaleMode.StretchToFill);
			GUI.color = new Color(1f, 1f, 1f, 1f - overlayFadeValue);
			GUI.DrawTexture(new Rect(0f, 0f, camera.pixelWidth, camera.pixelHeight), currentSplashScreen, ScaleMode.ScaleAndCrop);
			GUI.color = color;
		}
	}
}
