public interface LargeWorldEntitySpawner
{
	EntitySlot.Filler GetPrefabForSlot(IEntitySlot slot, bool filterKnown = true);

	void ResetSpawner();
}
