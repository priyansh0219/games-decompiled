using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class SwimFigureEight : CreatureAction
{
	public float swimVelocity = 2f;

	public float figureScale = 1f;

	public float timePerPoint = 2f;

	private float timeNextPoint = -1f;

	private int pointIndex;

	private Vector3[] points = new Vector3[14]
	{
		new Vector3(1f, 0f, 0f),
		new Vector3(3f, 0f, 3f),
		new Vector3(1f, 0f, 5f),
		new Vector3(0f, 2f, 5f),
		new Vector3(-1f, 0f, 5f),
		new Vector3(-3f, 0f, 3f),
		new Vector3(-1f, 0f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(3f, 0f, -3f),
		new Vector3(1f, 0f, -5f),
		new Vector3(0f, 2f, -5f),
		new Vector3(-1f, 0f, -5f),
		new Vector3(-3f, 0f, -3f),
		new Vector3(-1f, 0f, 0f)
	};

	private Vector3 origin;

	private float timeNextSwim;

	public override void StartPerform(Creature creature, float time)
	{
		origin = creature.transform.position;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (time > timeNextPoint)
		{
			pointIndex = (pointIndex + 1) % points.Length;
			timeNextPoint = time + timePerPoint;
		}
		Vector3 targetPosition = origin + points[pointIndex] * figureScale;
		base.swimBehaviour.SwimTo(targetPosition, swimVelocity);
	}

	public void OnDrawGizmos()
	{
		Vector3 start = origin;
		for (int i = 0; i < points.Length; i++)
		{
			Vector3 vector = origin + points[i] * figureScale;
			Debug.DrawLine(start, vector, Color.magenta);
			start = vector;
		}
		Vector3 end = origin + points[pointIndex] * figureScale;
		Debug.DrawLine(base.transform.position, end, Color.white);
	}
}
