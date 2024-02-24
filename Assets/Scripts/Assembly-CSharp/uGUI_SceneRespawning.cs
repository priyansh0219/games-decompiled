using UnityEngine;

public class uGUI_SceneRespawning : uGUI_Scene
{
	[AssertNotNull]
	public uGUI_TextFade loadingText;

	[AssertNotNull]
	public uGUI_Fader loadingBackground;

	[AssertLocalization]
	private const string loadingMessage = "Loading";

	[ContextMenu("Show")]
	public void Show()
	{
		loadingText.SetText(Language.main.Get("Loading"));
		loadingBackground.SetState(enabled: true);
	}

	[ContextMenu("Hide")]
	public void Hide()
	{
		loadingBackground.FadeOut();
	}
}
