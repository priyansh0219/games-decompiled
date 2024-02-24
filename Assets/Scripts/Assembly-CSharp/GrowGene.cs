using UnityEngine;

public class GrowGene : Gene
{
	private Vector3 initialLocalScale;

	private void Start()
	{
		onChangedEvent.AddHandler(base.gameObject, OnChanged);
		initialLocalScale = base.transform.localScale;
	}

	private void OnChanged(float newScalar)
	{
		float num = 3f;
		float num2 = 1f + newScalar * num;
		Debug.Log("GrowGene scaling " + base.gameObject.name + " by " + num2);
		base.transform.localScale = new Vector3(initialLocalScale.x * num2, initialLocalScale.y * num2, initialLocalScale.z * num2);
	}

	protected override void OnDestroy()
	{
		Debug.Log("Restoring old scale on " + base.gameObject.name);
		base.transform.localScale = initialLocalScale;
		base.OnDestroy();
	}
}
