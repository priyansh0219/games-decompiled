using UnityEngine;
using UnityEngine.UI;

public class CyclopsSilentRunningAbilityButton : MonoBehaviour
{
	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public LiveMixin liveMixin;

	[AssertNotNull]
	public Image image;

	[AssertNotNull]
	public Sprite activeSprite;

	[AssertNotNull]
	public Sprite inactiveSprite;

	public float silentRunningIteration = 5f;

	private bool active;

	private bool mouseHover;

	private void Update()
	{
		if (mouseHover)
		{
			HandReticle main = HandReticle.main;
			main.SetText(HandReticle.TextType.Hand, "RigForSilentRunning", translate: true, GameInput.Button.LeftHand);
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
				TurnOnSilentRunning();
			}
		}
		else
		{
			TurnOffSilentRunning();
		}
	}

	private void TurnOnSilentRunning()
	{
		active = true;
		subRoot.BroadcastMessage("RigForSilentRunning");
		InvokeRepeating("SilentRunningIteration", 0f, silentRunningIteration);
		image.sprite = activeSprite;
	}

	private void TurnOffSilentRunning()
	{
		active = false;
		subRoot.BroadcastMessage("SecureFromSilentRunning");
		CancelInvoke("SilentRunningIteration");
		image.sprite = inactiveSprite;
	}

	private void SilentRunningIteration()
	{
		float amountConsumed = 0f;
		if (!subRoot.powerRelay.ConsumeEnergy(subRoot.silentRunningPowerCost, out amountConsumed))
		{
			TurnOffSilentRunning();
		}
	}
}
