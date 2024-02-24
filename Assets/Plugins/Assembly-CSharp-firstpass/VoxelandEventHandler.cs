public interface VoxelandEventHandler
{
	void OnChunkBuilt(Voxeland land, int cx, int cy, int cz);

	void OnChunkDestroyed(Voxeland land, int cx, int cy, int cz);

	void OnChunkHighLOD(Voxeland land, int cx, int cy, int cz);

	void OnChunkLowLOD(Voxeland land, int cx, int cy, int cz);
}
