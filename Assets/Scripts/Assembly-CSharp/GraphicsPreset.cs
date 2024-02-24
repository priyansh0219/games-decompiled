using System;
using Gendarme;
using UnityEngine;

[SuppressMessage("Gendarme.Rules.Performance", "AvoidLargeStructureRule")]
public struct GraphicsPreset
{
	public int detail;

	public WaterSurface.Quality waterQuality;

	public int aaMode;

	public int aaQuality;

	public int aoQuality;

	public int ssrQuality;

	public bool bloom;

	public bool bloomLensDirt;

	public bool dof;

	public int motionBlurQuality;

	public bool dithering;

	public int vsyncRate;

	public int dynResTarget;

	private static readonly GraphicsPreset[] presets = new GraphicsPreset[3]
	{
		new GraphicsPreset
		{
			detail = 0,
			waterQuality = WaterSurface.RestrictQualityForSystem(WaterSurface.Quality.Medium),
			aaMode = 0,
			aaQuality = 1,
			ssrQuality = 0,
			aoQuality = 0,
			bloom = true,
			bloomLensDirt = false,
			motionBlurQuality = 0,
			dof = false,
			dithering = true,
			vsyncRate = -1,
			dynResTarget = -1
		},
		new GraphicsPreset
		{
			detail = 1,
			waterQuality = WaterSurface.RestrictQualityForSystem(WaterSurface.Quality.Medium),
			aaMode = 0,
			aaQuality = 2,
			ssrQuality = 0,
			aoQuality = 1,
			bloom = true,
			bloomLensDirt = false,
			motionBlurQuality = 1,
			dof = true,
			dithering = true,
			vsyncRate = -1,
			dynResTarget = -1
		},
		new GraphicsPreset
		{
			detail = 2,
			waterQuality = WaterSurface.RestrictQualityForSystem(WaterSurface.Quality.High),
			aaMode = 0,
			aaQuality = 3,
			ssrQuality = 3,
			aoQuality = 3,
			bloom = true,
			bloomLensDirt = true,
			motionBlurQuality = 3,
			dof = true,
			dithering = true,
			vsyncRate = -1,
			dynResTarget = -1
		}
	};

	private static readonly GraphicsPreset[] consolePresets = new GraphicsPreset[2]
	{
		new GraphicsPreset
		{
			detail = 0,
			waterQuality = WaterSurface.RestrictQualityForSystem(WaterSurface.Quality.High),
			aaMode = 0,
			aaQuality = 1,
			ssrQuality = 0,
			aoQuality = 1,
			bloom = true,
			bloomLensDirt = true,
			motionBlurQuality = 0,
			dof = true,
			dithering = false,
			vsyncRate = -1,
			dynResTarget = -1
		},
		new GraphicsPreset
		{
			detail = 1,
			waterQuality = WaterSurface.RestrictQualityForSystem(WaterSurface.Quality.High),
			aaMode = 0,
			aaQuality = 1,
			ssrQuality = 0,
			aoQuality = 1,
			bloom = true,
			bloomLensDirt = true,
			motionBlurQuality = 0,
			dof = true,
			dithering = false,
			vsyncRate = -1,
			dynResTarget = -1
		}
	};

	private static readonly GraphicsPreset[] switchPresets = new GraphicsPreset[1]
	{
		new GraphicsPreset
		{
			detail = 0,
			waterQuality = WaterSurface.RestrictQualityForSystem(WaterSurface.Quality.Medium),
			aaMode = 0,
			aaQuality = 0,
			ssrQuality = 0,
			aoQuality = 0,
			bloom = false,
			bloomLensDirt = false,
			motionBlurQuality = 0,
			dof = false,
			dithering = false,
			vsyncRate = -1,
			dynResTarget = -1
		}
	};

	private static readonly GraphicsPreset[] nextgenConsolePresets = new GraphicsPreset[2]
	{
		new GraphicsPreset
		{
			detail = 0,
			waterQuality = WaterSurface.RestrictQualityForSystem(WaterSurface.Quality.High),
			aaMode = 0,
			aaQuality = 1,
			ssrQuality = 0,
			aoQuality = 1,
			bloom = true,
			bloomLensDirt = true,
			motionBlurQuality = 0,
			dof = true,
			dithering = false,
			vsyncRate = 1,
			dynResTarget = 60
		},
		new GraphicsPreset
		{
			detail = 0,
			waterQuality = WaterSurface.RestrictQualityForSystem(WaterSurface.Quality.High),
			aaMode = 0,
			aaQuality = 3,
			ssrQuality = 3,
			aoQuality = 3,
			bloom = true,
			bloomLensDirt = true,
			motionBlurQuality = 3,
			dof = true,
			dithering = false,
			vsyncRate = 2,
			dynResTarget = 30
		}
	};

	private const bool SupportsCustomOverride = true;

	private const bool SupportsPresetSelection = true;

	public const bool SupportsVsyncRateChange = false;

	private const bool SupportsDynamicResolution = false;

	public static GraphicsPreset[] GetPresets()
	{
		return presets;
	}

	public void Apply()
	{
		GraphicsUtil.SetQualityLevel(detail);
		WaterSurface.SetQuality(waterQuality);
		UwePostProcessingManager.SetAaMode(aaMode);
		UwePostProcessingManager.SetAaQuality(aaQuality);
		UwePostProcessingManager.SetAoQuality(aoQuality);
		UwePostProcessingManager.SetSsrQuality(ssrQuality);
		UwePostProcessingManager.ToggleBloom(bloom);
		UwePostProcessingManager.ToggleBloomLensDirt(bloomLensDirt);
		UwePostProcessingManager.ToggleDof(dof);
		UwePostProcessingManager.SetMotionBlurQuality(motionBlurQuality);
		UwePostProcessingManager.ToggleDithering(dithering);
	}

	public static int GetPresetIndexForCurrentOptions()
	{
		GraphicsPreset value = default(GraphicsPreset);
		value.detail = QualitySettings.GetQualityLevel();
		value.waterQuality = WaterSurface.GetQuality();
		value.aaMode = UwePostProcessingManager.GetAaMode();
		value.aaQuality = UwePostProcessingManager.GetAaQuality();
		value.aoQuality = UwePostProcessingManager.GetAoQuality();
		value.ssrQuality = UwePostProcessingManager.GetSsrQuality();
		value.bloom = UwePostProcessingManager.GetBloomEnabled();
		value.bloomLensDirt = UwePostProcessingManager.GetBloomLensDirtEnabled();
		value.dof = UwePostProcessingManager.GetDofEnabled();
		value.motionBlurQuality = UwePostProcessingManager.GetMotionBlurQuality();
		value.dithering = UwePostProcessingManager.GetDitheringEnabled();
		value.vsyncRate = -1;
		value.dynResTarget = -1;
		GraphicsPreset[] array = GetPresets();
		int num = Array.IndexOf(array, value);
		if (num == -1)
		{
			return array.Length;
		}
		return num;
	}
}
