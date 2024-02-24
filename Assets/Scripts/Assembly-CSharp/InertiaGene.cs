using UnityEngine;

public class InertiaGene : Gene
{
	private Rigidbody host;

	private float originalDrag;

	private void Start()
	{
		Rigidbody component = base.gameObject.GetComponent<Rigidbody>();
		if ((bool)component)
		{
			host = component;
			originalDrag = host.drag;
			onChangedEvent.AddHandler(base.gameObject, DragScalarChanged);
			Debug.Log("Original drag: " + originalDrag);
		}
	}

	private void DragScalarChanged(float dragScalar)
	{
		host.drag = Mathf.Max(originalDrag - dragScalar * originalDrag, 0f);
		Debug.Log("Setting " + base.gameObject.name + " drag : " + dragScalar + " => " + host.drag);
	}

	protected override void OnDestroy()
	{
		host.drag = originalDrag;
		base.OnDestroy();
	}
}
