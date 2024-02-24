using UnityEngine;

public class OpenHatch : HandTarget, IHandTarget
{
	[AssertLocalization]
	public string openText = "Open";

	[AssertLocalization]
	public string closeText = "Close";

	public GameObject rotateTarget;

	public Vector3 rotateAmount = new Vector3(0f, 0.25f, 0f);

	public float animTime = 0.5f;

	public AudioClip openSound;

	public Collider doorCollider;

	private bool isOpen;

	private bool animating;

	public bool IsOpen()
	{
		return isOpen;
	}

	public void OnHandHover(GUIHand hand)
	{
		if (!animating)
		{
			HandReticle.main.SetText(HandReticle.TextType.Use, (!isOpen) ? openText : closeText, translate: true, GameInput.Button.Exit);
			HandReticle.main.SetText(HandReticle.TextType.UseSubscript, string.Empty, translate: true);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnHandClick(GUIHand guiHand)
	{
		SetOpenState(!isOpen);
	}

	public void SetOpenState(bool newState)
	{
		if (isOpen != newState)
		{
			float num = (newState ? 1f : (-1f));
			iTween.RotateBy((rotateTarget == null) ? base.gameObject : rotateTarget, iTween.Hash("amount", num * rotateAmount, "time", animTime, "oncomplete", "AnimDone"));
			animating = true;
			isOpen = newState;
			if ((bool)openSound)
			{
				AudioSource.PlayClipAtPoint(openSound, base.gameObject.transform.position);
			}
		}
	}

	private void AnimDone()
	{
		animating = false;
	}

	private void Update()
	{
		if ((bool)doorCollider)
		{
			bool flag = isOpen || animating;
			if (flag != doorCollider.isTrigger)
			{
				doorCollider.isTrigger = flag;
				Debug.Log("Setting " + doorCollider.gameObject.name + " isTrigger to " + flag);
			}
		}
	}
}
