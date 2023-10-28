using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class PawnGraphicSet
	{
		public Pawn pawn;

		public Graphic nakedGraphic;

		public Graphic rottingGraphic;

		public Graphic dessicatedGraphic;

		public Graphic corpseGraphic;

		public Graphic packGraphic;

		public DamageFlasher flasher;

		private static Dictionary<Material, Material> overlayMats = new Dictionary<Material, Material>();

		public Graphic headGraphic;

		public Graphic desiccatedHeadGraphic;

		public Graphic skullGraphic;

		public Graphic headStumpGraphic;

		public Graphic desiccatedHeadStumpGraphic;

		public Graphic hairGraphic;

		public Graphic beardGraphic;

		public Graphic swaddledBabyGraphic;

		public List<ApparelGraphicRecord> apparelGraphics = new List<ApparelGraphicRecord>();

		public Graphic bodyTattooGraphic;

		public Graphic faceTattooGraphic;

		public List<GeneGraphicRecord> geneGraphics = new List<GeneGraphicRecord>();

		public Graphic furCoveredGraphic;

		private List<Material> cachedMatsBodyBase = new List<Material>();

		private int cachedMatsBodyBaseHash = -1;

		public static readonly Color RottingColorDefault = new Color(0.34f, 0.32f, 0.3f);

		public static readonly Color DessicatedColorInsect = new Color(0.8f, 0.8f, 0.8f);

		private const float TattooOpacity = 0.8f;

		private const float SwaddleColorOffset = 0.1f;

		private const string SwaddleGraphicPath = "Things/Pawn/Humanlike/Apparel/SwaddledBaby/Swaddled_Child";

		public bool AllResolved => nakedGraphic != null;

		public GraphicMeshSet HairMeshSet => HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn(pawn);

		public GraphicMeshSet BeardMeshSet => HumanlikeMeshPoolUtility.GetHumanlikeBeardSetForPawn(pawn);

		public List<Material> MatsBodyBaseAt(Rot4 facing, bool dead, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool drawClothes = true)
		{
			int num = facing.AsInt + 1000 * (int)bodyCondition;
			if (drawClothes)
			{
				num += 10000;
			}
			if (dead)
			{
				num += 100000;
			}
			if (num != cachedMatsBodyBaseHash)
			{
				cachedMatsBodyBase.Clear();
				cachedMatsBodyBaseHash = num;
				if (bodyCondition == RotDrawMode.Fresh)
				{
					if (dead && corpseGraphic != null)
					{
						cachedMatsBodyBase.Add(corpseGraphic.MatAt(facing));
					}
					else
					{
						cachedMatsBodyBase.Add(nakedGraphic.MatAt(facing));
					}
				}
				else if (bodyCondition == RotDrawMode.Rotting || dessicatedGraphic == null)
				{
					cachedMatsBodyBase.Add(rottingGraphic.MatAt(facing));
				}
				else if (bodyCondition == RotDrawMode.Dessicated)
				{
					cachedMatsBodyBase.Add(dessicatedGraphic.MatAt(facing));
				}
				if (drawClothes)
				{
					for (int i = 0; i < apparelGraphics.Count; i++)
					{
						if ((apparelGraphics[i].sourceApparel.def.apparel.shellRenderedBehindHead || apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Shell) && !PawnRenderer.RenderAsPack(apparelGraphics[i].sourceApparel) && apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead && apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.EyeCover)
						{
							cachedMatsBodyBase.Add(apparelGraphics[i].graphic.MatAt(facing));
						}
					}
				}
			}
			return cachedMatsBodyBase;
		}

		public Material HeadMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false, bool portrait = false, bool allowOverride = true)
		{
			Material material = null;
			switch (bodyCondition)
			{
			case RotDrawMode.Fresh:
				material = ((!stump) ? headGraphic.MatAt(facing) : headStumpGraphic.MatAt(facing));
				break;
			case RotDrawMode.Rotting:
				material = ((!stump) ? desiccatedHeadGraphic.MatAt(facing) : desiccatedHeadStumpGraphic.MatAt(facing));
				break;
			case RotDrawMode.Dessicated:
				if (!stump)
				{
					material = skullGraphic.MatAt(facing);
				}
				break;
			}
			if (material != null && allowOverride)
			{
				if (!portrait && pawn.IsInvisible())
				{
					material = InvisibilityMatPool.GetInvisibleMat(material);
				}
				material = flasher.GetDamagedMat(material);
			}
			return material;
		}

		public Material HairMatAt(Rot4 facing, bool portrait = false, bool cached = false)
		{
			if (hairGraphic == null)
			{
				return null;
			}
			Material material = hairGraphic.MatAt(facing);
			if (!portrait && pawn.IsInvisible())
			{
				material = InvisibilityMatPool.GetInvisibleMat(material);
			}
			if (!cached)
			{
				return flasher.GetDamagedMat(material);
			}
			return material;
		}

		public Material FurMatAt(Rot4 facing, bool portrait = false, bool cached = false)
		{
			if (furCoveredGraphic == null)
			{
				return null;
			}
			Material material = furCoveredGraphic.MatAt(facing);
			if (!portrait && pawn.IsInvisible())
			{
				material = InvisibilityMatPool.GetInvisibleMat(material);
			}
			if (!cached)
			{
				return flasher.GetDamagedMat(material);
			}
			return material;
		}

		public Material BeardMatAt(Rot4 facing, bool portrait = false, bool cached = false)
		{
			if (beardGraphic == null)
			{
				return null;
			}
			Material material = beardGraphic.MatAt(facing);
			if (!portrait && pawn.IsInvisible())
			{
				material = InvisibilityMatPool.GetInvisibleMat(material);
			}
			if (!cached)
			{
				return flasher.GetDamagedMat(material);
			}
			return material;
		}

		public Material SwaddledBabyMatAt(Rot4 facing, bool portrait = false, bool cached = false)
		{
			if (swaddledBabyGraphic == null)
			{
				return null;
			}
			Material material = swaddledBabyGraphic.MatAt(facing);
			if (!portrait && pawn.IsInvisible())
			{
				material = InvisibilityMatPool.GetInvisibleMat(material);
			}
			if (!cached)
			{
				return flasher.GetDamagedMat(material);
			}
			return material;
		}

		public PawnGraphicSet(Pawn pawn)
		{
			this.pawn = pawn;
			flasher = new DamageFlasher(pawn);
		}

		public void ClearCache()
		{
			cachedMatsBodyBaseHash = -1;
		}

		public void ResolveAllGraphics()
		{
			ClearCache();
			if (pawn.RaceProps.Humanlike)
			{
				Color color = (pawn.story.SkinColorOverriden ? (RottingColorDefault * pawn.story.SkinColor) : RottingColorDefault);
				nakedGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(pawn.story.SkinColorOverriden), Vector2.one, pawn.story.SkinColor);
				rottingGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(pawn.story.SkinColorOverriden), Vector2.one, color);
				dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyDessicatedGraphicPath, ShaderDatabase.Cutout);
				if (ModLister.BiotechInstalled)
				{
					if (pawn.story.furDef != null)
					{
						furCoveredGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.furDef.GetFurBodyGraphicPath(pawn), ShaderDatabase.CutoutSkinOverlay, Vector2.one, pawn.story.HairColor);
					}
					else
					{
						furCoveredGraphic = null;
					}
				}
				if (ModsConfig.BiotechActive)
				{
					swaddledBabyGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Apparel/SwaddledBaby/Swaddled_Child", ShaderDatabase.Cutout, Vector2.one, SwaddleColor());
				}
				if (pawn.style != null && ModsConfig.IdeologyActive && (!ModLister.BiotechInstalled || pawn.genes == null || !pawn.genes.GenesListForReading.Any((Gene x) => x.def.graphicData != null && !x.def.graphicData.tattoosVisible && x.Active)))
				{
					Color skinColor = pawn.story.SkinColor;
					skinColor.a *= 0.8f;
					if (pawn.style.FaceTattoo != null && pawn.style.FaceTattoo != TattooDefOf.NoTattoo_Face)
					{
						faceTattooGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.style.FaceTattoo.texPath, ShaderDatabase.CutoutSkinOverlay, Vector2.one, skinColor, Color.white, null, pawn.story.headType.graphicPath);
					}
					else
					{
						faceTattooGraphic = null;
					}
					if (pawn.style.BodyTattoo != null && pawn.style.BodyTattoo != TattooDefOf.NoTattoo_Body)
					{
						bodyTattooGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.style.BodyTattoo.texPath, ShaderDatabase.CutoutSkinOverlay, Vector2.one, skinColor, Color.white, null, pawn.story.bodyType.bodyNakedGraphicPath);
					}
					else
					{
						bodyTattooGraphic = null;
					}
				}
				headGraphic = pawn.story.headType.GetGraphic(pawn.story.SkinColor, dessicated: false, pawn.story.SkinColorOverriden);
				desiccatedHeadGraphic = pawn.story.headType.GetGraphic(color, dessicated: true, pawn.story.SkinColorOverriden);
				skullGraphic = HeadTypeDefOf.Skull.GetGraphic(Color.white, dessicated: true);
				headStumpGraphic = HeadTypeDefOf.Stump.GetGraphic(pawn.story.SkinColor, dessicated: false, pawn.story.SkinColorOverriden);
				desiccatedHeadStumpGraphic = HeadTypeDefOf.Stump.GetGraphic(color, dessicated: true, pawn.story.SkinColorOverriden);
				CalculateHairMats();
				ResolveApparelGraphics();
				ResolveGeneGraphics();
				return;
			}
			PawnKindLifeStage curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
			if (pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null)
			{
				nakedGraphic = curKindLifeStage.bodyGraphicData.Graphic;
			}
			else
			{
				nakedGraphic = curKindLifeStage.femaleGraphicData.Graphic;
			}
			if (pawn.RaceProps.packAnimal)
			{
				packGraphic = GraphicDatabase.Get<Graphic_Multi>(nakedGraphic.path + "Pack", ShaderDatabase.Cutout, nakedGraphic.drawSize, Color.white);
			}
			Shader newShader = ((pawn.story == null) ? ShaderDatabase.CutoutSkin : ShaderUtility.GetSkinShader(pawn.story.SkinColorOverriden));
			if (curKindLifeStage.corpseGraphicData != null)
			{
				if (pawn.gender != Gender.Female || curKindLifeStage.femaleCorpseGraphicData == null)
				{
					corpseGraphic = curKindLifeStage.corpseGraphicData.Graphic.GetColoredVersion(curKindLifeStage.corpseGraphicData.Graphic.Shader, nakedGraphic.Color, nakedGraphic.ColorTwo);
					rottingGraphic = curKindLifeStage.corpseGraphicData.Graphic.GetColoredVersion(newShader, RottingColorDefault, RottingColorDefault);
				}
				else
				{
					corpseGraphic = curKindLifeStage.femaleCorpseGraphicData.Graphic.GetColoredVersion(curKindLifeStage.femaleCorpseGraphicData.Graphic.Shader, nakedGraphic.Color, nakedGraphic.ColorTwo);
					rottingGraphic = curKindLifeStage.femaleCorpseGraphicData.Graphic.GetColoredVersion(newShader, RottingColorDefault, RottingColorDefault);
				}
			}
			else
			{
				corpseGraphic = null;
				rottingGraphic = nakedGraphic.GetColoredVersion(newShader, RottingColorDefault, RottingColorDefault);
			}
			if (curKindLifeStage.dessicatedBodyGraphicData != null)
			{
				if (pawn.RaceProps.FleshType == FleshTypeDefOf.Insectoid)
				{
					if (pawn.gender != Gender.Female || curKindLifeStage.femaleDessicatedBodyGraphicData == null)
					{
						dessicatedGraphic = curKindLifeStage.dessicatedBodyGraphicData.Graphic.GetColoredVersion(ShaderDatabase.Cutout, DessicatedColorInsect, DessicatedColorInsect);
					}
					else
					{
						dessicatedGraphic = curKindLifeStage.femaleDessicatedBodyGraphicData.Graphic.GetColoredVersion(ShaderDatabase.Cutout, DessicatedColorInsect, DessicatedColorInsect);
					}
				}
				else if (pawn.gender != Gender.Female || curKindLifeStage.femaleDessicatedBodyGraphicData == null)
				{
					dessicatedGraphic = curKindLifeStage.dessicatedBodyGraphicData.GraphicColoredFor(pawn);
				}
				else
				{
					dessicatedGraphic = curKindLifeStage.femaleDessicatedBodyGraphicData.GraphicColoredFor(pawn);
				}
			}
			if (pawn.kindDef.alternateGraphics.NullOrEmpty())
			{
				return;
			}
			Rand.PushState(pawn.thingIDNumber ^ 0xB415);
			if (Rand.Value <= pawn.kindDef.alternateGraphicChance)
			{
				nakedGraphic = pawn.kindDef.alternateGraphics.RandomElementByWeight((AlternateGraphic x) => x.Weight).GetGraphic(nakedGraphic);
			}
			Rand.PopState();
		}

		public void SetAllGraphicsDirty()
		{
			if (AllResolved)
			{
				ResolveAllGraphics();
			}
			GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
		}

		public void ResolveApparelGraphics()
		{
			ClearCache();
			apparelGraphics.Clear();
			foreach (Apparel item in pawn.apparel.WornApparel)
			{
				if (ApparelGraphicRecordGetter.TryGetGraphicApparel(item, pawn.story.bodyType, out var rec))
				{
					apparelGraphics.Add(rec);
				}
			}
		}

		public void CalculateHairMats()
		{
			if (pawn.story.hairDef != null)
			{
				hairGraphic = (pawn.story.hairDef.noGraphic ? null : GraphicDatabase.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Transparent, Vector2.one, pawn.story.HairColor));
			}
			if (pawn.style != null && pawn.style.beardDef != null && !pawn.style.beardDef.noGraphic)
			{
				beardGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.style.beardDef.texPath, ShaderDatabase.Transparent, Vector2.one, pawn.story.HairColor);
			}
		}

		public void SetApparelGraphicsDirty()
		{
			if (AllResolved)
			{
				ResolveApparelGraphics();
			}
		}

		public void ResolveGeneGraphics()
		{
			if (!ModsConfig.BiotechActive || pawn.genes == null)
			{
				return;
			}
			Color rottingColor = (pawn.story.SkinColorOverriden ? (RottingColorDefault * pawn.story.SkinColor) : RottingColorDefault);
			Shader skinShader = ShaderUtility.GetSkinShader(pawn.story.SkinColorOverriden);
			geneGraphics.Clear();
			foreach (Gene item in pawn.genes.GenesListForReading)
			{
				if (item.def.HasGraphic && item.Active)
				{
					(Graphic, Graphic) graphics = item.def.graphicData.GetGraphics(pawn, skinShader, rottingColor);
					geneGraphics.Add(new GeneGraphicRecord(graphics.Item1, graphics.Item2, item));
				}
			}
		}

		private Color SwaddleColor()
		{
			Rand.PushState(pawn.thingIDNumber);
			float num = Rand.Range(0.6f, 0.89f);
			float num2 = Rand.Range(-0.1f, 0.1f);
			float num3 = Rand.Range(-0.1f, 0.1f);
			float num4 = Rand.Range(-0.1f, 0.1f);
			Rand.PopState();
			return new Color(num + num2, num + num3, num + num4);
		}

		public Material GetOverlayMat(Material mat, Color color)
		{
			if (!overlayMats.TryGetValue(mat, out var value))
			{
				value = MaterialAllocator.Create(mat);
				overlayMats.Add(mat, value);
			}
			value.SetColor(ShaderPropertyIDs.OverlayColor, color);
			value.SetFloat(ShaderPropertyIDs.OverlayOpacity, 0.5f);
			return value;
		}

		public void SetGeneGraphicsDirty()
		{
			if (AllResolved)
			{
				ResolveGeneGraphics();
			}
		}
	}
}
