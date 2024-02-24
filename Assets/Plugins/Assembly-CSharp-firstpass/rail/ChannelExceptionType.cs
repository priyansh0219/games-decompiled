namespace rail
{
	public enum ChannelExceptionType
	{
		kExceptionNone = 0,
		kExceptionLocalNetworkError = 1,
		kExceptionRelayAddressFailed = 2,
		kExceptionNegotiationRequestFailed = 3,
		kExceptionNegotiationResponseFailed = 4,
		kExceptionNegotiationResponseDataInvalid = 5,
		kExceptionNegotiationResponseTimeout = 6,
		kExceptionRelayServerOverload = 7,
		kExceptionRelayServerInternalError = 8,
		kExceptionRelayChannelUserFull = 9,
		kExceptionRelayChannelNotFound = 10,
		kExceptionRelayChannelEndByServer = 11
	}
}
