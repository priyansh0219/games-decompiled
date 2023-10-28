using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class TerrainDefGenerator_Carpet
	{
		public static IEnumerable<TerrainDef> ImpliedTerrainDefs()
		{
			IEnumerable<ColorDef> enumerable = DefDatabase<ColorDef>.AllDefs.Where((ColorDef x) => x.colorType == ColorType.Structure);
			foreach (ColorDef c in enumerable)
			{
				int index = 0;
				foreach (TerrainTemplateDef allDef in DefDatabase<TerrainTemplateDef>.AllDefs)
				{
					yield return CarpetFromBlueprint(allDef, c, index);
					index++;
				}
			}
		}

		public static TerrainDef CarpetFromBlueprint(TerrainTemplateDef tp, ColorDef colorDef, int index)
		{
			TerrainDef terrainDef = new TerrainDef
			{
				defName = tp.defName + colorDef.defName.Replace("Structure_", ""),
				label = tp.label.Formatted(colorDef.label),
				texturePath = tp.texturePath,
				researchPrerequisites = tp.researchPrerequisites,
				burnedDef = tp.burnedDef,
				costList = tp.costList,
				description = tp.description,
				colorDef = colorDef,
				designatorDropdown = tp.designatorDropdown,
				uiOrder = tp.uiOrder,
				statBases = tp.statBases,
				renderPrecedence = tp.renderPrecedenceStart - index,
				constructionSkillPrerequisite = tp.constructionSkillPrerequisite,
				canGenerateDefaultDesignator = tp.canGenerateDefaultDesignator,
				tags = tp.tags,
				dominantStyleCategory = tp.dominantStyleCategory,
				layerable = true,
				affordances = new List<TerrainAffordanceDef>
				{
					TerrainAffordanceDefOf.Light,
					TerrainAffordanceDefOf.Medium,
					TerrainAffordanceDefOf.Heavy
				},
				designationCategory = DesignationCategoryDefOf.Floors,
				fertility = 0f,
				constructEffect = EffecterDefOf.ConstructDirt,
				pollutionColor = new Color(1f, 1f, 1f, 0.8f),
				pollutionOverlayScale = new Vector2(0.75f, 0.75f),
				pollutionOverlayTexturePath = "Terrain/Surfaces/PollutionFloorSmooth",
				terrainAffordanceNeeded = TerrainAffordanceDefOf.Heavy
			};
			if (ModsConfig.BiotechActive)
			{
				terrainDef.pollutionShaderType = ShaderTypeDefOf.TerrainFadeRoughLinearBurn;
			}
			return terrainDef;
		}
	}
}
