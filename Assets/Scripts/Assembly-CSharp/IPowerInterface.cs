public interface IPowerInterface
{
	float GetPower();

	float GetMaxPower();

	bool ModifyPower(float amount, out float modified);

	bool HasInboundPower(IPowerInterface powerInterface);

	bool GetInboundHasSource(IPowerInterface powerInterface);
}
