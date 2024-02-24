using System.Collections.Generic;
using System.IO;
using UWE;
using UnityEngine;

public class SignalDatabase
{
	private const string csvFilename = "signals.csv";

	private List<SignalInfo> entries;

	public void Save(string dataDir)
	{
		CSVUtils.Save(Path.Combine(dataDir, "signals.csv"), entries);
	}

	public void Load(string dataDir)
	{
		string text = Path.Combine(dataDir, "signals.csv");
		if (File.Exists(text))
		{
			entries = CSVUtils.Load<SignalInfo>(text);
			return;
		}
		Debug.LogWarning("Could not load signal database from file");
		entries = new List<SignalInfo>();
	}

	public SignalInfo GetRandomEntry()
	{
		int index = Random.Range(0, entries.Count);
		return entries[index];
	}

	public void Add(string biome, Int3 batch, Int3 position, string description)
	{
		SignalInfo signalInfo = new SignalInfo();
		signalInfo.biome = biome;
		signalInfo.batch = batch;
		signalInfo.position = position;
		signalInfo.description = description;
		entries.Add(signalInfo);
	}

	public void Remove(SignalInfo info)
	{
		entries.Remove(info);
	}

	public GameObject SpawnEditorPreview()
	{
		GameObject gameObject = new GameObject("Signals");
		foreach (SignalInfo entry in entries)
		{
			GameObject gameObject2 = new GameObject(entry.description);
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.transform.position = entry.position.ToVector3();
			SignalPreview signalPreview = gameObject2.AddComponent<SignalPreview>();
			signalPreview.info = entry;
			signalPreview.description = entry.description;
		}
		return gameObject;
	}
}
