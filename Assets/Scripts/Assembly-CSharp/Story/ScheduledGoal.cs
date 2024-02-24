using ProtoBuf;

namespace Story
{
	[ProtoContract]
	public class ScheduledGoal
	{
		private const int currentVersion = 1;

		[ProtoMember(1)]
		public int version = 1;

		[ProtoMember(2)]
		public float timeExecute;

		[ProtoMember(3)]
		public string goalKey;

		[ProtoMember(4)]
		public GoalType goalType;
	}
}
