using FMOD;
using FMODUnity;
using UWE;
using UnityEngine;
using UnityEngine.XR;

public class ApplicationFocus : MonoBehaviour
{
	private const FreezeTime.Id freezerId = FreezeTime.Id.ApplicationFocus;

	private static bool frozen;

	private void OnApplicationFocus(bool hasFocus)
	{
		UpdateState();
	}

	public static void OnFocusChanged()
	{
		UpdateState();
	}

	public static void OnRunInBackgroundChanged()
	{
		UpdateState();
	}

	private static void UpdateState()
	{
		bool flag = Application.isFocused;
		if (XRSettings.enabled)
		{
			if (OVRManager.instance != null)
			{
				flag &= OVRManager.hasVrFocus;
			}
			VRUtil.GetLoadedSDK();
		}
		bool flag2 = !flag && !MiscSettings.runInBackground;
		if (frozen == flag2)
		{
			return;
		}
		frozen = flag2;
		if (frozen)
		{
			FreezeTime.Begin(FreezeTime.Id.ApplicationFocus);
		}
		else
		{
			FreezeTime.End(FreezeTime.Id.ApplicationFocus);
		}
		if (RuntimeManager.StudioSystem.isValid())
		{
			RuntimeManager.PauseAllEvents(frozen);
			FMOD.System coreSystem = RuntimeManager.CoreSystem;
			if (frozen)
			{
				coreSystem.mixerSuspend();
			}
			else
			{
				coreSystem.mixerResume();
			}
		}
	}
}
