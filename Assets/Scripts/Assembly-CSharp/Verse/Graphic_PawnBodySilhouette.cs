using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Graphic_PawnBodySilhouette : Graphic_Mote
	{
		private GraphicRequest request;

		private Pawn lastPawn;

		private Rot4 lastFacing;

		private Material bodyMaterial;

		private Material headMaterial;

		protected override bool ForcePropertyBlock => true;

		public override void Init(GraphicRequest req)
		{
			data = req.graphicData;
			path = req.path;
			maskPath = req.maskPath;
			color = req.color;
			colorTwo = req.colorTwo;
			drawSize = req.drawSize;
			request = req;
		}

		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
		{
			Mote mote = (Mote)thing;
			Color value = color;
			value.a *= mote.Alpha;
			Corpse obj = mote.link1.Target.Thing as Corpse;
			Pawn pawn = mote.link1.Target.Thing as Pawn;
			Pawn pawn2 = obj?.InnerPawn ?? pawn;
			if (pawn2 == null)
			{
				pawn2 = lastPawn;
			}
			PawnRenderer renderer = pawn2.Drawer.renderer;
			if (!renderer.graphics.AllResolved)
			{
				return;
			}
			Rot4 rot2 = ((pawn2.GetPosture() == PawnPosture.Standing) ? pawn2.Rotation : renderer.LayingFacing());
			Vector3 drawPos = pawn2.DrawPos;
			Building_Bed building_Bed = pawn2.CurrentBed();
			if (building_Bed != null)
			{
				Rot4 rotation = building_Bed.Rotation;
				rotation.AsInt += 2;
				drawPos -= rotation.FacingCell.ToVector3() * (pawn2.story.bodyType.bedOffset + pawn2.Drawer.renderer.BaseHeadOffsetAt(Rot4.South).z);
			}
			PawnPosture posture = pawn2.GetPosture();
			drawPos.y = mote.def.Altitude;
			if (lastPawn != pawn2 || lastFacing != rot2)
			{
				bodyMaterial = MakeMatFrom(request, renderer.graphics.MatsBodyBaseAt(rot2, pawn2.Dead, RotDrawMode.Fresh, drawClothes: false)[0].mainTexture);
			}
			Mesh mesh = null;
			mesh = ((!pawn2.RaceProps.Humanlike) ? renderer.graphics.nakedGraphic.MeshAt(rot2) : HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn2).MeshAt(rot2));
			bodyMaterial.SetVector("_pawnCenterWorld", new Vector4(drawPos.x, drawPos.z, 0f, 0f));
			bodyMaterial.SetVector("_pawnDrawSizeWorld", new Vector4(mesh.bounds.size.x, mesh.bounds.size.z, 0f, 0f));
			bodyMaterial.SetFloat(ShaderPropertyIDs.AgeSecs, mote.AgeSecs);
			bodyMaterial.SetColor(ShaderPropertyIDs.Color, value);
			Quaternion quaternion = Quaternion.AngleAxis((posture == PawnPosture.Standing) ? 0f : renderer.BodyAngle(), Vector3.up);
			if (building_Bed == null || building_Bed.def.building.bed_showSleeperBody)
			{
				GenDraw.DrawMeshNowOrLater(mesh, drawPos, quaternion, bodyMaterial, drawNow: false);
			}
			if (pawn2.RaceProps.Humanlike)
			{
				if (lastPawn != pawn2 || lastFacing != rot2)
				{
					headMaterial = MakeMatFrom(request, renderer.graphics.headGraphic.MatAt(rot2).mainTexture);
				}
				Vector3 vector = quaternion * renderer.BaseHeadOffsetAt(rot2) + new Vector3(0f, 0.001f, 0f);
				Mesh mesh2 = HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn2).MeshAt(rot2);
				headMaterial.SetVector("_pawnCenterWorld", new Vector4(drawPos.x, drawPos.z, 0f, 0f));
				headMaterial.SetVector("_pawnDrawSizeWorld", new Vector4(mesh2.bounds.size.x, mesh.bounds.size.z, 0f, 0f));
				headMaterial.SetFloat(ShaderPropertyIDs.AgeSecs, mote.AgeSecs);
				headMaterial.SetColor(ShaderPropertyIDs.Color, value);
				GenDraw.DrawMeshNowOrLater(mesh2, drawPos + vector, quaternion, headMaterial, drawNow: false);
			}
			if (pawn2 != null)
			{
				lastPawn = pawn2;
			}
			lastFacing = rot2;
		}

		private Material MakeMatFrom(GraphicRequest req, Texture mainTex)
		{
			MaterialRequest req2 = default(MaterialRequest);
			req2.mainTex = mainTex;
			req2.shader = req.shader;
			req2.color = color;
			req2.colorTwo = colorTwo;
			req2.renderQueue = req.renderQueue;
			req2.shaderParameters = req.shaderParameters;
			return MaterialPool.MatFrom(req2);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
