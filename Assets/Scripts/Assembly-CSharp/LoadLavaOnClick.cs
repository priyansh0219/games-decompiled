using UnityEngine;

public class LoadLavaOnClick : MonoBehaviour
{
	public GameMode gameMode;

	private void Update()
	{
		Cursor.visible = true;
	}

	private void OnMouseDown()
	{
		Utils.SetLegacyGameMode(gameMode);
		Application.LoadLevel("LavaTest");
	}
}
