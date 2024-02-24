using UnityEngine;

public class MainMenuLoadScreenshot : MonoBehaviour
{
	public TextAsset pngScreenshot;

	private void Start()
	{
		new Texture2D(2, 2).LoadImage(pngScreenshot.bytes);
	}
}
