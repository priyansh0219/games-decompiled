using System.Collections.Generic;
using UWE;
using UnityEngine;

public class VFXSchoolFishManager : MonoBehaviour
{
	private struct SchoolsTexture
	{
		public RenderTexture positionTexture;

		public RenderTexture velocityTexture;

		public void Release()
		{
			if ((bool)positionTexture && positionTexture.IsCreated())
			{
				positionTexture.Release();
			}
			if ((bool)velocityTexture && velocityTexture.IsCreated())
			{
				velocityTexture.Release();
			}
		}
	}

	public static VFXSchoolFishManager main;

	private const int maxFishPerSchool = 160;

	private const int numSchoolsPerTexture = 128;

	private const float maxDistanceFromCenter = 10f;

	private Material updateMaterial;

	public Shader updateShader;

	public List<Transform> repulsors;

	public List<VFXSchoolFish> schools;

	private Stack<int> freeSchoolIndices;

	private List<SchoolsTexture> schoolsTextures;

	private SchoolsTexture newSchoolsTexture;

	private List<Mesh> schoolMeshes;

	private Mesh quadMesh;

	private Texture2D targetPositionTexture;

	private Texture2D forceAndSpeedTexture;

	private ObjectPool<MaterialPropertyBlock> propertyBlockPool;

	public bool enableRepulsor = true;

	public bool enablePlayerRepulse = true;

	public bool enableAI = true;

	private static RenderBuffer[] colorBuffers = new RenderBuffer[2];

	private void Awake()
	{
		main = this;
		repulsors = new List<Transform>();
		schools = new List<VFXSchoolFish>();
		freeSchoolIndices = new Stack<int>();
		schoolsTextures = new List<SchoolsTexture>();
		newSchoolsTexture = CreateSchoolsTexture();
		schoolMeshes = new List<Mesh>();
		propertyBlockPool = ObjectPoolHelper.CreatePool<MaterialPropertyBlock>(256);
		DevConsole.RegisterConsoleCommand(this, "schoolfishai");
		DevConsole.RegisterConsoleCommand(this, "schoolfishrepulsor");
		DevConsole.RegisterConsoleCommand(this, "schoolfishrepulsedbyplayer");
		CreateResources();
	}

	private void OnDestroy()
	{
		newSchoolsTexture.Release();
		foreach (SchoolsTexture schoolsTexture in schoolsTextures)
		{
			schoolsTexture.Release();
		}
	}

	private void CreateResources()
	{
		targetPositionTexture = new Texture2D(160, 1, TextureFormat.RGBAHalf, mipChain: false, linear: true);
		targetPositionTexture.name = "Fish School targetPositionTexture";
		targetPositionTexture.filterMode = FilterMode.Point;
		forceAndSpeedTexture = new Texture2D(160, 1, TextureFormat.RGHalf, mipChain: false, linear: true);
		forceAndSpeedTexture.name = "Fish School forceAndSpeedTexture";
		forceAndSpeedTexture.filterMode = FilterMode.Point;
		Color[] array = new Color[160];
		Color color = default(Color);
		for (int i = 0; i < 160; i++)
		{
			color.r = Random.Range(-1f, 1f);
			color.g = Random.Range(-1f, 1f);
			color.b = Random.Range(-1f, 1f);
			color.a = Random.Range(-1f, 1f);
			array[i] = color;
		}
		targetPositionTexture.SetPixels(array);
		targetPositionTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		for (int j = 0; j < 160; j++)
		{
			float r = Random.Range(0f, 1f);
			float g = Random.Range(0f, 1f);
			array[j] = new Color(r, g, 0f, 0f);
		}
		forceAndSpeedTexture.SetPixels(array);
		forceAndSpeedTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		updateMaterial = new Material(updateShader);
		updateMaterial.SetTexture(ShaderPropertyID._TargetPositionTex, targetPositionTexture);
		updateMaterial.SetTexture(ShaderPropertyID._ForceAndSpeedTex, forceAndSpeedTexture);
		quadMesh = GraphicsUtil.CreateQuadMesh();
	}

	public void AddRepulsor(Transform trans)
	{
		repulsors.Add(trans);
	}

	public void RemoveRepulsor(Transform trans)
	{
		repulsors.Remove(trans);
	}

	public void AddSchool(VFXSchoolFish school)
	{
		int freeSchoolIndex = GetFreeSchoolIndex();
		schools[freeSchoolIndex] = school;
		school.managerIndex = freeSchoolIndex;
		float textureOffset = (float)(freeSchoolIndex % 128) / 128f;
		school.SetTextureOffset(textureOffset);
	}

	private int GetFreeSchoolIndex()
	{
		if (freeSchoolIndices.Count > 0)
		{
			return freeSchoolIndices.Pop();
		}
		int count = schools.Count;
		schools.Add(null);
		if (count / 128 >= schoolsTextures.Count)
		{
			schoolsTextures.Add(CreateSchoolsTexture());
		}
		return count;
	}

	public void RemoveSchool(VFXSchoolFish school)
	{
		freeSchoolIndices.Push(school.managerIndex);
		schools[school.managerIndex] = null;
		school.managerIndex = -1;
	}

	private void OnConsoleCommand_schoolfishai(NotificationCenter.Notification n)
	{
		enableAI = !enableAI;
	}

	private void OnConsoleCommand_schoolfishrepulsor(NotificationCenter.Notification n)
	{
		enableRepulsor = !enableRepulsor;
	}

	private void OnConsoleCommand_schoolfishrepulsedbyplayer(NotificationCenter.Notification n)
	{
		enablePlayerRepulse = !enablePlayerRepulse;
	}

	private void Update()
	{
		if (Time.deltaTime != 0f)
		{
			UpdateSchoolData();
			UpdateSchoolParticles();
		}
	}

	private void UpdateSchoolData()
	{
		try
		{
			int count = schools.Count;
			if (count == 0)
			{
				return;
			}
			Vector3 localPlayerPos = Utils.GetLocalPlayerPos();
			Vector3 forward = MainCamera.camera.transform.forward;
			for (int i = 0; i < count; i++)
			{
				VFXSchoolFish vFXSchoolFish = schools[i];
				if (vFXSchoolFish != null)
				{
					vFXSchoolFish.UpdateCheckForDisabled();
					vFXSchoolFish.UpdateRepulsor();
					vFXSchoolFish.UpdateDistances(localPlayerPos, forward);
				}
			}
		}
		finally
		{
		}
	}

	private void UpdateSchoolParticles()
	{
		float num = 1f / 128f;
		updateMaterial.SetFloat(ShaderPropertyID._TextureStep, num);
		updateMaterial.SetFloat(ShaderPropertyID._DeltaTime, Mathf.Min(Time.deltaTime, Time.maximumParticleDeltaTime));
		updateMaterial.SetFloat(ShaderPropertyID._MaxDistanceFromCenter, 10f);
		for (int i = 0; i < schoolsTextures.Count; i++)
		{
			colorBuffers[0] = newSchoolsTexture.positionTexture.colorBuffer;
			colorBuffers[1] = newSchoolsTexture.velocityTexture.colorBuffer;
			Graphics.SetRenderTarget(colorBuffers, newSchoolsTexture.positionTexture.depthBuffer);
			int num2 = i * 128;
			int num3 = Mathf.Min((i + 1) * 128, schools.Count);
			SchoolsTexture schoolsTexture = schoolsTextures[i];
			updateMaterial.SetTexture(ShaderPropertyID._PositionTex, schoolsTexture.positionTexture);
			updateMaterial.SetTexture(ShaderPropertyID._VelocityTex, schoolsTexture.velocityTexture);
			float num4 = 0.00390625f;
			for (int j = num2; j < num3; j++)
			{
				VFXSchoolFish vFXSchoolFish = schools[j];
				if (vFXSchoolFish != null && vFXSchoolFish.meshRenderer.enabled && vFXSchoolFish.isVisible)
				{
					vFXSchoolFish.SetupUpdateMaterial(updateMaterial);
					updateMaterial.SetFloat(ShaderPropertyID._TextureOffset, num4);
					updateMaterial.SetPass(0);
					Graphics.DrawMeshNow(quadMesh, Matrix4x4.identity);
					vFXSchoolFish.SetPositionAndVelocityTextures(newSchoolsTexture.positionTexture, newSchoolsTexture.velocityTexture);
				}
				num4 += num;
			}
			schoolsTextures[i] = newSchoolsTexture;
			newSchoolsTexture = schoolsTexture;
		}
	}

	public Mesh GetSchoolMesh(int numFish)
	{
		int num = 20;
		numFish = (numFish + (num - 1)) / num * num;
		numFish = Mathf.Min(160, numFish);
		for (int i = 0; i < schoolMeshes.Count; i++)
		{
			if (schoolMeshes[i].vertexCount == numFish * 4)
			{
				return schoolMeshes[i];
			}
		}
		Mesh mesh = CreateSchoolMesh(numFish, 10f);
		schoolMeshes.Add(mesh);
		return mesh;
	}

	private SchoolsTexture CreateSchoolsTexture()
	{
		SchoolsTexture result = default(SchoolsTexture);
		result.positionTexture = new RenderTexture(160, 128, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
		{
			name = "VFXSchoolFishManager.SchoolsTexture.positionTexture"
		};
		result.positionTexture.filterMode = FilterMode.Point;
		result.velocityTexture = new RenderTexture(160, 128, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
		{
			name = "VFXSchoolFishManager.SchoolsTexture.velocityTexture"
		};
		result.velocityTexture.filterMode = FilterMode.Point;
		return result;
	}

	private Mesh CreateSchoolMesh(int numFish, float maxRadius)
	{
		Mesh mesh = new Mesh();
		mesh.name = "VFXSchoolFish";
		Vector3[] array = new Vector3[numFish * 4];
		Vector2[] array2 = new Vector2[numFish * 4];
		int[] array3 = new int[numFish * 6];
		for (int i = 0; i < numFish; i++)
		{
			int num = i * 4;
			int num2 = i * 4 + 1;
			int num3 = i * 4 + 2;
			int num4 = i * 4 + 3;
			array[num] = new Vector3(-1f, -1f, 0f);
			array[num2] = new Vector3(1f, -1f, 0f);
			array[num3] = new Vector3(1f, 1f, 0f);
			array[num4] = new Vector3(-1f, 1f, 0f);
			float y = Random.Range(0f, 1f);
			array2[num4] = (array2[num3] = (array2[num2] = (array2[num] = new Vector2(((float)i + 0.5f) / 160f, y))));
			array3[i * 6] = num;
			array3[i * 6 + 1] = num2;
			array3[i * 6 + 2] = num3;
			array3[i * 6 + 3] = num3;
			array3[i * 6 + 4] = num4;
			array3[i * 6 + 5] = num;
		}
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.triangles = array3;
		mesh.bounds = new Bounds(Vector3.zero, Vector3.one * maxRadius);
		return mesh;
	}

	public MaterialPropertyBlock AcquirePropertyBlock()
	{
		return propertyBlockPool.Get();
	}

	public void ReturnPropertyBlock(MaterialPropertyBlock propertyBlock)
	{
		propertyBlock.Clear();
		propertyBlockPool.Return(propertyBlock);
	}
}
