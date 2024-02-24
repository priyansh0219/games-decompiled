using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class CullingCamera : MonoBehaviour
{
	[SerializeField]
	private Camera camera;

	[SerializeField]
	private Material depthOnlyMaterial;

	[SerializeField]
	private Material mipDownsampleMaterial;

	[SerializeField]
	private ComputeShader computeShader;

	[SerializeField]
	private Material blitMaterial;

	[SerializeField]
	private WBOIT wboit;

	private int numRenderersCulled;

	private CullingOccluderList occluderList;

	private CullingOccludeeList occludeeList;

	private RenderTexture[] downsampledDepthTextures;

	private const int maxOccludees = 3000;

	private const int depthTextureWidth = 512;

	private const int depthTextureHeight = 256;

	private ComputeBuffer boundingBoxesBuffer;

	private ComputeBuffer visibilityResultsBuffer;

	private int computeShaderKernelId;

	private bool waitingOnResults;

	private bool requireExtraOcclusionPass;

	public static CullingCamera main;

	private Material mipDownsampleMaterialInstance;

	public bool supported => false;

	private void Awake()
	{
		main = this;
	}

	private void OnDisable()
	{
		main = null;
	}

	private void Start()
	{
		if (CommandLine.GetFlag("-nocull"))
		{
			base.enabled = false;
			return;
		}
		if (!supported)
		{
			base.enabled = false;
			return;
		}
		GenerateDepthTextureMipChain();
		boundingBoxesBuffer = new ComputeBuffer(3000, 24);
		visibilityResultsBuffer = new ComputeBuffer(3000, 4);
		occludeeList = new CullingOccludeeList(3000);
		occluderList = new CullingOccluderList();
		computeShaderKernelId = computeShader.FindKernel("CSMain");
		computeShader.SetBuffer(computeShaderKernelId, ShaderPropertyID._BoundingBoxes, boundingBoxesBuffer);
		computeShader.SetBuffer(computeShaderKernelId, ShaderPropertyID._VisibilityResults, visibilityResultsBuffer);
		computeShader.SetTexture(computeShaderKernelId, ShaderPropertyID._DepthTexture, downsampledDepthTextures[0]);
		mipDownsampleMaterialInstance = new Material(mipDownsampleMaterial);
	}

	private bool RenderOccluders()
	{
		if (occluderList.Count == 0 && !requireExtraOcclusionPass)
		{
			return false;
		}
		RenderBuffer activeColorBuffer = Graphics.activeColorBuffer;
		RenderBuffer activeDepthBuffer = Graphics.activeDepthBuffer;
		Graphics.SetRenderTarget(downsampledDepthTextures[0].colorBuffer, downsampledDepthTextures[0].depthBuffer);
		GL.Clear(clearDepth: true, clearColor: true, Color.white);
		GL.PushMatrix();
		GL.LoadIdentity();
		GL.Viewport(new Rect(0f, 0f, 512f, 256f));
		GL.LoadProjectionMatrix(Matrix4x4.Perspective(camera.fieldOfView, 2f, camera.nearClipPlane, camera.farClipPlane));
		depthOnlyMaterial.SetVector(ShaderPropertyID._CameraClipPlanes, new Vector4(camera.nearClipPlane, camera.farClipPlane));
		depthOnlyMaterial.SetPass(0);
		bool result = false;
		Plane[] sharedFrustumPlanes = MainCamera.camera.GetSharedFrustumPlanes();
		for (int i = 0; i < occluderList.Count; i++)
		{
			CullingOccluder cullingOccluder = occluderList[i];
			if (cullingOccluder.occluderRenderer.enabled && GeometryUtility.TestPlanesAABB(sharedFrustumPlanes, cullingOccluder.worldBounds))
			{
				Matrix4x4 localToWorldMatrix = cullingOccluder.transform.localToWorldMatrix;
				Graphics.DrawMeshNow(cullingOccluder.meshFilter.sharedMesh, localToWorldMatrix);
				result = true;
			}
		}
		GL.PopMatrix();
		if (camera.enabled)
		{
			Graphics.SetRenderTarget(activeColorBuffer, activeDepthBuffer);
		}
		return result;
	}

	private void GenerateDepthTextureMipChain()
	{
		downsampledDepthTextures = new RenderTexture[10];
		int num = 512;
		int num2 = 256;
		for (int i = 0; i < 10; i++)
		{
			downsampledDepthTextures[i] = new RenderTexture(Mathf.Max(num, 1), Mathf.Max(num2, 1), (i == 0) ? 32 : 0);
			downsampledDepthTextures[i].useMipMap = i == 0;
			downsampledDepthTextures[i].autoGenerateMips = false;
			downsampledDepthTextures[i].filterMode = FilterMode.Point;
			num /= 2;
			num2 /= 2;
		}
	}

	private void DownsampleDepthTexture()
	{
		for (int i = 0; i < downsampledDepthTextures.Length - 1; i++)
		{
			RenderTexture renderTexture = downsampledDepthTextures[i];
			RenderTexture renderTexture2 = downsampledDepthTextures[i + 1];
			mipDownsampleMaterialInstance.SetVector(ShaderPropertyID._MipInfo, new Vector4(renderTexture.width, renderTexture.height, 0f, 0f));
			GL.Viewport(new Rect(0f, 0f, renderTexture2.width, renderTexture2.height));
			Graphics.SetRenderTarget(renderTexture2);
			GL.Clear(clearDepth: true, clearColor: true, Color.white);
			Graphics.Blit(renderTexture, renderTexture2, mipDownsampleMaterialInstance);
			Graphics.SetRenderTarget(downsampledDepthTextures[0], i + 1);
			Graphics.Blit(renderTexture2, blitMaterial);
		}
	}

	private void GatherBoundingBoxes()
	{
		occludeeList.UpdateDynamicBounds();
		boundingBoxesBuffer.SetData(occludeeList.boundingBoxes, 0, 0, occludeeList.maxBufferSizeRequired);
	}

	private void ExecuteComputeShader()
	{
		computeShader.SetMatrix(ShaderPropertyID._WorldToScreenMatrix, camera.projectionMatrix * camera.worldToCameraMatrix);
		computeShader.SetVector(ShaderPropertyID._CameraInfo, new Vector4(camera.pixelWidth, camera.pixelHeight, camera.nearClipPlane, camera.farClipPlane));
		computeShader.Dispatch(computeShaderKernelId, Mathf.CeilToInt(46.875f), 1, 1);
		AsyncGPUReadback.Request(visibilityResultsBuffer, OnComputeShaderResultsReady);
		SetWaitingOnResults(waiting: true);
	}

	private void OnComputeShaderResultsReady(AsyncGPUReadbackRequest request)
	{
		if (camera == null)
		{
			return;
		}
		try
		{
			NativeArray<CullingGPUDataStructures.OcclusionResult> data = request.GetData<CullingGPUDataStructures.OcclusionResult>();
			ApplyComputeShaderResults(data);
		}
		finally
		{
			SetWaitingOnResults(waiting: false);
		}
	}

	private void SetWaitingOnResults(bool waiting)
	{
		waitingOnResults = waiting;
		if (waiting)
		{
			occludeeList.Lock();
		}
		else
		{
			occludeeList.Unlock();
		}
	}

	private void ApplyComputeShaderResults(NativeArray<CullingGPUDataStructures.OcclusionResult> results)
	{
		numRenderersCulled = 0;
		for (int i = 0; i < occludeeList.usedList.Count; i++)
		{
			CullingOccludee obj = occludeeList.occludees[occludeeList.usedList[i]];
			int computeBufferPosition = obj.computeBufferPosition;
			bool flag = results[computeBufferPosition].visible > 0f;
			obj.SetVisible(flag);
			if (!flag)
			{
				numRenderersCulled++;
			}
		}
	}

	private void OnPreRender()
	{
		if (!waitingOnResults && occludeeList.Count != 0)
		{
			bool anyOccludersRendered = RenderOccluders();
			if (!CanSkipOcclusion(anyOccludersRendered))
			{
				DownsampleDepthTexture();
				GatherBoundingBoxes();
				ExecuteComputeShader();
			}
		}
	}

	public void DebugRender(CommandBuffer cb)
	{
		cb.Blit(downsampledDepthTextures[0], BuiltinRenderTextureType.CameraTarget);
	}

	private bool CanSkipOcclusion(bool anyOccludersRendered)
	{
		if (anyOccludersRendered)
		{
			requireExtraOcclusionPass = true;
			return false;
		}
		if (requireExtraOcclusionPass)
		{
			requireExtraOcclusionPass = false;
			return false;
		}
		return true;
	}

	public void RegisterOccludee(CullingOccludee occludee)
	{
		if (supported)
		{
			occludeeList.Add(occludee);
		}
	}

	public void DeregisterOccludee(CullingOccludee occludee)
	{
		if (supported)
		{
			occludeeList.Remove(occludee);
		}
	}

	public void RegisterOccluder(CullingOccluder occluder)
	{
		if (supported)
		{
			occluderList.Add(occluder);
		}
	}

	public void DeregisterOccluder(CullingOccluder occluder)
	{
		if (supported)
		{
			occluderList.Remove(occluder);
		}
	}

	private void OnDestroy()
	{
		if (boundingBoxesBuffer != null)
		{
			boundingBoxesBuffer.Dispose();
		}
		if (visibilityResultsBuffer != null)
		{
			visibilityResultsBuffer.Dispose();
		}
	}
}
