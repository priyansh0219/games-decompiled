using UnityEngine;

public class EcoManager : MonoBehaviour
{
	public static EcoManager main;

	public static bool debugFreeze;

	public static bool debugProfileEvents;

	public BiomeGroup debugBiomeGroup;

	public AnimationCurve kSeasonalPhytoPlankton;

	public GameObject ecoEventFeedbackPrefab;

	private EcoEventFeedback eventFeedback;

	public bool debugVerbose;

	private void Start()
	{
		main = this;
		GameObject gameObject = Object.Instantiate(ecoEventFeedbackPrefab);
		eventFeedback = gameObject.GetComponent<EcoEventFeedback>();
		gameObject.transform.parent = base.gameObject.transform;
		DevConsole.RegisterConsoleCommand(this, "dbslots");
		DevConsole.RegisterConsoleCommand(this, "dbregion");
		DevConsole.RegisterConsoleCommand(this, "ecoevent");
		DevConsole.RegisterConsoleCommand(this, "evprof");
		DevConsole.RegisterConsoleCommand(this, "evdb");
	}

	private void OnConsoleCommand_evdb(NotificationCenter.Notification n)
	{
	}

	private void OnConsoleCommand_evprof()
	{
		debugProfileEvents = !debugProfileEvents;
	}

	private void OnConsoleCommand_dbslots()
	{
		EntitySlot.debugSlots = true;
	}

	private void OnConsoleCommand_dbregion()
	{
		EcoRegion region = EcoRegionManager.main.GetRegion(MainCamera.camera.transform.position);
		if (region != null)
		{
			region.LogDebugInfo();
		}
		else
		{
			Debug.Log("No valid region found at current cam pos");
		}
	}

	private void OnConsoleCommand_ecoevent()
	{
	}

	public void AddEventFeedback(EcoEvent e)
	{
		eventFeedback.AddEventFeedback(e);
	}

	private void Update()
	{
		if (!debugFreeze)
		{
			EcoRegionManager.main.EcoUpdate();
		}
	}
}
