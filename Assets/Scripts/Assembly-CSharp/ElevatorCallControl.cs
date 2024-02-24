using UnityEngine;
using UnityEngine.UI;

[SkipProtoContractCheck]
public class ElevatorCallControl : HandTarget, IHandTarget
{
	public bool elevatorUp;

	[AssertNotNull]
	public Rocket rocket;

	[AssertNotNull]
	public Transform arrowIconHolder;

	[AssertNotNull]
	public Image elevatorIcon;

	[AssertNotNull]
	public RectTransform bottom;

	[AssertNotNull]
	public RectTransform top;

	private int arrowIndex;

	private bool animating;

	[AssertLocalization]
	private const string callElevatorHandText = "CallElevator";

	private void Start()
	{
		SetElevatorPosition();
	}

	private void SetElevatorPosition()
	{
		elevatorIcon.rectTransform.localPosition = Vector3.Lerp(bottom.localPosition, top.localPosition, rocket.elevatorPosition);
	}

	private void Update()
	{
		bool flag = true;
		if (rocket.elevatorState == Rocket.RocketElevatorStates.AtBottom)
		{
			flag = false;
		}
		if (rocket.elevatorState == Rocket.RocketElevatorStates.AtTop)
		{
			flag = false;
		}
		if (flag)
		{
			SetElevatorPosition();
		}
		flag = false;
		if (elevatorUp && rocket.elevatorState == Rocket.RocketElevatorStates.Up)
		{
			flag = true;
		}
		if (!elevatorUp && rocket.elevatorState == Rocket.RocketElevatorStates.Down)
		{
			flag = true;
		}
		if (flag)
		{
			if (!animating)
			{
				animating = true;
				InvokeRepeating("CycleArrows", 0f, 0.5f);
			}
		}
		else
		{
			if (!animating)
			{
				return;
			}
			animating = false;
			foreach (Transform item in arrowIconHolder)
			{
				Color color = item.GetComponent<Image>().color;
				item.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.25f);
			}
			CancelInvoke("CycleArrows");
		}
	}

	private void CycleArrows()
	{
		int num = 0;
		foreach (Transform item in arrowIconHolder)
		{
			Color color = item.GetComponent<Image>().color;
			if (num == arrowIndex)
			{
				item.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 1f);
			}
			else
			{
				item.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.25f);
			}
			num++;
		}
		arrowIndex++;
		if (arrowIndex > arrowIconHolder.childCount - 1)
		{
			arrowIndex = 0;
		}
	}

	public void OnHandHover(GUIHand hand)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, "CallElevator", translate: true, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Interact);
	}

	public void OnHandClick(GUIHand hand)
	{
		rocket.CallElevator(elevatorUp);
	}
}
