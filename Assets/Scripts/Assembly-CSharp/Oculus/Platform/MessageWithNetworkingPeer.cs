using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithNetworkingPeer : Message<NetworkingPeer>
	{
		public MessageWithNetworkingPeer(IntPtr c_message)
			: base(c_message)
		{
		}

		protected override NetworkingPeer GetDataFromMessage(IntPtr c_message)
		{
			IntPtr networkingPeer = CAPI.ovr_Message_GetNetworkingPeer(c_message);
			return new NetworkingPeer(CAPI.ovr_NetworkingPeer_GetID(networkingPeer), (PeerConnectionState)CAPI.ovr_NetworkingPeer_GetState(networkingPeer));
		}

		public override NetworkingPeer GetNetworkingPeer()
		{
			return base.Data;
		}
	}
}
