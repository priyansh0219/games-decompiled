using ProtoBuf;
using Story;
using UnityEngine;

[ProtoContract]
public class PrecursorGunStoryEvents : MonoBehaviour
{
	public static PrecursorGunStoryEvents main;

	[AssertNotNull]
	public Transform gun;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public FMOD_CustomEmitter gunTurnSFX;

	[AssertNotNull]
	public FMOD_CustomEmitter gunReturnToIdle;

	[AssertNotNull]
	public PrecursorGunAim gunAim;

	private void Start()
	{
		main = this;
		bool flag = StoryGoalManager.main.IsGoalComplete("PrecursorGunAim");
		animator.SetBool("Finished", flag);
		if (flag)
		{
			animator.SetBool("MoveIntoPosition", value: false);
		}
	}

	public void GunTakeAim()
	{
		animator.SetTrigger("MoveIntoPosition");
		gunTurnSFX.Play();
		Invoke("ResetPosition", 120f);
	}

	private void ResetPosition()
	{
		animator.SetBool("MoveIntoPosition", value: false);
		gunReturnToIdle.Play();
	}

	private void Update()
	{
		if (VFXSunbeam.main != null)
		{
			animator.SetBool("Shooting", VFXSunbeam.main.IsShooting());
		}
	}
}
