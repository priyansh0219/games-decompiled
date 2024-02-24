using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class ShaderManager
{
	private const string shaderVariantCollectionKey = "Shaders/ShaderVariantCollections/UWEshaderVariants.shadervariants";

	private const string preloadedShadersKey = "Shaders/PreloadedShaders.asset";

	private static PreloadedShaders _preloadedShaders;

	public static PreloadedShaders preloadedShaders => _preloadedShaders;

	public static IEnumerator Init()
	{
		AsyncOperationHandle<ShaderVariantCollection> shaderVariantHandle = AddressablesUtility.LoadAsync<ShaderVariantCollection>("Shaders/ShaderVariantCollections/UWEshaderVariants.shadervariants");
		AsyncOperationHandle<PreloadedShaders> preloadedShadersHandle = AddressablesUtility.LoadAsync<PreloadedShaders>("Shaders/PreloadedShaders.asset");
		yield return shaderVariantHandle;
		shaderVariantHandle.LogExceptionIfFailed("Loading ShaderVariantCollection has failed.");
		shaderVariantHandle.Result.WarmUp();
		yield return preloadedShadersHandle;
		preloadedShadersHandle.LogExceptionIfFailed("Loading PreloadedShaders has failed.");
		_preloadedShaders = preloadedShadersHandle.Result;
	}
}
