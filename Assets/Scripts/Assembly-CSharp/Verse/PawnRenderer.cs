using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.AI.Group;

namespace Verse
{
	public class PawnRenderer
	{
		private Pawn pawn;

		public PawnGraphicSet graphics;

		public PawnDownedWiggler wiggler;

		private PawnHeadOverlays statusOverlays;

		private PawnStatusEffecters effecters;

		private PawnWoundDrawer woundOverlays;

		private PawnFirefoamDrawer firefoamOverlays;

		private Graphic_Shadow shadowGraphic;

		private BodyPartRecord leftEyeCached;

		private BodyPartRecord rightEyeCached;

		private const float CarriedThingDrawAngle = 16f;

		private const float CarriedBabyDrawAngle = 70f;

		private const float SubInterval = 0.0028957527f;

		private const float YOffset_PrimaryEquipmentUnder = -0.0028957527f;

		private const float YOffset_CarriedThingUnder = -0.0028957527f;

		private const float YOffset_Behind = 0.0028957527f;

		private const float YOffset_Utility_South = 0.0057915053f;

		private const float YOffset_Body = 0.008687258f;

		private const float YOffsetInterval_Clothes = 0.0028957527f;

		private const float YOffset_Shell = 3f / 148f;

		private const float YOffset_Head = 0.023166021f;

		private const float YOffset_OnHeadDirect = 0.026061773f;

		private const float YOffset_OnHead = 0.028957527f;

		private const float YOffset_Utility = 0.028957527f;

		private const float YOffset_PostHead = 0.03185328f;

		private const float YOffset_CoveredInOverlay = 0.033301156f;

		private const float YOffset_CarriedThing = 0.03474903f;

		private const float YOffset_PrimaryEquipmentOver = 0.03474903f;

		private const float YOffset_Status = 0.037644785f;

		private const float CachedPawnTextureMinCameraZoom = 18f;

		private const string LeftEyeWoundAnchorTag = "LeftEye";

		private const string RightEyeWoundAnchorTag = "RightEye";

		private static Dictionary<Apparel, (Color, bool)> tmpOriginalColors = new Dictionary<Apparel, (Color, bool)>();

		private RotDrawMode CurRotDrawMode
		{
			get
			{
				if (pawn.Dead && pawn.Corpse != null)
				{
					return pawn.Corpse.CurRotDrawMode;
				}
				return RotDrawMode.Fresh;
			}
		}

		public PawnWoundDrawer WoundOverlays => woundOverlays;

		public PawnFirefoamDrawer FirefoamOverlays => firefoamOverlays;

		public PawnRenderer(Pawn pawn)
		{
			this.pawn = pawn;
			wiggler = new PawnDownedWiggler(pawn);
			statusOverlays = new PawnHeadOverlays(pawn);
			woundOverlays = new PawnWoundDrawer(pawn);
			firefoamOverlays = new PawnFirefoamDrawer(pawn);
			graphics = new PawnGraphicSet(pawn);
			effecters = new PawnStatusEffecters(pawn);
		}

		private PawnRenderFlags GetDefaultRenderFlags(Pawn pawn)
		{
			PawnRenderFlags pawnRenderFlags = PawnRenderFlags.None;
			if (pawn.IsInvisible())
			{
				pawnRenderFlags |= PawnRenderFlags.Invisible;
			}
			if (!pawn.health.hediffSet.HasHead)
			{
				pawnRenderFlags |= PawnRenderFlags.HeadStump;
			}
			return pawnRenderFlags;
		}

		private Mesh GetBlitMeshUpdatedFrame(PawnTextureAtlasFrameSet frameSet, Rot4 rotation, PawnDrawMode drawMode)
		{
			int index = frameSet.GetIndex(rotation, drawMode);
			if (frameSet.isDirty[index])
			{
				Find.PawnCacheCamera.rect = frameSet.uvRects[index];
				Find.PawnCacheRenderer.RenderPawn(pawn, frameSet.atlas, Vector3.zero, 1f, 0f, rotation, renderHead: true, drawMode == PawnDrawMode.BodyAndHead);
				Find.PawnCacheCamera.rect = new Rect(0f, 0f, 1f, 1f);
				frameSet.isDirty[index] = false;
			}
			return frameSet.meshes[index];
		}

		public static void CalculateCarriedDrawPos(Pawn pawn, Thing carriedThing, ref Vector3 carryDrawPos, out bool behind, out bool flip)
		{
			behind = false;
			flip = false;
			if (pawn.CurJob == null || !pawn.jobs.curDriver.ModifyCarriedThingDrawPos(ref carryDrawPos, ref behind, ref flip))
			{
				if (carriedThing is Pawn || carriedThing is Corpse)
				{
					if (carriedThing is Pawn pawn2 && pawn2.RaceProps.Humanlike && pawn2.DevelopmentalStage.Baby())
					{
						Vector2 vector = new Vector2(-0.1f, -0.28f).RotatedBy(pawn.Drawer.renderer.BodyAngle() * -1f);
						carryDrawPos += new Vector3(vector.x, 0f, vector.y);
					}
					else
					{
						carryDrawPos += new Vector3(0.44f, 0f, 0f);
					}
				}
				else
				{
					carryDrawPos += new Vector3(0.18f, 0f, 0.05f);
				}
			}
			if (pawn.DevelopmentalStage == DevelopmentalStage.Child)
			{
				carryDrawPos += new Vector3(0f, 0f, -0.1f);
			}
		}

		private void DrawCarriedThing(Vector3 drawLoc)
		{
			Thing carriedThing;
			if ((carriedThing = pawn.carryTracker?.CarriedThing) != null)
			{
				DrawCarriedThing(pawn, drawLoc, carriedThing);
			}
		}

		public static void DrawCarriedThing(Pawn pawn, Vector3 drawLoc, Thing carriedThing)
		{
			Vector3 carryDrawPos = drawLoc;
			CalculateCarriedDrawPos(pawn, carriedThing, ref carryDrawPos, out var behind, out var flip);
			if (behind)
			{
				carryDrawPos.y -= 0.03474903f;
			}
			else
			{
				carryDrawPos.y += 0.03474903f;
			}
			carriedThing.DrawAt(carryDrawPos, flip);
		}

		private void DrawInvisibleShadow(Vector3 drawLoc)
		{
			if (pawn.def.race.specialShadowData != null)
			{
				if (shadowGraphic == null)
				{
					shadowGraphic = new Graphic_Shadow(pawn.def.race.specialShadowData);
				}
				shadowGraphic.Draw(drawLoc, Rot4.North, pawn);
			}
			graphics.nakedGraphic?.ShadowGraphic?.Draw(drawLoc, Rot4.North, pawn);
		}

		private Vector3 GetBodyPos(Vector3 drawLoc, out bool showBody)
		{
			Building_Bed building_Bed = pawn.CurrentBed();
			Vector3 result;
			if (building_Bed != null && pawn.RaceProps.Humanlike)
			{
				showBody = building_Bed.def.building.bed_showSleeperBody;
				AltitudeLayer altLayer = (AltitudeLayer)Mathf.Max((int)building_Bed.def.altitudeLayer, 18);
				Vector3 vector = pawn.Position.ToVector3ShiftedWithAltitude(altLayer);
				Rot4 rotation = building_Bed.Rotation;
				rotation.AsInt += 2;
				float num = BaseHeadOffsetAt(Rot4.South).z + pawn.story.bodyType.bedOffset + building_Bed.def.building.bed_pawnDrawOffset;
				Vector3 vector2 = rotation.FacingCell.ToVector3();
				result = vector - vector2 * num;
				result.y += 0.008687258f;
			}
			else
			{
				showBody = true;
				result = drawLoc;
				if (pawn.ParentHolder is IThingHolderWithDrawnPawn thingHolderWithDrawnPawn)
				{
					result.y = thingHolderWithDrawnPawn.HeldPawnDrawPos_Y;
				}
				else if (!pawn.Dead && pawn.CarriedBy == null)
				{
					result.y = AltitudeLayer.LayingPawn.AltitudeFor() + 0.008687258f;
				}
			}
			showBody = pawn.mindState?.duty?.def?.drawBodyOverride ?? showBody;
			return result;
		}

		public GraphicMeshSet GetBodyOverlayMeshSet()
		{
			if (!pawn.RaceProps.Humanlike)
			{
				return HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn);
			}
			BodyTypeDef bodyType = pawn.story.bodyType;
			if (bodyType == BodyTypeDefOf.Male)
			{
				return MeshPool.humanlikeBodySet_Male;
			}
			if (bodyType == BodyTypeDefOf.Female)
			{
				return MeshPool.humanlikeBodySet_Female;
			}
			if (bodyType == BodyTypeDefOf.Thin)
			{
				return MeshPool.humanlikeBodySet_Thin;
			}
			if (bodyType == BodyTypeDefOf.Fat)
			{
				return MeshPool.humanlikeBodySet_Fat;
			}
			if (bodyType == BodyTypeDefOf.Hulk)
			{
				return MeshPool.humanlikeBodySet_Hulk;
			}
			return HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn);
		}

		public void RenderPawnAt(Vector3 drawLoc, Rot4? rotOverride = null, bool neverAimWeapon = false)
		{
			if (!graphics.AllResolved)
			{
				graphics.ResolveAllGraphics();
			}
			Rot4 rot = rotOverride ?? pawn.Rotation;
			PawnRenderFlags defaultRenderFlags = GetDefaultRenderFlags(pawn);
			defaultRenderFlags |= PawnRenderFlags.Clothes;
			defaultRenderFlags |= PawnRenderFlags.Headgear;
			if (neverAimWeapon)
			{
				defaultRenderFlags |= PawnRenderFlags.NeverAimWeapon;
			}
			RotDrawMode curRotDrawMode = CurRotDrawMode;
			bool flag = pawn.RaceProps.Humanlike && Find.CameraDriver.ZoomRootSize > 18f && curRotDrawMode != RotDrawMode.Dessicated && !pawn.IsInvisible() && !defaultRenderFlags.FlagSet(PawnRenderFlags.Portrait);
			PawnTextureAtlasFrameSet frameSet = null;
			if (flag && !GlobalTextureAtlasManager.TryGetPawnFrameSet(pawn, out frameSet, out var _))
			{
				flag = false;
			}
			if (pawn.GetPosture() == PawnPosture.Standing)
			{
				if (flag)
				{
					Material original = MaterialPool.MatFrom(new MaterialRequest(frameSet.atlas, ShaderDatabase.Cutout));
					original = OverrideMaterialIfNeeded(original, pawn);
					GenDraw.DrawMeshNowOrLater(GetBlitMeshUpdatedFrame(frameSet, rot, PawnDrawMode.BodyAndHead), drawLoc, Quaternion.AngleAxis(0f, Vector3.up), original, drawNow: false);
					DrawDynamicParts(drawLoc, 0f, rot, defaultRenderFlags);
				}
				else
				{
					RenderPawnInternal(drawLoc, 0f, renderBody: true, rot, curRotDrawMode, defaultRenderFlags);
				}
				DrawCarriedThing(drawLoc);
				if (!defaultRenderFlags.FlagSet(PawnRenderFlags.Invisible))
				{
					DrawInvisibleShadow(drawLoc);
				}
			}
			else
			{
				bool showBody;
				Vector3 bodyPos = GetBodyPos(drawLoc, out showBody);
				float angle = BodyAngle();
				Rot4 rot2 = LayingFacing();
				if (flag)
				{
					Material original2 = MaterialPool.MatFrom(new MaterialRequest(frameSet.atlas, ShaderDatabase.Cutout));
					original2 = OverrideMaterialIfNeeded(original2, pawn);
					GenDraw.DrawMeshNowOrLater(GetBlitMeshUpdatedFrame(frameSet, rot2, (!showBody) ? PawnDrawMode.HeadOnly : PawnDrawMode.BodyAndHead), bodyPos, Quaternion.AngleAxis(angle, Vector3.up), original2, drawNow: false);
					DrawDynamicParts(bodyPos, angle, rot, defaultRenderFlags);
				}
				else
				{
					RenderPawnInternal(bodyPos, angle, showBody, rot2, curRotDrawMode, defaultRenderFlags);
				}
				DrawCarriedThing(bodyPos);
			}
			if (pawn.Spawned && !pawn.Dead)
			{
				pawn.stances.StanceTrackerDraw();
				pawn.pather.PatherDraw();
				pawn.roping.RopingDraw();
			}
			DrawDebug();
		}

		public void RenderCache(Rot4 rotation, float angle, Vector3 positionOffset, bool renderHead, bool renderBody, bool portrait, bool renderHeadgear, bool renderClothes, IReadOnlyDictionary<Apparel, Color> overrideApparelColor = null, Color? overrideHairColor = null, bool stylingStation = false)
		{
			Vector3 zero = Vector3.zero;
			PawnRenderFlags pawnRenderFlags = GetDefaultRenderFlags(pawn);
			if (portrait)
			{
				pawnRenderFlags |= PawnRenderFlags.Portrait;
			}
			pawnRenderFlags |= PawnRenderFlags.Cache;
			pawnRenderFlags |= PawnRenderFlags.DrawNow;
			if (!renderHead)
			{
				pawnRenderFlags |= PawnRenderFlags.HeadStump;
			}
			if (renderHeadgear)
			{
				pawnRenderFlags |= PawnRenderFlags.Headgear;
			}
			if (renderClothes)
			{
				pawnRenderFlags |= PawnRenderFlags.Clothes;
			}
			if (stylingStation)
			{
				pawnRenderFlags |= PawnRenderFlags.StylingStation;
			}
			tmpOriginalColors.Clear();
			try
			{
				if (overrideApparelColor != null)
				{
					foreach (KeyValuePair<Apparel, Color> item in overrideApparelColor)
					{
						Apparel key = item.Key;
						CompColorable compColorable = key.TryGetComp<CompColorable>();
						if (compColorable != null)
						{
							tmpOriginalColors.Add(key, (compColorable.Color, compColorable.Active));
							key.SetColor(item.Value);
						}
					}
				}
				Color hairColor = Color.white;
				if (pawn.story != null)
				{
					hairColor = pawn.story.HairColor;
					if (overrideHairColor.HasValue)
					{
						pawn.story.HairColor = overrideHairColor.Value;
						pawn.Drawer.renderer.graphics.ResolveAllGraphics();
					}
				}
				RenderPawnInternal(zero + positionOffset, angle, renderBody, rotation, CurRotDrawMode, pawnRenderFlags);
				foreach (KeyValuePair<Apparel, (Color, bool)> tmpOriginalColor in tmpOriginalColors)
				{
					if (!tmpOriginalColor.Value.Item2)
					{
						tmpOriginalColor.Key.TryGetComp<CompColorable>().Disable();
					}
					else
					{
						tmpOriginalColor.Key.SetColor(tmpOriginalColor.Value.Item1);
					}
				}
				if (pawn.story != null && overrideHairColor.HasValue)
				{
					pawn.story.HairColor = hairColor;
					pawn.Drawer.renderer.graphics.ResolveAllGraphics();
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error rendering pawn portrait: " + ex);
			}
			finally
			{
				tmpOriginalColors.Clear();
			}
		}

		private void RenderPawnInternal(Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
		{
			if (!graphics.AllResolved)
			{
				graphics.ResolveAllGraphics();
			}
			Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
			Vector3 vector = rootLoc;
			if (pawn.ageTracker.CurLifeStage.bodyDrawOffset.HasValue)
			{
				vector += pawn.ageTracker.CurLifeStage.bodyDrawOffset.Value;
			}
			Vector3 vector2 = vector;
			Vector3 vector3 = vector;
			if (bodyFacing != Rot4.North)
			{
				vector3.y += 0.023166021f;
				vector2.y += 3f / 148f;
			}
			else
			{
				vector3.y += 3f / 148f;
				vector2.y += 0.023166021f;
			}
			Vector3 utilityLoc = vector;
			utilityLoc.y += ((bodyFacing == Rot4.South) ? 0.0057915053f : 0.028957527f);
			Mesh bodyMesh = null;
			Vector3 drawLoc;
			if (renderBody)
			{
				DrawPawnBody(vector, angle, bodyFacing, bodyDrawType, flags, out bodyMesh);
				if (bodyDrawType == RotDrawMode.Fresh && graphics.furCoveredGraphic != null)
				{
					Vector3 shellLoc = vector;
					shellLoc.y += 0.009187258f;
					DrawPawnFur(shellLoc, bodyFacing, quaternion, flags);
				}
				drawLoc = vector;
				drawLoc.y += 0.009687258f;
				if (bodyDrawType == RotDrawMode.Fresh)
				{
					woundOverlays.RenderPawnOverlay(drawLoc, bodyMesh, quaternion, flags.FlagSet(PawnRenderFlags.DrawNow), PawnOverlayDrawer.OverlayLayer.Body, bodyFacing, false);
				}
				if (flags.FlagSet(PawnRenderFlags.Clothes))
				{
					DrawBodyApparel(vector2, utilityLoc, bodyMesh, angle, bodyFacing, flags);
				}
				if (pawn.SwaddleBaby())
				{
					SwaddleBaby(vector2, bodyFacing, quaternion, flags);
				}
				if (ModLister.BiotechInstalled && pawn.genes != null)
				{
					DrawBodyGenes(vector, quaternion, angle, bodyFacing, bodyDrawType, flags);
				}
				drawLoc = vector;
				drawLoc.y += 0.022166021f;
				if (bodyDrawType == RotDrawMode.Fresh)
				{
					woundOverlays.RenderPawnOverlay(drawLoc, bodyMesh, quaternion, flags.FlagSet(PawnRenderFlags.DrawNow), PawnOverlayDrawer.OverlayLayer.Body, bodyFacing, true);
				}
			}
			Vector3 vector4 = Vector3.zero;
			drawLoc = vector;
			drawLoc.y += 0.028957527f;
			Mesh mesh = null;
			if (graphics.headGraphic != null)
			{
				vector4 = quaternion * BaseHeadOffsetAt(bodyFacing);
				Material material = graphics.HeadMatAt(bodyFacing, bodyDrawType, flags.FlagSet(PawnRenderFlags.HeadStump), flags.FlagSet(PawnRenderFlags.Portrait), !flags.FlagSet(PawnRenderFlags.Cache));
				if (material != null)
				{
					mesh = HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn).MeshAt(bodyFacing);
					GenDraw.DrawMeshNowOrLater(mesh, vector3 + vector4, quaternion, material, flags.FlagSet(PawnRenderFlags.DrawNow));
				}
			}
			if (bodyDrawType == RotDrawMode.Fresh)
			{
				woundOverlays.RenderPawnOverlay(drawLoc, bodyMesh, quaternion, flags.FlagSet(PawnRenderFlags.DrawNow), PawnOverlayDrawer.OverlayLayer.Head, bodyFacing);
			}
			if (graphics.headGraphic != null)
			{
				DrawHeadHair(vector, vector4, angle, bodyFacing, bodyFacing, bodyDrawType, flags, renderBody);
			}
			if (!flags.FlagSet(PawnRenderFlags.Portrait) && pawn.RaceProps.Animal && pawn.inventory != null && pawn.inventory.innerContainer.Count > 0 && graphics.packGraphic != null)
			{
				GenDraw.DrawMeshNowOrLater(bodyMesh, Matrix4x4.TRS(vector2, quaternion, Vector3.one), graphics.packGraphic.MatAt(bodyFacing), flags.FlagSet(PawnRenderFlags.DrawNow));
			}
			if (bodyDrawType == RotDrawMode.Fresh && firefoamOverlays.IsCoveredInFoam)
			{
				Vector3 drawLoc2 = vector;
				drawLoc2.y += 0.033301156f;
				if (renderBody)
				{
					firefoamOverlays.RenderPawnOverlay(drawLoc2, bodyMesh, quaternion, flags.FlagSet(PawnRenderFlags.DrawNow), PawnOverlayDrawer.OverlayLayer.Body, bodyFacing);
				}
				if (mesh != null)
				{
					drawLoc2 = vector3 + vector4;
					drawLoc2.y += 0.033301156f;
					firefoamOverlays.RenderPawnOverlay(drawLoc2, mesh, quaternion, flags.FlagSet(PawnRenderFlags.DrawNow), PawnOverlayDrawer.OverlayLayer.Head, bodyFacing);
				}
			}
			if (!flags.FlagSet(PawnRenderFlags.Portrait) && !flags.FlagSet(PawnRenderFlags.Cache))
			{
				DrawDynamicParts(vector, angle, bodyFacing, flags);
			}
		}

		private void DrawPawnBody(Vector3 rootLoc, float angle, Rot4 facing, RotDrawMode bodyDrawType, PawnRenderFlags flags, out Mesh bodyMesh)
		{
			Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
			Vector3 vector = rootLoc;
			vector.y += 0.008687258f;
			Vector3 loc = vector;
			loc.y += 0.0014478763f;
			bodyMesh = null;
			if (bodyDrawType == RotDrawMode.Dessicated && !pawn.RaceProps.Humanlike && graphics.dessicatedGraphic != null && !flags.FlagSet(PawnRenderFlags.Portrait))
			{
				graphics.dessicatedGraphic.Draw(vector, facing, pawn, angle);
				return;
			}
			if (pawn.RaceProps.Humanlike)
			{
				bodyMesh = HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn).MeshAt(facing);
			}
			else
			{
				bodyMesh = graphics.nakedGraphic.MeshAt(facing);
			}
			List<Material> list = graphics.MatsBodyBaseAt(facing, pawn.Dead, bodyDrawType, flags.FlagSet(PawnRenderFlags.Clothes));
			for (int i = 0; i < list.Count; i++)
			{
				Material material = ((pawn.RaceProps.IsMechanoid && pawn.Faction != null && pawn.Faction != Faction.OfMechanoids) ? graphics.GetOverlayMat(list[i], pawn.Faction.MechColor) : list[i]);
				Material mat = (flags.FlagSet(PawnRenderFlags.Cache) ? material : OverrideMaterialIfNeeded(material, pawn, flags.FlagSet(PawnRenderFlags.Portrait)));
				GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
				vector.y += 0.0028957527f;
			}
			if (ModsConfig.IdeologyActive && graphics.bodyTattooGraphic != null && bodyDrawType != RotDrawMode.Dessicated && (facing != Rot4.North || pawn.style.BodyTattoo.visibleNorth))
			{
				GenDraw.DrawMeshNowOrLater(GetBodyOverlayMeshSet().MeshAt(facing), loc, quat, graphics.bodyTattooGraphic.MatAt(facing), flags.FlagSet(PawnRenderFlags.DrawNow));
			}
		}

		private void DrawPawnFur(Vector3 shellLoc, Rot4 facing, Quaternion quat, PawnRenderFlags flags)
		{
			Mesh mesh = HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn).MeshAt(facing);
			Material mat = graphics.FurMatAt(facing, flags.FlagSet(PawnRenderFlags.Portrait), flags.FlagSet(PawnRenderFlags.Cache));
			GenDraw.DrawMeshNowOrLater(mesh, shellLoc, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
		}

		private void SwaddleBaby(Vector3 shellLoc, Rot4 facing, Quaternion quat, PawnRenderFlags flags)
		{
			Mesh mesh = HumanlikeMeshPoolUtility.GetSwaddledBabySet().MeshAt(facing);
			Material material = graphics.SwaddledBabyMatAt(facing, flags.FlagSet(PawnRenderFlags.Portrait), flags.FlagSet(PawnRenderFlags.Cache));
			if (material != null)
			{
				GenDraw.DrawMeshNowOrLater(mesh, shellLoc, quat, material, flags.FlagSet(PawnRenderFlags.DrawNow));
			}
		}

		private void DrawHeadHair(Vector3 rootLoc, Vector3 headOffset, float angle, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, bool bodyDrawn)
		{
			if (ShellFullyCoversHead(flags) && bodyDrawn)
			{
				return;
			}
			Vector3 onHeadLoc = rootLoc + headOffset;
			onHeadLoc.y += 0.028957527f;
			List<ApparelGraphicRecord> apparelGraphics = graphics.apparelGraphics;
			List<GeneGraphicRecord> geneGraphics = graphics.geneGraphics;
			Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
			int num;
			if (!pawn.DevelopmentalStage.Baby() && bodyDrawType != RotDrawMode.Dessicated)
			{
				num = (flags.FlagSet(PawnRenderFlags.HeadStump) ? 1 : 0);
				if (num == 0)
				{
					goto IL_0103;
				}
			}
			else
			{
				num = 1;
			}
			if (pawn.story?.hairDef == null)
			{
				goto IL_0103;
			}
			int num2 = (pawn.story.hairDef.noGraphic ? 1 : 0);
			goto IL_0104;
			IL_0103:
			num2 = 1;
			goto IL_0104;
			IL_0104:
			bool flag = (byte)num2 != 0;
			bool flag2 = num == 0 && bodyFacing != Rot4.North && pawn.DevelopmentalStage.Adult() && (pawn.style?.beardDef ?? BeardDefOf.NoBeard) != BeardDefOf.NoBeard;
			bool allFaceCovered = false;
			bool drawEyes = true;
			bool middleFaceCovered = false;
			bool flag3 = pawn.CurrentBed() != null && !pawn.CurrentBed().def.building.bed_showSleeperBody;
			bool flag4 = !flags.FlagSet(PawnRenderFlags.Portrait) && flag3;
			bool flag5 = flags.FlagSet(PawnRenderFlags.Headgear) && (!flags.FlagSet(PawnRenderFlags.Portrait) || !Prefs.HatsOnlyOnMap || flags.FlagSet(PawnRenderFlags.StylingStation));
			if (leftEyeCached == null)
			{
				leftEyeCached = pawn.def.race.body.AllParts.FirstOrDefault((BodyPartRecord p) => p.woundAnchorTag == "LeftEye");
			}
			if (rightEyeCached == null)
			{
				rightEyeCached = pawn.def.race.body.AllParts.FirstOrDefault((BodyPartRecord p) => p.woundAnchorTag == "RightEye");
			}
			bool hasLeftEye = leftEyeCached != null && !pawn.health.hediffSet.PartIsMissing(leftEyeCached);
			bool hasRightEye = rightEyeCached != null && !pawn.health.hediffSet.PartIsMissing(rightEyeCached);
			if (flag5)
			{
				for (int i = 0; i < apparelGraphics.Count; i++)
				{
					if ((flag4 && !apparelGraphics[i].sourceApparel.def.apparel.hatRenderedFrontOfFace) || (apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead && apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.EyeCover))
					{
						continue;
					}
					if (apparelGraphics[i].sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead))
					{
						flag2 = false;
						allFaceCovered = true;
						if (!apparelGraphics[i].sourceApparel.def.apparel.forceEyesVisibleForRotations.Contains(headFacing.AsInt))
						{
							drawEyes = false;
						}
					}
					if (!apparelGraphics[i].sourceApparel.def.apparel.hatRenderedFrontOfFace && !apparelGraphics[i].sourceApparel.def.apparel.forceRenderUnderHair)
					{
						flag = false;
					}
					if (apparelGraphics[i].sourceApparel.def.apparel.coversHeadMiddle)
					{
						middleFaceCovered = true;
					}
				}
			}
			TryDrawGenes(GeneDrawLayer.PostSkin);
			if (ModsConfig.IdeologyActive && graphics.faceTattooGraphic != null && bodyDrawType != RotDrawMode.Dessicated && !flags.FlagSet(PawnRenderFlags.HeadStump) && (bodyFacing != Rot4.North || pawn.style.FaceTattoo.visibleNorth))
			{
				Vector3 loc = rootLoc + headOffset;
				loc.y += 0.023166021f;
				if (bodyFacing == Rot4.North)
				{
					loc.y -= 0.001f;
				}
				else
				{
					loc.y += 0.001f;
				}
				GenDraw.DrawMeshNowOrLater(graphics.HairMeshSet.MeshAt(headFacing), loc, quat, graphics.faceTattooGraphic.MatAt(headFacing), flags.FlagSet(PawnRenderFlags.DrawNow));
			}
			TryDrawGenes(GeneDrawLayer.PostTattoo);
			if (headFacing != Rot4.North && (!allFaceCovered || drawEyes))
			{
				foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
				{
					if (hediff.def.eyeGraphicSouth != null && hediff.def.eyeGraphicEast != null)
					{
						GraphicData graphicData = (headFacing.IsHorizontal ? hediff.def.eyeGraphicEast : hediff.def.eyeGraphicSouth);
						bool flag6 = hediff.Part.woundAnchorTag == "LeftEye";
						DrawExtraEyeGraphic(graphicData.Graphic, hediff.def.eyeGraphicScale * pawn.ageTracker.CurLifeStage.eyeSizeFactor.GetValueOrDefault(1f), 0.0014f, flag6, !flag6);
					}
				}
			}
			if (flag2)
			{
				Vector3 loc2 = rootLoc + headOffset + quat * OffsetBeardLocationForHead(pawn.style.beardDef, pawn.story.headType, headFacing, Vector3.zero);
				Mesh mesh = graphics.BeardMeshSet.MeshAt(headFacing);
				Material material = graphics.BeardMatAt(headFacing, flags.FlagSet(PawnRenderFlags.Portrait), flags.FlagSet(PawnRenderFlags.Cache));
				if (material != null)
				{
					GenDraw.DrawMeshNowOrLater(mesh, loc2, quat, material, flags.FlagSet(PawnRenderFlags.DrawNow));
				}
			}
			if (flag5)
			{
				for (int j = 0; j < apparelGraphics.Count; j++)
				{
					if ((!flag4 || apparelGraphics[j].sourceApparel.def.apparel.hatRenderedFrontOfFace) && apparelGraphics[j].sourceApparel.def.apparel.forceRenderUnderHair)
					{
						DrawApparel(apparelGraphics[j]);
					}
				}
			}
			if (flag)
			{
				Mesh mesh2 = graphics.HairMeshSet.MeshAt(headFacing);
				Material material2 = graphics.HairMatAt(headFacing, flags.FlagSet(PawnRenderFlags.Portrait), flags.FlagSet(PawnRenderFlags.Cache));
				if (material2 != null)
				{
					GenDraw.DrawMeshNowOrLater(mesh2, onHeadLoc, quat, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
				}
			}
			TryDrawGenes(GeneDrawLayer.PostHair);
			if (flag5)
			{
				for (int k = 0; k < apparelGraphics.Count; k++)
				{
					if ((!flag4 || apparelGraphics[k].sourceApparel.def.apparel.hatRenderedFrontOfFace) && (apparelGraphics[k].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparelGraphics[k].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover) && !apparelGraphics[k].sourceApparel.def.apparel.forceRenderUnderHair)
					{
						DrawApparel(apparelGraphics[k]);
					}
				}
			}
			TryDrawGenes(GeneDrawLayer.PostHeadgear);
			void DrawApparel(ApparelGraphicRecord apparelRecord)
			{
				Mesh mesh3 = graphics.HairMeshSet.MeshAt(headFacing);
				if (!apparelRecord.sourceApparel.def.apparel.hatRenderedFrontOfFace)
				{
					Material material3 = apparelRecord.graphic.MatAt(bodyFacing);
					material3 = (flags.FlagSet(PawnRenderFlags.Cache) ? material3 : OverrideMaterialIfNeeded(material3, pawn, flags.FlagSet(PawnRenderFlags.Portrait)));
					GenDraw.DrawMeshNowOrLater(mesh3, onHeadLoc, quat, material3, flags.FlagSet(PawnRenderFlags.DrawNow));
				}
				else
				{
					Material material4 = apparelRecord.graphic.MatAt(bodyFacing);
					material4 = (flags.FlagSet(PawnRenderFlags.Cache) ? material4 : OverrideMaterialIfNeeded(material4, pawn, flags.FlagSet(PawnRenderFlags.Portrait)));
					Vector3 loc3 = rootLoc + headOffset;
					if (apparelRecord.sourceApparel.def.apparel.hatRenderedBehindHead)
					{
						loc3.y += 0.022166021f;
					}
					else
					{
						loc3.y += ((bodyFacing == Rot4.North && !apparelRecord.sourceApparel.def.apparel.hatRenderedAboveBody) ? 0.0028957527f : 0.03185328f);
					}
					GenDraw.DrawMeshNowOrLater(mesh3, loc3, quat, material4, flags.FlagSet(PawnRenderFlags.DrawNow));
				}
			}
			void DrawExtraEyeGraphic(Graphic graphic, float scale, float yOffset, bool drawLeft, bool drawRight)
			{
				bool narrowCrown = pawn.story.headType.narrow;
				Vector3? eyeOffsetEastWest = pawn.story.headType.eyeOffsetEastWest;
				Vector3 vector = rootLoc + headOffset + new Vector3(0f, 0.026061773f + yOffset, 0f) + quat * new Vector3(0f, 0f, -0.25f);
				BodyTypeDef.WoundAnchor woundAnchor = pawn.story.bodyType.woundAnchors.FirstOrDefault((BodyTypeDef.WoundAnchor a) => a.tag == "LeftEye" && a.rotation == headFacing && (headFacing == Rot4.South || a.narrowCrown.GetValueOrDefault() == narrowCrown));
				BodyTypeDef.WoundAnchor woundAnchor2 = pawn.story.bodyType.woundAnchors.FirstOrDefault((BodyTypeDef.WoundAnchor a) => a.tag == "RightEye" && a.rotation == headFacing && (headFacing == Rot4.South || a.narrowCrown.GetValueOrDefault() == narrowCrown));
				Material mat = graphic.MatAt(headFacing);
				if (headFacing == Rot4.South)
				{
					if (woundAnchor == null || woundAnchor2 == null)
					{
						return;
					}
					if (drawLeft)
					{
						GenDraw.DrawMeshNowOrLater(MeshPool.GridPlaneFlip(Vector2.one * scale), Matrix4x4.TRS(vector + quat * woundAnchor.offset, quat, Vector3.one), mat, flags.FlagSet(PawnRenderFlags.DrawNow));
					}
					if (drawRight)
					{
						GenDraw.DrawMeshNowOrLater(MeshPool.GridPlane(Vector2.one * scale), Matrix4x4.TRS(vector + quat * woundAnchor2.offset, quat, Vector3.one), mat, flags.FlagSet(PawnRenderFlags.DrawNow));
					}
				}
				if (headFacing == Rot4.East && drawRight)
				{
					if (woundAnchor2 == null)
					{
						return;
					}
					Vector3 vector2 = eyeOffsetEastWest ?? woundAnchor2.offset;
					GenDraw.DrawMeshNowOrLater(MeshPool.GridPlane(Vector2.one * scale), Matrix4x4.TRS(vector + quat * vector2, quat, Vector3.one), mat, flags.FlagSet(PawnRenderFlags.DrawNow));
				}
				if (headFacing == Rot4.West && drawLeft && woundAnchor != null)
				{
					Vector3 vector3 = woundAnchor.offset;
					if (eyeOffsetEastWest.HasValue)
					{
						vector3 = eyeOffsetEastWest.Value.ScaledBy(new Vector3(-1f, 1f, 1f));
					}
					GenDraw.DrawMeshNowOrLater(MeshPool.GridPlaneFlip(Vector2.one * scale), Matrix4x4.TRS(vector + quat * vector3, quat, Vector3.one), mat, flags.FlagSet(PawnRenderFlags.DrawNow));
				}
			}
			void DrawGene(GeneGraphicRecord geneRecord, GeneDrawLayer layer)
			{
				if ((bodyDrawType != RotDrawMode.Dessicated || geneRecord.sourceGene.def.graphicData.drawWhileDessicated) && (!(geneRecord.sourceGene.def.graphicData.drawLoc == GeneDrawLoc.HeadMiddle && allFaceCovered) || geneRecord.sourceGene.def.graphicData.drawIfFaceCovered) && (!(geneRecord.sourceGene.def.graphicData.drawLoc == GeneDrawLoc.HeadMiddle && middleFaceCovered) || geneRecord.sourceGene.def.graphicData.drawIfFaceCovered))
				{
					Vector3 loc4 = rootLoc + headOffset + quat * HeadGeneDrawLocation(geneRecord.sourceGene.def, pawn.story.headType, headFacing, Vector3.zero, layer);
					Material material5 = ((bodyDrawType == RotDrawMode.Rotting) ? geneRecord.rottingGraphic : geneRecord.graphic).MatAt(headFacing);
					material5 = (flags.FlagSet(PawnRenderFlags.Cache) ? material5 : OverrideMaterialIfNeeded(material5, pawn, flags.FlagSet(PawnRenderFlags.Portrait)));
					GenDraw.DrawMeshNowOrLater(graphics.HairMeshSet.MeshAt(headFacing), loc4, quat, material5, flags.FlagSet(PawnRenderFlags.DrawNow));
				}
			}
			void DrawGeneEyes(GeneGraphicRecord geneRecord)
			{
				if (!(headFacing == Rot4.North) && (bodyDrawType != RotDrawMode.Dessicated || geneRecord.sourceGene.def.graphicData.drawWhileDessicated) && (!(geneRecord.sourceGene.def.graphicData.drawLoc == GeneDrawLoc.HeadMiddle && allFaceCovered) || geneRecord.sourceGene.def.graphicData.drawIfFaceCovered || drawEyes))
				{
					Graphic graphic2 = ((bodyDrawType == RotDrawMode.Rotting) ? geneRecord.rottingGraphic : geneRecord.graphic);
					float drawScale = geneRecord.sourceGene.def.graphicData.drawScale;
					DrawExtraEyeGraphic(graphic2, drawScale * pawn.ageTracker.CurLifeStage.eyeSizeFactor.GetValueOrDefault(1f), 0.0012f, hasLeftEye, hasRightEye);
				}
			}
			void TryDrawGenes(GeneDrawLayer layer)
			{
				if (ModLister.BiotechInstalled && !flags.FlagSet(PawnRenderFlags.HeadStump))
				{
					for (int l = 0; l < geneGraphics.Count; l++)
					{
						if (geneGraphics[l].sourceGene.def.CanDrawNow(bodyFacing, layer))
						{
							if (geneGraphics[l].sourceGene.def.graphicData.drawOnEyes)
							{
								DrawGeneEyes(geneGraphics[l]);
							}
							else
							{
								DrawGene(geneGraphics[l], layer);
							}
						}
					}
				}
			}
		}

		private bool ShellFullyCoversHead(PawnRenderFlags flags)
		{
			if (!flags.FlagSet(PawnRenderFlags.Clothes))
			{
				return false;
			}
			List<ApparelGraphicRecord> apparelGraphics = graphics.apparelGraphics;
			for (int i = 0; i < apparelGraphics.Count; i++)
			{
				if (apparelGraphics[i].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell && apparelGraphics[i].sourceApparel.def.apparel.shellCoversHead)
				{
					return true;
				}
			}
			return false;
		}

		private Vector3 OffsetBeardLocationForHead(BeardDef beardDef, HeadTypeDef head, Rot4 headFacing, Vector3 beardLoc)
		{
			if (headFacing == Rot4.East)
			{
				beardLoc += Vector3.right * head.beardOffsetXEast;
			}
			else if (headFacing == Rot4.West)
			{
				beardLoc += Vector3.left * head.beardOffsetXEast;
			}
			beardLoc.y += 0.026061773f;
			beardLoc += head.beardOffset;
			beardLoc += pawn.style.beardDef.GetOffset(pawn.story.headType, headFacing);
			return beardLoc;
		}

		private Vector3 HeadGeneDrawLocation(GeneDef geneDef, HeadTypeDef head, Rot4 headFacing, Vector3 geneLoc, GeneDrawLayer layer)
		{
			switch (layer)
			{
			case GeneDrawLayer.PostHair:
			case GeneDrawLayer.PostHeadgear:
				geneLoc.y += 0.03335328f;
				break;
			case GeneDrawLayer.PostSkin:
				geneLoc.y += 0.026061773f;
				break;
			default:
				geneLoc.y += 0.028957527f;
				break;
			}
			geneLoc += geneDef.graphicData.DrawOffsetAt(headFacing);
			float narrowCrownHorizontalOffset = geneDef.graphicData.narrowCrownHorizontalOffset;
			if (narrowCrownHorizontalOffset != 0f && head.narrow && headFacing.IsHorizontal)
			{
				if (headFacing == Rot4.East)
				{
					geneLoc += Vector3.right * (0f - narrowCrownHorizontalOffset);
				}
				else if (headFacing == Rot4.West)
				{
					geneLoc += Vector3.right * narrowCrownHorizontalOffset;
				}
				geneLoc += Vector3.forward * (0f - narrowCrownHorizontalOffset);
			}
			return geneLoc;
		}

		private void DrawBodyGenes(Vector3 rootLoc, Quaternion quat, float angle, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
		{
			Vector2 bodyGraphicScale = pawn.story.bodyType.bodyGraphicScale;
			float num = (bodyGraphicScale.x + bodyGraphicScale.y) / 2f;
			foreach (GeneGraphicRecord geneGraphic in graphics.geneGraphics)
			{
				GeneGraphicData graphicData = geneGraphic.sourceGene.def.graphicData;
				if (graphicData.drawLoc == GeneDrawLoc.Tailbone && (bodyDrawType != RotDrawMode.Dessicated || geneGraphic.sourceGene.def.graphicData.drawWhileDessicated))
				{
					Vector3 v = graphicData.DrawOffsetAt(bodyFacing);
					v.x *= bodyGraphicScale.x;
					v.z *= bodyGraphicScale.y;
					Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(graphicData.drawScale * num, 1f, graphicData.drawScale * num), pos: rootLoc + v.RotatedBy(angle), q: quat);
					Material material = geneGraphic.graphic.MatAt(bodyFacing);
					material = (flags.FlagSet(PawnRenderFlags.Cache) ? material : OverrideMaterialIfNeeded(material, pawn, flags.FlagSet(PawnRenderFlags.Portrait)));
					GenDraw.DrawMeshNowOrLater((bodyFacing == Rot4.West) ? MeshPool.GridPlaneFlip(Vector2.one) : MeshPool.GridPlane(Vector2.one), matrix, material, flags.FlagSet(PawnRenderFlags.DrawNow));
				}
			}
		}

		private void DrawBodyApparel(Vector3 shellLoc, Vector3 utilityLoc, Mesh bodyMesh, float angle, Rot4 bodyFacing, PawnRenderFlags flags)
		{
			List<ApparelGraphicRecord> apparelGraphics = graphics.apparelGraphics;
			Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
			for (int i = 0; i < apparelGraphics.Count; i++)
			{
				ApparelGraphicRecord apparelGraphicRecord = apparelGraphics[i];
				if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell && !apparelGraphicRecord.sourceApparel.def.apparel.shellRenderedBehindHead)
				{
					Material material = apparelGraphicRecord.graphic.MatAt(bodyFacing);
					material = (flags.FlagSet(PawnRenderFlags.Cache) ? material : OverrideMaterialIfNeeded(material, pawn, flags.FlagSet(PawnRenderFlags.Portrait)));
					Vector3 loc = shellLoc;
					if (apparelGraphicRecord.sourceApparel.def.apparel.shellCoversHead)
					{
						loc.y += 0.0028957527f;
					}
					GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quaternion, material, flags.FlagSet(PawnRenderFlags.DrawNow));
				}
				if (RenderAsPack(apparelGraphicRecord.sourceApparel))
				{
					Material material2 = apparelGraphicRecord.graphic.MatAt(bodyFacing);
					material2 = (flags.FlagSet(PawnRenderFlags.Cache) ? material2 : OverrideMaterialIfNeeded(material2, pawn, flags.FlagSet(PawnRenderFlags.Portrait)));
					if (apparelGraphicRecord.sourceApparel.def.apparel.wornGraphicData != null)
					{
						Vector2 vector = apparelGraphicRecord.sourceApparel.def.apparel.wornGraphicData.BeltOffsetAt(bodyFacing, pawn.story.bodyType);
						Vector2 vector2 = apparelGraphicRecord.sourceApparel.def.apparel.wornGraphicData.BeltScaleAt(bodyFacing, pawn.story.bodyType);
						Matrix4x4 matrix = Matrix4x4.Translate(utilityLoc) * Matrix4x4.Rotate(quaternion) * Matrix4x4.Translate(new Vector3(vector.x, 0f, vector.y)) * Matrix4x4.Scale(new Vector3(vector2.x, 1f, vector2.y));
						GenDraw.DrawMeshNowOrLater(bodyMesh, matrix, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
					}
					else
					{
						GenDraw.DrawMeshNowOrLater(bodyMesh, shellLoc, quaternion, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
					}
				}
			}
		}

		private void DrawDynamicParts(Vector3 rootLoc, float angle, Rot4 pawnRotation, PawnRenderFlags flags)
		{
			Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
			DrawEquipment(rootLoc, pawnRotation, flags);
			if (pawn.apparel != null)
			{
				List<Apparel> wornApparel = pawn.apparel.WornApparel;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					wornApparel[i].DrawWornExtras();
				}
			}
			Vector3 bodyLoc = rootLoc;
			bodyLoc.y += 0.037644785f;
			statusOverlays.RenderStatusOverlays(bodyLoc, quat, HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn).MeshAt(pawnRotation));
		}

		private void DrawEquipment(Vector3 rootLoc, Rot4 pawnRotation, PawnRenderFlags flags)
		{
			if (pawn.Dead || !pawn.Spawned || pawn.equipment == null || pawn.equipment.Primary == null || (pawn.CurJob != null && pawn.CurJob.def.neverShowWeapon))
			{
				return;
			}
			Vector3 drawLoc = new Vector3(0f, (pawnRotation == Rot4.North) ? (-0.0028957527f) : 0.03474903f, 0f);
			Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
			float equipmentDrawDistanceFactor = pawn.ageTracker.CurLifeStage.equipmentDrawDistanceFactor;
			if (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid && (flags & PawnRenderFlags.NeverAimWeapon) == 0)
			{
				Vector3 vector = ((!stance_Busy.focusTarg.HasThing) ? stance_Busy.focusTarg.Cell.ToVector3Shifted() : stance_Busy.focusTarg.Thing.DrawPos);
				float num = 0f;
				if ((vector - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
				{
					num = (vector - pawn.DrawPos).AngleFlat();
				}
				Verb currentEffectiveVerb = pawn.CurrentEffectiveVerb;
				if (currentEffectiveVerb != null && currentEffectiveVerb.AimAngleOverride.HasValue)
				{
					num = currentEffectiveVerb.AimAngleOverride.Value;
				}
				drawLoc += rootLoc + new Vector3(0f, 0f, 0.4f + pawn.equipment.Primary.def.equippedDistanceOffset).RotatedBy(num) * equipmentDrawDistanceFactor;
				DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, num);
			}
			else if (CarryWeaponOpenly())
			{
				if (pawnRotation == Rot4.South)
				{
					drawLoc += rootLoc + new Vector3(0f, 0f, -0.22f) * equipmentDrawDistanceFactor;
					DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, 143f);
				}
				else if (pawnRotation == Rot4.North)
				{
					drawLoc += rootLoc + new Vector3(0f, 0f, -0.11f) * equipmentDrawDistanceFactor;
					DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, 143f);
				}
				else if (pawnRotation == Rot4.East)
				{
					drawLoc += rootLoc + new Vector3(0.2f, 0f, -0.22f) * equipmentDrawDistanceFactor;
					DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, 143f);
				}
				else if (pawnRotation == Rot4.West)
				{
					drawLoc += rootLoc + new Vector3(-0.2f, 0f, -0.22f) * equipmentDrawDistanceFactor;
					DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, 217f);
				}
			}
		}

		public void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)
		{
			Mesh mesh = null;
			float num = aimAngle - 90f;
			if (aimAngle > 20f && aimAngle < 160f)
			{
				mesh = MeshPool.plane10;
				num += eq.def.equippedAngleOffset;
			}
			else if (aimAngle > 200f && aimAngle < 340f)
			{
				mesh = MeshPool.plane10Flip;
				num -= 180f;
				num -= eq.def.equippedAngleOffset;
			}
			else
			{
				mesh = MeshPool.plane10;
				num += eq.def.equippedAngleOffset;
			}
			num %= 360f;
			CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
			if (compEquippable != null)
			{
				EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
				drawLoc += drawOffset;
				num += angleOffset;
			}
			Material material = null;
			material = ((!(eq.Graphic is Graphic_StackCount graphic_StackCount)) ? eq.Graphic.MatSingleFor(eq) : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq));
			Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(eq.Graphic.drawSize.x, 0f, eq.Graphic.drawSize.y), pos: drawLoc, q: Quaternion.AngleAxis(num, Vector3.up));
			Graphics.DrawMesh(mesh, matrix, material, 0);
		}

		private Material OverrideMaterialIfNeeded(Material original, Pawn pawn, bool portrait = false)
		{
			Material baseMat = ((!portrait && pawn.IsInvisible()) ? InvisibilityMatPool.GetInvisibleMat(original) : original);
			return graphics.flasher.GetDamagedMat(baseMat);
		}

		private bool CarryWeaponOpenly()
		{
			if (pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null)
			{
				return false;
			}
			if (pawn.Drafted)
			{
				return true;
			}
			if (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon)
			{
				return true;
			}
			if (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon)
			{
				return true;
			}
			Lord lord = pawn.GetLord();
			if (lord != null && lord.LordJob != null && lord.LordJob.AlwaysShowWeapon)
			{
				return true;
			}
			return false;
		}

		private Rot4 RotationForcedByJob()
		{
			if (pawn.jobs != null && pawn.jobs.curDriver != null && pawn.jobs.curDriver.ForcedLayingRotation.IsValid)
			{
				return pawn.jobs.curDriver.ForcedLayingRotation;
			}
			return Rot4.Invalid;
		}

		public Rot4 LayingFacing()
		{
			Rot4 result = RotationForcedByJob();
			if (result.IsValid)
			{
				return result;
			}
			PawnPosture posture = pawn.GetPosture();
			if (posture == PawnPosture.LayingOnGroundFaceUp || pawn.Deathresting)
			{
				return Rot4.South;
			}
			if (pawn.RaceProps.Humanlike)
			{
				if (pawn.DevelopmentalStage.Baby() && pawn.ParentHolder is Pawn_CarryTracker pawn_CarryTracker)
				{
					if (!(pawn_CarryTracker.pawn.Rotation == Rot4.West))
					{
						return Rot4.West;
					}
					return Rot4.East;
				}
				if (posture.FaceUp() && pawn.CurrentBed() != null)
				{
					return Rot4.South;
				}
				switch (pawn.thingIDNumber % 4)
				{
				case 0:
					return Rot4.South;
				case 1:
					return Rot4.South;
				case 2:
					return Rot4.East;
				case 3:
					return Rot4.West;
				}
			}
			else
			{
				switch (pawn.thingIDNumber % 4)
				{
				case 0:
					return Rot4.South;
				case 1:
					return Rot4.East;
				case 2:
					return Rot4.West;
				case 3:
					return Rot4.West;
				}
			}
			return Rot4.Random;
		}

		public float BodyAngle()
		{
			if (pawn.GetPosture() == PawnPosture.Standing)
			{
				return 0f;
			}
			Building_Bed building_Bed = pawn.CurrentBed();
			if (building_Bed != null && pawn.RaceProps.Humanlike)
			{
				Rot4 rotation = building_Bed.Rotation;
				rotation.AsInt += 2;
				return rotation.AsAngle;
			}
			if (pawn.ParentHolder is IThingHolderWithDrawnPawn thingHolderWithDrawnPawn)
			{
				return thingHolderWithDrawnPawn.HeldPawnBodyAngle;
			}
			if (pawn.RaceProps.Humanlike && pawn.DevelopmentalStage.Baby() && pawn.ParentHolder is Pawn_CarryTracker pawn_CarryTracker)
			{
				return ((pawn_CarryTracker.pawn.Rotation == Rot4.West) ? 290f : 70f) + pawn_CarryTracker.pawn.Drawer.renderer.BodyAngle();
			}
			if (pawn.Downed || pawn.Dead)
			{
				return wiggler.downedAngle;
			}
			if (pawn.RaceProps.Humanlike)
			{
				return LayingFacing().AsAngle;
			}
			if (RotationForcedByJob().IsValid)
			{
				return 0f;
			}
			Rot4 rot = Rot4.West;
			switch (pawn.thingIDNumber % 2)
			{
			case 0:
				rot = Rot4.West;
				break;
			case 1:
				rot = Rot4.East;
				break;
			}
			return rot.AsAngle;
		}

		public Vector3 BaseHeadOffsetAt(Rot4 rotation)
		{
			Vector2 vector = pawn.story.bodyType.headOffset * Mathf.Sqrt(pawn.ageTracker.CurLifeStage.bodySizeFactor);
			switch (rotation.AsInt)
			{
			case 0:
				return new Vector3(0f, 0f, vector.y);
			case 1:
				return new Vector3(vector.x, 0f, vector.y);
			case 2:
				return new Vector3(0f, 0f, vector.y);
			case 3:
				return new Vector3(0f - vector.x, 0f, vector.y);
			default:
				Log.Error("BaseHeadOffsetAt error in " + pawn);
				return Vector3.zero;
			}
		}

		public void Notify_DamageApplied(DamageInfo dam)
		{
			graphics.flasher.Notify_DamageApplied(dam);
			wiggler.Notify_DamageApplied(dam);
		}

		public void ProcessPostTickVisuals(int ticksPassed)
		{
			wiggler.ProcessPostTickVisuals(ticksPassed);
		}

		public void EffectersTick(bool suspended)
		{
			effecters.EffectersTick(suspended);
		}

		public static bool RenderAsPack(Apparel apparel)
		{
			if (apparel.def.apparel.LastLayer.IsUtilityLayer)
			{
				if (apparel.def.apparel.wornGraphicData != null)
				{
					return apparel.def.apparel.wornGraphicData.renderUtilityAsPack;
				}
				return true;
			}
			return false;
		}

		private void DrawDebug()
		{
			if (DebugViewSettings.drawDuties && Find.Selector.IsSelected(pawn) && pawn.mindState != null && pawn.mindState.duty != null)
			{
				pawn.mindState.duty.DrawDebug(pawn);
			}
		}
	}
}
