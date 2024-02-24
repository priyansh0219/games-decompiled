using UWE;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class uGUI_SceneLoading : uGUI_Scene
{
	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.UIPreLayout;

	public const float fadeInTime = 0.3f;

	[AssertNotNull]
	public Image loadingBar;

	[AssertNotNull]
	public uGUI_Fader loadingBackground;

	public bool debug;

	[Range(0f, 1f)]
	public float debugProgress;

	private Material materialBar;

	private float progress;

	private bool isLoading;

	private void Awake()
	{
		materialBar = new Material(loadingBar.material);
		loadingBar.material = materialBar;
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UIPreLayout, OnPreLayout);
	}

	private void OnDestroy()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UIPreLayout, OnPreLayout);
		UWE.Utils.DestroyWrap(materialBar);
	}

	private void OnPreLayout()
	{
		bool isWaiting = WaitScreen.IsWaiting;
		if (isLoading != isWaiting)
		{
			isLoading = isWaiting;
			if (isLoading)
			{
				SetProgress(0f);
				loadingBackground.FadeIn(0.3f, null);
			}
			else
			{
				SetProgress(1f);
				if (!XRSettings.enabled)
				{
					loadingBackground.FadeOut();
				}
				else
				{
					loadingBackground.SetState(enabled: false);
				}
			}
			CanvasGroup canvasGroup = loadingBackground.canvasGroup;
			canvasGroup.interactable = isLoading;
			canvasGroup.blocksRaycasts = isLoading;
		}
		if (isLoading || debug)
		{
			SetProgress(debug ? debugProgress : WaitScreen.CalcProgress());
		}
		if (loadingBackground.canvasGroup.alpha > 0f)
		{
			materialBar.SetFloat(ShaderPropertyID._Amount, progress);
		}
	}

	private void SetProgress(float value)
	{
		progress = Mathf.Clamp01(value);
	}
}
