using Verse;

namespace RimWorld
{
	public class PrisonerInteractionModeDef : Def
	{
		public int listOrder;

		public bool allowOnWildMan = true;

		public bool hideIfNoBloodfeeders;

		public bool hideOnHemogenicPawns;

		public bool mustBeAwake = true;

		public bool allowInClassicIdeoMode = true;

		public bool hideIfNotRecruitable;
	}
}
