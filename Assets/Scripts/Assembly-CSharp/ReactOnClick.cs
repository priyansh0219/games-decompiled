using UnityEngine;

public class ReactOnClick : HandTarget, IHandTarget
{
	public FMOD_CustomEmitter sound;

	public Animator animator;

	public string animation;

	public void OnHandHover(GUIHand hand)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, "Use", translate: true, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Hand);
	}

	public void OnHandClick(GUIHand hand)
	{
		if (sound != null)
		{
			sound.Play();
		}
		if (animator != null)
		{
			animator.SetTrigger(Animator.StringToHash(animation));
		}
	}
}
