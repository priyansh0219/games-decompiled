using System.Collections;
using UnityEngine;

public class CyclopsEngineChangeState : MonoBehaviour
{
	public bool changeToEngineState;

	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public CyclopsMotorMode motorMode;

	[AssertNotNull]
	public Transform screwIcon;

	private bool mouseHover;

	private string tooltipText;

	private float spinSpeed;

	private float speedModifier = 1f;

	private bool startEngine;

	private bool invalidButton;

	private void Update()
	{
		if (mouseHover)
		{
			HandReticle main = HandReticle.main;
			main.SetText(HandReticle.TextType.Hand, tooltipText, translate: true, GameInput.Button.LeftHand);
			main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		}
		float target = 0f;
		if (motorMode.engineOn)
		{
			startEngine = false;
			switch (motorMode.cyclopsMotorMode)
			{
			case CyclopsMotorMode.CyclopsMotorModes.Slow:
				target = 5f;
				break;
			case CyclopsMotorMode.CyclopsMotorModes.Standard:
				target = 15f;
				break;
			case CyclopsMotorMode.CyclopsMotorModes.Flank:
				target = 25f;
				break;
			}
		}
		else if (startEngine)
		{
			target = 2.5f;
		}
		spinSpeed = Mathf.MoveTowards(spinSpeed, target, Time.deltaTime * 15f);
		screwIcon.Rotate(new Vector3(0f, 0f, 0f - spinSpeed * Time.deltaTime * 60f), Space.Self);
	}

	public void OnMouseEnter()
	{
		if (!invalidButton && !(Player.main.currentSub != subRoot))
		{
			tooltipText = (motorMode.engineOn ? "CyclopsEngineOff" : "CyclopsEngineOn");
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
		if (!invalidButton && !(Player.main.currentSub != subRoot))
		{
			mouseHover = false;
			if (!motorMode.engineOn)
			{
				subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.enginePowerUpNotification);
				startEngine = true;
				StartCoroutine(EngineStartCameraShake(0.15f, 4.5f, 0f));
				StartCoroutine(EngineStartCameraShake(1f, -1f, 4.6f));
			}
			else
			{
				subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.enginePowerDownNotification);
			}
			subRoot.BroadcastMessage("InvokeChangeEngineState", !motorMode.engineOn, SendMessageOptions.RequireReceiver);
			invalidButton = true;
			Invoke("ResetInvalidButton", 2.5f);
		}
	}

	private void ResetInvalidButton()
	{
		invalidButton = false;
	}

	private IEnumerator EngineStartCameraShake(float intensity, float duration, float delay)
	{
		yield return new WaitForSeconds(delay);
		MainCameraControl.main.ShakeCamera(intensity, duration);
	}
}
