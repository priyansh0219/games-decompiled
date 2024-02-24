using UnityEngine;
using UnityEngine.UI;

public class CyclopsShieldButton : MonoBehaviour
{
	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public Image image;

	[AssertNotNull]
	public FMOD_CustomEmitter sfx;

	[AssertNotNull]
	public Sprite activeSprite;

	[AssertNotNull]
	public Sprite inactiveSprite;

	public float shieldRunningIteration = 5f;

	private bool active;

	private bool mouseHover;

	private void OnDisable()
	{
		if (active)
		{
			StopShield();
		}
	}

	private void Update()
	{
		if (mouseHover && !active)
		{
			HandReticle main = HandReticle.main;
			main.SetText(HandReticle.TextType.Hand, "CyclopsShield", translate: true, GameInput.Button.LeftHand);
			main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		}
	}

	public void OnMouseEnter()
	{
		if (!(Player.main.currentSub != subRoot))
		{
			mouseHover = true;
		}
	}

	public void OnMouseExit()
	{
		if (!(Player.main.currentSub != subRoot))
		{
			HandReticle.main.SetIcon(HandReticle.IconType.Default);
			mouseHover = false;
		}
	}

	public void OnClick()
	{
		if (Player.main.currentSub != subRoot)
		{
			return;
		}
		if (!active)
		{
			if (subRoot.powerRelay.IsPowered())
			{
				StartShield();
			}
		}
		else
		{
			StopShield();
		}
	}

	private void StartShield()
	{
		image.sprite = activeSprite;
		sfx.Play();
		active = true;
		InvokeRepeating("ShieldIteration", 0f, shieldRunningIteration);
		subRoot.StartSubShielded();
	}

	private void StopShield()
	{
		image.sprite = inactiveSprite;
		active = false;
		sfx.Stop();
		CancelInvoke("ShieldIteration");
		subRoot.EndSubShielded();
	}

	private void ShieldIteration()
	{
		float amountConsumed = 0f;
		if (!subRoot.powerRelay.ConsumeEnergy(subRoot.shieldPowerCost, out amountConsumed))
		{
			StopShield();
		}
	}
}
