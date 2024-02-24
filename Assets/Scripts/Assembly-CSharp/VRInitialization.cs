using Oculus.Platform;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

public class VRInitialization : MonoBehaviour
{
	private void EntitlementCallback(Message msg)
	{
		if (msg.IsError)
		{
			Debug.Log("Oculus Home entitlement not found. Exiting.");
			Application.Quit();
		}
		else
		{
			Debug.Log("Oculus Home entitlement found.");
		}
	}

	private void InitializeOVR()
	{
		if (OVRManager.instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (XRSettings.enabled)
		{
			base.gameObject.AddComponent<OVRManager>();
			VRUtil.Recenter();
			OVRManager.VrFocusAcquired += ApplicationFocus.OnFocusChanged;
			OVRManager.VrFocusLost += ApplicationFocus.OnFocusChanged;
			Core.Initialize("993520564054974");
		}
		OVRManager.AudioOutChanged += OnAudioOutChanged;
	}

	private void InitializeSteamVR()
	{
		OpenVR.Compositor.SetTrackingSpace(ETrackingUniverseOrigin.TrackingUniverseSeated);
	}

	private void Awake()
	{
		switch (VRUtil.GetLoadedSDK())
		{
		case VRUtil.SDK.Oculus:
			InitializeOVR();
			break;
		case VRUtil.SDK.OpenVR:
			InitializeSteamVR();
			break;
		}
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Update()
	{
		if (OVRManager.instance != null)
		{
			Request.RunCallbacks();
		}
	}

	private void OnAudioOutChanged()
	{
		SoundSystem.SetDevice(SoundSystem.GetDefaultDevice());
	}
}
