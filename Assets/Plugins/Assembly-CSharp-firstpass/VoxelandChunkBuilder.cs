public interface VoxelandChunkBuilder
{
	bool CanBuildMore();

	void Build(ChunkState state);

	int GetMaxBuildsThisFrame();
}
