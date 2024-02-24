namespace rail
{
	public class ChannelNetDelay : EventBase
	{
		public ulong channel_id;

		public RailID local_peer = new RailID();

		public uint net_delay_ms;
	}
}
