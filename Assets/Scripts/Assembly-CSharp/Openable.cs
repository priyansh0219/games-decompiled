using UnityEngine;

[SkipProtoContractCheck]
public class Openable : HandTarget, IHandTarget
{
	public GameObject rotateTarget;

	public Vector3 rotateAmount = new Vector3(0f, 0.25f, 0f);

	public float animTime = 0.5f;

	public string openSound = "event:/sub/base/door_open";

	public string closeSound = "event:/sub/base/door_open";

	public string cutOpenSound = "";

	public bool isOpen;

	public string messageOnOpen;

	public bool isLocked;

	public bool canLock;

	public Collider openChecker;

	public Collider closeChecker;

	private bool blocked;

	private bool wasLocked;

	private bool wasOpen;

	private Quaternion openedRotation;

	private Quaternion closedRotation;

	private Quaternion startRotation;

	private Quaternion endRotation;

	private float animationFraction;

	private float animationSpeed;

	private bool animating;

	[AssertLocalization]
	private const string lockedHandText = "Locked";

	[AssertLocalization]
	private const string closeHandText = "Close";

	[AssertLocalization]
	private const string openHandText = "Open";

	[AssertLocalization]
	private const string sealedHandText = "Sealed";

	[AssertLocalization]
	private const string sealedInstructionsHandText = "SealedInstructions";

	private bool IsSealed()
	{
		Sealed component = base.gameObject.GetComponent<Sealed>();
		if (component != null)
		{
			return component.IsSealed();
		}
		return false;
	}

	public void OnHandHover(GUIHand hand)
	{
		if (!hand.IsFreeToInteract() || animating)
		{
			return;
		}
		HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		if (!IsSealed())
		{
			if (isLocked)
			{
				HandReticle.main.SetText(HandReticle.TextType.Hand, "Locked", translate: true);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			}
			else
			{
				HandReticle.main.SetText(HandReticle.TextType.Hand, isOpen ? "Close" : "Open", translate: true, GameInput.Button.LeftHand);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			}
		}
		else
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "Sealed", translate: true);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "SealedInstructions", translate: true);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (!isLocked && hand.IsFreeToInteract() && !animating && !IsSealed())
		{
			if (!isOpen)
			{
				PlayOpenAnimation(openState: true, animTime);
			}
			else
			{
				PlayOpenAnimation(openState: false, animTime);
			}
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		Player component = collider.GetComponent<Player>();
		if (component != null)
		{
			blocked = true;
			component.AddOnTriggerExitRecovery(OnTriggerExit);
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		Player component = collider.GetComponent<Player>();
		if (component != null)
		{
			blocked = false;
			component.RemoveOnTriggerExitRecovery(OnTriggerExit);
		}
	}

	private void Start()
	{
		if (isOpen)
		{
			openedRotation = rotateTarget.transform.localRotation;
			closedRotation = openedRotation * Quaternion.Euler(-360f * rotateAmount);
		}
		else
		{
			closedRotation = rotateTarget.transform.localRotation;
			openedRotation = closedRotation * Quaternion.Euler(360f * rotateAmount);
		}
		if (rotateTarget == null)
		{
			rotateTarget = base.gameObject;
		}
		Sealed component = base.gameObject.GetComponent<Sealed>();
		if (component != null)
		{
			component.openedEvent.AddHandler(base.gameObject, OnCutOpen);
		}
	}

	private void OnCutOpen(Sealed sealedComp)
	{
		if (!isOpen)
		{
			PlayOpenAnimation(openState: true, animTime);
			if (cutOpenSound != "")
			{
				FMODUWE.PlayOneShot(cutOpenSound, base.transform.position);
			}
		}
	}

	private void Update()
	{
		if (animating && !blocked)
		{
			animationFraction += animationSpeed * Time.deltaTime;
			if (animationFraction >= 1f)
			{
				animationFraction = 1f;
				animating = false;
				SetEnabled(openChecker, state: false);
				SetEnabled(closeChecker, state: false);
			}
			rotateTarget.transform.localRotation = Quaternion.Lerp(startRotation, endRotation, animationFraction);
		}
	}

	public void PlayOpenAnimation(bool openState, float duration)
	{
		if (isOpen != openState)
		{
			SetEnabled(openChecker, openState);
			SetEnabled(closeChecker, !openState);
			startRotation = rotateTarget.transform.localRotation;
			endRotation = (openState ? openedRotation : closedRotation);
			animationFraction = 0f;
			animationSpeed = 1f / duration;
			animating = true;
			isOpen = openState;
			if (isOpen && messageOnOpen != "")
			{
				SendMessage(messageOnOpen, null, SendMessageOptions.DontRequireReceiver);
			}
			if (openState && openSound != "")
			{
				FMODUWE.PlayOneShot(openSound, base.transform.position);
			}
			else if (!openState && closeSound != "")
			{
				FMODUWE.PlayOneShot(closeSound, base.transform.position);
			}
		}
	}

	private void SetEnabled(Collider trigger, bool state)
	{
		if ((bool)trigger)
		{
			trigger.enabled = state;
		}
	}

	public void TemporaryLock(float duration)
	{
		wasLocked = isLocked;
		isLocked = true;
		Invoke("TryUnlock", duration);
	}

	public void TemporaryClose(float duration)
	{
		wasOpen = isOpen;
		Close();
		Invoke("TryOpen", duration);
	}

	private void TryOpen()
	{
		if (wasOpen)
		{
			Open();
		}
	}

	private void TryUnlock()
	{
		isLocked = wasLocked;
	}

	public bool Open()
	{
		if (!isOpen)
		{
			PlayOpenAnimation(openState: true, animTime);
			return true;
		}
		return false;
	}

	public bool Close()
	{
		if (isOpen)
		{
			PlayOpenAnimation(openState: false, animTime);
			return true;
		}
		return false;
	}

	public void LockDoors()
	{
		if (canLock)
		{
			isLocked = true;
		}
	}

	public void UnlockDoors()
	{
		if (canLock)
		{
			isLocked = false;
		}
	}
}
