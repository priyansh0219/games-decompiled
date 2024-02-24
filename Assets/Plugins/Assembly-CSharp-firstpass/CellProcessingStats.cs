public class CellProcessingStats
{
	public int inId;

	public float inTime;

	public float inAngle;

	public float inDistance;

	public float inPriority;

	public int outId;

	public float outTime;

	public float outAngle;

	public float outDistance;

	public float outPriority;

	public int inQueueLength;

	public int numPriorityChanges;

	public float timeToProcess => outTime - inTime;

	public float deltaPriority => outPriority - inPriority;

	public float deltaDistance => outDistance - inDistance;

	public float deltaAngle => outAngle - inAngle;
}
