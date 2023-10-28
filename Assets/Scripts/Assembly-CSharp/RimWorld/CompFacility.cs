using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompFacility : ThingComp
	{
		private List<Thing> linkedBuildings = new List<Thing>();

		private HashSet<Thing> thingsToNotify = new HashSet<Thing>();

		public bool CanBeActive
		{
			get
			{
				CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
				if (compPowerTrader != null && !compPowerTrader.PowerOn)
				{
					return false;
				}
				return true;
			}
		}

		public List<Thing> LinkedBuildings => linkedBuildings;

		public CompProperties_Facility Props => (CompProperties_Facility)props;

		public static void DrawLinesToPotentialThingsToLinkTo(ThingDef myDef, IntVec3 myPos, Rot4 myRot, Map map)
		{
			CompProperties_Facility compProperties = myDef.GetCompProperties<CompProperties_Facility>();
			if (compProperties?.linkableBuildings == null)
			{
				return;
			}
			Vector3 a = GenThing.TrueCenter(myPos, myRot, myDef.size, myDef.Altitude);
			for (int i = 0; i < compProperties.linkableBuildings.Count; i++)
			{
				foreach (Thing item in map.listerThings.ThingsOfDef(compProperties.linkableBuildings[i]))
				{
					CompAffectedByFacilities compAffectedByFacilities = item.TryGetComp<CompAffectedByFacilities>();
					if (compAffectedByFacilities != null && compAffectedByFacilities.CanPotentiallyLinkTo(myDef, myPos, myRot))
					{
						GenDraw.DrawLineBetween(a, item.TrueCenter());
						compAffectedByFacilities.DrawRedLineToPotentiallySupplantedFacility(myDef, myPos, myRot);
					}
				}
			}
		}

		public static void DrawPlaceMouseAttachmentsToPotentialThingsToLinkTo(float curX, ref float curY, ThingDef myDef, IntVec3 myPos, Rot4 myRot, Map map)
		{
			CompProperties_Facility compProperties = myDef.GetCompProperties<CompProperties_Facility>();
			int num = 0;
			for (int i = 0; i < compProperties.linkableBuildings.Count; i++)
			{
				foreach (Thing item in map.listerThings.ThingsOfDef(compProperties.linkableBuildings[i]))
				{
					CompAffectedByFacilities compAffectedByFacilities = item.TryGetComp<CompAffectedByFacilities>();
					if (compAffectedByFacilities != null && compAffectedByFacilities.CanPotentiallyLinkTo(myDef, myPos, myRot))
					{
						num++;
						if (num == 1)
						{
							DrawTextLine(ref curY, "FacilityPotentiallyLinkedTo".Translate() + ":");
						}
						DrawTextLine(ref curY, "  - " + item.LabelCap);
					}
				}
			}
			if (num == 0)
			{
				DrawTextLine(ref curY, "FacilityNoPotentialLinks".Translate());
			}
			void DrawTextLine(ref float y, string text)
			{
				float lineHeight = Text.LineHeight;
				Widgets.Label(new Rect(curX, y, 999f, lineHeight), text);
				y += lineHeight;
			}
		}

		public void Notify_NewLink(Thing thing)
		{
			for (int i = 0; i < linkedBuildings.Count; i++)
			{
				if (linkedBuildings[i] == thing)
				{
					Log.Error("Notify_NewLink was called but the link is already here.");
					return;
				}
			}
			linkedBuildings.Add(thing);
		}

		public void Notify_LinkRemoved(Thing thing)
		{
			for (int i = 0; i < linkedBuildings.Count; i++)
			{
				if (linkedBuildings[i] == thing)
				{
					linkedBuildings.RemoveAt(i);
					return;
				}
			}
			Log.Error("Notify_LinkRemoved was called but there is no such link here.");
		}

		public void Notify_LOSBlockerSpawnedOrDespawned()
		{
			RelinkAll();
		}

		public void Notify_ThingChanged()
		{
			RelinkAll();
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			LinkToNearbyBuildings();
		}

		public override void PostDeSpawn(Map map)
		{
			thingsToNotify.Clear();
			for (int i = 0; i < linkedBuildings.Count; i++)
			{
				thingsToNotify.Add(linkedBuildings[i]);
			}
			UnlinkAll();
			foreach (Thing item in thingsToNotify)
			{
				item.TryGetComp<CompAffectedByFacilities>().Notify_FacilityDespawned();
			}
		}

		public override void PostDrawExtraSelectionOverlays()
		{
			for (int i = 0; i < linkedBuildings.Count; i++)
			{
				if (linkedBuildings[i].TryGetComp<CompAffectedByFacilities>().IsFacilityActive(parent))
				{
					GenDraw.DrawLineBetween(parent.TrueCenter(), linkedBuildings[i].TrueCenter());
				}
				else
				{
					GenDraw.DrawLineBetween(parent.TrueCenter(), linkedBuildings[i].TrueCenter(), CompAffectedByFacilities.InactiveFacilityLineMat);
				}
			}
		}

		public override string CompInspectStringExtra()
		{
			CompProperties_Facility compProperties_Facility = Props;
			if (compProperties_Facility.statOffsets == null)
			{
				return null;
			}
			bool flag = AmIActiveForAnyone();
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < compProperties_Facility.statOffsets.Count; i++)
			{
				StatDef stat = compProperties_Facility.statOffsets[i].stat;
				stringBuilder.Append(stat.LabelCap);
				stringBuilder.Append(": ");
				stringBuilder.Append(compProperties_Facility.statOffsets[i].ValueToStringAsOffset);
				if (!flag)
				{
					stringBuilder.Append(" (");
					stringBuilder.Append("InactiveFacility".Translate());
					stringBuilder.Append(")");
				}
				if (i < compProperties_Facility.statOffsets.Count - 1)
				{
					stringBuilder.AppendLine();
				}
			}
			return stringBuilder.ToString();
		}

		private void RelinkAll()
		{
			LinkToNearbyBuildings();
		}

		private void LinkToNearbyBuildings()
		{
			UnlinkAll();
			CompProperties_Facility compProperties_Facility = Props;
			if (compProperties_Facility.linkableBuildings == null)
			{
				return;
			}
			for (int i = 0; i < compProperties_Facility.linkableBuildings.Count; i++)
			{
				foreach (Thing item in parent.Map.listerThings.ThingsOfDef(compProperties_Facility.linkableBuildings[i]))
				{
					CompAffectedByFacilities compAffectedByFacilities = item.TryGetComp<CompAffectedByFacilities>();
					if (compAffectedByFacilities != null && compAffectedByFacilities.CanLinkTo(parent))
					{
						linkedBuildings.Add(item);
						compAffectedByFacilities.Notify_NewLink(parent);
					}
				}
			}
		}

		private bool AmIActiveForAnyone()
		{
			for (int i = 0; i < linkedBuildings.Count; i++)
			{
				if (linkedBuildings[i].TryGetComp<CompAffectedByFacilities>().IsFacilityActive(parent))
				{
					return true;
				}
			}
			return false;
		}

		private void UnlinkAll()
		{
			for (int i = 0; i < linkedBuildings.Count; i++)
			{
				linkedBuildings[i].TryGetComp<CompAffectedByFacilities>().Notify_LinkRemoved(parent);
			}
			linkedBuildings.Clear();
		}
	}
}
