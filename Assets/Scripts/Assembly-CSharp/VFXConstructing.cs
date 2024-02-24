using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class VFXConstructing : MonoBehaviour
{
	[NonSerialized]
	public float timeToConstruct = 1f;

	public float delay;

	[NonSerialized]
	[ProtoMember(1)]
	public float constructed = 1000f;

	public bool isDone;

	public GameObject informGameObject;

	public Material ghostMaterial;

	public Texture alphaTexture;

	public Texture alphaDetailTexture;

	public Color wireColor = new Color(0.25f, 0.95f, 1f, 1f);

	public float blurOffset = 0.03f;

	public float alphaScale = 0.06f;

	public float cutOff = 0.88f;

	public float alphaEnd = 50f;

	public float lineWidth = 48f;

	private Material[] materials;

	private Material[][] originalMaterials;

	private Renderer[] renderers;

	private string marmoUberShaderName = "MarmosetUBER";

	private string clipDistanceFieldShaderName = "Custom/DistanceField/ClipDistanceField";

	public Shader[] transparentShaders;

	private VFXOverlayMaterial ghostOverlay;

	public List<Behaviour> disableBehaviours;

	private Rigidbody rBody;

	private bool rBodyKinematic;

	public GameObject surfaceSplashFX;

	public FMODAsset surfaceSplashSound;

	public float surfaceSplashVelocity = 0.1f;

	public float heightOffset;

	public Vector3 localRotation = new Vector3(0f, 0f, 0f);

	[AssertNotNull]
	public FMODAsset constructSound;

	public void StartConstruction()
	{
		constructed = -1f;
	}

	public bool IsConstructed()
	{
		return constructed >= 1f;
	}

	private void Construct()
	{
		ghostMaterial = (Material)Resources.Load("Materials/constructingGhost");
		if (ghostMaterial != null)
		{
			ghostOverlay = base.gameObject.AddComponent<VFXOverlayMaterial>();
			ghostOverlay.ApplyOverlay(ghostMaterial, "VFXConstructing", instantiateMaterial: false);
		}
		for (int i = 0; i < disableBehaviours.Count; i++)
		{
			disableBehaviours[i].enabled = false;
		}
		if ((bool)rBody)
		{
			rBodyKinematic = rBody.isKinematic;
			rBody.isKinematic = true;
		}
		if (constructSound != null)
		{
			FMODUWE.PlayOneShot(constructSound, base.transform.position);
		}
		originalMaterials = new Material[renderers.Length][];
		for (int j = 0; j < renderers.Length; j++)
		{
			if (renderers[j] == null)
			{
				continue;
			}
			originalMaterials[j] = new Material[renderers[j].sharedMaterials.Length];
			materials = renderers[j].materials;
			for (int k = 0; k < materials.Length; k++)
			{
				if (materials[k].shader != null && materials[k].shader.name == clipDistanceFieldShaderName)
				{
					continue;
				}
				originalMaterials[j][k] = new Material(materials[k]);
				if (materials[k].shader != null && materials[k].shader.name == marmoUberShaderName)
				{
					materials[k].EnableKeyword("FX_BUILDING");
					materials[k].SetTexture(ShaderPropertyID._EmissiveTex, alphaDetailTexture);
					materials[k].SetColor(ShaderPropertyID._BorderColor, wireColor);
					materials[k].SetFloat(ShaderPropertyID._Built, 0f);
					materials[k].SetFloat(ShaderPropertyID._Cutoff, 0.42f);
					materials[k].SetVector(ShaderPropertyID._BuildParams, new Vector4(0.035f, 0.07f, 0.08f, -0.12f));
					materials[k].SetFloat(ShaderPropertyID._NoiseStr, 1.9f);
					materials[k].SetFloat(ShaderPropertyID._NoiseThickness, 0.52f);
					materials[k].SetFloat(ShaderPropertyID._BuildLinear, 0f);
					materials[k].SetFloat(ShaderPropertyID._MyCullVariable, 0f);
					continue;
				}
				bool flag = false;
				for (int l = 0; l < transparentShaders.Length; l++)
				{
					if (materials[k].shader.name == transparentShaders[l].name)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					materials[k].EnableKeyword("FX_CONSTRUCTING_ALPHA");
				}
			}
		}
		Shader.SetGlobalFloat(ShaderPropertyID._SubConstructProgress, constructed);
		constructed = 0f;
	}

	private void UpdateConstruct()
	{
		constructed += Time.deltaTime / timeToConstruct;
		Shader.SetGlobalFloat(ShaderPropertyID._SubConstructProgress, constructed);
	}

	private void WakeUpSubmarine()
	{
		for (int i = 0; i < disableBehaviours.Count; i++)
		{
			disableBehaviours[i].enabled = true;
		}
		if ((bool)rBody)
		{
			rBody.isKinematic = rBodyKinematic;
		}
		BroadcastMessage("SubConstructionComplete", null, SendMessageOptions.DontRequireReceiver);
		SendMessageUpwards("SubConstructionComplete", null, SendMessageOptions.DontRequireReceiver);
	}

	private void EndConstruct()
	{
		constructed = 1000f;
		base.enabled = false;
	}

	private void PlaySplashFX()
	{
		if (surfaceSplashFX != null)
		{
			GameObject obj = Utils.SpawnPrefabAt(surfaceSplashFX, base.transform, base.transform.position);
			obj.transform.localEulerAngles = localRotation;
			obj.SendMessage("Play", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void PlaySplashSoundEffect()
	{
		if (surfaceSplashSound != null)
		{
			Utils.PlayFMODAsset(surfaceSplashSound, base.transform);
		}
	}

	private void ApplySplashImpulse()
	{
		if ((bool)GetComponent<Rigidbody>())
		{
			GetComponent<Rigidbody>().AddForce(GetComponent<Rigidbody>().velocity * (surfaceSplashVelocity - 1f), ForceMode.VelocityChange);
		}
	}

	private void RevertMaterials()
	{
		if (ghostOverlay != null)
		{
			ghostOverlay.RemoveOverlay();
		}
		if (originalMaterials != null)
		{
			for (int i = 0; i < renderers.Length; i++)
			{
				if (renderers[i] == null)
				{
					continue;
				}
				for (int j = 0; j < renderers[i].materials.Length; j++)
				{
					if (i < originalMaterials.Length && j < originalMaterials[i].Length && !(originalMaterials[i][j] == null) && !(renderers[i].materials[j] == null))
					{
						renderers[i].materials[j].CopyPropertiesFromMaterial(originalMaterials[i][j]);
						UnityEngine.Object.Destroy(originalMaterials[i][j]);
					}
				}
			}
		}
		Shader.SetGlobalFloat(ShaderPropertyID._SubConstructProgress, 0f);
	}

	private void Awake()
	{
		Regenerate();
	}

	public void Regenerate()
	{
		renderers = GetComponentsInChildren<Renderer>();
		rBody = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		delay -= Time.deltaTime;
		if (constructed < 0f)
		{
			Construct();
		}
		else
		{
			if (!(delay < 0f))
			{
				return;
			}
			if (constructed < 1f)
			{
				UpdateConstruct();
			}
			else
			{
				if (!(constructed < 100f))
				{
					return;
				}
				if (!isDone)
				{
					RevertMaterials();
					WakeUpSubmarine();
					isDone = true;
					if (informGameObject != null)
					{
						informGameObject.BroadcastMessage("OnConstructionDone", base.gameObject, SendMessageOptions.RequireReceiver);
						informGameObject = null;
					}
				}
				if (base.transform.position.y < heightOffset)
				{
					PlaySplashFX();
					PlaySplashSoundEffect();
					ApplySplashImpulse();
					EndConstruct();
				}
				else if (constructed > 3f)
				{
					EndConstruct();
				}
			}
		}
	}

	private void OnDestroy()
	{
		RevertMaterials();
	}
}
