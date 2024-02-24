using UnityEngine;

public class UpdateScheduler : MonoBehaviour
{
	[SerializeField]
	private float updateFrequency;

	private float updateTimer;

	public readonly ScheduledUpdateBehaviourSet updateSet = new ScheduledUpdateBehaviourSet();

	private int currentPositionInSet;

	public static UpdateScheduler Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	private void Update()
	{
		updateTimer += Time.deltaTime;
		int value = Mathf.CeilToInt(updateTimer / updateFrequency * (float)updateSet.Count) - currentPositionInSet;
		int max = updateSet.Count - currentPositionInSet;
		value = Mathf.Clamp(value, 0, max);
		updateSet.CallUpdate(currentPositionInSet, value);
		currentPositionInSet += value;
		if (updateTimer >= updateFrequency)
		{
			currentPositionInSet = 0;
		}
		while (updateTimer >= updateFrequency)
		{
			updateTimer -= updateFrequency;
		}
	}

	public void DebugGUI()
	{
		BehaviourUpdateManager.DebugGUI("Update Scheduler", updateSet);
	}
}
