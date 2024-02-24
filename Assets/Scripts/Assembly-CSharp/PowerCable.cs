using UWE;
using UnityEngine;

public class PowerCable : HandTarget, IHandTarget, IRopeProperties
{
	public GameObject physics;

	private PowerGenerator _generator;

	private PowerRelay _relay;

	private Player player;

	private Vector3 startPosition;

	private Vector3 endPosition;

	private float length;

	[AssertLocalization]
	private const string powerCableInstructionsHandText = "PowerCableInstructions";

	[AssertLocalization]
	private const string pickupCableHandText = "PickupCable";

	public void StartDragging(Player p)
	{
		player = p;
		physics.SetActive(value: false);
	}

	public void StopDragging()
	{
		endPosition = player.GetComponent<Inventory>().toolSocket.transform.position;
		physics.transform.position = endPosition;
		physics.SetActive(value: true);
		player = null;
	}

	private void TryAttach()
	{
		UWE.Utils.TraceForFPSTarget(player.gameObject, 4f, 0.15f, out var closestObj, out var _);
		if (closestObj != null)
		{
			PowerPlug component = closestObj.GetComponent<PowerPlug>();
			if (component != null && !component.occupied)
			{
				component.occupied = true;
				player = null;
				endPosition = component.transform.position;
			}
		}
	}

	private void Update()
	{
		if (player != null && Player.main == player)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "PowerCableInstructions", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
			if (GameInput.GetButtonHeld(GameInput.Button.LeftHand))
			{
				TryAttach();
			}
			if (GameInput.GetButtonHeld(GameInput.Button.RightHand))
			{
				StopDragging();
			}
		}
	}

	public void OnHandHover(GUIHand hand)
	{
		if (player == null)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "PickupCable", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (player == null)
		{
			StartDragging(hand.player);
		}
	}

	private void Start()
	{
		startPosition = base.transform.position;
		endPosition = startPosition;
		length = 0f;
	}

	public Vector3 GetStartPosition()
	{
		return startPosition;
	}

	public Vector3 GetEndPosition()
	{
		if (player != null)
		{
			endPosition = player.GetComponent<Inventory>().toolSocket.transform.position;
		}
		return endPosition;
	}

	public float GetLength()
	{
		return (endPosition - startPosition).magnitude;
	}
}
