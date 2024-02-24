using System;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class AnteChamber : MonoBehaviour
{
	public const float scanDuration = 120f;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public float timeScanBegin = -1f;

	[NonSerialized]
	[ProtoMember(3)]
	public bool scanning;

	[AssertNotNull]
	public PrecursorPrisonAntechamber_ParamInterconnect anteChamberAnimation;

	[AssertNotNull]
	public Drillable drillable;

	[AssertNotNull]
	public VFXController beamvfxcontroller;

	[AssertNotNull]
	public FMODAsset pillarUpSound;

	[AssertNotNull]
	public FMODAsset pillarDownSound;

	[AssertNotNull]
	public FMODAsset scanSequenceBeginSound;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter scanSequenceLoopSound;

	[AssertNotNull]
	public FMODAsset scanSequenceEndSound;

	[AssertNotNull]
	public Texture2D _EmissiveTex;

	[AssertNotNull]
	public Transform pillarTr;

	[AssertNotNull]
	public Transform scannerTr;

	private bool playerNearby;

	private bool pillarRaised = true;

	private List<Material> crystalMaterials = new List<Material>();

	private void Start()
	{
		drillable.onDrilled += OnDrilled;
		CacheMaterials();
		if (scanning)
		{
			anteChamberAnimation.StartScanAnimation();
			scanSequenceLoopSound.Play();
			beamvfxcontroller.Play();
			Collider[] componentsInChildren = drillable.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			UpdatePillar();
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < crystalMaterials.Count; i++)
		{
			Material material = crystalMaterials[i];
			if (material != null)
			{
				UnityEngine.Object.Destroy(material);
			}
		}
	}

	private void Update()
	{
		if (scanning)
		{
			UpdateScanAmountMaterials();
		}
	}

	private void CacheMaterials()
	{
		Renderer[] componentsInChildren = drillable.GetComponentsInChildren<Renderer>();
		float value = drillable.transform.position.y - 0.5f;
		float value2 = drillable.transform.position.y + 2.5f;
		foreach (Renderer renderer in componentsInChildren)
		{
			for (int j = 0; j < renderer.materials.Length; j++)
			{
				renderer.materials[j].EnableKeyword("FX_BUILDING");
				renderer.materials[j].SetFloat(ShaderPropertyID._Built, 1f);
				renderer.materials[j].SetFloat(ShaderPropertyID._minYpos, value);
				renderer.materials[j].SetFloat(ShaderPropertyID._maxYpos, value2);
				renderer.materials[j].SetTexture(ShaderPropertyID._EmissiveTex, _EmissiveTex);
				renderer.materials[j].SetColor(ShaderPropertyID._BorderColor, new Color(0.75f, 1f, 0.9f, 1f));
				renderer.materials[j].SetFloat(ShaderPropertyID._Cutoff, 0.42f);
				renderer.materials[j].SetVector(ShaderPropertyID._BuildParams, new Vector4(2f, 0.7f, 3f, -0.25f));
				renderer.materials[j].SetFloat(ShaderPropertyID._NoiseStr, 0.25f);
				renderer.materials[j].SetFloat(ShaderPropertyID._NoiseThickness, 0.49f);
				renderer.materials[j].SetFloat(ShaderPropertyID._BuildLinear, 1f);
				renderer.materials[j].SetFloat(ShaderPropertyID._MyCullVariable, 0f);
				crystalMaterials.Add(renderer.materials[j]);
			}
		}
	}

	public void OnDrilled(Drillable drillable)
	{
		timeScanBegin = DayNightCycle.main.timePassedAsFloat + 5f;
		scanning = true;
		Invoke("OnCrystalScanBegin", 5f);
	}

	private bool IsPlayerControlled(GameObject go)
	{
		GameObject gameObject = UWE.Utils.GetEntityRoot(go);
		if (!gameObject)
		{
			gameObject = go;
		}
		return gameObject.GetComponentInChildren<Player>() != null;
	}

	private void OnRoomEnter(GameObject obj)
	{
		if (IsPlayerControlled(obj))
		{
			anteChamberAnimation.PlayerInRoom(inRoom: true);
		}
	}

	private void OnRoomExit(GameObject obj)
	{
		if (IsPlayerControlled(obj))
		{
			anteChamberAnimation.PlayerInRoom(inRoom: false);
		}
	}

	private void OnNearbyEnter(GameObject obj)
	{
		if (IsPlayerControlled(obj))
		{
			playerNearby = true;
			UpdatePillar();
		}
	}

	private void OnNearbyExit(GameObject obj)
	{
		if (IsPlayerControlled(obj))
		{
			playerNearby = false;
			UpdatePillar();
		}
	}

	private void UpdatePillar()
	{
		bool flag = !playerNearby || scanning;
		if (flag != pillarRaised)
		{
			if (flag)
			{
				Utils.PlayFMODAsset(pillarUpSound, pillarTr);
			}
			else
			{
				Utils.PlayFMODAsset(pillarDownSound, pillarTr);
			}
		}
		anteChamberAnimation.SetPillarRaised(flag);
		pillarRaised = flag;
	}

	public void OnCrystalScanBegin()
	{
		drillable.Restore();
		anteChamberAnimation.StartScanAnimation();
		Utils.PlayFMODAsset(scanSequenceBeginSound, scannerTr);
		scanSequenceLoopSound.Play();
		Collider[] componentsInChildren = drillable.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		UpdateScanAmountMaterials();
		UpdatePillar();
		Invoke("TriggerVFX", 4f);
	}

	public void TriggerVFX()
	{
		beamvfxcontroller.Play();
	}

	public void OnCrystalRestored()
	{
		scanning = false;
		UpdateScanAmountMaterials();
		Utils.PlayFMODAsset(scanSequenceEndSound, scannerTr);
		beamvfxcontroller.StopAndDestroy(1f);
		scanSequenceLoopSound.Stop();
		Collider[] componentsInChildren = drillable.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
		UpdatePillar();
		anteChamberAnimation.StopScanAnimation();
	}

	private void UpdateScanAmountMaterials()
	{
		float value = drillable.transform.position.y - 0.5f;
		float value2 = drillable.transform.position.y + 2.5f;
		float num = 1f;
		if (scanning)
		{
			num = Mathf.Clamp((DayNightCycle.main.timePassedAsFloat - timeScanBegin) / 120f, 0f, 120f);
		}
		for (int i = 0; i < crystalMaterials.Count; i++)
		{
			Material material = crystalMaterials[i];
			if (material != null)
			{
				material.SetFloat(ShaderPropertyID._Built, num);
				material.SetFloat(ShaderPropertyID._minYpos, value);
				material.SetFloat(ShaderPropertyID._maxYpos, value2);
			}
		}
		if (scanning && num >= 1f)
		{
			OnCrystalRestored();
		}
	}
}
