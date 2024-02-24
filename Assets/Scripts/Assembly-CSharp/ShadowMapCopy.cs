using UnityEngine;
using UnityEngine.Rendering;

public class ShadowMapCopy : MonoBehaviour
{
	private CommandBuffer afterShadowCommandBuffer;

	private void Start()
	{
		afterShadowCommandBuffer = new CommandBuffer();
		afterShadowCommandBuffer.name = "ShadowMapCopy";
		afterShadowCommandBuffer.SetGlobalTexture("_UweSunShadowMap", new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive));
		Light component = GetComponent<Light>();
		if ((bool)component)
		{
			component.AddCommandBuffer(LightEvent.AfterShadowMap, afterShadowCommandBuffer);
		}
	}

	private void OnDestroy()
	{
		if (afterShadowCommandBuffer != null)
		{
			Light component = GetComponent<Light>();
			if ((bool)component)
			{
				component.RemoveCommandBuffer(LightEvent.AfterShadowMap, afterShadowCommandBuffer);
			}
		}
	}
}
