using System;
using System.Collections.Generic;
using UnityEngine;

public class VFXSchoolFish : MonoBehaviour
{
	private VFXSchoolFishManager mng;

	public int managerIndex = -1;

	private bool firstUpdate = true;

	public int quantity = 200;

	public int randomQuantity = 100;

	public float seaLevel;

	public float gravity = 1f;

	public float rotationalForce = 1f;

	public Transform repulsor;

	public float repulseForce = 5f;

	public float repelDistanceSq = 120f;

	private float distanceThresholdSq = 2500f;

	public float attractForceMin = -0.2f;

	public float attractForceMax = -0.4f;

	public float posMin = -5f;

	public float posMax = 5f;

	public float speedMin = 0.01f;

	public float speedMax = 0.05f;

	private Vector3 playerRepulsorPos;

	private bool playerIsClose;

	private float playerDistSq = float.MaxValue;

	private float closestRepulsorDistSq = float.MaxValue;

	private int closestRepulsorIndex = -1;

	private float timeBetweenRepulsorUpdates = 0.5f;

	private float nextRepulsorUpdate;

	private float timeBetweenDistanceUpdates = 0.25f;

	private float nextDistanceUpdate;

	public Vector2 particleSize = new Vector2(0.36f, 0.18f);

	public float particleRandomScaleMin = 0.25f;

	public float particleRandomScaleMax = 1f;

	private bool distanceUpdateDisabled;

	[AssertNotNull]
	public MeshFilter meshFilter;

	[AssertNotNull]
	public MeshRenderer meshRenderer;

	[NonSerialized]
	public bool disableRepulse;

	private MaterialPropertyBlock propertyBlock;

	private bool bInManager;

	private static Vector4[] repulsorPos = new Vector4[2];

	public bool isVisible { get; private set; }

	private void Init()
	{
		mng = VFXSchoolFishManager.main;
		quantity = UnityEngine.Random.Range(quantity - randomQuantity, quantity + randomQuantity);
		nextRepulsorUpdate = Time.time + UnityEngine.Random.value * timeBetweenRepulsorUpdates;
		nextDistanceUpdate = Time.time + UnityEngine.Random.value * timeBetweenDistanceUpdates;
		meshFilter.sharedMesh = VFXSchoolFishManager.main.GetSchoolMesh(quantity);
		propertyBlock = mng.AcquirePropertyBlock();
		meshRenderer.GetPropertyBlock(propertyBlock);
		Vector2 vector = particleSize * particleRandomScaleMax;
		Vector2 vector2 = particleSize * particleRandomScaleMin;
		Vector3 vector3 = vector - vector2;
		propertyBlock.SetVector("_SizeRangeAndMin", new Vector4(vector3.x, vector3.y, vector2.x, vector2.y));
		meshRenderer.SetPropertyBlock(propertyBlock);
	}

	private void OnEnable()
	{
		if (!bInManager)
		{
			mng.AddSchool(this);
			bInManager = true;
			firstUpdate = true;
		}
	}

	private void OnDisable()
	{
		if (bInManager)
		{
			mng.RemoveSchool(this);
			bInManager = false;
		}
	}

	private void OnDestroy()
	{
		if (bInManager)
		{
			mng.RemoveSchool(this);
			bInManager = false;
		}
		if (propertyBlock != null)
		{
			mng.ReturnPropertyBlock(propertyBlock);
		}
	}

	public void UpdateRepulsor()
	{
		if (!mng.enableRepulsor || Time.time < nextRepulsorUpdate)
		{
			return;
		}
		nextRepulsorUpdate = Time.time + timeBetweenRepulsorUpdates;
		closestRepulsorDistSq = 100000f;
		closestRepulsorIndex = -1;
		Vector3 position = base.transform.position;
		List<Transform> repulsors = mng.repulsors;
		for (int i = 0; i < repulsors.Count; i++)
		{
			if (repulsors[i] != null)
			{
				float sqrMagnitude = (position - repulsors[i].position).sqrMagnitude;
				if (sqrMagnitude < closestRepulsorDistSq)
				{
					closestRepulsorDistSq = sqrMagnitude;
					closestRepulsorIndex = i;
				}
			}
		}
		if (closestRepulsorIndex > -1)
		{
			repulsor = repulsors[closestRepulsorIndex];
		}
		else
		{
			repulsor = null;
		}
	}

	private void Awake()
	{
		Init();
	}

	public void UpdateDistances(Vector3 playerPos, Vector3 cameraFwd)
	{
		if (!distanceUpdateDisabled && !(Time.time < nextDistanceUpdate))
		{
			nextDistanceUpdate = Time.time + timeBetweenDistanceUpdates;
			_ = Vector3.zero;
			playerDistSq = (base.transform.position - playerPos).sqrMagnitude;
			playerIsClose = playerDistSq < distanceThresholdSq;
			playerRepulsorPos = playerPos;
		}
	}

	public void UpdateCheckForDisabled()
	{
		meshRenderer.enabled = Player.main.displaySurfaceWater;
		if (!Player.main.displaySurfaceWater || !isVisible)
		{
			if (!Player.main.displaySurfaceWater && !distanceUpdateDisabled)
			{
				distanceUpdateDisabled = true;
				playerIsClose = false;
			}
		}
		else if (Player.main.displaySurfaceWater && isVisible && Player.main.displaySurfaceWater && distanceUpdateDisabled)
		{
			distanceUpdateDisabled = false;
		}
	}

	public void SetupUpdateMaterial(Material updateMaterial)
	{
		updateMaterial.SetFloat(ShaderPropertyID._SnapToTargetPosition, firstUpdate ? 1f : 0f);
		firstUpdate = false;
		updateMaterial.SetMatrix(ShaderPropertyID._LocalToWorldMatrix, base.transform.localToWorldMatrix);
		repulsorPos[0] = Vector4.zero;
		repulsorPos[1] = Vector4.zero;
		if (!disableRepulse)
		{
			if (mng.enableRepulsor && repulsor != null && closestRepulsorDistSq < repelDistanceSq)
			{
				Vector3 position = repulsor.position;
				repulsorPos[0] = new Vector4(position.x, position.y, position.z, 1f);
			}
			if (mng.enablePlayerRepulse && playerIsClose && playerDistSq < repelDistanceSq)
			{
				repulsorPos[1] = new Vector4(playerRepulsorPos.x, playerRepulsorPos.y, playerRepulsorPos.z, 1f);
			}
		}
		updateMaterial.SetVectorArray(ShaderPropertyID._RepulsorPos, repulsorPos);
		updateMaterial.SetFloat(ShaderPropertyID._RepulseForce, repulseForce);
		updateMaterial.SetFloat(ShaderPropertyID._SeaLevel, seaLevel);
		updateMaterial.SetFloat(ShaderPropertyID._Gravity, gravity);
		updateMaterial.SetFloat(ShaderPropertyID._TargetPositionScale, posMax);
		Vector4 value = default(Vector4);
		value.x = attractForceMax - attractForceMin;
		value.y = attractForceMin;
		value.z = speedMax - speedMin;
		value.w = speedMin;
		updateMaterial.SetVector(ShaderPropertyID._ForceAndSpeedRangeAndMin, value);
	}

	public void SetTextureOffset(float textureOffset)
	{
		meshRenderer.GetPropertyBlock(propertyBlock);
		propertyBlock.SetVector(ShaderPropertyID._TextureOffset, new Vector4(0f, textureOffset, 0f, 0f));
		meshRenderer.SetPropertyBlock(propertyBlock);
	}

	public void SetPositionAndVelocityTextures(Texture positionTexture, Texture velocityTexture)
	{
		meshRenderer.GetPropertyBlock(propertyBlock);
		propertyBlock.SetTexture(ShaderPropertyID._PositionTex, positionTexture);
		propertyBlock.SetTexture(ShaderPropertyID._VelocityTex, velocityTexture);
		meshRenderer.SetPropertyBlock(propertyBlock);
	}

	private void OnBecameVisible()
	{
		isVisible = true;
	}

	private void OnBecameInvisible()
	{
		isVisible = false;
	}
}
