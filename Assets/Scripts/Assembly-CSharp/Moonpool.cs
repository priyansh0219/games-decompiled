using UnityEngine;

public class Moonpool : MonoBehaviour
{
	public Light[] lights;

	private void Start()
	{
		if (GetComponentInParent<Base>().isGhost)
		{
			SetLights(powered: false);
			return;
		}
		PowerRelay componentInParent = GetComponentInParent<PowerRelay>();
		componentInParent.powerUpEvent.AddHandler(base.gameObject, PowerUpEvent);
		componentInParent.powerDownEvent.AddHandler(base.gameObject, PowerDownEvent);
		SetLights(componentInParent.IsPowered());
	}

	private void SetLights(bool powered)
	{
		Light[] array = lights;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = powered;
		}
	}

	private void PowerUpEvent(PowerRelay relay)
	{
		SetLights(powered: true);
	}

	private void PowerDownEvent(PowerRelay relay)
	{
		SetLights(powered: false);
	}
}
