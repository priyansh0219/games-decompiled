public interface IEntitySlot
{
	BiomeType GetBiomeType();

	bool IsTypeAllowed(EntitySlot.Type slotType);

	float GetDensity();

	bool IsCreatureSlot();
}
