using System.Collections.Generic;

namespace rail
{
	public interface IRailNetwork
	{
		RailResult AcceptSessionRequest(RailID local_peer, RailID remote_peer);

		RailResult SendData(RailID local_peer, RailID remote_peer, byte[] data_buf, uint data_len, uint message_type);

		RailResult SendData(RailID local_peer, RailID remote_peer, byte[] data_buf, uint data_len);

		RailResult SendReliableData(RailID local_peer, RailID remote_peer, byte[] data_buf, uint data_len, uint message_type);

		RailResult SendReliableData(RailID local_peer, RailID remote_peer, byte[] data_buf, uint data_len);

		bool IsDataReady(RailID local_peer, out uint data_len, out uint message_type);

		bool IsDataReady(RailID local_peer, out uint data_len);

		RailResult ReadData(RailID local_peer, RailID remote_peer, byte[] data_buf, uint data_len, uint message_type);

		RailResult ReadData(RailID local_peer, RailID remote_peer, byte[] data_buf, uint data_len);

		RailResult BlockMessageType(RailID local_peer, uint message_type);

		RailResult UnblockMessageType(RailID local_peer, uint message_type);

		RailResult CloseSession(RailID local_peer, RailID remote_peer);

		RailResult ResolveHostname(string domain, List<string> ip_list);

		RailResult GetSessionState(RailID remote_peer, RailNetworkSessionState session_state);
	}
}
