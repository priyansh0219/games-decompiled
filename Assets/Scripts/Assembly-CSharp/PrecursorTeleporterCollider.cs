using UWE;
using UnityEngine;

public class PrecursorTeleporterCollider : MonoBehaviour
{
	private void OnTriggerEnter(Collider col)
	{
		if (!col.isTrigger)
		{
			GameObject entityRoot = UWE.Utils.GetEntityRoot(col.gameObject);
			if (!entityRoot)
			{
				entityRoot = col.gameObject;
			}
			GameObject gameObject = null;
			if (entityRoot.Equals(Player.main.gameObject))
			{
				gameObject = Player.main.gameObject;
			}
			Vehicle component = entityRoot.GetComponent<Vehicle>();
			if ((bool)component && component.GetPilotingMode())
			{
				gameObject = entityRoot;
			}
			if ((bool)gameObject)
			{
				SendMessageUpwards("BeginTeleportPlayer", gameObject, SendMessageOptions.RequireReceiver);
			}
		}
	}
}
