using UnityEngine;

public class BallastWeight : MonoBehaviour
{
	public float maxVolume = 10f;

	public Vector3 visualScaleDelta;

	public GameObject weightModel;

	private float localFraction;

	private float masterFraction;

	private Vector3 origLocalScale;

	private float origVolume;

	public void Awake()
	{
		origLocalScale = weightModel.transform.localScale;
		origVolume = GetComponent<Buoyancy>().volume;
	}

	public void OnUseMove(float fraction)
	{
		localFraction = fraction;
	}

	public void SetMasterBallastFraction(float val)
	{
		masterFraction = Mathf.Clamp01(val);
	}

	private void Update()
	{
		float num = masterFraction + localFraction;
		GetComponent<Buoyancy>().volume = origVolume - num * maxVolume;
		weightModel.transform.localScale = origLocalScale + num * visualScaleDelta;
	}

	public static void UpdateAllWeights(SubRoot sub, Vector3 ssHalfSpaceNormal, float masterFraction)
	{
		BallastWeight[] componentsInChildren = sub.GetComponentsInChildren<BallastWeight>();
		foreach (BallastWeight ballastWeight in componentsInChildren)
		{
			Vector3 worldCenterOfMass = sub.GetWorldCenterOfMass();
			Vector3 vector = ballastWeight.transform.position - worldCenterOfMass;
			Vector3 rhs = sub.transform.InverseTransformDirection(vector);
			Debug.DrawLine(worldCenterOfMass, worldCenterOfMass + vector, Color.yellow);
			if (Vector3.Dot(ssHalfSpaceNormal, rhs) > 0f)
			{
				ballastWeight.SetMasterBallastFraction(masterFraction);
			}
		}
	}

	public static void UpdateAllWeightsWS(SubRoot sub, Vector3 wsHalfSpaceNormal, float masterFraction)
	{
		BallastWeight[] componentsInChildren = sub.GetComponentsInChildren<BallastWeight>();
		foreach (BallastWeight ballastWeight in componentsInChildren)
		{
			Vector3 worldCenterOfMass = sub.GetWorldCenterOfMass();
			Vector3 vector = ballastWeight.transform.position - worldCenterOfMass;
			Debug.DrawLine(worldCenterOfMass, worldCenterOfMass + vector, Color.yellow);
			if (Vector3.Dot(wsHalfSpaceNormal, vector) > 0f)
			{
				ballastWeight.SetMasterBallastFraction(masterFraction);
			}
		}
	}
}
