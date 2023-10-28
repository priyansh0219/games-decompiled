using Verse;

namespace RimWorld
{
	public class CompTargetable_AllAnimalsOnTheMap : CompTargetable_AllPawnsOnTheMap
	{
		protected override TargetingParameters GetTargetingParameters()
		{
			TargetingParameters targetingParameters = base.GetTargetingParameters();
			targetingParameters.validator = delegate(TargetInfo targ)
			{
				if (!BaseTargetValidator(targ.Thing))
				{
					return false;
				}
				return targ.Thing is Pawn pawn && pawn.RaceProps.Animal;
			};
			return targetingParameters;
		}
	}
}
