using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public static class MainSceneLoading
{
	public static IEnumerator Launch(WaitScreen.ManualWaitItem waitDummy = null)
	{
		if (waitDummy == null)
		{
			waitDummy = WaitScreen.Add("FadeInDummy");
		}
		float num = 0.3f - (float)waitDummy.pastSecs;
		if (num >= 0f)
		{
			yield return new WaitForSecondsRealtime(num);
		}
		WaitScreen.ManualWaitItem waitLevelLoad = WaitScreen.Add("SceneMain");
		waitLevelLoad.SetProgress(1f);
		WaitScreen.Remove(waitDummy);
		SetFastLoadMode(useFastLoadMode: true);
		AsyncOperationHandle<SceneInstance> levelLoadingOp = AddressablesUtility.LoadSceneAsync("Main", LoadSceneMode.Single);
		while (!levelLoadingOp.IsDone)
		{
			yield return null;
		}
		WaitScreen.Remove(waitLevelLoad);
		while (WaitScreen.IsWaiting)
		{
			yield return null;
		}
		SetFastLoadMode(useFastLoadMode: false);
		WaitScreen.ReportStageDurations();
	}

	private static void SetFastLoadMode(bool useFastLoadMode)
	{
		PlatformUtils.main.GetServices()?.SetUseFastLoadMode(useFastLoadMode);
		MainCameraV2 main = MainCameraV2.main;
		if ((bool)main)
		{
			main.SetLoadingScreenOptimizations(useFastLoadMode);
			WBOIT component = main.GetComponent<WBOIT>();
			if ((bool)component)
			{
				component.SetLoadingScreenOptimizations(useFastLoadMode);
			}
		}
	}
}
