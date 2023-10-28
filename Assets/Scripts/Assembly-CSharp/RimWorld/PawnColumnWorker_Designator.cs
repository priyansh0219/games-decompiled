using Verse;

namespace RimWorld
{
	public abstract class PawnColumnWorker_Designator : PawnColumnWorker_Checkbox
	{
		protected abstract DesignationDef DesignationType { get; }

		protected virtual void Notify_DesignationAdded(Pawn pawn)
		{
		}

		protected override bool GetValue(Pawn pawn)
		{
			return GetDesignation(pawn) != null;
		}

		protected override void SetValue(Pawn pawn, bool value, PawnTable table)
		{
			if (value == GetValue(pawn))
			{
				return;
			}
			if (table.SortingBy == def)
			{
				table.SetDirty();
			}
			if (value)
			{
				pawn.MapHeld.designationManager.AddDesignation(new Designation(pawn, DesignationType));
				Notify_DesignationAdded(pawn);
				return;
			}
			Designation designation = GetDesignation(pawn);
			if (designation != null)
			{
				pawn.MapHeld.designationManager.RemoveDesignation(designation);
			}
		}

		private Designation GetDesignation(Pawn pawn)
		{
			return pawn.MapHeld?.designationManager.DesignationOn(pawn, DesignationType);
		}
	}
}
