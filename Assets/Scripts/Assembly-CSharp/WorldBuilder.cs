public interface WorldBuilder
{
	int GetExplicitBlockType(int x, int y, int z);

	Int3 GetPlayerStartPosition();

	void DoAlgorithmicPass(WorldReadWrite world, Int3 mins, Int3 maxs);
}
