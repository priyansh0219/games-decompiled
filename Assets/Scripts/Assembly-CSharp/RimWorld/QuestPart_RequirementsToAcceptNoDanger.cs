using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_RequirementsToAcceptNoDanger : QuestPart_RequirementsToAccept
	{
		public MapParent mapParent;

		public Faction dangerTo;

		public override IEnumerable<GlobalTargetInfo> Culprits
		{
			get
			{
				if (mapParent != null && mapParent.HasMap && GenHostility.AnyHostileActiveThreatTo(mapParent.Map, dangerTo, out var threat))
				{
					yield return (Thing)threat;
				}
			}
		}

		public override AcceptanceReport CanAccept()
		{
			if (mapParent != null && mapParent.HasMap && GenHostility.AnyHostileActiveThreatTo(mapParent.Map, dangerTo))
			{
				return new AcceptanceReport("QuestRequiresNoDangerOnMap".Translate());
			}
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref mapParent, "mapParent");
			Scribe_References.Look(ref dangerTo, "dangerTo");
		}
	}
}
