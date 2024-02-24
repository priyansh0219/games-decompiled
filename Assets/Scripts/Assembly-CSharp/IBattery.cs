public interface IBattery
{
	float charge { get; set; }

	float capacity { get; }

	string GetChargeValueText();
}
