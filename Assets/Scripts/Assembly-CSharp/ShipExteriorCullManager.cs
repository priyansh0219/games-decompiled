using System.Collections.Generic;
using UnityEngine;

public class ShipExteriorCullManager : MonoBehaviour
{
	[SerializeField]
	[AssertNotNull]
	private CrashedShipExploder crashedShipExploder;

	[SerializeField]
	private int updateEveryXFrames = 10;

	private List<ShipExteriorCull> potentialCullers = new List<ShipExteriorCull>();

	public static ShipExteriorCullManager main;

	private void OnEnable()
	{
		main = this;
	}

	private void OnDisable()
	{
		main = null;
	}

	public void Register(ShipExteriorCull shipExteriorCull)
	{
		potentialCullers.Add(shipExteriorCull);
	}

	public void Deregister(ShipExteriorCull shipExteriorCull)
	{
		potentialCullers.Remove(shipExteriorCull);
	}

	private void Update()
	{
		if (Time.frameCount % updateEveryXFrames != 0)
		{
			return;
		}
		bool flag = false;
		foreach (ShipExteriorCull potentialCuller in potentialCullers)
		{
			if (potentialCuller.IsInVolume())
			{
				flag = true;
				break;
			}
		}
		crashedShipExploder.CullExplodedExterior(!flag);
	}
}
