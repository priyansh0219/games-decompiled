using System.Collections.Generic;
using UnityEngine;

public class PDAData : ScriptableObject
{
	public const string assetPath = "Assets/Prefabs/PDA/PDAData.asset";

	public Sprite defaultLogIcon;

	public List<PDALog.EntryData> log = new List<PDALog.EntryData>();

	public List<PDAEncyclopedia.EntryData> encyclopedia = new List<PDAEncyclopedia.EntryData>();

	public List<PDAScanner.EntryData> scanner = new List<PDAScanner.EntryData>();

	public List<TechType> defaultTech = new List<TechType>();

	public List<KnownTech.AnalysisTech> analysisTech = new List<KnownTech.AnalysisTech>();

	public List<KnownTech.CompoundTech> compoundTech = new List<KnownTech.CompoundTech>();

	public static void Initialize(PDAData pdaData)
	{
		pdaData = Object.Instantiate(pdaData);
		PDALog.Initialize(pdaData);
		PDAEncyclopedia.Initialize(pdaData);
		PDAScanner.Initialize(pdaData);
		KnownTech.Initialize(pdaData);
	}

	public static void Deinitialize()
	{
		PDALog.Deinitialize();
		PDAEncyclopedia.Deinitialize();
		PDAScanner.Deinitialize();
		KnownTech.Deinitialize();
	}
}
