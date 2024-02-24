using System;
using UnityEngine;

public static class SimplifyMeshPlugin
{
	[Serializable]
	public class Settings
	{
		public double maxError = 0.2;

		public double antiSliverWeight = 0.005;

		public bool skipRandomPhase;

		public bool writeDebugOutput;
	}

	public struct Face
	{
		public int a;

		public int b;

		public int c;

		public int material;

		public static int SizeBytes = 16;

		public int GetVert(int num)
		{
			switch (num)
			{
			default:
				return c;
			case 1:
				return b;
			case 0:
				return a;
			}
		}
	}

	public static void SimplifyMesh(float maxError, float antiSliverWeight, ref Vector3 vertices, ref byte vertexFixed, ref int numVerts, ref Face faces, ref int numFaces, ref int old2newVertIds, bool skipRandomPhase, bool writeOutput)
	{
		UnityUWE.SimplifyMesh(maxError, antiSliverWeight, ref vertices, ref vertexFixed, ref numVerts, ref faces, ref numFaces, ref old2newVertIds, skipRandomPhase, writeOutput);
	}

	public static void SimplifyMesh(Settings set, Vector3[] vertices, byte[] vertexFixed, ref int numVerts, Face[] faces, ref int numFaces, int[] old2newVertIds)
	{
		SimplifyMesh((float)set.maxError, (float)set.antiSliverWeight, ref vertices[0], ref vertexFixed[0], ref numVerts, ref faces[0], ref numFaces, ref old2newVertIds[0], set.skipRandomPhase, set.writeDebugOutput);
	}

	public static void DumpObj(ref Vector3 vertices, ref byte vertexFixed, int numVerts, ref Face faces, int numFaces)
	{
		UnityUWE.DumpObj(ref vertices, ref vertexFixed, numVerts, ref faces, numFaces);
	}

	public static int CountTrues(ref byte bools, int size)
	{
		return UnityUWE.CountTrues(ref bools, size);
	}

	public static float SumComponents(ref Vector3 verts, int numVerts)
	{
		return UnityUWE.SumComponents(ref verts, numVerts);
	}

	public static Face[] Indices2FaceArray(int[] indices)
	{
		int num = 0;
		for (int i = 0; i < indices.Length / 3; i++)
		{
			int num2 = indices[3 * i];
			int num3 = indices[3 * i + 1];
			int num4 = indices[3 * i + 2];
			if (num2 == num3 || num2 == num4)
			{
				break;
			}
			num++;
		}
		Face[] array = new Face[num];
		for (int j = 0; j < num; j++)
		{
			array[j].a = indices[3 * j];
			array[j].b = indices[3 * j + 1];
			array[j].c = indices[3 * j + 2];
		}
		return array;
	}
}
