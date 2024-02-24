using System.Collections.Generic;
using UnityEngine;

public class FloatersTarget : MonoBehaviour
{
	private LiveMixin liveMixin;

	private List<Floater> attachedFloaters = new List<Floater>();

	private void OnEnable()
	{
		liveMixin = GetComponent<LiveMixin>();
	}

	public void OnFloaterAttached(Floater floater)
	{
		attachedFloaters.Add(floater);
		floater.transform.parent = base.transform;
		if (LargeWorld.main != null && LargeWorld.main.streamer != null && LargeWorld.main.streamer.cellManager != null)
		{
			LargeWorld.main.streamer.cellManager.UnregisterEntity(floater.gameObject);
		}
	}

	public void OnFloaterDetached(Floater floater)
	{
		attachedFloaters.Remove(floater);
		if (LargeWorld.main != null && LargeWorld.main.streamer != null && LargeWorld.main.streamer.cellManager != null)
		{
			LargeWorld.main.streamer.cellManager.RegisterEntity(floater.gameObject);
		}
	}

	private void DetachAll()
	{
		for (int num = attachedFloaters.Count - 1; num >= 0; num--)
		{
			if (attachedFloaters[num] != null && attachedFloaters[num].enabled)
			{
				attachedFloaters[num].Disconnect();
			}
		}
		attachedFloaters.Clear();
	}

	public void OnKill()
	{
		if (liveMixin != null && liveMixin.destroyOnDeath)
		{
			DetachAll();
		}
	}

	private void OnExamine()
	{
		DetachAll();
	}
}
