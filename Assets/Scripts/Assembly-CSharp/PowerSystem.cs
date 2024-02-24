public static class PowerSystem
{
	public enum Status
	{
		Offline = 0,
		Emergency = 1,
		Normal = 2
	}

	public static bool ConsumeEnergy(this IPowerInterface powerInterface, float amount, out float amountConsumed)
	{
		float modified;
		bool result = powerInterface.ModifyPower(0f - amount, out modified);
		amountConsumed = 0f - modified;
		return result;
	}

	public static bool AddEnergy(this IPowerInterface powerInterface, float amount, out float amountStored)
	{
		return powerInterface.ModifyPower(amount, out amountStored);
	}
}
