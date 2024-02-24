using System.Collections.Generic;
using UnityEngine;

public interface IVoxelandChunk : IVoxelandChunkInfo
{
	VoxelandChunkWorkspace ws { get; }

	int downsamples { get; }

	int offsetX { get; }

	int offsetY { get; }

	int offsetZ { get; }

	int meshRes { get; }

	bool skipHiRes { get; }

	float surfaceDensityValue { get; }

	bool disableGrass { get; }

	bool debugThoroughTopologyChecks { get; }

	void OnTypeUsed(byte typeNum);

	bool IsBlockVisible(int x, int y, int z);

	Vector3 ComputeSurfaceIntersection(Vector3 p0, Vector3 p1, byte d0, byte d1);

	IEnumerable<VoxelandChunk.GrassPos> EnumerateGrass(VoxelandTypeBase settings, byte typeFilter, int randSeed, double reduction);
}
