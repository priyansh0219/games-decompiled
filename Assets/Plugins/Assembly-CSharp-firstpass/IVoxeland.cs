public interface IVoxeland
{
	bool debugBlocky { get; }

	bool debugLogMeshing { get; }

	bool debugOneType { get; }

	VoxelandNormalsSmooth normalsSmooth { get; }

	Voxeland.FaceCreator faceCreator { get; }

	VoxelandBlockType[] types { get; }

	Int3 meshMins { get; }

	Int3 meshMaxs { get; }

	VoxelandData data { get; }

	void RasterizeVoxels(Voxeland.RasterWorkspace ws, int wx0, int wy0, int wz0, int downsampleLevels);

	bool IsLimitedMeshing();
}
