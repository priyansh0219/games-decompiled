using UnityEngine;

public class WorldReadySpawn : DelayedSpawn
{
	private void Update()
	{
		LargeWorldStreamer main = LargeWorldStreamer.main;
		if ((bool)main && main.IsReady())
		{
			Int3 containingBatch = main.GetContainingBatch(base.transform.position);
			if (main.IsBatchReadyToCompile(containingBatch))
			{
				Spawn();
				Object.Destroy(base.gameObject);
			}
		}
	}
}
