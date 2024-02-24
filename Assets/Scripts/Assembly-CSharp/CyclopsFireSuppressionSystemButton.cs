using UnityEngine;
using UnityEngine.UI;

public class CyclopsFireSuppressionSystemButton : MonoBehaviour, ICyclopsAbility
{
	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public Button button;

	[AssertNotNull]
	public Image image;

	[AssertNotNull]
	public SubFire subFire;

	[AssertNotNull]
	public GameObject overlay;

	private float cooldown;

	private bool active = true;

	private bool mouseHover;

	private float fireSuppressionSystemStartTime;

	private void Update()
	{
		if (mouseHover)
		{
			HandReticle main = HandReticle.main;
			main.SetText(HandReticle.TextType.Hand, "CyclopsFireSuppressionSystem", translate: true, GameInput.Button.LeftHand);
			main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			main.SetIcon(HandReticle.IconType.Hand);
		}
		if (!active)
		{
			float fillAmount = Mathf.Clamp01((Time.time - fireSuppressionSystemStartTime) / subRoot.fireSuppressionCD);
			image.fillAmount = fillAmount;
		}
		else
		{
			image.fillAmount = 1f;
		}
	}

	public void OnMouseEnter()
	{
		if (!(Player.main.currentSub != subRoot) && active)
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

	private void Start()
	{
		cooldown = subRoot.fireSuppressionCD;
	}

	public void OnClick()
	{
		if (active && subRoot.powerRelay.IsPowered() && !(Player.main.currentSub != subRoot))
		{
			subFire.ActivateFireSuppressionSystem();
			StartCooldown();
		}
	}

	public void StartCooldown()
	{
		button.enabled = false;
		active = false;
		mouseHover = false;
		overlay.SetActive(value: true);
		fireSuppressionSystemStartTime = Time.time;
		Invoke("DoResetCooldown", cooldown);
	}

	private void DoResetCooldown()
	{
		ResetCooldown();
	}

	public void ResetCooldown()
	{
		active = true;
		button.enabled = true;
		overlay.SetActive(value: false);
	}
}
