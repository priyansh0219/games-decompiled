using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UWE;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[ExecuteInEditMode]
public class VoxelandChunk : MonoBehaviour, IVoxelandChunk, IVoxelandChunkInfo, IVoxelandChunk2, IManagedUpdateBehaviour, IManagedBehaviour
{
	public sealed class TypeUseByLayerComparer : IComparer<TypeUse>
	{
		private readonly IVoxeland land;

		public TypeUseByLayerComparer(IVoxeland land)
		{
			this.land = land;
		}

		public int Compare(TypeUse x, TypeUse y)
		{
			return CompareTypeUseByLayer(land, x, y);
		}
	}

	[Serializable]
	public class TypeUse
	{
		public byte num;

		public int count;
	}

	[Serializable]
	public class VLGrassVert
	{
		public Vector3 pos;

		public Vector3 normal;

		public Vector4 tangent;

		public Vector2 uv;

		public Color32 color;
	}

	[Serializable]
	public class VLGrassTri
	{
		public int v0;

		public int v1;

		public int v2;
	}

	[Serializable]
	public class VLGrassMesh
	{
		public byte type = byte.MaxValue;

		public Mesh mesh;

		public Material material;

		public List<VLGrassVert> verts = new List<VLGrassVert>();

		public List<VLGrassTri> tris = new List<VLGrassTri>();

		public void Reset()
		{
			verts.Clear();
			tris.Clear();
			type = byte.MaxValue;
		}
	}

	public struct VoxelandVertTest
	{
		public Vector3 pos;

		public Vector3 relaxed;

		public Vector3 normal;

		public Vector4 tangent;

		public bool processed;

		public VoxelandVert[] neigs;

		public byte neigCount;

		public VoxelandFace[] adjFaces;

		public float gloss;

		public int layerVertIndex;

		public int layerLowVertIndex;

		public float[] blendWeights;

		public float layerBlend;
	}

	[Serializable]
	public class VoxelandVert
	{
		public byte facePos;

		public bool welded;

		public Vector3 pos;

		public Vector3 relaxed;

		public Vector3 normal;

		public Vector4 tangent;

		public bool processed;

		public VoxelandVert[] neigs;

		public byte neigCount;

		public VoxelandFace[] adjFaces;

		public float gloss;

		public int layerVertIndex;

		public int layerLowVertIndex;

		public float[] blendWeights;

		public float layerBlend;

		public static Vector3[] posTable = new Vector3[54]
		{
			new Vector3(0f, 1f, 1f),
			new Vector3(0.5f, 1f, 1f),
			new Vector3(1f, 1f, 1f),
			new Vector3(1f, 1f, 0.5f),
			new Vector3(1f, 1f, 0f),
			new Vector3(0.5f, 1f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(0f, 1f, 0.5f),
			new Vector3(0.5f, 1f, 0.5f),
			new Vector3(1f, 0f, 1f),
			new Vector3(0.5f, 0f, 1f),
			new Vector3(0f, 0f, 1f),
			new Vector3(0f, 0f, 0.5f),
			new Vector3(0f, 0f, 0f),
			new Vector3(0.5f, 0f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(1f, 0f, 0.5f),
			new Vector3(0.5f, 0f, 0.5f),
			new Vector3(1f, 1f, 0f),
			new Vector3(1f, 1f, 0.5f),
			new Vector3(1f, 1f, 1f),
			new Vector3(1f, 0.5f, 1f),
			new Vector3(1f, 0f, 1f),
			new Vector3(1f, 0f, 0.5f),
			new Vector3(1f, 0f, 0f),
			new Vector3(1f, 0.5f, 0f),
			new Vector3(1f, 0.5f, 0.5f),
			new Vector3(0f, 1f, 1f),
			new Vector3(0f, 1f, 0.5f),
			new Vector3(0f, 1f, 0f),
			new Vector3(0f, 0.5f, 0f),
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 0f, 0.5f),
			new Vector3(0f, 0f, 1f),
			new Vector3(0f, 0.5f, 1f),
			new Vector3(0f, 0.5f, 0.5f),
			new Vector3(0f, 1f, 0f),
			new Vector3(0.5f, 1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(1f, 0.5f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(0.5f, 0f, 0f),
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 0.5f, 0f),
			new Vector3(0.5f, 0.5f, 0f),
			new Vector3(1f, 1f, 1f),
			new Vector3(0.5f, 1f, 1f),
			new Vector3(0f, 1f, 1f),
			new Vector3(0f, 0.5f, 1f),
			new Vector3(0f, 0f, 1f),
			new Vector3(0.5f, 0f, 1f),
			new Vector3(1f, 0f, 1f),
			new Vector3(1f, 0.5f, 1f),
			new Vector3(0.5f, 0.5f, 1f)
		};

		public static int EstimateBytes()
		{
			return 280;
		}

		public override string ToString()
		{
			return string.Concat(pos, " (", facePos, "/", welded ? "we" : "bbq", ")");
		}

		public void Log()
		{
			VoxelandFace[] array = adjFaces;
			foreach (VoxelandFace voxelandFace in array)
			{
				if (voxelandFace != null)
				{
					Debug.Log(string.Concat(voxelandFace));
				}
			}
		}

		public VoxelandVert Reset()
		{
			facePos = byte.MaxValue;
			welded = false;
			pos = Vector3.zero;
			relaxed = Vector3.zero;
			normal = Vector3.zero;
			processed = false;
			Array.Clear(neigs, 0, neigs.Length);
			neigCount = 0;
			Array.Clear(adjFaces, 0, adjFaces.Length);
			return this;
		}

		public VoxelandVert()
		{
			adjFaces = new VoxelandFace[7];
			neigs = new VoxelandVert[7];
			blendWeights = null;
		}

		public void AddNeig(VoxelandVert vert)
		{
			switch (neigCount)
			{
			case 0:
				neigs[0] = vert;
				neigCount = 1;
				return;
			case 1:
				if (neigs[0] != vert)
				{
					neigs[1] = vert;
					neigCount = 2;
				}
				return;
			}
			for (int i = 0; i < neigCount; i++)
			{
				if (neigs[i] == vert)
				{
					return;
				}
			}
			if (neigCount < neigs.Length)
			{
				neigs[neigCount] = vert;
				neigCount++;
			}
		}

		public void AddFace(VoxelandFace face)
		{
			int num = 0;
			for (num = 0; num < 7 && (adjFaces[num] == null || adjFaces[num] != face); num++)
			{
				if (adjFaces[num] == null)
				{
					adjFaces[num] = face;
					break;
				}
			}
		}

		public void Replace(VoxelandVert v2)
		{
			for (int i = 0; i < 7; i++)
			{
				if (v2.adjFaces[i] == null)
				{
					continue;
				}
				for (int j = 0; j < 8; j++)
				{
					if (v2.adjFaces[i].verts[j] == v2)
					{
						v2.adjFaces[i].verts[j] = this;
					}
				}
			}
			for (int k = 0; k < 7; k++)
			{
				if (v2.adjFaces[k] != null)
				{
					AddFace(v2.adjFaces[k]);
				}
			}
		}

		public Vector3 GetRelax()
		{
			Vector3 vector = new Vector3(0f, 0f, 0f);
			int num = 0;
			for (int i = 0; i < neigs.Length && neigs[i] != null; i++)
			{
				vector += neigs[i].pos - pos;
				num++;
			}
			return vector / num;
		}

		public void CacheBlendWeights(List<TypeUse> usedTypes)
		{
			int num = 0;
			int num2 = 0;
			for (num = 0; num < adjFaces.Length && adjFaces[num] != null; num++)
			{
				num2++;
			}
			int count = usedTypes.Count;
			if (blendWeights == null || blendWeights.Length < count)
			{
				blendWeights = new float[count];
			}
			for (int i = 0; i < count; i++)
			{
				int num3 = 0;
				for (num = 0; num < num2; num++)
				{
					if (adjFaces[num].type == usedTypes[i].num)
					{
						num3++;
					}
				}
				blendWeights[i] = (float)num3 * 1f / (float)num2;
			}
		}

		public float GetCachedBlendWeight(int usedId)
		{
			if (blendWeights == null || usedId >= blendWeights.Length)
			{
				return 0f;
			}
			return blendWeights[usedId];
		}

		public void CacheTangent(bool skipHiRes)
		{
			VoxelandFace voxelandFace = adjFaces[0];
			VoxelandVert[] verts = voxelandFace.verts;
			Vector3 rhs = (skipHiRes ? ((verts[0].pos + verts[6].pos) * 0.5f - (verts[2].pos + verts[4].pos) * 0.5f).normalized : (voxelandFace.verts[3].pos - voxelandFace.verts[7].pos).normalized);
			Vector3 rhs2 = Vector3.Cross(normal, rhs);
			rhs2 = Vector3.Cross(normal, rhs2);
			tangent = new Vector4(rhs2.x, rhs2.y, rhs2.z, -1f);
		}

		public bool ComputeIsOnChunkBorder()
		{
			for (int i = 0; i < 7 && adjFaces[i] != null; i++)
			{
				if (!adjFaces[i].block.visible)
				{
					return true;
				}
			}
			return false;
		}

		public bool ComputeIsVisible()
		{
			for (int i = 0; i < 7 && adjFaces[i] != null; i++)
			{
				if (adjFaces[i].block.visible)
				{
					return true;
				}
			}
			return false;
		}

		public Vector3 GetSmoothedNormal()
		{
			Vector3 vector = new Vector3(0f, 0f, 0f);
			int num = 0;
			for (int i = 0; i < neigs.Length && neigs[i] != null; i++)
			{
				vector += neigs[i].normal;
				num++;
			}
			return vector / num;
		}

		public void CacheAmbient()
		{
		}

		public float CacheGloss(VoxelandBlockType[] types)
		{
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < 7 && adjFaces[i] != null; i++)
			{
				int num3 = Mathf.Min(adjFaces[i].type, types.Length - 1);
				VoxelandBlockType voxelandBlockType = types[num3];
				if (voxelandBlockType == null)
				{
					Debug.LogWarningFormat("CacheGloss trying to access null type at index {0} - falling back on index 0", num3);
					voxelandBlockType = GetFallbackBlockType(types);
				}
				float cachedGloss = voxelandBlockType.cachedGloss;
				num += cachedGloss;
				num2++;
			}
			if (num2 == 0)
			{
				Debug.LogError("Vertex had no faces!");
			}
			gloss = num / (float)num2;
			return gloss;
		}

		public int CountAdjFaces()
		{
			for (int i = 0; i < adjFaces.Length; i++)
			{
				if (adjFaces[i] == null)
				{
					return i;
				}
			}
			return 7;
		}
	}

	public enum Dir
	{
		Up = 0,
		Down = 1,
		Right = 2,
		Left = 3,
		Out = 4,
		In = 5
	}

	[Serializable]
	public class VoxelandFace
	{
		public VoxelandVert[] verts;

		public byte type;

		public byte dir;

		public VoxelandBlock block;

		public Vector3 surfaceIntx;

		public static int[] dirToPosX = new int[6] { 0, 0, 1, -1, 0, 0 };

		public static int[] dirToPosY = new int[6] { 1, -1, 0, 0, 0, 0 };

		public static int[] dirToPosZ = new int[6] { 0, 0, 0, 0, -1, 1 };

		public static int[] opposite = new int[6] { 1, 0, 3, 2, 5, 4 };

		public static int[] prewPoint = new int[8] { 7, 0, 1, 2, 3, 4, 5, 6 };

		public static int[] nextPoint = new int[8] { 1, 2, 3, 4, 5, 6, 7, 0 };

		private static string Dir2String(byte dir)
		{
			switch (dir)
			{
			case 0:
				return "Up";
			case 1:
				return "Down";
			case 2:
				return "Right";
			case 3:
				return "Left";
			case 4:
				return "Out";
			case 5:
				return "In";
			default:
				return "None";
			}
		}

		public static int EstimateBytes()
		{
			return 140;
		}

		public VoxelandFace Reset()
		{
			Array.Clear(verts, 0, verts.Length);
			type = byte.MaxValue;
			dir = byte.MaxValue;
			block = null;
			return this;
		}

		public VoxelandFace()
		{
			verts = new VoxelandVert[9];
		}

		public void LinkVerts(bool skipHiRes)
		{
			if (skipHiRes)
			{
				verts[0].AddNeig(verts[2]);
				verts[0].AddNeig(verts[6]);
				verts[2].AddNeig(verts[0]);
				verts[2].AddNeig(verts[4]);
				verts[4].AddNeig(verts[2]);
				verts[4].AddNeig(verts[6]);
				verts[6].AddNeig(verts[0]);
				verts[6].AddNeig(verts[4]);
				return;
			}
			verts[0].AddNeig(verts[7]);
			verts[0].AddNeig(verts[1]);
			verts[1].AddNeig(verts[0]);
			verts[1].AddNeig(verts[2]);
			verts[1].AddNeig(verts[8]);
			verts[2].AddNeig(verts[1]);
			verts[2].AddNeig(verts[3]);
			verts[3].AddNeig(verts[4]);
			verts[3].AddNeig(verts[2]);
			verts[3].AddNeig(verts[8]);
			verts[4].AddNeig(verts[3]);
			verts[4].AddNeig(verts[5]);
			verts[5].AddNeig(verts[6]);
			verts[5].AddNeig(verts[4]);
			verts[5].AddNeig(verts[8]);
			verts[6].AddNeig(verts[5]);
			verts[6].AddNeig(verts[7]);
			verts[7].AddNeig(verts[6]);
			verts[7].AddNeig(verts[0]);
			verts[7].AddNeig(verts[8]);
			verts[8].AddNeig(verts[1]);
			verts[8].AddNeig(verts[3]);
			verts[8].AddNeig(verts[5]);
			verts[8].AddNeig(verts[7]);
		}

		public override string ToString()
		{
			return block.x + "," + block.y + "," + block.z + ":" + Dir2String(dir);
		}

		public void WeldVerts(VoxelandFace f2, int p1, int p2, VoxelandChunkWorkspace ws)
		{
			if (f2 == this)
			{
				Debug.Log("save face given to weldverts");
			}
			if (verts[p1] == null || verts[p1] != f2.verts[p2])
			{
				if (verts[p1] == null && f2.verts[p2] == null)
				{
					verts[p1] = (f2.verts[p2] = ws.NewVert());
					verts[p1].welded = true;
				}
				else if (f2.verts[p2] == null && verts[p1] != null)
				{
					f2.verts[p2] = verts[p1];
				}
				else if (verts[p1] == null && f2.verts[p2] != null)
				{
					verts[p1] = f2.verts[p2];
				}
				verts[p1].AddFace(this);
				verts[p1].AddFace(f2);
				if (verts[p1] != null && f2.verts[p2] != null && verts[p1] != f2.verts[p2])
				{
					verts[p1].Replace(f2.verts[p2]);
				}
			}
		}

		public void WeldEdges(VoxelandBlock block2, int dir2, int pA1, int pA2, int pA3, int pB1, int pB2, int pB3, VoxelandChunkWorkspace ws, bool skipHiRes)
		{
			if (verts[pA2] == null && block2 != null && block2.faces[dir2] != null && block2.faces[dir2].verts[pB2] == null)
			{
				VoxelandFace f = block2.faces[dir2];
				WeldVerts(f, pA1, pB1, ws);
				if (!skipHiRes)
				{
					WeldVerts(f, pA2, pB2, ws);
				}
				WeldVerts(f, pA3, pB3, ws);
			}
		}
	}

	[Serializable]
	public class VoxelandBlock
	{
		public int x;

		public int y;

		public int z;

		public VoxelandFace[] faces;

		public bool visible;

		public static int EstimateBytes()
		{
			return 124;
		}

		public VoxelandBlock()
		{
			faces = new VoxelandFace[6];
		}

		public VoxelandBlock Reset(int nx, int ny, int nz)
		{
			x = nx;
			y = ny;
			z = nz;
			Array.Clear(faces, 0, faces.Length);
			visible = false;
			return this;
		}

		public Int3 ToInt3()
		{
			return new Int3(x, y, z);
		}
	}

	public class GrassPos
	{
		public VoxelandFace face;

		public int quadNum;

		public bool skipHiRes;

		public Quaternion quat;

		public Vector3 csOrigin;

		public float scale;

		public Vector3 faceNormal;

		public Vector3 GetMadeUpFaceVert(int vid)
		{
			VoxelandVert[] verts = face.verts;
			switch (vid)
			{
			case 0:
			case 2:
			case 4:
			case 6:
				return verts[vid].pos;
			case 1:
				return (verts[0].pos + verts[2].pos) * 0.5f;
			case 3:
				return (verts[2].pos + verts[4].pos) * 0.5f;
			case 5:
				return (verts[4].pos + verts[6].pos) * 0.5f;
			case 7:
				return (verts[6].pos + verts[0].pos) * 0.5f;
			case 8:
				return (verts[0].pos + verts[2].pos + verts[4].pos + verts[6].pos) * 0.25f;
			default:
				return Vector3.zero;
			}
		}

		public Vector3 GetVert(int i)
		{
			VoxelandVert[] verts = face.verts;
			if (skipHiRes)
			{
				switch (i)
				{
				case 0:
					return GetMadeUpFaceVert(FaceQuadsVert0[quadNum]);
				case 1:
					return GetMadeUpFaceVert(FaceQuadsVert1[quadNum]);
				case 2:
					return GetMadeUpFaceVert(FaceQuadsVert2[quadNum]);
				default:
					return GetMadeUpFaceVert(FaceQuadsVert3[quadNum]);
				}
			}
			switch (i)
			{
			case 0:
				return verts[FaceQuadsVert0[quadNum]].pos;
			case 1:
				return verts[FaceQuadsVert1[quadNum]].pos;
			case 2:
				return verts[FaceQuadsVert2[quadNum]].pos;
			default:
				return verts[FaceQuadsVert3[quadNum]].pos;
			}
		}

		public void ComputeTransform(ref Unity.Mathematics.Random rng, VoxelandTypeBase settings)
		{
			Vector3 vert = GetVert(0);
			Vector3 vert2 = GetVert(1);
			Vector3 vert3 = GetVert(2);
			Vector3 vert4 = GetVert(3);
			Vector3 vector = (vert + vert2 + vert3 + vert4) / 4f;
			quat = Quaternion.identity;
			if (skipHiRes)
			{
				faceNormal = (face.verts[0].normal + face.verts[2].normal + face.verts[4].normal + face.verts[6].normal).normalized;
			}
			else
			{
				faceNormal = face.verts[8].normal;
			}
			quat.SetFromToRotation(Vector3.up, faceNormal);
			if (settings.grassRandomSpin)
			{
				float angle = Mathf.Lerp(0f, 360f, (float)rng.NextDouble());
				quat *= Quaternion.AngleAxis(angle, Vector3.up);
			}
			if (settings.grassZUp)
			{
				quat *= Quaternion.AngleAxis(-90f, Vector3.right);
			}
			Vector3 normalized = (vert2 - vert).normalized;
			Vector3 normalized2 = (vert3 - vert).normalized;
			float num = Mathf.Min(0.5f, settings.grassJitter);
			Vector3 vector2 = normalized * num * ((float)rng.NextDouble() - 0.5f) + normalized2 * num * ((float)rng.NextDouble() - 0.5f);
			scale = Mathf.Lerp(settings.grassMinScale, settings.grassMaxScale, rng.NextFloat());
			csOrigin = vector + vector2;
		}
	}

	public interface GrassCB
	{
		void Begin(string label);

		void End();
	}

	public const int MeshOverlap = 2;

	public const int DataOverlap = 3;

	public static readonly Vector3 OverlapOffset = new Vector3(2f, 2f, 2f);

	public static readonly Vector3 half3 = new Vector3(0.5f, 0.5f, 0.5f);

	public static readonly ArrayPool<Vector2> vector2Pool = new ArrayPool<Vector2>(8, 1024, 4);

	public static readonly ArrayPool<Vector3> vector3Pool = new ArrayPool<Vector3>(12, 1024, 4);

	public static readonly ArrayPool<Vector4> vector4Pool = new ArrayPool<Vector4>(16, 1024, 4);

	public static readonly ArrayPool<Color32> colorPool = new ArrayPool<Color32>(4, 1024, 4);

	public static readonly ArrayPool<int> intPool = new ArrayPool<int>(4, 4096, 4);

	public Voxeland land;

	public int cx;

	public int cy;

	public int cz;

	public int offsetX;

	public int offsetY;

	public int offsetZ;

	public int meshRes = -1;

	public int downsamples;

	public float surfaceDensityValue;

	public bool generateCollider = true;

	public bool skipHiRes;

	public bool disableGrass;

	public bool debugThoroughTopologyChecks;

	private const float fadeInDuration = 1f;

	private bool fadingIn;

	private float fadeAmount;

	public readonly List<MeshRenderer> hiRenders = new List<MeshRenderer>();

	public readonly List<MeshFilter> hiFilters = new List<MeshFilter>();

	public MeshFilter opaqueFilter;

	public readonly List<MeshFilter> loFilters = new List<MeshFilter>();

	public readonly List<TerrainChunkPiece> chunkPieces = new List<TerrainChunkPiece>();

	[NonSerialized]
	public MeshCollider collision;

	public readonly List<TypeUse> usedTypes = new List<TypeUse>();

	[NonSerialized]
	private List<VoxelandCoords> faceMap;

	public readonly List<MeshRenderer> grassRenders = new List<MeshRenderer>();

	public readonly List<MeshFilter> grassFilters = new List<MeshFilter>();

	private List<Renderer> attachedRenderers = new List<Renderer>();

	[NonSerialized]
	public VoxelandChunkWorkspace ws;

	public static readonly bool[] FaceVertHiResOnly = new bool[9] { false, true, false, true, false, true, false, true, true };

	public static int MaxVisibleFaces = 0;

	public static int[] AONborDU = new int[17]
	{
		-1, 0, 1, -1, 1, -1, 0, 1, -1, 0,
		1, -1, 0, 1, -1, 0, 1
	};

	public static int[] AONborDV = new int[17]
	{
		-1, -1, -1, 0, 0, 1, 1, 1, -1, -1,
		-1, 0, 0, 0, 1, 1, 1
	};

	public static int[] AONborDN = new int[17]
	{
		1, 1, 1, 1, 1, 1, 1, 1, 2, 2,
		2, 2, 2, 2, 2, 2, 2
	};

	public static int[] DirNX = new int[6] { 0, 0, 1, -1, 0, 0 };

	public static int[] DirNY = new int[6] { 1, -1, 0, 0, 0, 0 };

	public static int[] DirNZ = new int[6] { 0, 0, 0, 0, -1, 1 };

	public static int[] DirUX = new int[6] { 1, -1, 0, 0, 1, -1 };

	public static int[] DirUY = new int[6];

	public static int[] DirUZ = new int[6] { 0, 0, 1, 1, 0, 0 };

	public static int[] DirVX = new int[6];

	public static int[] DirVY = new int[6] { 0, 0, 1, 1, 1, 1 };

	public static int[] DirVZ = new int[6] { 1, -1, 0, 0, 0, 0 };

	public static int[] FaceQuadsVert0 = new int[4] { 0, 1, 7, 8 };

	public static int[] FaceQuadsVert1 = new int[4] { 1, 2, 8, 3 };

	public static int[] FaceQuadsVert2 = new int[4] { 8, 3, 5, 4 };

	public static int[] FaceQuadsVert3 = new int[4] { 7, 8, 6, 5 };

	public int managedUpdateIndex { get; set; }

	List<TypeUse> IVoxelandChunkInfo.usedTypes => usedTypes;

	IVoxeland IVoxelandChunkInfo.land => land;

	VoxelandChunkWorkspace IVoxelandChunk.ws => ws;

	int IVoxelandChunk.downsamples => downsamples;

	int IVoxelandChunk.offsetX => offsetX;

	int IVoxelandChunk.offsetY => offsetY;

	int IVoxelandChunk.offsetZ => offsetZ;

	int IVoxelandChunk.meshRes => meshRes;

	bool IVoxelandChunk.skipHiRes => skipHiRes;

	float IVoxelandChunk.surfaceDensityValue => surfaceDensityValue;

	bool IVoxelandChunk.disableGrass => disableGrass;

	bool IVoxelandChunk.debugThoroughTopologyChecks => debugThoroughTopologyChecks;

	List<MeshFilter> IVoxelandChunk2.hiFilters => hiFilters;

	List<MeshRenderer> IVoxelandChunk2.hiRenders => hiRenders;

	List<MeshFilter> IVoxelandChunk2.grassFilters => grassFilters;

	List<MeshRenderer> IVoxelandChunk2.grassRenders => grassRenders;

	MeshCollider IVoxelandChunk2.collision
	{
		get
		{
			return collision;
		}
		set
		{
			collision = value;
		}
	}

	List<TerrainChunkPiece> IVoxelandChunk2.chunkPieces => chunkPieces;

	public static bool Approx(float a, float b)
	{
		return Mathf.Approximately(a, b);
	}

	public string GetProfileTag()
	{
		return "VoxelandChunk";
	}

	public static VoxelandBlockType GetFallbackBlockType(VoxelandBlockType[] types)
	{
		for (int i = 0; i < types.Length; i++)
		{
			if (types[i] != null)
			{
				return types[i];
			}
		}
		return null;
	}

	public GameObject CreateDebugObj(string name)
	{
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		obj.name = name;
		obj.transform.parent = land.transform;
		return obj;
	}

	public static void SetDensityBasedPositions(VoxelandChunkWorkspace ws)
	{
		List<VoxelandFace> faces = ws.faces;
		for (int i = 0; i < faces.Count; i++)
		{
			VoxelandFace voxelandFace = faces[i];
			if (!voxelandFace.verts[0].processed)
			{
				voxelandFace.verts[0].pos = ComputeDensityBasedPosition(voxelandFace.verts[0]);
				voxelandFace.verts[0].processed = true;
			}
			if (!voxelandFace.verts[2].processed)
			{
				voxelandFace.verts[2].pos = ComputeDensityBasedPosition(voxelandFace.verts[2]);
				voxelandFace.verts[2].processed = true;
			}
			if (!voxelandFace.verts[4].processed)
			{
				voxelandFace.verts[4].pos = ComputeDensityBasedPosition(voxelandFace.verts[4]);
				voxelandFace.verts[4].processed = true;
			}
			if (!voxelandFace.verts[6].processed)
			{
				voxelandFace.verts[6].pos = ComputeDensityBasedPosition(voxelandFace.verts[6]);
				voxelandFace.verts[6].processed = true;
			}
		}
		List<VoxelandVert> verts = ws.verts;
		for (int j = 0; j < verts.Count; j++)
		{
			verts[j].processed = false;
		}
	}

	public void BuildMesh()
	{
		BuildMesh(skipRelax: false, int.MaxValue);
	}

	public void BuildMesh(bool skipRelax)
	{
		BuildMesh(skipRelax, int.MaxValue);
	}

	public void BuildMesh(bool skipRelax, int maxTypes)
	{
		BuildMeshImpl(skipRelax, maxTypes);
	}

	private void BuildMeshImpl(bool skipRelax, int maxTypes)
	{
		BuildMesh(this, skipRelax, maxTypes);
	}

	public static void BuildMesh(IVoxelandChunk chunk, bool skipRelax, int maxTypes)
	{
		VoxelandChunkWorkspace voxelandChunkWorkspace = chunk.ws;
		IVoxeland voxeland = chunk.land;
		bool flag = chunk.skipHiRes;
		bool flag2 = chunk.disableGrass;
		if (voxelandChunkWorkspace == null)
		{
			Debug.LogError("BuildMesh called with no workspace set!");
			return;
		}
		voxelandChunkWorkspace.faces.Clear();
		CalculateMesh(chunk, maxTypes);
		GetVertsList(voxelandChunkWorkspace, flag);
		if (!voxeland.debugBlocky)
		{
			SetDensityBasedPositions(voxelandChunkWorkspace);
		}
		if (voxeland.normalsSmooth == VoxelandNormalsSmooth.none)
		{
			Debug.LogError("Unsupported vertex normal mode");
		}
		if (!flag)
		{
			PositionSecondaryVerts(voxelandChunkWorkspace);
		}
		if (voxeland.normalsSmooth == VoxelandNormalsSmooth.crisp)
		{
			Debug.LogError("Unsupported vertex normal mode");
		}
		if (!voxeland.debugBlocky && !skipRelax)
		{
			Relax(voxelandChunkWorkspace, flag ? 0.5f : 1f);
		}
		switch (voxeland.normalsSmooth)
		{
		case VoxelandNormalsSmooth.mesh:
			GetNormals(voxelandChunkWorkspace);
			break;
		case VoxelandNormalsSmooth.smooth:
			GetNormals(voxelandChunkWorkspace);
			SmoothNormals();
			break;
		}
		if (voxeland.debugLogMeshing)
		{
			ExpensiveWorkspaceDump(voxelandChunkWorkspace, voxeland);
		}
		ComputeVisibleFacesInfo(chunk);
		if (!flag2)
		{
			GrassPhase1(chunk);
		}
	}

	public void BuildLayerObjects()
	{
		if (ws.visibleFaces.Count == 0)
		{
			loFilters.Clear();
			hiFilters.Clear();
			hiRenders.Clear();
			base.gameObject.name += " no visible";
			return;
		}
		GameObject obj = base.gameObject;
		obj.name = obj.name + " T" + usedTypes.Count;
		hiRenders.Clear();
		hiFilters.Clear();
		loFilters.Clear();
		for (int i = 0; i < usedTypes.Count; i++)
		{
			ShadowCastingMode shadowCastingMode = ShadowCastingMode.Off;
			if (land.castShadows && i == 0)
			{
				shadowCastingMode = ShadowCastingMode.On;
			}
			MeshRenderer meshRenderer = null;
			MeshFilter meshFilter = null;
			if (!skipHiRes)
			{
				GameObject obj2 = new GameObject("HiResChunk" + i);
				obj2.transform.parent = base.transform;
				obj2.transform.localPosition = new Vector3(0f, 0f, 0f);
				obj2.transform.localScale = new Vector3(1f, 1f, 1f);
				obj2.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
				meshFilter = obj2.AddComponent<MeshFilter>();
				meshRenderer = obj2.AddComponent<MeshRenderer>();
				meshRenderer.shadowCastingMode = shadowCastingMode;
				hiFilters.Add(meshFilter);
				hiRenders.Add(meshRenderer);
			}
			GameObject obj3 = new GameObject("LoResChunk" + i);
			obj3.transform.parent = base.transform;
			obj3.transform.localPosition = new Vector3(0f, 0f, 0f);
			obj3.transform.localScale = new Vector3(1f, 1f, 1f);
			obj3.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			MeshFilter meshFilter2 = obj3.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer2 = obj3.AddComponent<MeshRenderer>();
			obj3.GetComponent<Renderer>().shadowCastingMode = shadowCastingMode;
			loFilters.Add(meshFilter2);
			if (!skipHiRes)
			{
				meshFilter.mesh = new Mesh();
			}
			meshFilter2.mesh = new Mesh();
			PushLayer(skipHiRes ? null : meshFilter.sharedMesh, meshFilter2.sharedMesh, i);
			Material[] materials = ComputeMaterialsForLayer(i, debugAllOpaque: false, land.debugUseLQShader);
			Renderer renderer = meshRenderer;
			Renderer renderer2 = meshRenderer2;
			if (!skipHiRes)
			{
				renderer.materials = materials;
				renderer.sortingOrder = i;
			}
			renderer2.materials = materials;
			renderer2.sortingOrder = i;
			if (!skipHiRes)
			{
				renderer2.enabled = false;
			}
			if (!skipHiRes)
			{
				attachedRenderers.Add(renderer);
			}
			attachedRenderers.Add(renderer2);
		}
	}

	public Vector3 ComputeSurfaceIntersection(Vector3 p0, Vector3 p1, byte d0, byte d1)
	{
		return ComputeSurfaceIntersection(p0, p1, d0, d1, surfaceDensityValue, downsamples);
	}

	public static Vector3 ComputeSurfaceIntersection(Vector3 p0, Vector3 p1, byte d0, byte d1, float surfaceDensityValue, int downsamples)
	{
		Vector3 normalized = (p1 - p0).normalized;
		float num = 1 << downsamples;
		if (d0 == 0 && d1 == 0)
		{
			return (p0 + p1) / 2f;
		}
		if (d1 == 0)
		{
			return p0 + VoxelandData.OctNode.DecodeNearDensity(d0) * normalized / num;
		}
		if (d0 == 0)
		{
			return p1 + VoxelandData.OctNode.DecodeNearDensity(d1) * normalized / num;
		}
		float num2 = 0f;
		float num3 = VoxelandData.OctNode.DecodeNearDensity(d0);
		float num4 = VoxelandData.OctNode.DecodeNearDensity(d1);
		if (d0 != d1)
		{
			num2 = (surfaceDensityValue * 1f - num4) / (num3 - num4);
		}
		return (1f - num2) * p1 + num2 * p0;
	}

	private static Vector3 ComputeDensityBasedPosition(VoxelandVert vert)
	{
		Vector3 zero = Vector3.zero;
		int num = 0;
		for (int i = 0; i < vert.adjFaces.Length; i++)
		{
			VoxelandFace voxelandFace = vert.adjFaces[i];
			if (voxelandFace == null)
			{
				break;
			}
			zero += voxelandFace.surfaceIntx;
			num++;
		}
		return zero * 1f / num;
	}

	public bool IsBlockVisible(int x, int y, int z)
	{
		return IsBlockVisible(meshRes, x, y, z);
	}

	public static bool IsBlockVisible(int meshRes, int x, int y, int z)
	{
		if (x < 2 || x >= 2 + meshRes || z < 2 || z >= 2 + meshRes || y < 2 || y >= 2 + meshRes)
		{
			return false;
		}
		return true;
	}

	public static int CompareTypeUseByCountReverse(TypeUse b, TypeUse a)
	{
		return a.count - b.count;
	}

	public static int CompareTypeUseByLayer(IVoxeland land, TypeUse a, TypeUse b)
	{
		if (land.types[a.num] == null)
		{
			Debug.LogError("Null block type at slot # " + a.num);
			return 0;
		}
		if (land.types[b.num] == null)
		{
			Debug.LogError("Null block type at slot # " + b.num);
			return 0;
		}
		int layer = land.types[a.num].layer;
		int layer2 = land.types[b.num].layer;
		if (layer == layer2)
		{
			return a.num - b.num;
		}
		return layer - layer2;
	}

	public void OnTypeUsed(byte typeNum)
	{
		OnTypeUsed(usedTypes, typeNum);
	}

	public static void OnTypeUsed(List<TypeUse> usedTypes, byte typeNum)
	{
		bool flag = false;
		for (int i = 0; i < usedTypes.Count; i++)
		{
			if (usedTypes[i].num == typeNum)
			{
				usedTypes[i].count++;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			TypeUse typeUse = new TypeUse();
			typeUse.num = typeNum;
			typeUse.count = 1;
			usedTypes.Add(typeUse);
		}
	}

	public static void CalculateMesh(IVoxelandChunk chunk, int maxTypes)
	{
		VoxelandChunkWorkspace voxelandChunkWorkspace = chunk.ws;
		int num = chunk.offsetX;
		int num2 = chunk.offsetY;
		int num3 = chunk.offsetZ;
		IVoxeland voxeland = chunk.land;
		List<TypeUse> list = chunk.usedTypes;
		int num4 = chunk.downsamples;
		bool flag = chunk.skipHiRes;
		int num5 = chunk.meshRes;
		bool flag2 = chunk.debugThoroughTopologyChecks;
		int num6 = 0;
		List<VoxelandFace> faces = voxelandChunkWorkspace.faces;
		Array3<VoxelandBlock> blocks = voxelandChunkWorkspace.blocks;
		list.Clear();
		if (voxeland.faceCreator != null)
		{
			voxeland.faceCreator.CreateFaces(chunk);
		}
		else
		{
			voxeland.RasterizeVoxels(voxelandChunkWorkspace.rws, num - (3 << num4), num2 - (3 << num4), num3 - (3 << num4), num4);
			int num7 = voxeland.types.Length - 1;
			int num8 = 0;
			int num9 = 0;
			int num10 = 0;
			int num11 = voxelandChunkWorkspace.blocksLen.x;
			int num12 = voxelandChunkWorkspace.blocksLen.y;
			int num13 = voxelandChunkWorkspace.blocksLen.z;
			if (voxeland.IsLimitedMeshing())
			{
				Int3 @int = new Int3(chunk.offsetX, chunk.offsetY, chunk.offsetZ);
				Int3 int2 = new Int3(2 << num4);
				Int3 int3 = voxeland.meshMins - @int + int2 >> num4;
				Int3 int4 = voxeland.meshMaxs - @int + int2 >> num4;
				num8 = Mathf.Max(num8, int3.x + 1);
				num9 = Mathf.Max(num9, int3.y + 1);
				num10 = Mathf.Max(num10, int3.z + 1);
				num11 = Mathf.Min(num11, int4.x);
				num12 = Mathf.Min(num12, int4.y);
				num13 = Mathf.Min(num13, int4.z);
			}
			for (int i = num8; i < num11; i++)
			{
				for (int j = num9; j < num12; j++)
				{
					for (int k = num10; k < num13; k++)
					{
						byte b = QueryTypeGrid(voxelandChunkWorkspace, i, j, k);
						byte b2 = QueryDensityGrid(voxelandChunkWorkspace, i, j, k);
						if (!VoxelandData.OctNode.IsBelowSurface(b, b2))
						{
							continue;
						}
						if (b == 0)
						{
							Debug.LogError("Got an under-surface voxel with 0 block type! Probably a density-write error.");
						}
						for (int l = 0; l < 6; l++)
						{
							int num14 = i + VoxelandFace.dirToPosX[l];
							int num15 = j + VoxelandFace.dirToPosY[l];
							int num16 = k + VoxelandFace.dirToPosZ[l];
							byte type = QueryTypeGrid(voxelandChunkWorkspace, num14, num15, num16);
							byte b3 = QueryDensityGrid(voxelandChunkWorkspace, num14, num15, num16);
							if (!VoxelandData.OctNode.IsBelowSurface(type, b3))
							{
								VoxelandBlock voxelandBlock = blocks[i, j, k];
								if (voxelandBlock == null)
								{
									voxelandBlock = (blocks[i, j, k] = voxelandChunkWorkspace.NewBlock(i, j, k));
									voxelandBlock.visible = IsBlockVisible(num5, i, j, k);
								}
								VoxelandFace voxelandFace = voxelandChunkWorkspace.NewFace();
								faces.Add(voxelandFace);
								voxelandBlock.faces[l] = voxelandFace;
								if (voxeland.debugOneType)
								{
									voxelandFace.type = 1;
								}
								else
								{
									voxelandFace.type = (byte)((b > num7) ? 1 : b);
								}
								chunk.OnTypeUsed(voxelandFace.type);
								voxelandFace.block = voxelandBlock;
								voxelandFace.dir = (byte)l;
								Vector3 p = new Vector3(i, j, k) + half3 - OverlapOffset;
								Vector3 p2 = new Vector3(num14, num15, num16) + half3 - OverlapOffset;
								voxelandFace.surfaceIntx = chunk.ComputeSurfaceIntersection(p, p2, b2, b3);
							}
						}
					}
				}
			}
		}
		if (list.Count > maxTypes)
		{
			list.Sort(CompareTypeUseByCountReverse);
			list.RemoveRange(maxTypes, list.Count - maxTypes);
		}
		TypeUseByLayerComparer comparer = new TypeUseByLayerComparer(voxeland);
		list.Sort(comparer);
		for (num6 = 0; num6 < faces.Count; num6++)
		{
			VoxelandFace voxelandFace = faces[num6];
			VoxelandBlock voxelandBlock = voxelandFace.block;
			int i = voxelandBlock.x;
			int j = voxelandBlock.y;
			int k = voxelandBlock.z;
			switch ((Dir)voxelandFace.dir)
			{
			case Dir.Up:
				voxelandFace.WeldEdges(voxelandBlock, 5, 0, 1, 2, 2, 1, 0, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 2, 2, 3, 4, 2, 1, 0, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 4, 4, 5, 6, 2, 1, 0, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 3, 6, 7, 0, 2, 1, 0, voxelandChunkWorkspace, flag);
				if (i > 0)
				{
					voxelandFace.WeldEdges(blocks[i - 1, j, k], 0, 6, 7, 0, 4, 3, 2, voxelandChunkWorkspace, flag);
				}
				if (k > 0)
				{
					voxelandFace.WeldEdges(blocks[i, j, k - 1], 0, 4, 5, 6, 2, 1, 0, voxelandChunkWorkspace, flag);
				}
				break;
			case Dir.Down:
				voxelandFace.WeldEdges(voxelandBlock, 5, 0, 1, 2, 6, 5, 4, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 3, 2, 3, 4, 6, 5, 4, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 4, 4, 5, 6, 6, 5, 4, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 2, 6, 7, 0, 6, 5, 4, voxelandChunkWorkspace, flag);
				if (i > 0)
				{
					voxelandFace.WeldEdges(blocks[i - 1, j, k], 1, 2, 3, 4, 0, 7, 6, voxelandChunkWorkspace, flag);
				}
				if (k > 0)
				{
					voxelandFace.WeldEdges(blocks[i, j, k - 1], 1, 4, 5, 6, 2, 1, 0, voxelandChunkWorkspace, flag);
				}
				break;
			case Dir.Right:
				voxelandFace.WeldEdges(voxelandBlock, 0, 0, 1, 2, 4, 3, 2, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 5, 2, 3, 4, 0, 7, 6, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 1, 4, 5, 6, 0, 7, 6, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 4, 6, 7, 0, 4, 3, 2, voxelandChunkWorkspace, flag);
				if (j > 0)
				{
					voxelandFace.WeldEdges(blocks[i, j - 1, k], 2, 4, 5, 6, 2, 1, 0, voxelandChunkWorkspace, flag);
				}
				if (k > 0)
				{
					voxelandFace.WeldEdges(blocks[i, j, k - 1], 2, 6, 7, 0, 4, 3, 2, voxelandChunkWorkspace, flag);
				}
				break;
			case Dir.Left:
				voxelandFace.WeldEdges(voxelandBlock, 0, 0, 1, 2, 0, 7, 6, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 4, 2, 3, 4, 0, 7, 6, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 1, 4, 5, 6, 4, 3, 2, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 5, 6, 7, 0, 4, 3, 2, voxelandChunkWorkspace, flag);
				if (j > 0)
				{
					voxelandFace.WeldEdges(blocks[i, j - 1, k], 3, 4, 5, 6, 2, 1, 0, voxelandChunkWorkspace, flag);
				}
				if (k > 0)
				{
					voxelandFace.WeldEdges(blocks[i, j, k - 1], 3, 2, 3, 4, 0, 7, 6, voxelandChunkWorkspace, flag);
				}
				break;
			case Dir.Out:
				voxelandFace.WeldEdges(voxelandBlock, 0, 0, 1, 2, 6, 5, 4, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 2, 2, 3, 4, 0, 7, 6, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 1, 4, 5, 6, 6, 5, 4, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 3, 6, 7, 0, 4, 3, 2, voxelandChunkWorkspace, flag);
				if (j > 0)
				{
					voxelandFace.WeldEdges(blocks[i, j - 1, k], 4, 4, 5, 6, 2, 1, 0, voxelandChunkWorkspace, flag);
				}
				if (i > 0)
				{
					voxelandFace.WeldEdges(blocks[i - 1, j, k], 4, 6, 7, 0, 4, 3, 2, voxelandChunkWorkspace, flag);
				}
				break;
			case Dir.In:
				voxelandFace.WeldEdges(voxelandBlock, 0, 0, 1, 2, 2, 1, 0, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 3, 2, 3, 4, 0, 7, 6, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 1, 4, 5, 6, 2, 1, 0, voxelandChunkWorkspace, flag);
				voxelandFace.WeldEdges(voxelandBlock, 2, 6, 7, 0, 4, 3, 2, voxelandChunkWorkspace, flag);
				if (j > 0)
				{
					voxelandFace.WeldEdges(blocks[i, j - 1, k], 5, 4, 5, 6, 2, 1, 0, voxelandChunkWorkspace, flag);
				}
				if (i > 0)
				{
					voxelandFace.WeldEdges(blocks[i - 1, j, k], 5, 2, 3, 4, 0, 7, 6, voxelandChunkWorkspace, flag);
				}
				break;
			}
		}
		int num17 = num5 + 4 - 1;
		for (num6 = 0; num6 < faces.Count; num6++)
		{
			VoxelandFace voxelandFace = faces[num6];
			VoxelandBlock voxelandBlock = voxelandFace.block;
			int i = voxelandBlock.x;
			int j = voxelandBlock.y;
			int k = voxelandBlock.z;
			int l = voxelandFace.dir;
			if (l == 0 && j < num17)
			{
				if (i > 0)
				{
					voxelandFace.WeldEdges(blocks[i - 1, j + 1, k], 2, 6, 7, 0, 6, 5, 4, voxelandChunkWorkspace, flag);
				}
				if (k > 0)
				{
					voxelandFace.WeldEdges(blocks[i, j + 1, k - 1], 5, 4, 5, 6, 6, 5, 4, voxelandChunkWorkspace, flag);
				}
				if (i < num17)
				{
					voxelandFace.WeldEdges(blocks[i + 1, j + 1, k], 3, 2, 3, 4, 6, 5, 4, voxelandChunkWorkspace, flag);
				}
				if (k < num17)
				{
					voxelandFace.WeldEdges(blocks[i, j + 1, k + 1], 4, 0, 1, 2, 6, 5, 4, voxelandChunkWorkspace, flag);
				}
			}
			else if (l == 1 && j > 0)
			{
				if (i > 0)
				{
					voxelandFace.WeldEdges(blocks[i - 1, j - 1, k], 2, 2, 3, 4, 2, 1, 0, voxelandChunkWorkspace, flag);
				}
				if (k > 0)
				{
					voxelandFace.WeldEdges(blocks[i, j - 1, k - 1], 5, 4, 5, 6, 2, 1, 0, voxelandChunkWorkspace, flag);
				}
				if (i < num17)
				{
					voxelandFace.WeldEdges(blocks[i + 1, j - 1, k], 3, 6, 7, 0, 2, 1, 0, voxelandChunkWorkspace, flag);
				}
				if (k < num17)
				{
					voxelandFace.WeldEdges(blocks[i, j - 1, k + 1], 4, 0, 1, 2, 2, 1, 0, voxelandChunkWorkspace, flag);
				}
			}
			else if (l == 2 && i < num17)
			{
				if (j > 0)
				{
					voxelandFace.WeldEdges(blocks[i + 1, j - 1, k], 0, 4, 5, 6, 0, 7, 6, voxelandChunkWorkspace, flag);
				}
				if (k > 0)
				{
					voxelandFace.WeldEdges(blocks[i + 1, j, k - 1], 5, 6, 7, 0, 4, 3, 2, voxelandChunkWorkspace, flag);
				}
				if (j < num17)
				{
					voxelandFace.WeldEdges(blocks[i + 1, j + 1, k], 1, 0, 1, 2, 4, 3, 2, voxelandChunkWorkspace, flag);
				}
				if (k < num17)
				{
					voxelandFace.WeldEdges(blocks[i + 1, j, k + 1], 4, 2, 3, 4, 0, 7, 6, voxelandChunkWorkspace, flag);
				}
			}
			else if (l == 3 && i > 0)
			{
				if (j > 0)
				{
					voxelandFace.WeldEdges(blocks[i - 1, j - 1, k], 0, 4, 5, 6, 4, 3, 2, voxelandChunkWorkspace, flag);
				}
				if (k > 0)
				{
					voxelandFace.WeldEdges(blocks[i - 1, j, k - 1], 5, 2, 3, 4, 0, 7, 6, voxelandChunkWorkspace, flag);
				}
				if (j < num17)
				{
					voxelandFace.WeldEdges(blocks[i - 1, j + 1, k], 1, 0, 1, 2, 0, 7, 6, voxelandChunkWorkspace, flag);
				}
				if (k < num17)
				{
					voxelandFace.WeldEdges(blocks[i - 1, j, k + 1], 4, 6, 7, 0, 4, 3, 2, voxelandChunkWorkspace, flag);
				}
			}
			else if (l == 4 && k > 0)
			{
				if (j > 0)
				{
					voxelandFace.WeldEdges(blocks[i, j - 1, k - 1], 0, 4, 5, 6, 2, 1, 0, voxelandChunkWorkspace, flag);
				}
				if (i > 0)
				{
					voxelandFace.WeldEdges(blocks[i - 1, j, k - 1], 2, 6, 7, 0, 4, 3, 2, voxelandChunkWorkspace, flag);
				}
				if (j < num17)
				{
					voxelandFace.WeldEdges(blocks[i, j + 1, k - 1], 1, 0, 1, 2, 2, 1, 0, voxelandChunkWorkspace, flag);
				}
				if (i < num17)
				{
					voxelandFace.WeldEdges(blocks[i + 1, j, k - 1], 3, 2, 3, 4, 0, 7, 6, voxelandChunkWorkspace, flag);
				}
			}
			else if (l == 5 && k < num17)
			{
				if (j > 0)
				{
					voxelandFace.WeldEdges(blocks[i, j - 1, k + 1], 0, 4, 5, 6, 6, 5, 4, voxelandChunkWorkspace, flag);
				}
				if (i > 0)
				{
					voxelandFace.WeldEdges(blocks[i - 1, j, k + 1], 2, 2, 3, 4, 0, 7, 6, voxelandChunkWorkspace, flag);
				}
				if (j < num17)
				{
					voxelandFace.WeldEdges(blocks[i, j + 1, k + 1], 1, 0, 1, 2, 6, 5, 4, voxelandChunkWorkspace, flag);
				}
				if (i < num17)
				{
					voxelandFace.WeldEdges(blocks[i + 1, j, k + 1], 3, 6, 7, 0, 4, 3, 2, voxelandChunkWorkspace, flag);
				}
			}
		}
		for (num6 = 0; num6 < faces.Count; num6++)
		{
			VoxelandFace voxelandFace = faces[num6];
			VoxelandBlock voxelandBlock = voxelandFace.block;
			int i = voxelandBlock.x;
			int j = voxelandBlock.y;
			int k = voxelandBlock.z;
			int l = voxelandFace.dir;
			for (int m = 0; m < 9; m++)
			{
				if (!flag || !FaceVertHiResOnly[m])
				{
					VoxelandVert voxelandVert = voxelandFace.verts[m];
					if (voxelandVert == null)
					{
						voxelandVert = voxelandChunkWorkspace.NewVert();
						voxelandVert.welded = false;
						voxelandFace.verts[m] = voxelandVert;
					}
					voxelandVert.facePos = (byte)m;
					voxelandVert.pos = VoxelandVert.posTable[l * 9 + m] + new Vector3(i - 2, j - 2, k - 2);
					voxelandVert.AddFace(voxelandFace);
				}
			}
		}
		if (flag2)
		{
			PerformTopologyChecks(voxelandChunkWorkspace, flag);
		}
		for (int n = 0; n < faces.Count; n++)
		{
			faces[n].LinkVerts(flag);
		}
	}

	public static void PerformTopologyChecks(VoxelandChunkWorkspace ws, bool skipHiRes)
	{
		List<VoxelandFace> faces = ws.faces;
		Vector3 rhs = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		for (int i = 0; i < faces.Count; i++)
		{
			VoxelandFace voxelandFace = faces[i];
			for (int j = 0; j < 9; j++)
			{
				if (!skipHiRes || !FaceVertHiResOnly[j])
				{
					VoxelandVert obj = voxelandFace.verts[j];
					rhs = Vector3.Min(obj.pos, rhs);
					vector = Vector3.Max(obj.pos, rhs);
				}
			}
		}
		for (int k = 0; k < faces.Count; k++)
		{
			for (int l = 0; l < faces[k].verts.Length; l++)
			{
				VoxelandVert voxelandVert = faces[k].verts[l];
				if (voxelandVert == null)
				{
					continue;
				}
				if (voxelandVert.neigCount > 0)
				{
					Debug.LogFormat("vert had nbors already?? {0}", voxelandVert);
				}
				if (voxelandVert.welded && voxelandVert.CountAdjFaces() < 2)
				{
					Debug.LogFormat("Welded vert had less than 2 faces!!! WTF?? {0}", voxelandVert);
					voxelandVert.Log();
				}
				if (voxelandVert.facePos == 8)
				{
					if (voxelandVert.CountAdjFaces() != 1)
					{
						Debug.LogFormat("face vert has != 1 faces: {0}", voxelandVert);
					}
					continue;
				}
				Vector3 pos = voxelandVert.pos;
				if (Approx(pos.x, rhs.x) || Approx(pos.y, rhs.y) || Approx(pos.z, rhs.z) || Approx(pos.x, vector.x) || Approx(pos.y, vector.y) || Approx(pos.z, vector.z))
				{
					continue;
				}
				if (!voxelandVert.welded)
				{
					Debug.LogFormat("non-perim corner/edge vert was not welded...eh? {0}", voxelandVert);
					voxelandVert.Log();
				}
				if (!FaceVertHiResOnly[voxelandVert.facePos])
				{
					if (voxelandVert.CountAdjFaces() < 3)
					{
						Debug.LogFormat("non-perim corner vert has less than 3 faces: {0}", voxelandVert);
						voxelandVert.Log();
					}
				}
				else if (voxelandVert.CountAdjFaces() != 2)
				{
					Debug.LogFormat("non-perim edge vert has != 2 faces: {0}", voxelandVert);
					voxelandVert.Log();
				}
			}
		}
	}

	public static void ExpensiveWorkspaceDump(VoxelandChunkWorkspace ws, IVoxeland land)
	{
		StreamWriter streamWriter = FileUtils.CreateTextFile("verts-debug" + ((land.faceCreator != null) ? "-override" : "") + ".txt");
		streamWriter.WriteLine("total verts: " + ws.verts.Count);
		for (int i = 0; i < ws.verts.Count; i++)
		{
			VoxelandVert voxelandVert = ws.verts[i];
			streamWriter.WriteLine("vert pos = " + voxelandVert.pos);
			for (int j = 0; j < voxelandVert.adjFaces.Length; j++)
			{
				VoxelandFace voxelandFace = voxelandVert.adjFaces[j];
				if (voxelandFace != null)
				{
					streamWriter.WriteLine(string.Concat(voxelandVert.pos, " adj to face face x", voxelandFace.block.x, " y", voxelandFace.block.y, " z", voxelandFace.block.z, " d", voxelandFace.dir, " intx ", voxelandFace.surfaceIntx));
				}
			}
		}
		streamWriter.Close();
		streamWriter = FileUtils.CreateTextFile("faces" + ((land.faceCreator != null) ? "-override" : "") + ".txt");
		foreach (VoxelandFace face in ws.faces)
		{
			if (face != null)
			{
				streamWriter.WriteLine("face x" + face.block.x + " y" + face.block.y + " z" + face.block.z + " d" + face.dir + " intx " + face.surfaceIntx);
			}
		}
		streamWriter.Close();
		streamWriter = FileUtils.CreateTextFile("blocks" + ((land.faceCreator != null) ? "-override" : "") + ".txt");
		foreach (VoxelandBlock block in ws.blocks)
		{
			if (block == null)
			{
				continue;
			}
			streamWriter.WriteLine("block x" + block.x + " y" + block.y + " z" + block.z + " v" + block.visible.ToString());
			for (int k = 0; k < 6; k++)
			{
				VoxelandFace voxelandFace2 = block.faces[k];
				if (voxelandFace2 != null)
				{
					streamWriter.WriteLine("block x" + block.x + " y" + block.y + " z" + block.z + " v" + block.visible.ToString() + " dir " + k + " face x" + voxelandFace2.block.x + " y" + voxelandFace2.block.y + " z" + voxelandFace2.block.z + " d" + voxelandFace2.dir + " intx" + voxelandFace2.surfaceIntx);
				}
			}
		}
		streamWriter.Close();
	}

	private static void GetVertsList(VoxelandChunkWorkspace ws, bool skipHiRes)
	{
		List<VoxelandFace> faces = ws.faces;
		List<VoxelandVert> verts = ws.verts;
		verts.Clear();
		for (int i = 0; i < faces.Count; i++)
		{
			for (int j = 0; j < 9; j++)
			{
				if ((!skipHiRes || !FaceVertHiResOnly[j]) && !faces[i].verts[j].processed)
				{
					faces[i].verts[j].processed = true;
					verts.Add(faces[i].verts[j]);
				}
			}
		}
		for (int k = 0; k < verts.Count; k++)
		{
			verts[k].processed = false;
		}
	}

	private static void PositionSecondaryVerts(VoxelandChunkWorkspace ws)
	{
		List<VoxelandFace> faces = ws.faces;
		for (int i = 0; i < faces.Count; i++)
		{
			VoxelandFace voxelandFace = faces[i];
			voxelandFace.verts[1].pos = (voxelandFace.verts[0].pos + voxelandFace.verts[2].pos) * 0.5f;
			voxelandFace.verts[3].pos = (voxelandFace.verts[2].pos + voxelandFace.verts[4].pos) * 0.5f;
			voxelandFace.verts[5].pos = (voxelandFace.verts[4].pos + voxelandFace.verts[6].pos) * 0.5f;
			voxelandFace.verts[7].pos = (voxelandFace.verts[6].pos + voxelandFace.verts[0].pos) * 0.5f;
			voxelandFace.verts[8].pos = (voxelandFace.verts[0].pos + voxelandFace.verts[2].pos + voxelandFace.verts[4].pos + voxelandFace.verts[6].pos) * 0.25f;
		}
	}

	public static void Relax(VoxelandChunkWorkspace ws, float scale)
	{
		List<VoxelandVert> verts = ws.verts;
		for (int i = 0; i < verts.Count; i++)
		{
			verts[i].relaxed = verts[i].GetRelax();
		}
		for (int j = 0; j < verts.Count; j++)
		{
			verts[j].pos += scale * verts[j].relaxed;
		}
	}

	private static void GetNormals(VoxelandChunkWorkspace ws)
	{
		for (int i = 0; i < ws.verts.Count; i++)
		{
			VoxelandVert voxelandVert = ws.verts[i];
			Vector3 zero = Vector3.zero;
			for (int j = 0; j < voxelandVert.adjFaces.Length; j++)
			{
				VoxelandFace voxelandFace = voxelandVert.adjFaces[j];
				if (voxelandFace != null)
				{
					Vector3 normalized = Vector3.Cross(voxelandFace.verts[0].pos - voxelandFace.verts[4].pos, voxelandFace.verts[2].pos - voxelandFace.verts[6].pos).normalized;
					zero += normalized;
				}
			}
			voxelandVert.normal = zero.normalized;
		}
	}

	public static void SmoothNormals()
	{
		throw new Exception("This function is deprecated --steve@uwe 9/5/2014 1:56:17 PM");
	}

	public static void NormalizeNormals(VoxelandChunkWorkspace ws)
	{
		List<VoxelandVert> verts = ws.verts;
		for (int i = 0; i < verts.Count; i++)
		{
			verts[i].normal = verts[i].normal.normalized;
		}
	}

	private int GetAONborDX(int nborNum, int dir)
	{
		return AONborDN[nborNum] * DirNX[dir] + AONborDU[nborNum] * DirUX[dir] + AONborDV[nborNum] * DirVX[dir];
	}

	private int GetAONborDY(int nborNum, int dir)
	{
		return AONborDN[nborNum] * DirNY[dir] + AONborDU[nborNum] * DirUY[dir] + AONborDV[nborNum] * DirVY[dir];
	}

	private int GetAONborDZ(int nborNum, int dir)
	{
		return AONborDN[nborNum] * DirNZ[dir] + AONborDU[nborNum] * DirUZ[dir] + AONborDV[nborNum] * DirVZ[dir];
	}

	public static void ComputeVisibleFacesInfo(IVoxelandChunk chunk)
	{
		VoxelandChunkWorkspace voxelandChunkWorkspace = chunk.ws;
		IVoxeland voxeland = chunk.land;
		List<TypeUse> list = chunk.usedTypes;
		bool flag = chunk.skipHiRes;
		if (voxelandChunkWorkspace == null)
		{
			Debug.LogError("Workspace not set!");
			return;
		}
		List<VoxelandFace> faces = voxelandChunkWorkspace.faces;
		List<VoxelandFace> visibleFaces = voxelandChunkWorkspace.visibleFaces;
		visibleFaces.Clear();
		for (int i = 0; i < faces.Count; i++)
		{
			if (faces[i].block.visible)
			{
				visibleFaces.Add(faces[i]);
			}
		}
		int num = 0;
		for (num = 0; num < voxelandChunkWorkspace.verts.Count; num++)
		{
			VoxelandVert voxelandVert = voxelandChunkWorkspace.verts[num];
			if (voxelandVert.ComputeIsVisible())
			{
				voxelandVert.normal = voxelandChunkWorkspace.verts[num].normal.normalized;
				voxelandVert.CacheTangent(flag);
				voxelandVert.CacheGloss(voxeland.types);
				voxelandVert.CacheBlendWeights(list);
			}
		}
	}

	public void PushLayer(Mesh hi, Mesh lo, int layer)
	{
		if (ws == null)
		{
			Debug.LogError("Workspace not set!");
			return;
		}
		List<VoxelandFace> visibleFaces = ws.visibleFaces;
		List<VoxelandVert> verts = ws.verts;
		int num = 0;
		if (ws.layerFaces == null || ws.layerFaces.Length < visibleFaces.Count)
		{
			ws.layerFaces = new VoxelandFace[2 * visibleFaces.Count];
		}
		if (ws.layerVerts == null || ws.layerVerts.Length < verts.Count)
		{
			ws.layerVerts = new VoxelandVert[2 * verts.Count];
		}
		if (ws.lowLayerVerts == null || ws.lowLayerVerts.Length < verts.Count / 2)
		{
			ws.lowLayerVerts = new VoxelandVert[2 * verts.Count / 2];
		}
		for (num = 0; num < verts.Count; num++)
		{
			verts[num].layerVertIndex = -1;
			verts[num].layerLowVertIndex = -1;
		}
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		VoxelandFace voxelandFace = null;
		VoxelandVert voxelandVert = null;
		for (int i = 0; i < visibleFaces.Count; i++)
		{
			voxelandFace = visibleFaces[i];
			bool flag = false;
			if (layer == 0)
			{
				flag = true;
			}
			else
			{
				for (num = 0; num < 9; num++)
				{
					voxelandVert = voxelandFace.verts[num];
					if (voxelandVert.GetCachedBlendWeight(layer) > 0f)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				continue;
			}
			ws.layerFaces[num2++] = voxelandFace;
			for (num = 0; num < 9; num++)
			{
				voxelandVert = voxelandFace.verts[num];
				if (voxelandVert.layerVertIndex == -1)
				{
					ws.layerVerts[num3] = voxelandVert;
					voxelandVert.layerVertIndex = num3++;
					if (num == 0 || num == 2 || num == 4 || num == 6)
					{
						ws.lowLayerVerts[num4] = voxelandVert;
						voxelandVert.layerLowVertIndex = num4++;
					}
					voxelandVert.layerBlend = ((layer == 0) ? 1f : voxelandVert.GetCachedBlendWeight(layer));
				}
			}
		}
		if (num3 == 0)
		{
			lo.Clear();
			if (hi != null)
			{
				hi.Clear();
			}
		}
		else
		{
			if (land.debugSkipMeshUpload)
			{
				return;
			}
			if (hi != null)
			{
				Vector3[] array = vector3Pool.Get(num3);
				for (num = 0; num < num3; num++)
				{
					array[num] = ws.layerVerts[num].pos;
				}
				hi.SetVertices(array, num3);
				vector3Pool.Return(array);
				Vector3[] array2 = vector3Pool.Get(num3);
				for (num = 0; num < num3; num++)
				{
					array2[num] = ws.layerVerts[num].normal;
				}
				hi.SetNormals(array2, num3);
				vector3Pool.Return(array2);
				Vector4[] array3 = vector4Pool.Get(num3);
				for (num = 0; num < num3; num++)
				{
					array3[num] = ws.layerVerts[num].tangent;
				}
				hi.SetTangents(array3, num3);
				vector4Pool.Return(array3);
				Vector2[] array4 = vector2Pool.Get(num3);
				for (num = 0; num < num3; num++)
				{
					array4[num] = new Vector2(ws.layerVerts[num].layerBlend, ws.layerVerts[num].gloss);
				}
				hi.SetUVs(0, array4, num3);
				vector2Pool.Return(array4);
				int[] array5 = intPool.Get(num2 * 24);
				for (int j = 0; j < num2; j++)
				{
					voxelandFace = ws.layerFaces[j];
					int num5 = j * 24;
					array5[num5] = voxelandFace.verts[7].layerVertIndex;
					array5[num5 + 1] = voxelandFace.verts[0].layerVertIndex;
					array5[num5 + 2] = voxelandFace.verts[1].layerVertIndex;
					array5[num5 + 3] = voxelandFace.verts[1].layerVertIndex;
					array5[num5 + 4] = voxelandFace.verts[8].layerVertIndex;
					array5[num5 + 5] = voxelandFace.verts[7].layerVertIndex;
					array5[num5 + 6] = voxelandFace.verts[8].layerVertIndex;
					array5[num5 + 7] = voxelandFace.verts[1].layerVertIndex;
					array5[num5 + 8] = voxelandFace.verts[2].layerVertIndex;
					array5[num5 + 9] = voxelandFace.verts[2].layerVertIndex;
					array5[num5 + 10] = voxelandFace.verts[3].layerVertIndex;
					array5[num5 + 11] = voxelandFace.verts[8].layerVertIndex;
					array5[num5 + 12] = voxelandFace.verts[5].layerVertIndex;
					array5[num5 + 13] = voxelandFace.verts[8].layerVertIndex;
					array5[num5 + 14] = voxelandFace.verts[3].layerVertIndex;
					array5[num5 + 15] = voxelandFace.verts[3].layerVertIndex;
					array5[num5 + 16] = voxelandFace.verts[4].layerVertIndex;
					array5[num5 + 17] = voxelandFace.verts[5].layerVertIndex;
					array5[num5 + 18] = voxelandFace.verts[6].layerVertIndex;
					array5[num5 + 19] = voxelandFace.verts[7].layerVertIndex;
					array5[num5 + 20] = voxelandFace.verts[8].layerVertIndex;
					array5[num5 + 21] = voxelandFace.verts[8].layerVertIndex;
					array5[num5 + 22] = voxelandFace.verts[5].layerVertIndex;
					array5[num5 + 23] = voxelandFace.verts[6].layerVertIndex;
				}
				hi.SetTrianglesUWE(array5, num2 * 24);
				intPool.Return(array5);
			}
			Vector3[] array6 = vector3Pool.Get(num4);
			for (num = 0; num < num4; num++)
			{
				array6[num] = ws.lowLayerVerts[num].pos;
			}
			lo.SetVertices(array6, num4);
			vector3Pool.Return(array6);
			Vector3[] array7 = vector3Pool.Get(num4);
			for (num = 0; num < num4; num++)
			{
				array7[num] = ws.lowLayerVerts[num].normal;
			}
			lo.SetNormals(array7, num4);
			vector3Pool.Return(array7);
			Vector4[] array8 = vector4Pool.Get(num4);
			for (num = 0; num < num4; num++)
			{
				array8[num] = ws.lowLayerVerts[num].tangent;
			}
			lo.SetTangents(array8, num4);
			vector4Pool.Return(array8);
			Vector2[] array9 = vector2Pool.Get(num4);
			for (num = 0; num < num4; num++)
			{
				array9[num] = new Vector2(ws.lowLayerVerts[num].layerBlend, ws.lowLayerVerts[num].gloss);
			}
			lo.SetUVs(0, array9, num4);
			vector2Pool.Return(array9);
			int[] array10 = intPool.Get(num2 * 6);
			for (int k = 0; k < num2; k++)
			{
				voxelandFace = ws.layerFaces[k];
				int num6 = k * 6;
				array10[num6] = voxelandFace.verts[0].layerLowVertIndex;
				array10[num6 + 1] = voxelandFace.verts[2].layerLowVertIndex;
				array10[num6 + 2] = voxelandFace.verts[4].layerLowVertIndex;
				array10[num6 + 3] = voxelandFace.verts[0].layerLowVertIndex;
				array10[num6 + 4] = voxelandFace.verts[4].layerLowVertIndex;
				array10[num6 + 5] = voxelandFace.verts[6].layerLowVertIndex;
			}
			lo.SetTrianglesUWE(array10, num2 * 6);
			intPool.Return(array10);
		}
	}

	public Material[] ComputeMaterialsForLayer(int layer, bool debugAllOpaque, bool useLQShader)
	{
		int num = ((layer != 0) ? 1 : 2);
		Material[] array = new Material[num];
		if (debugAllOpaque)
		{
			for (int i = 0; i < num; i++)
			{
				array[i] = land.opaqueMaterial;
			}
		}
		else if (!land.debugUseDummyMaterial)
		{
			int num2 = usedTypes[layer].num;
			if (num2 >= land.types.Length)
			{
				Debug.Log("Chunk used a block type (" + num2 + ") that was not in the list. Did you forget to setup some types in the land?");
			}
			else if (land.types[num2] == null)
			{
				Debug.Log("Block type " + num2 + " was null");
			}
			else if (land.types[num2].material == null)
			{
				Debug.Log("Block type " + num2 + " had null material");
			}
			else
			{
				_ = land.debugSolidColorMaterials;
				array[0] = land.types[num2].material;
				if (useLQShader)
				{
					array[0].shader = land.debugLQShader;
				}
			}
			if (layer == 0)
			{
				if (land.opaqueMaterial == null)
				{
					Debug.LogError("No opaque material set in voxeland!");
				}
				else
				{
					array[1] = land.opaqueMaterial;
				}
			}
		}
		else
		{
			for (int j = 0; j < num; j++)
			{
				array[j] = land.debugDummyMaterial;
			}
		}
		return array;
	}

	public void BuildAmbient()
	{
	}

	public static void GrassPhase1(IVoxelandChunk chunk)
	{
		VoxelandChunkWorkspace voxelandChunkWorkspace = chunk.ws;
		IVoxeland voxeland = chunk.land;
		List<TypeUse> list = chunk.usedTypes;
		if (voxelandChunkWorkspace == null)
		{
			Debug.LogError("Workspace not set!");
			return;
		}
		VoxelandBlockType[] types = voxeland.types;
		for (int i = 0; i < list.Count; i++)
		{
			int num = list[i].num;
			if (types[num].hasGrassAbove)
			{
				GrassPhase1(chunk, num);
			}
		}
	}

	public IEnumerable<GrassPos> EnumerateGrass(VoxelandTypeBase settings, byte typeFilter, int randSeed, double reduction)
	{
		return EnumerateGrass(this, settings, typeFilter, randSeed, reduction);
	}

	public static IEnumerable<GrassPos> EnumerateGrass(IVoxelandChunk chunk, VoxelandTypeBase settings, byte typeFilter, int randSeed, double reduction)
	{
		VoxelandChunkWorkspace voxelandChunkWorkspace = chunk.ws;
		int offsetX = chunk.offsetX;
		int offsetY = chunk.offsetY;
		int offsetZ = chunk.offsetZ;
		bool skipHiRes = chunk.skipHiRes;
		if (voxelandChunkWorkspace == null)
		{
			Debug.LogError("Workspace not set!");
			yield break;
		}
		List<VoxelandFace> faces = voxelandChunkWorkspace.faces;
		GrassPos rv = new GrassPos
		{
			skipHiRes = skipHiRes
		};
		float density = settings.grassDensity;
		float minNormalY = Mathf.Cos((float)settings.grassMaxTilt * ((float)System.Math.PI / 180f));
		float maxNormalY = Mathf.Cos((float)settings.grassMinTilt * ((float)System.Math.PI / 180f));
		Unity.Mathematics.Random reductionRng = VoxelandMisc.CreateRandom(randSeed);
		Unity.Mathematics.Random placementRng = VoxelandMisc.CreateRandom(offsetX * 9999 + offsetY * 999 + offsetZ * 99 + randSeed * 9);
		for (int f = 0; f < faces.Count; f++)
		{
			VoxelandFace face = faces[f];
			if (!face.block.visible || (typeFilter > 0 && face.type != typeFilter))
			{
				continue;
			}
			if (skipHiRes)
			{
				Vector3 normalized = (face.verts[0].normal + face.verts[2].normal + face.verts[4].normal + face.verts[6].normal).normalized;
				if (normalized.y < minNormalY || normalized.y > maxNormalY)
				{
					continue;
				}
			}
			else if (face.verts[8].normal.y < minNormalY || face.verts[8].normal.y > maxNormalY)
			{
				continue;
			}
			for (int quad = 0; quad < 4; quad++)
			{
				if (reduction > 0.0 && reductionRng.NextDouble() < reduction)
				{
					continue;
				}
				rv.face = face;
				rv.quadNum = quad;
				if (settings.perlinGrass)
				{
					Vector3 vert = rv.GetVert(0);
					Vector3 vert2 = rv.GetVert(1);
					Vector3 vert3 = rv.GetVert(2);
					Vector3 vert4 = rv.GetVert(3);
					Vector3 vector = (vert + vert2 + vert3 + vert4) / 4f;
					Vector3 vector2 = new Vector3(offsetX, offsetY, offsetZ) + vector;
					if (Mathf.PerlinNoise(vector2.x / settings.perlinPeriod, vector2.z / settings.perlinPeriod) > density)
					{
						continue;
					}
				}
				else if (placementRng.NextDouble() > (double)density)
				{
					continue;
				}
				yield return rv;
			}
		}
	}

	private static void GrassPhase1(IVoxelandChunk chunk, int blockTypeId)
	{
		VoxelandChunkWorkspace voxelandChunkWorkspace = chunk.ws;
		IVoxeland voxeland = chunk.land;
		int num = chunk.offsetX;
		int num2 = chunk.offsetY;
		int num3 = chunk.offsetZ;
		_ = chunk.skipHiRes;
		if (voxelandChunkWorkspace == null)
		{
			Debug.LogError("Workspace not set!");
			return;
		}
		VoxelandBlockType voxelandBlockType = voxeland.types[blockTypeId];
		Unity.Mathematics.Random rng = VoxelandMisc.CreateRandom(num * 9999 + num2 * 999 + num3 * 99);
		if (voxelandBlockType.grassVerts == null)
		{
			Debug.LogWarning("WARNING: Block type " + blockTypeId + " (" + voxelandBlockType.name + ") has grass checked but no grass mesh assigned or assigned grass mesh is not read-enabled!");
			return;
		}
		if (voxelandChunkWorkspace.nextGrassMesh >= voxelandChunkWorkspace.grassMeshes.Count)
		{
			voxelandChunkWorkspace.grassMeshes.Add(new VLGrassMesh());
		}
		VLGrassMesh vLGrassMesh = voxelandChunkWorkspace.grassMeshes[voxelandChunkWorkspace.nextGrassMesh++];
		vLGrassMesh.Reset();
		vLGrassMesh.type = (byte)blockTypeId;
		vLGrassMesh.mesh = voxelandBlockType.grassMesh;
		vLGrassMesh.material = voxelandBlockType.grassMaterial;
		foreach (GrassPos item in EnumerateGrass(chunk, voxelandBlockType, vLGrassMesh.type, blockTypeId, 0.0))
		{
			item.ComputeTransform(ref rng, voxelandBlockType);
			int count = vLGrassMesh.verts.Count;
			for (int i = 0; i < voxelandBlockType.grassVerts.Length; i++)
			{
				VLGrassVert vLGrassVert = voxelandChunkWorkspace.NewGrassVert();
				vLGrassVert.pos = item.csOrigin + item.quat * (item.scale * voxelandBlockType.grassVerts[i]);
				vLGrassVert.normal = item.quat * voxelandBlockType.grassNormals[i];
				vLGrassVert.tangent = item.quat * voxelandBlockType.grassTangents[i];
				vLGrassVert.uv = voxelandBlockType.grassUVs[i];
				float num4 = Vector3.Dot(vLGrassVert.pos - item.csOrigin, item.faceNormal);
				vLGrassVert.color = new Color((int)rng.NextByte(), (int)rng.NextByte(), (int)rng.NextByte(), (int)(byte)(num4 * 255f));
				vLGrassMesh.verts.Add(vLGrassVert);
			}
			for (int j = 0; j < voxelandBlockType.grassTris.Length / 3; j++)
			{
				VLGrassTri vLGrassTri = voxelandChunkWorkspace.NewGrassTri();
				vLGrassTri.v0 = count + voxelandBlockType.grassTris[3 * j];
				vLGrassTri.v1 = count + voxelandBlockType.grassTris[3 * j + 1];
				vLGrassTri.v2 = count + voxelandBlockType.grassTris[3 * j + 2];
				vLGrassMesh.tris.Add(vLGrassTri);
			}
		}
	}

	public void BuildGrass()
	{
		BuildGrass(null);
	}

	public void BuildGrass(GrassCB cb)
	{
		if (ws == null)
		{
			Debug.LogError("Workspace not set!");
			return;
		}
		for (int i = 0; i < ws.nextGrassMesh; i++)
		{
			VLGrassMesh vLGrassMesh = ws.grassMeshes[i];
			if (vLGrassMesh.material == null)
			{
				Debug.LogError("No grass material assigned for block type = " + vLGrassMesh.type);
				continue;
			}
			string text = vLGrassMesh.material.name;
			if ((bool)vLGrassMesh.mesh)
			{
				text = vLGrassMesh.mesh.name;
			}
			GameObject obj = new GameObject("Grass type " + vLGrassMesh.type + " (" + text + ")");
			obj.transform.parent = base.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
			meshFilter.mesh = new Mesh();
			grassFilters.Add(meshFilter);
			MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			meshRenderer.material = vLGrassMesh.material;
			grassRenders.Add(meshRenderer);
			cb?.Begin(vLGrassMesh.material.name);
			int num = vLGrassMesh.verts.Count;
			int num2 = 0;
			if (num > 65000)
			{
				Debug.Log("WARNING: Too many grass verts, truncating to 65000. grass mesh = " + text + ", land name = " + land.gameObject.name + ", chunk " + base.gameObject.name);
				num = 65000;
			}
			Vector3[] array = vector3Pool.Get(num);
			if (!land.debugUploadNullGrass)
			{
				for (num2 = 0; num2 < num; num2++)
				{
					array[num2] = vLGrassMesh.verts[num2].pos;
				}
			}
			meshFilter.sharedMesh.SetVertices(array, num);
			vector3Pool.Return(array);
			Vector3[] array2 = vector3Pool.Get(num);
			if (!land.debugUploadNullGrass)
			{
				for (num2 = 0; num2 < num; num2++)
				{
					array2[num2] = vLGrassMesh.verts[num2].normal;
				}
			}
			meshFilter.sharedMesh.SetNormals(array2, num);
			vector3Pool.Return(array2);
			Vector2[] array3 = vector2Pool.Get(num);
			if (!land.debugUploadNullGrass)
			{
				for (num2 = 0; num2 < num; num2++)
				{
					array3[num2] = vLGrassMesh.verts[num2].uv;
				}
			}
			meshFilter.sharedMesh.SetUVs(0, array3, num);
			vector2Pool.Return(array3);
			Vector4[] array4 = vector4Pool.Get(num);
			if (!land.debugUploadNullGrass)
			{
				for (num2 = 0; num2 < num; num2++)
				{
					array4[num2] = vLGrassMesh.verts[num2].tangent;
				}
			}
			meshFilter.sharedMesh.SetTangents(array4, num);
			vector4Pool.Return(array4);
			Color32[] array5 = colorPool.Get(num);
			if (!land.debugUploadNullGrass)
			{
				for (num2 = 0; num2 < num; num2++)
				{
					array5[num2] = vLGrassMesh.verts[num2].color;
				}
			}
			meshFilter.sharedMesh.SetColors(array5, num);
			colorPool.Return(array5);
			int num3 = Mathf.Min(vLGrassMesh.tris.Count, 21667);
			int num4 = num3 * 3;
			int[] array6 = intPool.Get(num4);
			Array.Clear(array6, 0, array6.Length);
			if (!land.debugUploadNullGrass)
			{
				for (int j = 0; j < num3; j++)
				{
					int num5 = j * 3;
					array6[num5] = vLGrassMesh.tris[j].v0;
					array6[num5 + 1] = vLGrassMesh.tris[j].v1;
					array6[num5 + 2] = vLGrassMesh.tris[j].v2;
				}
			}
			meshFilter.sharedMesh.SetTrianglesUWE(array6, num4);
			intPool.Return(array6);
			attachedRenderers.Add(meshRenderer);
			cb?.End();
		}
	}

	private static byte QueryTypeGrid(VoxelandChunkWorkspace ws, int x, int y, int z)
	{
		return ws.rws.typesGrid[1 + x, 1 + y, 1 + z];
	}

	private static byte QueryDensityGrid(VoxelandChunkWorkspace ws, int x, int y, int z)
	{
		return ws.rws.densityGrid[1 + x, 1 + y, 1 + z];
	}

	public void SetFaceMap(List<VoxelandCoords> map)
	{
		faceMap = map;
	}

	public MeshCollider EnsureCollision()
	{
		return EnsureCollision(this);
	}

	public static MeshCollider EnsureCollision(IVoxelandChunk2 chunk)
	{
		if (!chunk.collision)
		{
			GameObject obj = new GameObject("override collider");
			obj.SetActive(value: false);
			MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
			meshCollider.gameObject.layer = 30;
			SetParent(meshCollider.transform, chunk.transform);
			chunk.collision = meshCollider;
			return meshCollider;
		}
		return chunk.collision;
	}

	public void AttachCollision(MeshCollider coll)
	{
		SetParent(coll.transform, base.transform);
		collision = coll;
	}

	private static void SetParent(Transform transform, Transform parent)
	{
		transform.SetParent(parent, worldPositionStays: false);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}

	public MeshCollider DetachCollision()
	{
		MeshCollider result = collision;
		collision.gameObject.SetActive(value: false);
		collision.transform.SetParent(null, worldPositionStays: false);
		collision = null;
		return result;
	}

	public List<VoxelandCoords> GetFaceMap()
	{
		Debug.LogError("We only save visible faces in edit-mode for now. Bug Steve about this if we need this in play-mode for a good reason.");
		return faceMap;
	}

	public bool NeedsRebuild()
	{
		return faceMap == null;
	}

	public bool IsChunkNumber(int cx, int cy, int cz)
	{
		if (this.cx == cx && this.cy == cy)
		{
			return this.cz == cz;
		}
		return false;
	}

	public static void DestroyWrap(UnityEngine.Object obj, bool forceImmediate)
	{
		if (!forceImmediate)
		{
			UnityEngine.Object.Destroy(obj);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}
	}

	public static void DestroyWrap(UnityEngine.Object obj)
	{
		DestroyWrap(obj, forceImmediate: false);
	}

	public void DestroyChunkLayer(MeshFilter filter)
	{
		filter.sharedMesh.Clear(keepVertexLayout: false);
		DestroyWrap(filter.sharedMesh);
	}

	public void DestroySelf()
	{
		DestroyWrap(base.gameObject);
	}

	public void FreeResources()
	{
		int num = 0;
		if (hiFilters != null)
		{
			for (num = 0; num < hiFilters.Count; num++)
			{
				if ((bool)hiFilters[num] && (bool)hiFilters[num].sharedMesh)
				{
					DestroyChunkLayer(hiFilters[num]);
				}
			}
		}
		if (loFilters != null)
		{
			for (num = 0; num < loFilters.Count; num++)
			{
				if ((bool)loFilters[num] && (bool)loFilters[num].sharedMesh)
				{
					DestroyChunkLayer(loFilters[num]);
				}
			}
		}
		if (collision != null)
		{
			MeshCollider meshCollider = collision;
			if (meshCollider != null && meshCollider.sharedMesh != null)
			{
				DestroyWrap(meshCollider.sharedMesh);
			}
		}
		for (int i = 0; i < grassFilters.Count; i++)
		{
			if (grassFilters[i] != null && grassFilters[i].sharedMesh != null)
			{
				DestroyWrap(grassFilters[i].sharedMesh);
			}
		}
	}

	public void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
		FreeResources();
	}

	private void OnEnable()
	{
		BehaviourUpdateUtils.Register(this);
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	public void OnApplicationQuit()
	{
		FreeResources();
	}

	public void DisableAllMeshRenderers()
	{
		SetRenderersEnabled(enabled: false, fade: false);
	}

	public void SetRenderersEnabled(bool enabled, bool fade)
	{
		if (enabled)
		{
			fadingIn = fade;
			if (!fade)
			{
				fadeAmount = 1f;
			}
			SyncFadeInAmount();
			BehaviourUpdateUtils.Register(this);
		}
		else
		{
			fadingIn = false;
		}
		if (opaqueFilter != null)
		{
			opaqueFilter.GetComponent<Renderer>().enabled = enabled;
		}
		if (hiRenders != null)
		{
			for (int i = 0; i < hiRenders.Count; i++)
			{
				hiRenders[i].enabled = enabled;
			}
		}
		if (loFilters != null)
		{
			for (int j = 0; j < loFilters.Count; j++)
			{
				loFilters[j].GetComponent<Renderer>().enabled = enabled;
			}
		}
		if (grassRenders != null)
		{
			for (int k = 0; k < grassRenders.Count; k++)
			{
				grassRenders[k].enabled = enabled;
			}
		}
	}

	public void ClearMeshes()
	{
		if (hiFilters != null)
		{
			for (int i = 0; i < hiFilters.Count; i++)
			{
				hiFilters[i].sharedMesh.Clear(keepVertexLayout: false);
				hiFilters[i].sharedMesh.name = "TERRAIN hifilter unused";
			}
		}
		if (loFilters != null)
		{
			for (int j = 0; j < loFilters.Count; j++)
			{
				loFilters[j].sharedMesh.Clear(keepVertexLayout: false);
				loFilters[j].sharedMesh.name = "TERRAIN lofilter unused";
			}
		}
		if (grassFilters != null)
		{
			for (int k = 0; k < grassFilters.Count; k++)
			{
				grassFilters[k].sharedMesh.Clear(keepVertexLayout: false);
				grassFilters[k].sharedMesh.name = "TERRAIN grass unused";
			}
		}
		if (collision != null)
		{
			MeshCollider meshCollider = collision;
			if (meshCollider != null && meshCollider.sharedMesh != null)
			{
				meshCollider.sharedMesh.Clear(keepVertexLayout: false);
				meshCollider.sharedMesh.name = "TERRAIN collider unused";
			}
		}
	}

	public Vector3 GetLandSpaceMeshOrigin()
	{
		return new Vector3(offsetX, offsetY, offsetZ);
	}

	public int CountFilterDrawCalls(MeshFilter f)
	{
		return f.gameObject.GetComponent<MeshRenderer>().sharedMaterials.Length;
	}

	public int CountDrawCalls()
	{
		int num = 0;
		if (opaqueFilter != null)
		{
			num++;
		}
		if (hiFilters != null)
		{
			foreach (MeshFilter hiFilter in hiFilters)
			{
				num += CountFilterDrawCalls(hiFilter);
			}
		}
		if (loFilters != null)
		{
			foreach (MeshFilter loFilter in loFilters)
			{
				num += CountFilterDrawCalls(loFilter);
			}
		}
		if (grassFilters != null)
		{
			foreach (MeshFilter grassFilter in grassFilters)
			{
				num += CountFilterDrawCalls(grassFilter);
			}
		}
		return num;
	}

	public void ManagedUpdate()
	{
		if (fadingIn)
		{
			fadeAmount += Time.unscaledDeltaTime / 1f;
			if (fadeAmount > 1f)
			{
				fadeAmount = 1f;
				fadingIn = false;
			}
			SyncFadeInAmount();
		}
		if (!fadingIn)
		{
			BehaviourUpdateUtils.Deregister(this);
		}
	}

	private void SyncFadeInAmount()
	{
		for (int i = 0; i < attachedRenderers.Count; i++)
		{
			attachedRenderers[i].SetFadeAmount(fadeAmount);
		}
	}

	[SpecialName]
	Transform IVoxelandChunk2.get_transform()
	{
		return base.transform;
	}

	[SpecialName]
	GameObject IVoxelandChunk2.get_gameObject()
	{
		return base.gameObject;
	}
}
