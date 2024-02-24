using UnityEngine;
using UnityEngine.UI;

public class CyclopsSonarButton : MonoBehaviour
{
	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public Image image;

	[AssertNotNull]
	public FMOD_CustomEmitter soundFX;

	[AssertNotNull]
	public Sprite activeSprite;

	[AssertNotNull]
	public Sprite inactiveSprite;

	public float pingIterationDuration = 5f;

	private float cooldown;

	private bool active = true;

	private bool mouseHover;

	private bool _sonarActive;

	private bool sonarActive
	{
		get
		{
			return _sonarActive;
		}
		set
		{
			_sonarActive = value;
			if (value)
			{
				image.sprite = activeSprite;
			}
			else
			{
				image.sprite = inactiveSprite;
			}
		}
	}

	private void Update()
	{
		if (mouseHover)
		{
			string text = (sonarActive ? "CyclopsSonar_Deactivate" : "CyclopsSonar_Activate");
			HandReticle main = HandReticle.main;
			main.SetText(HandReticle.TextType.Hand, text, translate: true, GameInput.Button.LeftHand);
			main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		}
		if (Player.main.GetMode() == Player.Mode.Normal && sonarActive)
		{
			TurnOffSonar();
		}
	}

	public void OnMouseEnter()
	{
		if (!(Player.main.currentSub != subRoot) && active)
		{
			animator.SetTrigger("Highlighted");
			mouseHover = true;
		}
	}

	public void OnMouseExit()
	{
		if (!(Player.main.currentSub != subRoot))
		{
			animator.SetTrigger("Normal");
			HandReticle.main.SetIcon(HandReticle.IconType.Default);
			mouseHover = false;
		}
	}

	private void Start()
	{
		cooldown = subRoot.sonarCD;
	}

	public void OnClick()
	{
		if (active && subRoot.powerRelay.IsPowered() && !(Player.main.currentSub != subRoot))
		{
			mouseHover = false;
			animator.SetTrigger("Normal");
			if (sonarActive)
			{
				TurnOffSonar();
			}
			else
			{
				TurnOnSonar();
			}
		}
	}

	private void TurnOffSonar()
	{
		CancelInvoke("SonarPing");
		sonarActive = false;
	}

	private void TurnOnSonar()
	{
		sonarActive = true;
		InvokeRepeating("SonarPing", 0f, pingIterationDuration);
	}

	private void SonarPing()
	{
		float amountConsumed = 0f;
		if (!subRoot.powerRelay.ConsumeEnergy(subRoot.sonarPowerCost, out amountConsumed))
		{
			TurnOffSonar();
			return;
		}
		SNCameraRoot.main.SonarPing();
		soundFX.Play();
	}
}
