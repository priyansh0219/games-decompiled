using System;
using UnityEngine;
using UnityEngine.XR;

public class VRLoadingOverlay : MonoBehaviour
{
	private static VRLoadingOverlay instance;

	private bool show;

	private Texture2D texture;

	private IntPtr texId = IntPtr.Zero;

	public static void Show()
	{
		if (XRSettings.enabled && OVRManager.isHmdPresent)
		{
			Create();
			instance.ShowInternal();
		}
	}

	public static void Hide()
	{
		if (XRSettings.enabled && OVRManager.isHmdPresent)
		{
			Create();
			instance.HideInternal();
		}
	}

	private static void Create()
	{
		if (instance == null)
		{
			GameObject obj = new GameObject("VRLoadingOverlay");
			instance = obj.AddComponent<VRLoadingOverlay>();
			UnityEngine.Object.DontDestroyOnLoad(obj);
		}
	}

	private void Awake()
	{
		texture = new Texture2D(4, 4, TextureFormat.RGB24, mipChain: false);
		Color[] pixels = new Color[texture.width * texture.height];
		texture.SetPixels(pixels);
		texture.Apply();
		texId = texture.GetNativeTexturePtr();
	}

	private void ShowInternal()
	{
		show = true;
	}

	private void HideInternal()
	{
		show = false;
		VRUtil.ClearOverlay();
	}

	private void OnRenderObject()
	{
		if (show)
		{
			float distance = 1f;
			VRUtil.ShowOverlay(texId, base.transform.lossyScale, distance);
		}
	}
}
