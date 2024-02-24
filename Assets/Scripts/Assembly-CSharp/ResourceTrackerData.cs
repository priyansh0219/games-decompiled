using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceTrackerData.asset", menuName = "Subnautica/Create Resource Tracker Data")]
public class ResourceTrackerData : ScriptableObject, ICompileTimeCheckable, ILocalizationCheckable
{
	[Serializable]
	public class TechTooltip
	{
		public TechType techType;

		public string text;
	}

	[Tooltip("List of TechTypes that the Mineral Detector should not be able to track.")]
	public List<TechType> undetectableTechTypes;

	[Tooltip("List of tooltips for scanned techs for the Mineral Detector.")]
	public TechTooltip[] mineralDetectorScannedTooltips;

	public string CompileTimeCheck()
	{
		HashSet<TechType> hashSet = new HashSet<TechType>();
		for (int i = 0; i < mineralDetectorScannedTooltips.Length; i++)
		{
			if (!hashSet.Add(mineralDetectorScannedTooltips[i].techType))
			{
				return $"ResourceTrackerData has a duplicate Tech Type {mineralDetectorScannedTooltips[i].techType} in Mineral Detector Scanned Tooltips at index {i}.";
			}
		}
		return null;
	}

	public string CompileTimeCheck(ILanguage language)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < mineralDetectorScannedTooltips.Length; i++)
		{
			string text = language.CheckKey(mineralDetectorScannedTooltips[i].text);
			if (text != null)
			{
				stringBuilder.Append(text);
			}
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Insert(0, "ResourceTrackerData has a localization issue in Mineral Detector Scanned Tooltips: ");
			return stringBuilder.ToString();
		}
		return null;
	}
}
