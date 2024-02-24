using System.Collections.Generic;
using UnityEngine;

public class VFXWaterSpray : MonoBehaviour
{
	public bool isInBase;

	public Transform rayCaster;

	public GameObject puddlePrefab;

	public GameObject flowMarkPrefab;

	public LayerMask layerMask;

	public Renderer[] meshRenderers;

	public ParticleSystem particles;

	private bool isPlaying;

	private bool puddleIsPlaying;

	private bool flowMarkIsPlaying;

	public float waterlevel;

	private Collider waterPlaneCollider;

	private GameObject puddleInstance;

	private GameObject flowMarkInstance;

	private List<Material> materials = new List<Material>();

	private Vector3 offset = new Vector3(0f, 0.05f, 0f);

	private bool waterLevelAboveCollision;

	private Vector3 puddlePos;

	private Vector3 waterPlanePosHelper = Vector3.zero;

	public bool GetIsPlaying()
	{
		return isPlaying;
	}

	private static Vector3 GetPuddleOnWaterPlane(Vector3 origPos, Vector3 dir, Vector3 planePos)
	{
		return origPos + Vector3.Normalize(dir) * (Vector3.Dot(planePos - origPos, Vector3.up) / Vector3.Dot(dir, Vector3.up));
	}

	private void StopPuddleFX()
	{
		puddleInstance.transform.parent = null;
		puddleInstance.GetComponent<ParticleSystem>().Stop();
		puddleIsPlaying = false;
	}

	private void UpdateWaterImpact()
	{
		if (puddleInstance == null)
		{
			SpawnPuddle();
			return;
		}
		if (isInBase && puddleIsPlaying && waterlevel > rayCaster.transform.position.y)
		{
			StopPuddleFX();
			return;
		}
		float value = Mathf.Pow((Vector3.Dot(rayCaster.forward, Vector3.down) + 1f) / 2.5f, 0.75f);
		Vector3 vector = Vector3.Slerp(Vector3.down, rayCaster.forward, Mathf.Clamp(value, 0f, 0.55f));
		RaycastHit hitInfo;
		if (waterLevelAboveCollision && isInBase)
		{
			waterPlanePosHelper.y = waterlevel;
			puddlePos = GetPuddleOnWaterPlane(rayCaster.position, vector, waterPlanePosHelper);
			puddleInstance.transform.position = puddlePos;
		}
		else if (Physics.Raycast(rayCaster.position, vector, out hitInfo, 6f, layerMask))
		{
			if (!isInBase || hitInfo.point.y > waterlevel)
			{
				puddleInstance.transform.rotation = Quaternion.LookRotation(hitInfo.normal, Vector3.up);
				puddlePos = hitInfo.point + offset;
				puddleInstance.transform.position = puddlePos;
			}
			else
			{
				waterLevelAboveCollision = true;
			}
			if (hitInfo.collider == waterPlaneCollider && flowMarkInstance == null)
			{
				SpawnFlowMark();
			}
			if (!puddleIsPlaying)
			{
				puddleInstance.GetComponent<ParticleSystem>().Play();
				puddleIsPlaying = true;
			}
		}
		else if (puddleIsPlaying)
		{
			StopPuddleFX();
		}
	}

	private void InitMaterials()
	{
		materials = new List<Material>();
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			materials.Add(renderer.material);
		}
	}

	private void UpdateMaterials()
	{
		for (int i = 0; i < materials.Count; i++)
		{
			materials[i].SetFloat(ShaderPropertyID._LocalFloodLevel, waterlevel);
		}
	}

	private void SpawnPuddle()
	{
		puddleInstance = Object.Instantiate(puddlePrefab);
		puddleInstance.transform.parent = base.transform;
	}

	private void SpawnFlowMark()
	{
		if (flowMarkPrefab != null && puddleInstance != null)
		{
			flowMarkInstance = Object.Instantiate(flowMarkPrefab);
			flowMarkInstance.transform.parent = puddleInstance.transform;
			flowMarkInstance.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
			flowMarkInstance.transform.localPosition = Vector3.zero;
		}
	}

	public void Play()
	{
		if (!isPlaying)
		{
			isPlaying = true;
			particles.Play();
			waterLevelAboveCollision = false;
			Renderer[] array = meshRenderers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = true;
			}
			Utils.StartAllFMODEvents(base.gameObject);
		}
	}

	public void Stop()
	{
		if (isPlaying)
		{
			isPlaying = false;
			particles.Stop();
			Renderer[] array = meshRenderers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
			if (puddleInstance != null)
			{
				puddleInstance.GetComponent<ParticleSystem>().Stop();
				Object.Destroy(puddleInstance, 1f);
				puddleInstance = null;
			}
			Utils.StopAllFMODEvents(base.gameObject);
		}
	}

	private void Start()
	{
		InitMaterials();
	}

	private void Update()
	{
		if (isPlaying)
		{
			if (isInBase)
			{
				UpdateMaterials();
			}
			if (rayCaster != null && puddlePrefab != null)
			{
				UpdateWaterImpact();
			}
		}
	}

	private void OnEnable()
	{
		if (isPlaying && !particles.isPlaying)
		{
			particles.Play();
		}
	}

	private void OnDestroy()
	{
		if (materials == null)
		{
			return;
		}
		foreach (Material material in materials)
		{
			Object.Destroy(material);
		}
	}
}
