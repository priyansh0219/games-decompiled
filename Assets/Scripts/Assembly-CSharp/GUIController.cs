using UWE;
using UnityEngine;

public class GUIController : MonoBehaviour
{
	public enum HidePhase
	{
		None = 0,
		Mask = 1,
		HUD = 2,
		MaskHUD = 3,
		All = 4
	}

	public static GUIController main;

	private HidePhase hidePhase;

	private void Awake()
	{
		main = this;
	}

	public HidePhase GetHidePhase()
	{
		return hidePhase;
	}

	private void Update()
	{
		if (!FreezeTime.PleaseWait && Input.GetKeyDown(KeyCode.F6))
		{
			hidePhase++;
			if (hidePhase > HidePhase.All)
			{
				hidePhase = HidePhase.None;
			}
			SetHidePhase(hidePhase);
		}
	}

	public static void SetHidePhase(HidePhase hidePhase)
	{
		switch (hidePhase)
		{
		case HidePhase.None:
			HideForScreenshots.Hide(HideForScreenshots.HideType.None);
			break;
		case HidePhase.Mask:
			HideForScreenshots.Hide(HideForScreenshots.HideType.Mask);
			break;
		case HidePhase.HUD:
			HideForScreenshots.Hide(HideForScreenshots.HideType.HUD);
			break;
		case HidePhase.MaskHUD:
			HideForScreenshots.Hide(HideForScreenshots.HideType.Mask | HideForScreenshots.HideType.HUD);
			break;
		case HidePhase.All:
			HideForScreenshots.Hide(HideForScreenshots.HideType.Mask | HideForScreenshots.HideType.HUD | HideForScreenshots.HideType.ViewModel);
			break;
		default:
			Debug.LogErrorFormat("undefined hide phase {0}", hidePhase);
			break;
		}
	}
}
