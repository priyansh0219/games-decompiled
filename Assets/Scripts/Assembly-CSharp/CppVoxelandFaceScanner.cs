using UWE;
using UnityEngine;

public class CppVoxelandFaceScanner : Voxeland.FaceCreator
{
	public bool debug;

	public static void VoxelandScanFaces(Int3 usedSize, ref byte typeGrid, ref byte densityGrid, Int3 gridSize, ref VoxelandChunkWorkspace.ChunkFaceId faceList, int maxFaces, ref int numFaces)
	{
		UnityUWE.VoxelandScanFaces(usedSize, ref typeGrid, ref densityGrid, gridSize, ref faceList, maxFaces, ref numFaces);
	}

	public static Int3 FaceDir2Offset(byte dir)
	{
		return new Int3(VoxelandChunk.VoxelandFace.dirToPosX[dir], VoxelandChunk.VoxelandFace.dirToPosY[dir], VoxelandChunk.VoxelandFace.dirToPosZ[dir]);
	}

	public void CreateFaces(IVoxelandChunk ck)
	{
		VoxelandChunkWorkspace ws = ck.ws;
		Voxeland.RasterWorkspace rws = ck.ws.rws;
		IVoxeland land = ck.land;
		Array3<VoxelandChunk.VoxelandBlock> blocks = ws.blocks;
		land.RasterizeVoxels(rws, ck.offsetX - (3 << ck.downsamples), ck.offsetY - (3 << ck.downsamples), ck.offsetZ - (3 << ck.downsamples), ck.downsamples);
		int numFaces = 0;
		while (true)
		{
			VoxelandScanFaces(rws.size, ref rws.typesGrid.data[0], ref rws.densityGrid.data[0], rws.typesGrid.Dims(), ref ws.faceList[0], ws.faceList.Length, ref numFaces);
			if (numFaces < ws.faceList.Length)
			{
				break;
			}
			ws.faceList = new VoxelandChunkWorkspace.ChunkFaceId[ws.faceList.Length + ws.faceList.Length / 2];
			for (int i = 0; i < ws.faceList.Length; i++)
			{
				ws.faceList[i].Reset();
			}
		}
		if (debug)
		{
			for (int j = 0; j < numFaces; j++)
			{
				_ = ref ws.faceList[j];
			}
		}
		Int3.Bounds bounds = new Int3.Bounds(Int3.zero, blocks.Dims() - 1);
		if (land.IsLimitedMeshing())
		{
			Int3 offset = ck.GetOffset();
			Int3 @int = new Int3(2) << ck.downsamples;
			Int3 int2 = land.meshMins - offset + @int >> ck.downsamples;
			Int3 int3 = land.meshMaxs - offset + @int >> ck.downsamples;
			bounds = bounds.IntersectionWith(new Int3.Bounds(int2 + 1, int3 - 1));
		}
		for (int k = 0; k < numFaces; k++)
		{
			VoxelandChunkWorkspace.ChunkFaceId chunkFaceId = ck.ws.faceList[k];
			if (chunkFaceId.dir > 6)
			{
				Debug.LogFormat("Skipping bad face.... Face Direction was {0}", chunkFaceId.dir);
				continue;
			}
			Int3 int4 = new Int3(chunkFaceId.dx, chunkFaceId.dy, chunkFaceId.dz);
			Int3 int5 = int4 - 1;
			if (bounds.Contains(int5))
			{
				Int3 int6 = int5 + FaceDir2Offset(chunkFaceId.dir);
				VoxelandChunk.VoxelandBlock voxelandBlock = blocks.Get(int5);
				if (voxelandBlock == null)
				{
					voxelandBlock = ws.NewBlock(int5.x, int5.y, int5.z);
					blocks.Set(int5, voxelandBlock);
					voxelandBlock.visible = ck.IsBlockVisible(int5.x, int5.y, int5.z);
				}
				VoxelandChunk.VoxelandFace voxelandFace = ws.NewFace();
				voxelandFace.dir = chunkFaceId.dir;
				voxelandFace.block = voxelandBlock;
				ws.faces.Add(voxelandFace);
				if (voxelandFace.dir < voxelandBlock.faces.Length)
				{
					voxelandBlock.faces[voxelandFace.dir] = voxelandFace;
				}
				else
				{
					Debug.LogFormat("Bad face direction when building faces. Faces Length = {0} Face Direction = {1}", voxelandBlock.faces.Length, voxelandFace.dir);
				}
				byte d = rws.densityGrid.Get(int5 + 1);
				byte d2 = rws.densityGrid.Get(int6 + 1);
				if (land.debugOneType)
				{
					voxelandFace.type = 1;
				}
				else
				{
					voxelandFace.type = rws.typesGrid.Get(int4);
				}
				ck.OnTypeUsed(voxelandFace.type);
				voxelandFace.surfaceIntx = ck.ComputeSurfaceIntersection(int5 - 2 + UWE.Utils.half3, int6 - 2 + UWE.Utils.half3, d, d2);
			}
		}
	}
}
