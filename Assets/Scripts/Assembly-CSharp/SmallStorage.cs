using UnityEngine;

public class SmallStorage : MonoBehaviour
{
	public Animator animator;

	public FMODAsset openSound;

	public FMODAsset closeSound;

	private void Start()
	{
	}

	public void ToggleDoor()
	{
		bool flag = !animator.GetBool("opened");
		animator.SetBool("opened", flag);
		if (flag && (bool)openSound)
		{
			Utils.PlayFMODAsset(openSound, base.transform);
		}
		if (!flag && (bool)closeSound)
		{
			Utils.PlayFMODAsset(closeSound, base.transform);
		}
	}
}
