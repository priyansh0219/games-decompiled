using UnityEngine;
using UnityEngine.PostProcessing;

public sealed class MainCameraV2 : MonoBehaviour
{
	[SerializeField]
	[AssertNotNull]
	private Camera cam;

	public static MainCameraV2 main { get; private set; }

	private void Start()
	{
		main = this;
	}

	private void OnDestroy()
	{
		main = null;
	}

	public void SetLoadingScreenOptimizations(bool isLoadingScreen)
	{
		SetComponentActivation<WaterSurfaceOnCamera>(!isLoadingScreen);
		SetComponentActivation<WBOIT>(!isLoadingScreen);
		SetComponentActivation<WaterscapeVolumeOnCamera>(!isLoadingScreen);
		SetComponentActivation<UwePostProcessingManager>(!isLoadingScreen);
		SetComponentActivation<PostProcessingBehaviour>(!isLoadingScreen);
	}

	private void SetComponentActivation<T>(bool isEnabled)
	{
		if (TryGetComponent<T>(out var component))
		{
			(component as MonoBehaviour).enabled = isEnabled;
		}
	}
}
