using Verse;

namespace RimWorld
{
	public class SignalAction_Message : SignalAction
	{
		public string message;

		public bool historical = true;

		public MessageTypeDef messageType;

		public LookTargets lookTargets;

		protected override void DoAction(SignalArgs args)
		{
			Messages.Message(message, lookTargets, messageType ?? MessageTypeDefOf.NeutralEvent, historical);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref message, "message");
			Scribe_Values.Look(ref historical, "historical", defaultValue: false);
			Scribe_Deep.Look(ref lookTargets, "lookTargets");
			Scribe_Defs.Look(ref messageType, "messageType");
		}
	}
}
