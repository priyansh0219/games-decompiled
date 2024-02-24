using System;
using UnityEngine;
using UnityEngine.XR;

public static class VRUtil
{
	public enum SDK
	{
		None = 0,
		Oculus = 1,
		OpenVR = 2
	}

	public static event Action OnRecenter;

	public static void Recenter()
	{
		if (XRSettings.enabled)
		{
			InputTracking.Recenter();
			if (VRUtil.OnRecenter != null)
			{
				VRUtil.OnRecenter();
			}
		}
	}

	public static SDK GetLoadedSDK()
	{
		if (XRSettings.loadedDeviceName == "Oculus")
		{
			return SDK.Oculus;
		}
		if (XRSettings.loadedDeviceName == "OpenVR")
		{
			return SDK.OpenVR;
		}
		return SDK.None;
	}

	public static bool GetAudioDeviceGuid(out Guid guid)
	{
		if (XRSettings.enabled && OVRPlugin.headphonesPresent)
		{
			string audioOutId = OVRPlugin.audioOutId;
			guid = new Guid(audioOutId);
			return true;
		}
		guid = default(Guid);
		return false;
	}

	public static void ShowOverlay(IntPtr texId, Vector3 scale, float distance)
	{
		OVRPose identity = OVRPose.identity;
		identity.position += new Vector3(0f, 0f, distance);
		for (int i = 0; i < 3; i++)
		{
			scale[i] /= Camera.current.transform.lossyScale[i];
		}
		OVRPlugin.Sizei eyeTextureSize = OVRPlugin.GetEyeTextureSize(OVRPlugin.Eye.Left);
		float num = (float)eyeTextureSize.w / (float)eyeTextureSize.h;
		scale = new Vector3(1f, 1f / num, 1f);
		scale *= 3f * distance;
		OVRPlugin.SetOverlayQuad(onTop: true, headLocked: true, texId, texId, IntPtr.Zero, identity.flipZ().ToPosef(), scale.ToVector3f());
	}

	public static void ClearOverlay()
	{
		OVRPlugin.SetOverlayQuad(onTop: true, headLocked: true, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, OVRPose.identity.ToPosef(), Vector3.one.ToVector3f());
	}
}
