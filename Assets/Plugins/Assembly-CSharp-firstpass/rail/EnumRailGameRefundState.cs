namespace rail
{
	public enum EnumRailGameRefundState
	{
		kRailGameRefundStateUnknown = 0,
		kRailGameRefundStateApplyReceived = 1000,
		kRailGameRefundStateUserCancelApply = 1100,
		kRailGameRefundStateAdminCancelApply = 1101,
		kRailGameRefundStateRefundSuccess = 1200,
		kRailGameRefundStateRefundFailed = 1201
	}
}
