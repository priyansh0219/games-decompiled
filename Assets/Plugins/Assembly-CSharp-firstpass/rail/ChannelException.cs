namespace rail
{
	public class ChannelException : EventBase
	{
		public ChannelExceptionType exception_type;

		public ulong channel_id;

		public RailID local_peer = new RailID();
	}
}
