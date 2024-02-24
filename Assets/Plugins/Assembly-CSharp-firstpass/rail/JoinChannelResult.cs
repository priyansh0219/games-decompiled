namespace rail
{
	public class JoinChannelResult : EventBase
	{
		public ulong channel_id;

		public RailID local_peer = new RailID();
	}
}
