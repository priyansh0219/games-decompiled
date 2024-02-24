public interface IVoxelGrid
{
	VoxelandData.OctNode GetVoxel(int x, int y, int z);

	bool GetVoxelMask(int x, int y, int z);
}
