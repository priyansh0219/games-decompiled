using TMPro;
using UnityEngine;

[SkipProtoContractCheck]
public class CockpitSwitch : HandTarget, IHandTarget
{
	[AssertNotNull(AssertNotNullAttribute.Options.IgnorePrefabs)]
	public RocketPreflightCheckManager preflightCheckManager;

	[AssertNotNull(AssertNotNullAttribute.Options.IgnorePrefabs)]
	public PreflightCheckSwitch preflightCheckSwitch;

	[AssertNotNull(AssertNotNullAttribute.Options.IgnorePrefabs)]
	public Animator animator;

	[AssertNotNull(AssertNotNullAttribute.Options.IgnorePrefabs)]
	public PlayerCinematicController informCinematicController;

	public PreflightCheck preflightCheck;

	public GameObject collision;

	public float activateSystemDelay;

	public bool completed;

	public TextMeshProUGUI flavorText;

	[AssertNotNull]
	public string toolTipString;

	private void Start()
	{
		if (preflightCheckManager.GetPreflightComplete(preflightCheck))
		{
			animator.SetBool("Completed", value: true);
			completed = true;
			if ((bool)collision)
			{
				collision.SetActive(value: false);
			}
		}
		if ((bool)flavorText)
		{
			string text = preflightCheckManager.ReturnLocalizedPreflightCheckName(preflightCheck);
			if (!string.IsNullOrEmpty(text))
			{
				flavorText.text = Language.main.Get(text);
			}
		}
	}

	public void OnHandHover(GUIHand guiHand)
	{
		if (!completed)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, toolTipString, translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnHandClick(GUIHand guiHand)
	{
		if (!completed)
		{
			animator.SetTrigger("Activate");
			completed = true;
			if (informCinematicController != null)
			{
				informCinematicController.StartCinematicMode(Player.main);
			}
			SystemReady();
			if ((bool)collision)
			{
				collision.SetActive(value: false);
			}
		}
	}

	private void SystemReady()
	{
		preflightCheckSwitch.CompletePreflightCheck();
	}
}
