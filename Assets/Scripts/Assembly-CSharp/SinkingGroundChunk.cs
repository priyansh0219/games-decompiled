using UnityEngine;

public class SinkingGroundChunk : MonoBehaviour
{
	private enum Stage
	{
		toSurface = 0,
		Sinking = 1
	}

	public Transform modelTransform;

	public float toSurfaceTime = 1f;

	public float sinkTime = 60f;

	public float sinkHeight = 1f;

	private Stage stage;

	private Vector3 onSurfaceModelPosition;

	private Vector3 sinkedModelPosition;

	private float startTime;

	private void Start()
	{
		onSurfaceModelPosition = modelTransform.localPosition;
		sinkedModelPosition = modelTransform.localPosition - Vector3.up * sinkHeight;
		modelTransform.localPosition = sinkedModelPosition;
		stage = Stage.toSurface;
		startTime = Time.time;
	}

	private void Update()
	{
		if (stage == Stage.toSurface)
		{
			float num = Mathf.Clamp01((Time.time - startTime) / toSurfaceTime);
			modelTransform.localPosition = Vector3.Slerp(sinkedModelPosition, onSurfaceModelPosition, num);
			if (num == 1f)
			{
				stage = Stage.Sinking;
			}
		}
		else
		{
			float num2 = Mathf.Clamp01((Time.time - (startTime + toSurfaceTime)) / sinkTime);
			modelTransform.localPosition = Vector3.Lerp(onSurfaceModelPosition, sinkedModelPosition, num2);
			if (num2 == 1f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
