using UnityEngine;
using Verse;

namespace RimWorld.QuestGen
{
	public abstract class QuestNode_Root_AncientComplex : QuestNode
	{
		protected static readonly SimpleCurve ThreatPointsOverPointsCurve = new SimpleCurve
		{
			new CurvePoint(35f, 38.5f),
			new CurvePoint(400f, 165f),
			new CurvePoint(10000f, 4125f)
		};

		protected virtual SimpleCurve ComplexSizeOverPointsCurve => new SimpleCurve
		{
			new CurvePoint(0f, 30f),
			new CurvePoint(10000f, 50f)
		};

		protected virtual SimpleCurve TerminalsOverRoomCountCurve => new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(10f, 4f),
			new CurvePoint(20f, 6f),
			new CurvePoint(50f, 10f)
		};

		protected virtual ComplexDef ComplexDef => ComplexDefOf.AncientComplex;

		protected virtual SitePartDef SitePartDef => SitePartDefOf.AncientComplex;

		public virtual ComplexSketch GenerateSketch(float points, bool generateTerminals = true)
		{
			int num = (int)ComplexSizeOverPointsCurve.Evaluate(points);
			ComplexSketch complexSketch = ComplexDef.Worker.GenerateSketch(new IntVec2(num, num));
			if (generateTerminals)
			{
				int num2 = Mathf.FloorToInt(TerminalsOverRoomCountCurve.Evaluate(complexSketch.layout.Rooms.Count));
				for (int i = 0; i < num2; i++)
				{
					complexSketch.thingsToSpawn.Add(ThingMaker.MakeThing(ThingDefOf.AncientTerminal));
				}
			}
			return complexSketch;
		}
	}
}
