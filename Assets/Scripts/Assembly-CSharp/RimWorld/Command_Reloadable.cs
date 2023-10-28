using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Command_Reloadable : Command_VerbTarget
	{
		private readonly CompReloadable comp;

		public Color? overrideColor;

		public override string TopRightLabel => comp.LabelRemaining;

		public override Color IconDrawColor => overrideColor ?? base.IconDrawColor;

		public Command_Reloadable(CompReloadable comp)
		{
			this.comp = comp;
		}

		public override void GizmoUpdateOnMouseover()
		{
			verb.DrawHighlight(LocalTargetInfo.Invalid);
		}

		public override bool GroupsWith(Gizmo other)
		{
			if (!base.GroupsWith(other))
			{
				return false;
			}
			if (!(other is Command_Reloadable command_Reloadable))
			{
				return false;
			}
			return comp.parent.def == command_Reloadable.comp.parent.def;
		}
	}
}
