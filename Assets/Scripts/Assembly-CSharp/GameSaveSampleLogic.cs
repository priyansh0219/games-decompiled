using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xbox;
using UnityEngine;
using UnityEngine.UI;

public class GameSaveSampleLogic : MonoBehaviour
{
	[Serializable]
	private class PlayerSaveData
	{
		public string name;

		public int level;
	}

	public Text output;

	private PlayerSaveData playerSaveData;

	private void Start()
	{
		playerSaveData = new PlayerSaveData();
		playerSaveData.name = "Jane Doe";
		playerSaveData.level = 2;
	}

	public void Save()
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream memoryStream = new MemoryStream())
		{
			binaryFormatter.Serialize(memoryStream, playerSaveData);
			Gdk.Helpers.Save(memoryStream.ToArray());
			output.text = "\n Saved game data:\n Name: " + playerSaveData.name + "\n Level: " + playerSaveData.level;
		}
	}

	public void Load()
	{
		Gdk.Helpers.OnGameSaveLoaded -= OnGameSaveLoaded;
		Gdk.Helpers.OnGameSaveLoaded += OnGameSaveLoaded;
		Gdk.Helpers.LoadSaveData();
	}

	private void OnGameSaveLoaded(object sender, GameSaveLoadedArgs saveData)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream serializationStream = new MemoryStream(saveData.Data))
		{
			object obj = binaryFormatter.Deserialize(serializationStream);
			playerSaveData = obj as PlayerSaveData;
			output.text = "\n Loaded save game:\n Name: " + playerSaveData.name + "\n Level: " + playerSaveData.level;
		}
	}
}
