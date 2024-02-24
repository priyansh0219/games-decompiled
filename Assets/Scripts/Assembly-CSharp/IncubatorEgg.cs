using UnityEngine;

public class IncubatorEgg : MonoBehaviour
{
	[AssertNotNull]
	public FMODAsset hatchSound;

	[AssertNotNull]
	public GameObject seaEmperorBabyPrefab;

	[AssertNotNull]
	public Transform attachPoint;

	[AssertNotNull]
	public IncubatorEggAnimation animationController;

	[AssertNotNull]
	public VFXController fxControl;

	private int babyIdentifier;

	private string animParameter;

	private GameObject babyGO;

	public void StartHatch(int babyId, string hatchAnimation)
	{
		babyIdentifier = babyId;
		animParameter = hatchAnimation;
		Invoke("HatchNow", Random.Range(1.5f, 5f));
	}

	private void HatchNow()
	{
		fxControl.Play();
		Utils.PlayFMODAsset(hatchSound, base.transform, 30f);
		babyGO = Object.Instantiate(seaEmperorBabyPrefab);
		babyGO.transform.SetParent(attachPoint);
		babyGO.transform.localPosition = Vector3.zero;
		babyGO.transform.localRotation = Quaternion.identity;
		animationController.StartHatchAnimation(babyIdentifier, animParameter, babyGO);
		Invoke("PlayFxOnBaby", 2f);
	}

	private void PlayFxOnBaby()
	{
		VFXController component = babyGO.GetComponent<VFXController>();
		if (component != null)
		{
			component.Play(0);
		}
	}
}
