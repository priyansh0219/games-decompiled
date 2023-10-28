using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Study : Designator
	{
		public override int DraggableDimensions => 2;

		protected override DesignationDef Designation => DesignationDefOf.Study;

		public Designator_Study()
		{
			defaultLabel = "DesignatorStudy".Translate();
			defaultDesc = "DesignatorStudyDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/Study");
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
			soundSucceeded = SoundDefOf.Designate_Claim;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			return StudyThingsInCell(c).Any();
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			foreach (Thing item in StudyThingsInCell(c))
			{
				DesignateThing(item);
			}
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if (base.Map.designationManager.DesignationOn(t, Designation) != null)
			{
				return false;
			}
			if (!t.def.IsBuildingArtificial)
			{
				return false;
			}
			CompStudiable compStudiable = t.TryGetComp<CompStudiable>();
			return compStudiable != null && !compStudiable.Completed;
		}

		public override void DesignateThing(Thing t)
		{
			base.Map.designationManager.AddDesignation(new Designation(t, Designation));
		}

		private IEnumerable<Thing> StudyThingsInCell(IntVec3 c)
		{
			if (c.Fogged(base.Map))
			{
				yield break;
			}
			List<Thing> thingList = c.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (CanDesignateThing(thingList[i]).Accepted)
				{
					yield return thingList[i];
				}
			}
		}
	}
}
