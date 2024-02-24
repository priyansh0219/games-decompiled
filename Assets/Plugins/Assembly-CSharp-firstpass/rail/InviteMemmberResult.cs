namespace rail
{
	public class InviteMemmberResult : EventBase
	{
		public ulong channel_id;

		public RailID local_peer = new RailID();

		public RailID remote_peer = new RailID();
	}
}
