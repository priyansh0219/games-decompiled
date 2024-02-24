using UnityEngine;

public class ScaleVariance : MonoBehaviour
{
	public float maxScale = 1.25f;

	public float minScale = 0.75f;

	private void Start()
	{
		float num = minScale + Random.value * (maxScale - minScale);
		base.transform.localScale = base.transform.localScale * num;
		Debug.Log(base.gameObject.name + ".ScaleVariance.Start() - scaling by " + num);
	}
}
