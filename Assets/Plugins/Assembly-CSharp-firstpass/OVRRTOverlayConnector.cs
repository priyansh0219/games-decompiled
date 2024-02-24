using System;
using UnityEngine;
using UnityEngine.XR;

public class OVRRTOverlayConnector : MonoBehaviour
{
	public int alphaBorderSizePixels = 3;

	private const int overlayRTChainSize = 3;

	private int overlayRTIndex;

	private IntPtr[] overlayTexturePtrs = new IntPtr[3];

	private RenderTexture[] overlayRTChain = new RenderTexture[3];

	public GameObject ovrOverlayObj;

	private RenderTexture srcRT;

	private Camera ownerCamera;

	private void ConstructRenderTextureChain()
	{
		for (int i = 0; i < 3; i++)
		{
			overlayRTChain[i] = new RenderTexture(srcRT.width, srcRT.height, 1, srcRT.format, RenderTextureReadWrite.sRGB);
			overlayRTChain[i].antiAliasing = 1;
			overlayRTChain[i].depth = 0;
			overlayRTChain[i].wrapMode = TextureWrapMode.Clamp;
			overlayRTChain[i].hideFlags = HideFlags.HideAndDontSave;
			overlayRTChain[i].Create();
			overlayTexturePtrs[i] = overlayRTChain[i].GetNativeTexturePtr();
		}
	}

	private void Start()
	{
		ownerCamera = GetComponent<Camera>();
		srcRT = ownerCamera.targetTexture;
		ConstructRenderTextureChain();
	}

	private void OnPostRender()
	{
		if ((bool)srcRT)
		{
			Graphics.Blit(srcRT, overlayRTChain[overlayRTIndex]);
			ovrOverlayObj.GetComponent<OVROverlay>().OverrideOverlayTextureInfo(overlayRTChain[overlayRTIndex], overlayTexturePtrs[overlayRTIndex], XRNode.LeftEye);
			overlayRTIndex++;
			overlayRTIndex %= 3;
		}
	}
}
