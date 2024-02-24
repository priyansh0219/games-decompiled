namespace rail
{
	public class CreateChannelResult : EventBase
	{
		public ulong channel_id;

		public RailID local_peer = new RailID();
	}
}
