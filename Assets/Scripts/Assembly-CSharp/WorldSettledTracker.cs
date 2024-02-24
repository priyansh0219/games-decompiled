using System;
using UnityEngine;

public class WorldSettledTracker : MonoBehaviour
{
	public Vector3 boundsSize = new Vector3(5f, 5f, 5f);

	public bool worldSettled { get; private set; }

	public event Action OnWorldSettledChanged;

	private void FixedUpdate()
	{
		bool num = worldSettled;
		worldSettled = LargeWorldStreamer.main.IsRangeActiveAndBuilt(new Bounds(base.transform.position, boundsSize));
		if (num != worldSettled && this.OnWorldSettledChanged != null)
		{
			this.OnWorldSettledChanged();
		}
	}
}
