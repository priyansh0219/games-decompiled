using System.ComponentModel;

namespace Oculus.Platform.Models
{
	public enum PeerConnectionState : uint
	{
		[Description("UNKNOWN")]
		Unknown = 0u,
		[Description("CONNECTED")]
		Connected = 1u,
		[Description("TIMEOUT")]
		Timeout = 2u
	}
}
