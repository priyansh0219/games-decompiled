using UnityEngine;

[RequireComponent(typeof(PlayerDistanceTracker))]
public class RadiatePlayerInRange : MonoBehaviour
{
	public float radiateRadius = 20f;

	private PlayerDistanceTracker tracker;

	private void Start()
	{
		tracker = GetComponent<PlayerDistanceTracker>();
		InvokeRepeating("Radiate", 0f, 0.2f);
		DevConsole.RegisterConsoleCommand(this, "radiation");
	}

	private void Radiate()
	{
		bool flag = GameModeUtils.HasRadiation() && (NoDamageConsoleCommand.main == null || !NoDamageConsoleCommand.main.GetNoDamageCheat());
		float distanceToPlayer = tracker.distanceToPlayer;
		if (distanceToPlayer <= radiateRadius && flag && radiateRadius > 0f)
		{
			float num = Mathf.Clamp01(1f - distanceToPlayer / radiateRadius);
			float num2 = num;
			if (Inventory.main.equipment.GetCount(TechType.RadiationSuit) > 0)
			{
				num -= num2 * 0.5f;
			}
			if (Inventory.main.equipment.GetCount(TechType.RadiationHelmet) > 0)
			{
				num -= num2 * 0.23f * 2f;
			}
			if (Inventory.main.equipment.GetCount(TechType.RadiationGloves) > 0)
			{
				num -= num2 * 0.23f;
			}
			num = Mathf.Clamp01(num);
			Player.main.SetRadiationAmount(num);
		}
		else
		{
			Player.main.SetRadiationAmount(0f);
		}
	}

	private void OnConsoleCommand_radiation()
	{
		GameModeUtils.ToggleCheat(GameModeOption.NoRadiation);
		ErrorMessage.AddDebug("radiation is now " + !GameModeUtils.IsCheatActive(GameModeOption.NoRadiation));
		GetComponent<DamagePlayerInRadius>().enabled = GameModeUtils.HasRadiation();
	}
}
