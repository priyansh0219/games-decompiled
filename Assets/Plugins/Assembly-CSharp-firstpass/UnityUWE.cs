using System.Runtime.InteropServices;
using UnityEngine;

public static class UnityUWE
{
	private const string DllName = "UnityUWE";

	[DllImport("UnityUWE")]
	public static extern void FloodSimStep(ref byte flowGrid, ref float valueGrid, Int3 arrayDims, Int3 usedDims, float dt);

	[DllImport("UnityUWE")]
	public static extern void RasterizeNativeEntry(ref byte nodes, ushort nodeId, ref byte typesOut, ref byte densityOut, Int3 usedSize, Int3 gridSize, int wx, int wy, int wz, int ox, int oy, int oz, int h);

	[DllImport("UnityUWE")]
	public static extern void VoxelandScanFaces(Int3 usedSize, ref byte typeGrid, ref byte densityGrid, Int3 gridSize, ref VoxelandChunkWorkspace.ChunkFaceId faceList, int maxFaces, ref int numFaces);

	[DllImport("UnityUWE")]
	public static extern void SimplifyMesh(float maxError, float antiSliverWeight, ref Vector3 vertices, ref byte vertexFixed, ref int numVerts, ref SimplifyMeshPlugin.Face faces, ref int numFaces, ref int old2newVertIds, bool skipRandomPhase, bool writeOutput);

	[DllImport("UnityUWE")]
	public static extern float Return123Test();

	[DllImport("UnityUWE")]
	public static extern void DoubleArray(ref float arr, int count);

	[DllImport("UnityUWE")]
	public static extern void DumpObj(ref Vector3 vertices, ref byte vertexFixed, int numVerts, ref SimplifyMeshPlugin.Face faces, int numFaces);

	[DllImport("UnityUWE")]
	public static extern int CountTrues(ref byte bools, int size);

	[DllImport("UnityUWE")]
	public static extern float SumComponents(ref Vector3 verts, int numVerts);

	public static void Quit(int exitCode)
	{
		Application.Quit(exitCode);
	}
}
