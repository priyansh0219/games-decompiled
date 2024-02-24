using System;
using UnityEngine;

public class LightShadowQuality : MonoBehaviour
{
	[Serializable]
	public class ShadowQualityPair
	{
		public int qualitySetting;

		public LightShadows lightShadows;
	}

	public Light light;

	public ShadowQualityPair[] qualityPairs;

	public int currentQualitySettings;

	private void Start()
	{
		UpdateSettings(QualitySettings.GetQualityLevel());
		InvokeRepeating("CheckSettings", 0f, 0.5f);
	}

	private void CheckSettings()
	{
		if (base.gameObject.activeInHierarchy)
		{
			int qualityLevel = QualitySettings.GetQualityLevel();
			if (qualityLevel != currentQualitySettings)
			{
				UpdateSettings(qualityLevel);
			}
		}
	}

	private void UpdateSettings(int newQualityLevel)
	{
		for (int i = 0; i < qualityPairs.Length; i++)
		{
			if (qualityPairs[i].qualitySetting == newQualityLevel)
			{
				light.shadows = qualityPairs[i].lightShadows;
			}
		}
		currentQualitySettings = newQualityLevel;
	}
}
