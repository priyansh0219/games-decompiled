using System.Collections.Generic;
using Gendarme;
using UnityEngine;

public class SkyApplierUpdater : MonoBehaviour
{
	public const int numSlices = 10;

	public const int invalidIndex = -1;

	private static SkyApplierUpdater instance;

	private List<SkyApplier> skyAppliers = new List<SkyApplier>();

	public static SkyApplierUpdater main => instance;

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	protected void Awake()
	{
		instance = this;
	}

	public void Add(SkyApplier skyApplier)
	{
		skyApplier.updaterIndex = skyAppliers.Count;
		skyAppliers.Add(skyApplier);
	}

	public void Remove(SkyApplier skyApplier)
	{
		int updaterIndex = skyApplier.updaterIndex;
		int index = skyAppliers.Count - 1;
		skyAppliers[updaterIndex] = skyAppliers[index];
		skyAppliers[updaterIndex].updaterIndex = updaterIndex;
		skyAppliers.RemoveAt(index);
		skyApplier.updaterIndex = -1;
	}

	private void Update()
	{
		int num = (skyAppliers.Count + 10 - 1) / 10;
		int num2 = Time.frameCount % 10 * num;
		int num3 = num2 + num;
		for (int i = num2; i < num3 && i < skyAppliers.Count; i++)
		{
			skyAppliers[i].UpdateSkyIfNecessary();
		}
	}
}
