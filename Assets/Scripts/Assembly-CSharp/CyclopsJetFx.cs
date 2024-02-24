using UnityEngine;

public class CyclopsJetFx : MonoBehaviour, ISubTurnHandler
{
	[AssertNotNull]
	public VFXController fxControl;

	private ShipSide currentShipSide;

	private float timeOfLastSubTurn;

	private bool isPlaying;

	public void OnSubTurn(ShipSide useShipSide)
	{
		timeOfLastSubTurn = Time.time;
		isPlaying = true;
		if (currentShipSide != useShipSide)
		{
			if (useShipSide == ShipSide.Starboard)
			{
				fxControl.Play(0);
				fxControl.Stop(1);
			}
			else
			{
				fxControl.Stop(0);
				fxControl.Play(1);
			}
			currentShipSide = useShipSide;
		}
	}

	private void Update()
	{
		if (isPlaying && timeOfLastSubTurn + 0.25f < Time.time)
		{
			fxControl.Stop();
			isPlaying = false;
		}
	}
}
