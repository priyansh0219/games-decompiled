using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnWoundDrawer : PawnOverlayDrawer
	{
		private static List<BodyTypeDef.WoundAnchor> tmpAnchors = new List<BodyTypeDef.WoundAnchor>();

		private BodyPartRecord debugDrawPart;

		private bool debugDrawAllParts;

		public const int MaxVisibleHediffsNonHuman = 5;

		public PawnWoundDrawer(Pawn pawn)
			: base(pawn)
		{
		}

		[DebugAction("General", "Enable wound debug draw", false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void WoundDebug()
		{
			IntVec3 c = UI.MouseCell();
			Pawn pawn = c.GetFirstPawn(Find.CurrentMap);
			if (pawn == null || pawn.def.race == null || pawn.def.race.body == null)
			{
				return;
			}
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			list.Add(new DebugMenuOption("All", DebugMenuOptionMode.Action, delegate
			{
				pawn.Drawer.renderer.WoundOverlays.debugDrawAllParts = true;
				pawn.Drawer.renderer.WoundOverlays.ClearCache();
				PortraitsCache.SetDirty(pawn);
				GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
			}));
			List<BodyPartRecord> allParts = pawn.def.race.body.AllParts;
			for (int i = 0; i < allParts.Count; i++)
			{
				BodyPartRecord part = allParts[i];
				list.Add(new DebugMenuOption(part.LabelCap, DebugMenuOptionMode.Action, delegate
				{
					pawn.Drawer.renderer.WoundOverlays.debugDrawPart = part;
					pawn.Drawer.renderer.WoundOverlays.ClearCache();
					PortraitsCache.SetDirty(pawn);
					GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", "Wound debug export (non-humanlike)", false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void WoundDebugExport()
		{
			string text = Application.dataPath + "\\woundDump";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			HashSet<RaceProperties> hashSet = new HashSet<RaceProperties>();
			foreach (PawnKindDef item in DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef pkd) => !pkd.RaceProps.Humanlike))
			{
				if (!hashSet.Contains(item.RaceProps))
				{
					Pawn pawn = PawnGenerator.GeneratePawn(item);
					for (int i = 0; i < 4; i++)
					{
						Rot4 rot = new Rot4((byte)i);
						RenderTexture temporary = RenderTexture.GetTemporary(256, 256, 32, RenderTextureFormat.ARGB32);
						temporary.name = "WoundDebugExport";
						pawn.Drawer.renderer.WoundOverlays.debugDrawAllParts = true;
						pawn.Drawer.renderer.WoundOverlays.ClearCache();
						Find.PawnCacheRenderer.RenderPawn(pawn, temporary, Vector3.zero, 1f, 0f, rot);
						pawn.Drawer.renderer.WoundOverlays.debugDrawAllParts = false;
						pawn.Drawer.renderer.WoundOverlays.ClearCache();
						Texture2D texture2D = new Texture2D(temporary.width, temporary.height, TextureFormat.ARGB32, 0, linear: false);
						RenderTexture.active = temporary;
						texture2D.ReadPixels(new Rect(0f, 0f, temporary.width, temporary.height), 0, 0, recalculateMipMaps: true);
						RenderTexture.active = null;
						RenderTexture.ReleaseTemporary(temporary);
						File.WriteAllBytes(string.Concat((string)(text + "\\" + pawn.def.LabelCap + "_"), rot, ".png"), texture2D.EncodeToPNG());
					}
					pawn.Destroy();
					hashSet.Add(item.RaceProps);
				}
			}
			Log.Message("Dumped to " + text);
		}

		protected override void WriteCache(CacheKey key, List<DrawCall> writeTarget)
		{
			Rot4 pawnRot = key.pawnRot;
			Quaternion quat = key.quat;
			Mesh bodyMesh = key.bodyMesh;
			Vector3 drawLoc = key.drawLoc;
			OverlayLayer layer = key.layer;
			if (debugDrawPart != null)
			{
				List<BodyTypeDef.WoundAnchor> list = FindAnchors(debugDrawPart);
				if (list.Count > 0)
				{
					foreach (BodyTypeDef.WoundAnchor item in list)
					{
						if (AnchorUseable(item))
						{
							Material overlayMat = MaterialPool.MatFrom(new MaterialRequest(BaseContent.WhiteTex, ShaderDatabase.SolidColor, item.debugColor));
							CalcAnchorData(item, out var anchorOffset2, out var range2);
							if (item.layer == layer)
							{
								writeTarget.Add(new DrawCall
								{
									overlayMat = overlayMat,
									TRS = Matrix4x4.TRS(drawLoc + anchorOffset2, quat, Vector3.one * range2 * pawn.story.bodyType.woundScale),
									overlayMesh = MeshPool.circle,
									displayOverApparel = true,
									maskTexOffset = Vector4.zero,
									maskTexScale = Vector4.one
								});
							}
						}
					}
					return;
				}
				GetDefaultAnchor(out var anchorOffset3, out var range3);
				writeTarget.Add(new DrawCall
				{
					overlayMat = BaseContent.BadMat,
					TRS = Matrix4x4.TRS(drawLoc + anchorOffset3, quat, Vector3.one * range3),
					overlayMesh = MeshPool.circle,
					displayOverApparel = true,
					maskTexOffset = Vector4.zero,
					maskTexScale = Vector4.one
				});
				return;
			}
			if (debugDrawAllParts)
			{
				if (pawn.story != null && pawn.story.bodyType != null && pawn.story.bodyType.woundAnchors != null)
				{
					foreach (BodyTypeDef.WoundAnchor woundAnchor2 in pawn.story.bodyType.woundAnchors)
					{
						if (AnchorUseable(woundAnchor2))
						{
							Material overlayMat2 = MaterialPool.MatFrom(new MaterialRequest(BaseContent.WhiteTex, ShaderDatabase.SolidColor, woundAnchor2.debugColor));
							CalcAnchorData(woundAnchor2, out var anchorOffset4, out var range4);
							if (woundAnchor2.layer == layer)
							{
								writeTarget.Add(new DrawCall
								{
									overlayMat = overlayMat2,
									TRS = Matrix4x4.TRS(drawLoc + anchorOffset4, quat, Vector3.one * range4),
									overlayMesh = MeshPool.circle,
									displayOverApparel = true,
									maskTexOffset = Vector4.zero,
									maskTexScale = Vector4.one
								});
							}
						}
					}
				}
				else
				{
					GetDefaultAnchor(out var anchorOffset5, out var range5);
					writeTarget.Add(new DrawCall
					{
						overlayMat = BaseContent.BadMat,
						TRS = Matrix4x4.TRS(drawLoc + anchorOffset5, quat, Vector3.one * range5),
						overlayMesh = MeshPool.circle,
						displayOverApparel = true,
						maskTexOffset = Vector4.zero,
						maskTexScale = Vector4.one
					});
				}
			}
			List<Hediff_MissingPart> missingPartsCommonAncestors = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
			for (int i = 0; i < pawn.health.hediffSet.hediffs.Count && (pawn.RaceProps.Humanlike || writeTarget.Count < 5); i++)
			{
				Hediff hediff = pawn.health.hediffSet.hediffs[i];
				if (hediff.Part == null || !hediff.Visible || !hediff.def.displayWound || (hediff is Hediff_MissingPart && !missingPartsCommonAncestors.Contains(hediff)))
				{
					continue;
				}
				if (hediff is Hediff_AddedPart && pawn.apparel != null)
				{
					bool flag = false;
					foreach (Apparel item2 in pawn.apparel.WornApparel)
					{
						if (item2.def.apparel.blocksAddedPartWoundGraphics && item2.def.apparel.CoversBodyPart(hediff.Part))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				float range6 = 0f;
				Vector3 anchorOffset6 = Vector3.zero;
				string text = null;
				if (pawn.story != null && pawn.story.bodyType != null && pawn.story.bodyType.woundAnchors != null)
				{
					List<BodyTypeDef.WoundAnchor> list2 = FindAnchors(hediff.Part);
					if (list2.Count > 0)
					{
						for (int num = list2.Count - 1; num >= 0; num--)
						{
							if (list2[num].layer != layer || !AnchorUseable(list2[num]))
							{
								list2.RemoveAt(num);
							}
						}
						if (list2.Count == 0)
						{
							continue;
						}
						BodyTypeDef.WoundAnchor woundAnchor = list2.RandomElement();
						CalcAnchorData(woundAnchor, out anchorOffset6, out range6);
						range6 = hediff.def.woundAnchorRange ?? range6;
						text = woundAnchor.tag;
					}
				}
				else
				{
					GetDefaultAnchor(out anchorOffset6, out range6);
				}
				Quaternion.AngleAxis(Rand.Range(0, 360), Vector3.up);
				Rand.PushState(pawn.thingIDNumber * i * pawnRot.AsInt);
				try
				{
					FleshTypeDef.ResolvedWound resolvedWound = pawn.RaceProps.FleshType.ChooseWoundOverlay(hediff);
					if (resolvedWound != null && (resolvedWound.wound.displayPermanent || !(hediff is Hediff_Injury hd) || !hd.IsPermanent()))
					{
						Vector3 insideUnitCircleVec = Rand.InsideUnitCircleVec3;
						if (pawnRot == Rot4.East)
						{
							insideUnitCircleVec.x *= -1f;
						}
						Vector3 vector = Vector3.zero;
						if (text == "LeftEye" || text == "RightEye")
						{
							vector = pawn.Drawer.renderer.BaseHeadOffsetAt(pawnRot);
						}
						Vector3 vector2 = resolvedWound.wound.drawOffsetSouth;
						if (pawnRot.IsHorizontal)
						{
							vector2 = resolvedWound.wound.drawOffsetEastWest.ScaledBy((pawnRot == Rot4.East) ? Vector3.one : new Vector3(-1f, 1f, 1f));
						}
						Vector3 pos = drawLoc + vector + anchorOffset6 + quat * insideUnitCircleVec * range6 + quat * vector2;
						Vector3 vector3 = drawLoc + vector + anchorOffset6 + insideUnitCircleVec * range6 + quat * vector2;
						Vector4? maskTexOffset = null;
						Vector4? maskTexScale = null;
						bool flip;
						Material material = resolvedWound.GetMaterial(pawnRot, out flip);
						if (resolvedWound.wound.flipOnWoundAnchorTag != null && resolvedWound.wound.flipOnWoundAnchorTag == text && resolvedWound.wound.flipOnRotation == pawnRot)
						{
							flip = !flip;
						}
						Mesh mesh = (flip ? MeshPool.GridPlaneFlip(Vector2.one * 0.25f) : MeshPool.GridPlane(Vector2.one * 0.25f));
						if (!pawn.def.race.Humanlike)
						{
							MaterialRequest req = default(MaterialRequest);
							bool flag2 = (pawn.Drawer.renderer.graphics.nakedGraphic.EastFlipped && pawnRot == Rot4.East) || (pawn.Drawer.renderer.graphics.nakedGraphic.WestFlipped && pawnRot == Rot4.West);
							req.maskTex = (Texture2D)pawn.Drawer.renderer.graphics.nakedGraphic.MatAt(pawnRot).mainTexture;
							req.mainTex = material.mainTexture;
							req.color = material.color;
							req.shader = material.shader;
							material = MaterialPool.MatFrom(req);
							Vector3 size = bodyMesh.bounds.size;
							Vector3 extents = bodyMesh.bounds.extents;
							Vector3 size2 = mesh.bounds.size;
							Vector3 extents2 = mesh.bounds.extents;
							Vector3 vector4 = vector3 - extents2;
							Vector3 vector5 = drawLoc - extents;
							maskTexScale = new Vector4(size2.x / size.x, size2.z / size.z);
							maskTexOffset = new Vector4((vector4.x - vector5.x) / size.x, (vector4.z - vector5.z) / size.z, flag2 ? 1 : 0);
						}
						Matrix4x4 tRS = Matrix4x4.TRS(pos, quat, Vector3.one * resolvedWound.wound.scale);
						writeTarget.Add(new DrawCall
						{
							overlayMat = material,
							TRS = tRS,
							overlayMesh = mesh,
							displayOverApparel = resolvedWound.wound.displayOverApparel,
							useSkinColor = resolvedWound.wound.tintWithSkinColor,
							maskTexScale = maskTexScale,
							maskTexOffset = maskTexOffset
						});
					}
				}
				finally
				{
					Rand.PopState();
				}
			}
			bool AnchorUseable(BodyTypeDef.WoundAnchor anchor)
			{
				if ((!anchor.rotation.HasValue || anchor.rotation.Value == pawnRot || (anchor.canMirror && anchor.rotation.Value == pawnRot.Opposite)) && (!anchor.narrowCrown.HasValue || (pawn.story != null && pawn.story.headType.narrow == anchor.narrowCrown.Value)))
				{
					if (!pawn.health.hediffSet.HasHead)
					{
						return anchor.layer != OverlayLayer.Head;
					}
					return true;
				}
				return false;
			}
			void CalcAnchorData(BodyTypeDef.WoundAnchor anchor, out Vector3 anchorOffset, out float range)
			{
				anchorOffset = anchor.offset;
				if (anchor.rotation == pawnRot.Opposite)
				{
					anchorOffset.x *= -1f;
				}
				if ((anchor.tag == "LeftEye" || anchor.tag == "RightEye") && pawnRot.IsHorizontal)
				{
					Vector3? eyeOffsetEastWest = pawn.story.headType.eyeOffsetEastWest;
					if (eyeOffsetEastWest.HasValue)
					{
						if (pawnRot == Rot4.East)
						{
							anchorOffset = eyeOffsetEastWest.Value;
						}
						else
						{
							anchorOffset = eyeOffsetEastWest.Value.ScaledBy(new Vector3(-1f, 1f, 1f));
						}
					}
				}
				anchorOffset = quat * anchorOffset;
				range = anchor.range;
			}
			List<BodyTypeDef.WoundAnchor> FindAnchors(BodyPartRecord curPart)
			{
				tmpAnchors.Clear();
				if (pawn.story == null || pawn.story.bodyType == null || pawn.story.bodyType.woundAnchors.NullOrEmpty())
				{
					return tmpAnchors;
				}
				int num2 = 0;
				while (tmpAnchors.Count == 0 && curPart != null && num2 < 100)
				{
					if (curPart.woundAnchorTag != null)
					{
						foreach (BodyTypeDef.WoundAnchor woundAnchor3 in pawn.story.bodyType.woundAnchors)
						{
							if (woundAnchor3.tag == curPart.woundAnchorTag)
							{
								tmpAnchors.Add(woundAnchor3);
							}
						}
					}
					else
					{
						foreach (BodyTypeDef.WoundAnchor woundAnchor4 in pawn.story.bodyType.woundAnchors)
						{
							if (curPart.IsInGroup(woundAnchor4.group))
							{
								tmpAnchors.Add(woundAnchor4);
							}
						}
					}
					curPart = curPart.parent;
					num2++;
				}
				if (num2 == 100)
				{
					Log.Error("PawnWoundDrawer.RenderOverBody.FindAnchors while() loop ran into iteration limit! This is never supposed to happen! Is there a cyclic body part parent reference?");
				}
				return tmpAnchors;
			}
			void GetDefaultAnchor(out Vector3 anchorOffset, out float range)
			{
				anchorOffset = quat * bodyMesh.bounds.center;
				range = Mathf.Min(bodyMesh.bounds.extents.x, bodyMesh.bounds.extents.z) / 2f;
			}
		}
	}
}
