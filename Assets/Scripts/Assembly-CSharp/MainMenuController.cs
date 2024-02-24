using UnityEngine;

public class MainMenuController : MonoBehaviour
{
	private void Start()
	{
		Application.backgroundLoadingPriority = ThreadPriority.High;
		int qualityLevel = QualitySettings.GetQualityLevel();
		int plasticChangeSetOfBuild = SNUtils.GetPlasticChangeSetOfBuild(0);
		Debug.Log("changeset " + plasticChangeSetOfBuild + ", QualityLevel = " + qualityLevel);
		if (SystemInfo.systemMemorySize < 6144)
		{
			Debug.Log("Forcing texture limit to 3 due to < 6GB of RAM");
			QualitySettings.masterTextureLimit = 3;
		}
		else
		{
			Debug.Log("OK Running Win64");
		}
		Debug.LogFormat("Application version: {0}", Application.version);
		Debug.Log(string.Concat("SystemInfo: \n deviceModel = ", SystemInfo.deviceModel, "\n deviceName = ", SystemInfo.deviceName, "\n deviceType = ", SystemInfo.deviceType, "\n deviceUniqueIdentifier = ", SystemInfo.deviceUniqueIdentifier, "\n graphicsDeviceID = ", SystemInfo.graphicsDeviceID, "\n graphicsDeviceName = ", SystemInfo.graphicsDeviceName, "\n graphicsDeviceType = ", SystemInfo.graphicsDeviceType, "\n graphicsDeviceVendor = ", SystemInfo.graphicsDeviceVendor, "\n graphicsDeviceVendorID = ", SystemInfo.graphicsDeviceVendorID, "\n graphicsDeviceVersion = ", SystemInfo.graphicsDeviceVersion, "\n graphicsMemorySize = ", SystemInfo.graphicsMemorySize, "\n graphicsShaderLevel = ", SystemInfo.graphicsShaderLevel, "\n npotSupport = ", SystemInfo.npotSupport, "\n maxTextureSize = ", SystemInfo.maxTextureSize, "\n operatingSystem = ", SystemInfo.operatingSystem, "\n processorCount = ", SystemInfo.processorCount, "\n processorFrequency = ", SystemInfo.processorFrequency, "\n processorType = ", SystemInfo.processorType, "\n supportedRenderTargetCount = ", SystemInfo.supportedRenderTargetCount, "\n supports3DTextures = ", SystemInfo.supports3DTextures.ToString(), "\n supportsAccelerometer = ", SystemInfo.supportsAccelerometer.ToString(), "\n supportsComputeShaders = ", SystemInfo.supportsComputeShaders.ToString(), "\n supportsGyroscope = ", SystemInfo.supportsGyroscope.ToString(), "\n supportsImageEffects = ", SystemInfo.supportsImageEffects.ToString(), "\n supportsInstancing = ", SystemInfo.supportsInstancing.ToString(), "\n supportsLocationService = ", SystemInfo.supportsLocationService.ToString(), "\n supportsRawShadowDepthSampling = ", SystemInfo.supportsRawShadowDepthSampling.ToString(), "\n supportsRenderTextures = ", SystemInfo.supportsRenderTextures.ToString(), "\n supportsRenderToCubemap = ", SystemInfo.supportsRenderToCubemap.ToString(), "\n supportsShadows = ", SystemInfo.supportsShadows.ToString(), "\n supportsSparseTextures = ", SystemInfo.supportsSparseTextures.ToString(), "\n supportsStencil = ", SystemInfo.supportsStencil, "\n supportsVibration = ", SystemInfo.supportsVibration.ToString(), "\n systemMemorySize = ", SystemInfo.systemMemorySize, "\n"));
	}

	private void Update()
	{
	}
}
