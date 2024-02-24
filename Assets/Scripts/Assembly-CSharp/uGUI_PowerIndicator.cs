using TMPro;
using UnityEngine;

public class uGUI_PowerIndicator : MonoBehaviour
{
	[AssertNotNull]
	public TextMeshProUGUI text;

	private bool initialized;

	private int cachedPower = -1;

	private int cachedMaxPower = -1;

	private PowerSystem.Status cachedStatus = PowerSystem.Status.Normal;

	[AssertLocalization(2)]
	private const string hudPowerStatusFormatKey = "HUDPowerStatus";

	private void OnDisable()
	{
		Deinitialize();
	}

	private void LateUpdate()
	{
		Initialize();
		UpdatePower();
	}

	private void Initialize()
	{
		if (!initialized && !(Player.main == null))
		{
			initialized = true;
		}
	}

	private void Deinitialize()
	{
		if (initialized)
		{
			initialized = false;
		}
	}

	private bool IsPowerEnabled(out int power, out int maxPower, out PowerSystem.Status status)
	{
		power = 0;
		maxPower = 0;
		status = PowerSystem.Status.Offline;
		if (!initialized)
		{
			return false;
		}
		if (!uGUI.isMainLevel)
		{
			return false;
		}
		if (uGUI.isIntro)
		{
			return false;
		}
		if (LaunchRocket.isLaunching)
		{
			return false;
		}
		if (!GameModeUtils.RequiresPower())
		{
			return false;
		}
		Player main = Player.main;
		if (main == null || main.cinematicModeActive)
		{
			return false;
		}
		PDA pDA = main.GetPDA();
		if (pDA != null && pDA.isInUse)
		{
			return false;
		}
		if (main.escapePod.value)
		{
			EscapePod currentEscapePod = main.currentEscapePod;
			if (currentEscapePod != null)
			{
				PowerRelay component = currentEscapePod.GetComponent<PowerRelay>();
				if (component != null)
				{
					power = Mathf.RoundToInt(component.GetPower());
					maxPower = Mathf.RoundToInt(component.GetMaxPower());
					status = component.GetPowerStatus();
					return true;
				}
			}
		}
		SubRoot currentSub = main.currentSub;
		if (currentSub != null && currentSub.isBase)
		{
			uGUI_CameraDrone main2 = uGUI_CameraDrone.main;
			if (main2 != null && main2.GetCamera() != null)
			{
				return false;
			}
			PowerRelay powerRelay = currentSub.powerRelay;
			if (powerRelay != null)
			{
				power = Mathf.RoundToInt(powerRelay.GetPower());
				maxPower = Mathf.RoundToInt(powerRelay.GetMaxPower());
				status = powerRelay.GetPowerStatus();
				return true;
			}
		}
		return false;
	}

	private void UpdatePower()
	{
		int power;
		int maxPower;
		PowerSystem.Status status;
		bool flag = IsPowerEnabled(out power, out maxPower, out status);
		text.enabled = flag;
		if (!flag)
		{
			return;
		}
		if (cachedPower != power || cachedMaxPower != maxPower)
		{
			cachedPower = power;
			cachedMaxPower = maxPower;
			text.text = Language.main.GetFormat("HUDPowerStatus", cachedPower, cachedMaxPower);
		}
		if (cachedStatus != status)
		{
			cachedStatus = status;
			switch (cachedStatus)
			{
			case PowerSystem.Status.Offline:
				text.color = Color.red;
				break;
			case PowerSystem.Status.Emergency:
				text.color = Color.yellow;
				break;
			case PowerSystem.Status.Normal:
				text.color = Color.white;
				break;
			}
		}
	}
}
