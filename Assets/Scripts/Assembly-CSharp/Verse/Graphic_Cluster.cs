using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Graphic_Cluster : Graphic_Collection
	{
		private const float PositionVariance = 0.45f;

		private const float SizeVariance = 0.2f;

		private const float SizeFactorMin = 0.8f;

		private const float SizeFactorMax = 1.2f;

		public override Material MatSingle => subGraphics[Rand.Range(0, subGraphics.Length)].MatSingle;

		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
		{
			Log.ErrorOnce("Graphic_Scatter cannot draw realtime.", 9432243);
		}

		public override void Print(SectionLayer layer, Thing thing, float extraRotation)
		{
			Vector3 vector = thing.TrueCenter();
			Rand.PushState();
			Rand.Seed = thing.Position.GetHashCode();
			int num = ((thing is Filth filth) ? filth.thickness : 3);
			for (int i = 0; i < num; i++)
			{
				Material material = MatSingle;
				Vector3 center = vector + new Vector3(Rand.Range(-0.45f, 0.45f), 0f, Rand.Range(-0.45f, 0.45f));
				Vector2 size = new Vector2(Rand.Range(data.drawSize.x * 0.8f, data.drawSize.x * 1.2f), Rand.Range(data.drawSize.y * 0.8f, data.drawSize.y * 1.2f));
				float rot = (float)Rand.RangeInclusive(0, 360) + extraRotation;
				bool flipUv = Rand.Value < 0.5f;
				Graphic.TryGetTextureAtlasReplacementInfo(material, thing.def.category.ToAtlasGroup(), flipUv, vertexColors: true, out material, out var uvs, out var vertexColor);
				Printer_Plane.PrintPlane(layer, center, size, material, rot, flipUv, uvs, new Color32[4] { vertexColor, vertexColor, vertexColor, vertexColor });
			}
			Rand.PopState();
		}

		public override string ToString()
		{
			return "Scatter(subGraphic[0]=" + subGraphics[0].ToString() + ", count=" + subGraphics.Length + ")";
		}
	}
}
