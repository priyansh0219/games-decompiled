namespace RimWorld
{
	public class ITab_Pawn_Slave : ITab_Pawn_Visitor
	{
		public override bool IsVisible => base.SelPawn.IsSlaveOfColony;

		public ITab_Pawn_Slave()
		{
			labelKey = "TabSlave";
			tutorTag = "Slave";
		}
	}
}
