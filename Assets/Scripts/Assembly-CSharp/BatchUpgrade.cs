using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BatchUpgrade
{
	private static readonly IBatchUpgrade[] batchUpgrades = new IBatchUpgrade[30]
	{
		new B54Upgrade(),
		new B55Upgrade(),
		new B56Upgrade(),
		new B57Upgrade(),
		new B58Upgrade(),
		new B59Upgrade(),
		new B60Upgrade(),
		new B61Upgrade(),
		new B63Upgrade(),
		new B64Upgrade(),
		new B65Upgrade(),
		new B66Upgrade(),
		new B68Upgrade(),
		new B69Upgrade(),
		new B70Upgrade(),
		new B71Upgrade(),
		new B72Upgrade(),
		new B73Upgrade(),
		new B74Upgrade(),
		new B75Upgrade(),
		new B75aUpgrade(),
		new B76Upgrade(),
		new B77Upgrade(),
		new B78Upgrade(),
		new B79Upgrade(),
		new B81Upgrade(),
		new B82Upgrade(),
		new B89Upgrade(),
		new BatchUpgrade_70752(),
		new BatchUpgrade_71172()
	};

	public static bool NeedsUpgrade(int changeSet)
	{
		return batchUpgrades.Any((IBatchUpgrade p) => p.GetChangeset() > changeSet);
	}

	public static void UpgradeBatches(int changeSet)
	{
		Debug.LogFormat("--- UpgradeBatches : changeSet {0}", changeSet);
		IBatchUpgrade[] array = batchUpgrades;
		foreach (IBatchUpgrade batchUpgrade in array)
		{
			Debug.LogFormat("    checking upgrade version {0} - wants upgrade? {1}", batchUpgrade.GetChangeset(), batchUpgrade.GetChangeset() > changeSet);
		}
		HashSet<Int3> hashSet = new HashSet<Int3>(batchUpgrades.Where((IBatchUpgrade p) => p.GetChangeset() > changeSet).SelectMany((IBatchUpgrade p) => p.GetBatches()), Int3.equalityComparer);
		List<string> list = new List<string>();
		foreach (Int3 item in hashSet)
		{
			string compiledOctreesCacheFilename = LargeWorldStreamer.GetCompiledOctreesCacheFilename(item);
			list.Add(LargeWorldStreamer.GetCompiledOctreesCachePath(string.Empty, compiledOctreesCacheFilename));
			list.Add(CellManager.GetCacheBatchCellsPath(string.Empty, item));
		}
		SaveLoadManager.main.DeleteFilesInTemporaryStorage(list);
		SaveLoadManager.main.DeleteFilesInTemporaryStorage("batch-objects-*.*");
	}
}
