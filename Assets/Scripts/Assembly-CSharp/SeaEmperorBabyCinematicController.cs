using UWE;
using UnityEngine;

public class SeaEmperorBabyCinematicController : MonoBehaviour
{
	[AssertNotNull]
	public SeaEmperorBaby baby;

	[AssertNotNull]
	public Rigidbody useRigidbody;

	[AssertNotNull]
	public DropEnzymes dropEnzymes;

	private string currentCinematic = string.Empty;

	private bool resetScale;

	private void OnCinematicModeEnd()
	{
		SafeAnimator.SetBool(baby.GetAnimator(), currentCinematic, value: false);
		resetScale = true;
		SetCinematicMode(cinematicOn: false);
		baby.SwimToTeleporter();
	}

	private void Update()
	{
		if (resetScale)
		{
			baby.transform.localScale = baby.motherInteractionScale;
			resetScale = false;
		}
	}

	public void StartCinematic(string animationParameter)
	{
		currentCinematic = animationParameter;
		SafeAnimator.SetBool(baby.GetAnimator(), currentCinematic, value: true);
	}

	public void SetCinematicMode(bool cinematicOn)
	{
		baby.enabled = !cinematicOn;
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(useRigidbody, cinematicOn);
		if (cinematicOn)
		{
			LargeWorldStreamer.main.cellManager.UnregisterEntity(baby.gameObject);
			return;
		}
		baby.transform.SetParent(null, worldPositionStays: true);
		LargeWorldStreamer.main.cellManager.RegisterEntity(baby.gameObject);
	}

	public void SpawnCureBall()
	{
		dropEnzymes.SpawnCureBall();
	}

	public void ReleaseCureBall()
	{
		dropEnzymes.ReleaseSpawnedCureBall();
	}
}
