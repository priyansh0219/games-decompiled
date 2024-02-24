public interface VoxelandRasterizer
{
	void Rasterize(Voxeland land, Array3<byte> windowOut, Array3<byte> densityOut, Int3 size, int wx0, int wy0, int wz0, int downsamples);

	bool IsRangeUniform(Int3.Bounds range);

	bool IsRangeLoaded(Int3.Bounds range, int downsamples);

	void OnPreBuildRange(Int3.Bounds range);

	void LayoutDebugGUI();
}
