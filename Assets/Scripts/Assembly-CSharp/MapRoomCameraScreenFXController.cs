using UnityEngine;

public class MapRoomCameraScreenFXController : MonoBehaviour
{
	public MapRoomCameraScreenFX fx;

	private void OnPreRender()
	{
		fx.noiseFactor = ((uGUI_CameraDrone.main != null) ? uGUI_CameraDrone.main.noise : 0f);
	}
}
