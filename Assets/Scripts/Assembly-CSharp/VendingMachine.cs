using UnityEngine;

public class VendingMachine : MonoBehaviour
{
	[AssertNotNull]
	public TechType[] snacks;

	public float useInterval = 1f;

	public FMODAsset useSound;

	private PowerRelay powerRelay;

	private float timeLastUse;

	private void Start()
	{
		powerRelay = PowerSource.FindRelay(base.transform);
	}

	public void OnHover(HandTargetEventData eventData)
	{
		if (GetCanBeUsed())
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "Use", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Interact);
		}
	}

	public void OnUse(HandTargetEventData eventData)
	{
		if (GetCanBeUsed())
		{
			timeLastUse = Time.time;
			CraftData.AddToInventory(snacks.GetRandom(), 1, noMessage: false, spawnIfCantAdd: false);
			if (useSound != null)
			{
				FMODUWE.PlayOneShot(useSound, base.transform.position);
			}
		}
	}

	private bool GetCanBeUsed()
	{
		if (Time.time < timeLastUse + useInterval)
		{
			return false;
		}
		if (powerRelay == null || !powerRelay.IsPowered())
		{
			return false;
		}
		return true;
	}
}
