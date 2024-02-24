using UnityEngine;

public class Centrifuge : MonoBehaviour
{
	public Animator animator;

	public Transform soundOrigin;

	[AssertNotNull]
	public FMODAsset openSound;

	[AssertNotNull]
	public FMODAsset closeSound;

	[AssertNotNull]
	public FMODAsset workSound;

	private void Start()
	{
		if (soundOrigin == null)
		{
			soundOrigin = base.transform;
		}
	}
}
