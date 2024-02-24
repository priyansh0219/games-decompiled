using UnityEngine;

public class CoffeeVendingMachine : MonoBehaviour
{
	public float useInterval = 19f;

	public VFXController vfxController;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter idleSound;

	[AssertNotNull]
	public FMOD_CustomEmitter waterSoundSlot1;

	[AssertNotNull]
	public FMOD_CustomEmitter waterSoundSlot2;

	[AssertNotNull]
	public string hoverText = "Use";

	public float spawnDelay = 17f;

	public float maxDistToPlayer = 20f;

	private float timeLastUseSlot1;

	private float timeLastUseSlot2;

	private PowerRelay powerRelay;

	private void Start()
	{
		powerRelay = PowerSource.FindRelay(base.transform);
		idleSound.Play();
	}

	private void OnDisable()
	{
		timeLastUseSlot1 = 0f;
		timeLastUseSlot2 = 0f;
		CancelInvoke();
	}

	public void OnHover(HandTargetEventData eventData)
	{
		if (base.enabled && !(powerRelay == null) && powerRelay.IsPowered() && (Time.time > timeLastUseSlot1 + useInterval || Time.time > timeLastUseSlot2 + useInterval))
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, hoverText, translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Interact);
		}
	}

	public void OnMachineUse(HandTargetEventData eventData)
	{
		if (base.enabled && !(powerRelay == null) && powerRelay.IsPowered())
		{
			if (Time.time > timeLastUseSlot1 + useInterval)
			{
				vfxController.Play(0);
				waterSoundSlot1.Play();
				timeLastUseSlot1 = Time.time;
				Invoke("SpawnCoffee", spawnDelay);
			}
			else if (Time.time > timeLastUseSlot2 + useInterval)
			{
				vfxController.Play(1);
				waterSoundSlot2.Play();
				timeLastUseSlot2 = Time.time;
				Invoke("SpawnCoffee", spawnDelay);
			}
		}
	}

	private void SpawnCoffee()
	{
		GameObject localPlayer = Utils.GetLocalPlayer();
		if (Vector3.Distance(base.transform.position, localPlayer.transform.position) < maxDistToPlayer)
		{
			CraftData.AddToInventory(TechType.Coffee, 1, noMessage: false, spawnIfCantAdd: false);
		}
	}
}
