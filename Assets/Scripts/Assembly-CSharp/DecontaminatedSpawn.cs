using UnityEngine;

public class DecontaminatedSpawn : DelayedSpawn
{
	private void Update()
	{
		LargeWorldStreamer main = LargeWorldStreamer.main;
		if ((bool)main && main.IsReady())
		{
			LeakingRadiation main2 = LeakingRadiation.main;
			if ((bool)main2 && !(main2.currentRadius > 0f))
			{
				Spawn();
				Object.Destroy(base.gameObject);
			}
		}
	}
}
