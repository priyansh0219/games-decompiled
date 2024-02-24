using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class VFXScanning : MonoBehaviour
{
	public Material scanMaterial;

	private Dictionary<Renderer, int> renderersToScan;

	private Dictionary<Camera, CommandBuffer> m_Cameras = new Dictionary<Camera, CommandBuffer>();

	public void UpdateRenderersToScan()
	{
		renderersToScan = new Dictionary<Renderer, int>();
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].sharedMesh != null && componentsInChildren[i].isVisible)
			{
				renderersToScan[componentsInChildren[i]] = componentsInChildren[i].sharedMesh.subMeshCount;
			}
		}
		Renderer[] componentsInChildren2 = GetComponentsInChildren<Renderer>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			MeshFilter component = componentsInChildren2[j].GetComponent<MeshFilter>();
			if (component != null && component.sharedMesh != null && componentsInChildren2[j].isVisible)
			{
				renderersToScan[componentsInChildren2[j]] = component.sharedMesh.subMeshCount;
			}
		}
	}

	public void OnDestroy()
	{
		foreach (KeyValuePair<Camera, CommandBuffer> camera in m_Cameras)
		{
			if (camera.Key != null)
			{
				camera.Key.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, camera.Value);
			}
		}
	}

	public void StartScan(Material mat)
	{
		UpdateRenderersToScan();
		Camera camera = MainCamera.camera;
		if (!camera)
		{
			return;
		}
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.name = "Scanner Effect";
		m_Cameras[camera] = commandBuffer;
		commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget);
		foreach (KeyValuePair<Renderer, int> item in renderersToScan)
		{
			for (int i = 0; i < item.Value; i++)
			{
				commandBuffer.DrawRenderer(item.Key, mat, i, -1);
			}
		}
		camera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBuffer);
	}
}
