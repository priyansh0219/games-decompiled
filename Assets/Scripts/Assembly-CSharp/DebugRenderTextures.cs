using System;
using System.Collections.Generic;
using UWE;
using UnityEngine;
using UnityEngine.Rendering;

public class DebugRenderTextures : MonoBehaviour
{
	public enum Mode
	{
		None = 0,
		Diffuse = 1,
		Occlusion = 2,
		Specular = 3,
		Smoothness = 4,
		Normals = 5,
		NormalsAlpha = 6,
		Emission = 7,
		EmissionAlpha = 8
	}

	private Dictionary<Mode, string> shaderKeywords = new Dictionary<Mode, string>
	{
		{
			Mode.Diffuse,
			"MODE_DIFFUSE"
		},
		{
			Mode.Occlusion,
			"MODE_OCCLUSION"
		},
		{
			Mode.Specular,
			"MODE_SPECULAR"
		},
		{
			Mode.Smoothness,
			"MODE_SMOOTHNESS"
		},
		{
			Mode.Normals,
			"MODE_NORMALS"
		},
		{
			Mode.NormalsAlpha,
			"MODE_NORMALS_ALPHA"
		},
		{
			Mode.Emission,
			"MODE_EMISSION"
		},
		{
			Mode.EmissionAlpha,
			"MODE_EMISSION_ALPHA"
		}
	};

	private const string commandName = "debugrt";

	private const string shaderPath = "Debug/DebugRenderTextures";

	private const CameraEvent cameraEvent = CameraEvent.AfterGBuffer;

	private static DebugRenderTextures singleton;

	private Mode[] allModes;

	private CachedEnumString<Mode> modeNames;

	private Material material;

	private Mode mode;

	private float scale = 0.35f;

	private float alpha = 1f;

	private Camera cachedCamera;

	private CommandBuffer commandBuffer;

	private RenderTexture renderTexture;

	private RenderTargetIdentifier renderTextureId;

	private RenderTextureDescriptor cachedDescriptor;

	private string info;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void Initialize()
	{
		DevConsole.RegisterConsoleCommand("debugrt", OnConsoleCommand_debugrt);
	}

	private static void OnConsoleCommand_debugrt(NotificationCenter.Notification n)
	{
		if (singleton == null)
		{
			GameObject obj = new GameObject("DebugRenderTextures")
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			UnityEngine.Object.DontDestroyOnLoad(obj);
			singleton = obj.AddComponent<DebugRenderTextures>();
		}
		singleton.Toggle(n);
	}

	private void Awake()
	{
		allModes = (Mode[])Enum.GetValues(typeof(Mode));
		modeNames = new CachedEnumString<Mode>(EqualityComparer<Mode>.Default);
		Shader shader = Resources.Load<Shader>("Debug/DebugRenderTextures");
		if (shader == null)
		{
			Debug.LogErrorFormat("Unable to load shader from resources at path {0}!", "Debug/DebugRenderTextures");
		}
		material = new Material(shader);
		material.SetVector(ShaderPropertyID._Position, new Vector4(1f, 1f, 0f, 0f));
		material.SetFloat(ShaderPropertyID._Scale, scale);
		material.SetFloat(ShaderPropertyID._Alpha, alpha);
		base.useGUILayout = false;
	}

	private void DoGUI()
	{
		if (renderTexture != null && Event.current.type == EventType.Repaint && cachedCamera != null)
		{
			Graphics.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), renderTexture, material);
			Dbg.WriteRaw(TextAnchor.UpperRight, new Vector2(10f, 10f), info);
		}
	}

	private void OnDestroy()
	{
		DestroyRenderTexture();
		if (material != null)
		{
			UnityEngine.Object.Destroy(material);
			material = null;
		}
		RemoveCommandBufferFromCamera();
		if (commandBuffer != null)
		{
			commandBuffer.Release();
			commandBuffer = null;
		}
		renderTextureId = default(RenderTargetIdentifier);
		cachedDescriptor = default(RenderTextureDescriptor);
	}

	private void Toggle(NotificationCenter.Notification n)
	{
		if (n.data != null && n.data.Count > 0 && UWE.Utils.TryParseEnum<Mode>((string)n.data[0], out var result))
		{
			SetMode(result);
			if (DevConsole.ParseFloat(n, 1, out var value, scale))
			{
				SetScale(value);
			}
			if (DevConsole.ParseFloat(n, 2, out var value2, alpha))
			{
				SetAlpha(value2);
			}
			ErrorMessage.AddError($"DebugRT mode now {mode} ({(int)mode})");
		}
		else
		{
			ErrorMessage.AddError(string.Format("Usage: {0} mode [scale] [alpha], where mode is {1}", "debugrt", string.Join(", ", modeNames.Names)));
		}
	}

	private void SetMode(Mode value)
	{
		if (mode != value && Array.IndexOf(allModes, value) >= 0)
		{
			if (shaderKeywords.TryGetValue(mode, out var value2))
			{
				material.DisableKeyword(value2);
			}
			if (shaderKeywords.TryGetValue(value, out value2))
			{
				material.EnableKeyword(value2);
			}
			if (value == Mode.None)
			{
				ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.OnGUI, DoGUI);
				Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(PreRender));
				RemoveCommandBufferFromCamera();
				DestroyRenderTexture();
			}
			else if (mode == Mode.None)
			{
				Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(PreRender));
				ManagedUpdate.Subscribe(ManagedUpdate.Queue.OnGUI, DoGUI);
			}
			mode = value;
			UpdateCommandBuffer();
			UpdateInfo();
		}
	}

	private void SetScale(float value)
	{
		value = Mathf.Clamp(value, 0.001f, 1f);
		if (!Mathf.Approximately(scale, value))
		{
			scale = value;
			material.SetFloat(ShaderPropertyID._Scale, scale);
		}
	}

	private void SetAlpha(float value)
	{
		value = Mathf.Clamp(value, 0f, 1f);
		if (!Mathf.Approximately(alpha, value))
		{
			alpha = value;
			material.SetFloat(ShaderPropertyID._Alpha, alpha);
		}
	}

	private void PreRender(Camera camera)
	{
		if (mode == Mode.None || !camera.CompareTag("MainCamera") || camera.renderingPath != RenderingPath.DeferredShading)
		{
			return;
		}
		RenderTextureDescriptor descriptor = GetDescriptor(camera, GetBuiltinRenderTextureType(mode));
		if (renderTexture == null || !Equals(cachedDescriptor, descriptor))
		{
			DestroyRenderTexture();
			cachedDescriptor = descriptor;
			renderTexture = new RenderTexture(cachedDescriptor);
			renderTexture.name = "DebugRenderTexture";
			renderTexture.filterMode = FilterMode.Bilinear;
			renderTexture.wrapMode = TextureWrapMode.Clamp;
			if (!renderTexture.Create())
			{
				Debug.LogError("Failed to create renderTexture!");
			}
			renderTextureId = new RenderTargetIdentifier(renderTexture);
			material.SetTexture(ShaderPropertyID._MainTex, renderTexture);
			UpdateCommandBuffer();
		}
		if (cachedCamera != camera)
		{
			RemoveCommandBufferFromCamera();
			cachedCamera = camera;
			cachedCamera.AddCommandBuffer(CameraEvent.AfterGBuffer, commandBuffer);
		}
	}

	private void UpdateCommandBuffer()
	{
		if (commandBuffer == null)
		{
			commandBuffer = new CommandBuffer();
			commandBuffer.name = "DebugRenderTextures";
		}
		else
		{
			commandBuffer.Clear();
		}
		BuiltinRenderTextureType builtinRenderTextureType = GetBuiltinRenderTextureType(mode);
		commandBuffer.Blit(builtinRenderTextureType, renderTextureId);
	}

	private void UpdateInfo()
	{
		info = $"{modeNames.Get(mode)} ({(int)mode}), {GetBuiltinRenderTextureType(mode)}";
	}

	private bool Equals(RenderTextureDescriptor a, RenderTextureDescriptor b)
	{
		if (a.width == b.width && a.height == b.height)
		{
			return a.colorFormat == b.colorFormat;
		}
		return false;
	}

	private void DestroyRenderTexture()
	{
		if (renderTexture != null)
		{
			if (renderTexture.IsCreated())
			{
				renderTexture.Release();
			}
			renderTexture = null;
		}
	}

	private void RemoveCommandBufferFromCamera()
	{
		if (cachedCamera != null)
		{
			if (commandBuffer != null)
			{
				cachedCamera.RemoveCommandBuffer(CameraEvent.AfterGBuffer, commandBuffer);
			}
			cachedCamera = null;
		}
	}

	private static RenderTextureDescriptor GetDescriptor(Camera camera, BuiltinRenderTextureType type)
	{
		RenderTextureFormat colorFormat = RenderTextureFormat.ARGB32;
		int depthBufferBits = 0;
		RenderTextureDescriptor result = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight, colorFormat, depthBufferBits);
		result.colorFormat = colorFormat;
		result.sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear;
		result.useMipMap = false;
		result.msaaSamples = 1;
		return result;
	}

	private static BuiltinRenderTextureType GetBuiltinRenderTextureType(Mode mode)
	{
		switch (mode)
		{
		default:
			return BuiltinRenderTextureType.None;
		case Mode.Diffuse:
			return BuiltinRenderTextureType.GBuffer0;
		case Mode.Occlusion:
			return BuiltinRenderTextureType.GBuffer0;
		case Mode.Specular:
			return BuiltinRenderTextureType.GBuffer1;
		case Mode.Smoothness:
			return BuiltinRenderTextureType.GBuffer1;
		case Mode.Normals:
			return BuiltinRenderTextureType.GBuffer2;
		case Mode.NormalsAlpha:
			return BuiltinRenderTextureType.GBuffer2;
		case Mode.Emission:
			return BuiltinRenderTextureType.GBuffer3;
		case Mode.EmissionAlpha:
			return BuiltinRenderTextureType.GBuffer3;
		}
	}
}
