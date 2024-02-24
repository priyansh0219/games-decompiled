using UWE;
using UnityEngine;

public class RocketAtmoTrigger : MonoBehaviour
{
	[AssertNotNull]
	public FMOD_CustomEmitter atmoSFX;

	[AssertNotNull]
	public Collider trigger;

	private bool playerInside;

	private void Start()
	{
		InvokeRepeating("CheckPlayerPosition", 0f, 1f);
	}

	private void CheckPlayerPosition()
	{
		Player main = Player.main;
		if (main != null)
		{
			SetPlayerInside(main, UWE.Utils.IsInsideCollider(trigger, main.transform.position));
		}
	}

	private void SetPlayerInside(Player player, bool state)
	{
		if (state != playerInside)
		{
			playerInside = state;
			player.precursorOutOfWater = playerInside;
			if (playerInside)
			{
				atmoSFX.Play();
			}
			else
			{
				atmoSFX.Stop();
			}
		}
	}
}
