namespace rail
{
	public class ChannelMemberStateChanged : EventBase
	{
		public ulong channel_id;

		public RailID local_peer = new RailID();

		public RailChannelMemberState member_state;

		public RailID remote_peer = new RailID();
	}
}
