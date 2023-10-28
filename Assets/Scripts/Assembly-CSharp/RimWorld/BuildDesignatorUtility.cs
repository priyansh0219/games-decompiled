using Verse;

namespace RimWorld
{
	public static class BuildDesignatorUtility
	{
		public static void TryDrawPowerGridAndAnticipatedConnection(BuildableDef def, Rot4 rotation)
		{
			if (!(def is ThingDef thingDef) || (!thingDef.EverTransmitsPower && !thingDef.ConnectToPower))
			{
				return;
			}
			OverlayDrawHandler.DrawPowerGridOverlayThisFrame();
			if (thingDef.ConnectToPower)
			{
				IntVec3 intVec = UI.MouseCell();
				CompPower compPower = PowerConnectionMaker.BestTransmitterForConnector(intVec, Find.CurrentMap);
				if (compPower != null && !compPower.parent.Position.Fogged(compPower.parent.Map))
				{
					PowerNetGraphics.RenderAnticipatedWirePieceConnecting(intVec, rotation, def.Size, compPower.parent);
				}
			}
		}
	}
}
