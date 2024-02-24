using System.Collections.Generic;
using UWE;
using UnityEngine;

public class LargeWorldEntityUpdater : MonoBehaviour
{
	public const int numSlices = 100;

	public const int invalidIndex = -1;

	private static LargeWorldEntityUpdater instance;

	private readonly List<LargeWorldEntity> largeWorldEntities = new List<LargeWorldEntity>();

	public static LargeWorldEntityUpdater main => instance;

	protected void Awake()
	{
		instance = this;
	}

	public void Add(LargeWorldEntity largeWorldEntity)
	{
		largeWorldEntity.updaterIndex = largeWorldEntities.Count;
		largeWorldEntities.Add(largeWorldEntity);
	}

	public void Remove(LargeWorldEntity largeWorldEntity)
	{
		int updaterIndex = largeWorldEntity.updaterIndex;
		int index = largeWorldEntities.Count - 1;
		largeWorldEntities[updaterIndex] = largeWorldEntities[index];
		largeWorldEntities[updaterIndex].updaterIndex = updaterIndex;
		largeWorldEntities.RemoveAt(index);
		largeWorldEntity.updaterIndex = -1;
	}

	private void Update()
	{
		LargeWorldStreamer largeWorldStreamer = LargeWorldStreamer.main;
		if ((bool)largeWorldStreamer && largeWorldStreamer.IsReady())
		{
			int num = UWE.Utils.CeilDiv(largeWorldEntities.Count, 100);
			int num2 = Time.frameCount % 100 * num;
			int num3 = num2 + num;
			for (int i = num2; i < num3 && i < largeWorldEntities.Count; i++)
			{
				largeWorldEntities[i].UpdateCell(largeWorldStreamer);
			}
		}
	}
}
