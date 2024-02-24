public interface VoxelandBulkChunkBuilder : VoxelandChunkBuilder
{
	void OnBeginBuildingChunks(Voxeland land, int totalChunks);

	void OnEndBuildingChunks(Voxeland land);
}
