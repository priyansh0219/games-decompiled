using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreStartScreen : MonoBehaviour
{
	private IEnumerator Start()
	{
		PlatformUtils.AddTemporaryCamera();
		yield return ShaderManager.Init();
		yield return LanguageSDF.PreloadFontAssets();
		AddressablesUtility.LoadSceneAsync("StartScreen", LoadSceneMode.Single);
	}
}
