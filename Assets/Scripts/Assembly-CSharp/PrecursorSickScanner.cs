using UnityEngine;

public class PrecursorSickScanner : MonoBehaviour
{
	[AssertNotNull]
	public FMODAsset sickVO;

	[AssertNotNull]
	public FMODAsset notSickVO;

	public bool toogleDoorOnCured = true;

	private void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.Equals(Player.main.gameObject))
		{
			CheckIfSick();
		}
	}

	private void CheckIfSick()
	{
		if (Player.main.infectedMixin.IsInfected())
		{
			Utils.PlayFMODAsset(sickVO, base.transform);
			return;
		}
		Utils.PlayFMODAsset(notSickVO, base.transform);
		if (toogleDoorOnCured)
		{
			BroadcastMessage("ToggleDoor", true, SendMessageOptions.DontRequireReceiver);
		}
	}
}
