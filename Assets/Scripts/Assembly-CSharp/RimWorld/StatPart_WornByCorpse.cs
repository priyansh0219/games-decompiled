using Verse;

namespace RimWorld
{
	public class StatPart_WornByCorpse : StatPart
	{
		private const float Factor = 0.1f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && req.Thing is Apparel apparel && apparel.WornByCorpse)
			{
				val *= 0.1f;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && req.Thing is Apparel apparel && apparel.WornByCorpse)
			{
				return "StatsReport_WornByCorpse".Translate() + ": x" + 0.1f.ToStringPercent();
			}
			return null;
		}
	}
}
