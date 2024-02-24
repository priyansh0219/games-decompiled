using UnityEngine;

public class OxygenArea : MonoBehaviour
{
	public float oxygenPerSecond = 10f;

	public GameObject firstPersonOxygenFXPrefab;

	public FMODAsset popSound;

	private float timeFXTriggered;

	private void OnTriggerStay(Collider other)
	{
		OxygenManager component = other.gameObject.GetComponent<OxygenManager>();
		if ((bool)component)
		{
			component.AddOxygen(oxygenPerSecond * Time.deltaTime);
			if (other.gameObject.FindAncestor<Player>() == Utils.GetLocalPlayerComp() && timeFXTriggered + 2f < Time.time)
			{
				Player.main.PlayOneShotPS(firstPersonOxygenFXPrefab);
				Utils.PlayFMODAsset(popSound, other.gameObject.transform, 3f);
				timeFXTriggered = Time.time;
			}
		}
	}
}
