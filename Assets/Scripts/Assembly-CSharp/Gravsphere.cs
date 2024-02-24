using System;
using System.Collections;
using System.Collections.Generic;
using UWE;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Gravsphere : DropTool
{
	[Serializable]
	public struct GravPad
	{
		public Transform transform;

		public string paramName;

		public float value;

		public bool active;

		[AssertNotNull]
		public Renderer vfxHaloRenderer;

		public void UpdateVFX(Color color)
		{
			vfxHaloRenderer.enabled = color != Color.clear;
			for (int i = 0; i < vfxHaloRenderer.materials.Length; i++)
			{
				vfxHaloRenderer.materials[i].SetColor(ShaderPropertyID._Color, color);
			}
		}

		public void DestroyMats()
		{
			for (int i = 0; i < vfxHaloRenderer.materials.Length; i++)
			{
				UnityEngine.Object.Destroy(vfxHaloRenderer.materials[i]);
			}
		}
	}

	public GravPad[] pads;

	[AssertNotNull]
	public GameObject vfxPrefab;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter activeLoop;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public Rigidbody rigidBody;

	[AssertNotNull]
	public Collider trigger;

	private List<Rigidbody> inRange = new List<Rigidbody>();

	private List<Rigidbody> attractableList = new List<Rigidbody>();

	private Dictionary<int, VFXElectricLine> effects = new Dictionary<int, VFXElectricLine>();

	private HashSet<int> removeList = new HashSet<int>();

	private bool[] newPadStatus;

	private int currentIndex;

	private int lastIndex;

	private const int maxAttached = 12;

	private const float gravitateForce = 0.7f;

	private const float maxForce = 15f;

	private static readonly TechType[] allowedTechTypes = new TechType[122]
	{
		TechType.Biter,
		TechType.Bladderfish,
		TechType.Bleeder,
		TechType.Boomerang,
		TechType.CaveCrawler,
		TechType.Eyeye,
		TechType.GarryFish,
		TechType.GhostRayBlue,
		TechType.GhostRayRed,
		TechType.HoleFish,
		TechType.Hoopfish,
		TechType.Hoverfish,
		TechType.Jellyray,
		TechType.Jumper,
		TechType.LavaLarva,
		TechType.Oculus,
		TechType.Peeper,
		TechType.PrecursorDroid,
		TechType.Reginald,
		TechType.Skyray,
		TechType.Spadefish,
		TechType.RabbitRay,
		TechType.Mesmer,
		TechType.CrabSquid,
		TechType.Stalker,
		TechType.LavaLizard,
		TechType.GasPod,
		TechType.Quartz,
		TechType.ScrapMetal,
		TechType.FiberMesh,
		TechType.LimestoneChunk,
		TechType.Copper,
		TechType.Lead,
		TechType.Salt,
		TechType.MercuryOre,
		TechType.CalciumChunk,
		TechType.Glass,
		TechType.Titanium,
		TechType.Silicone,
		TechType.Gold,
		TechType.Magnesium,
		TechType.Sulphur,
		TechType.Lodestone,
		TechType.Bleach,
		TechType.Silver,
		TechType.BatteryAcidOld,
		TechType.TitaniumIngot,
		TechType.SandstoneChunk,
		TechType.CrashPowder,
		TechType.Diamond,
		TechType.BasaltChunk,
		TechType.ShaleChunk,
		TechType.ObsidianChunk,
		TechType.Lithium,
		TechType.PlasteelIngot,
		TechType.EnameledGlass,
		TechType.PowerCell,
		TechType.ComputerChip,
		TechType.Fiber,
		TechType.Enamel,
		TechType.AcidOld,
		TechType.VesselOld,
		TechType.CombustibleOld,
		TechType.OpalGem,
		TechType.Uranium,
		TechType.AluminumOxide,
		TechType.HydrochloricAcid,
		TechType.Magnetite,
		TechType.AminoAcids,
		TechType.Polyaniline,
		TechType.AramidFibers,
		TechType.Graphene,
		TechType.Aerogel,
		TechType.Nanowires,
		TechType.Benzene,
		TechType.Lubricant,
		TechType.UraniniteCrystal,
		TechType.ReactorRod,
		TechType.DepletedReactorRod,
		TechType.PrecursorIonCrystal,
		TechType.Kyanite,
		TechType.Nickel,
		TechType.KelpForestEgg,
		TechType.GrassyPlateausEgg,
		TechType.GrandReefsEgg,
		TechType.MushroomForestEgg,
		TechType.KooshZoneEgg,
		TechType.TwistyBridgesEgg,
		TechType.LavaZoneEgg,
		TechType.StalkerEgg,
		TechType.StalkerEggUndiscovered,
		TechType.ReefbackEgg,
		TechType.ReefbackEggUndiscovered,
		TechType.SpadefishEgg,
		TechType.SpadefishEggUndiscovered,
		TechType.RabbitrayEgg,
		TechType.RabbitrayEggUndiscovered,
		TechType.MesmerEgg,
		TechType.MesmerEggUndiscovered,
		TechType.JumperEgg,
		TechType.JumperEggUndiscovered,
		TechType.SandsharkEgg,
		TechType.SandsharkEggUndiscovered,
		TechType.JellyrayEgg,
		TechType.JellyrayEggUndiscovered,
		TechType.BonesharkEgg,
		TechType.BonesharkEggUndiscovered,
		TechType.CrabsnakeEgg,
		TechType.CrabsnakeEggUndiscovered,
		TechType.ShockerEgg,
		TechType.ShockerEggUndiscovered,
		TechType.GasopodEgg,
		TechType.GasopodEggUndiscovered,
		TechType.CrashEgg,
		TechType.CrashEggUndiscovered,
		TechType.CrabsquidEgg,
		TechType.CrabsquidEggUndiscovered,
		TechType.CutefishEgg,
		TechType.CutefishEggUndiscovered,
		TechType.LavaLizardEgg,
		TechType.LavaLizardEggUndiscovered,
		TechType.GenericEgg
	};

	private void Start()
	{
		pickupable.pickedUpEvent.AddHandler(base.gameObject, OnPickedUp);
		pickupable.droppedEvent.AddHandler(base.gameObject, OnDropped);
		int num = pads.Length;
		newPadStatus = new bool[num];
		trigger.enabled = !pickupable.attached;
		animator.SetBool("deployed", !pickupable.attached);
	}

	public void OnPickedUp(Pickupable p)
	{
		DeactivatePads();
		animator.SetBool("deployed", value: false);
		trigger.enabled = false;
	}

	public void OnDropped(Pickupable p)
	{
		animator.SetBool("deployed", value: true);
		trigger.enabled = false;
		StartCoroutine(ActivateAsync());
	}

	private IEnumerator ActivateAsync()
	{
		yield return new WaitForSeconds(2f);
		trigger.enabled = true;
	}

	private void DeactivatePads()
	{
		for (int i = 0; i < newPadStatus.Length; i++)
		{
			pads[i].active = false;
			pads[i].value = 0f;
			newPadStatus[i] = false;
		}
	}

	private bool IsValidTarget(GameObject obj)
	{
		bool result = false;
		TechType techType = CraftData.GetTechType(obj);
		Pickupable component = obj.GetComponent<Pickupable>();
		if (!component || !component.attached)
		{
			for (int i = 0; i < allowedTechTypes.Length; i++)
			{
				if (allowedTechTypes[i] == techType)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private void OnTriggerEnter(Collider collider)
	{
		Rigidbody componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Rigidbody>(collider.gameObject);
		if (IsValidTarget(collider.gameObject) && componentInHierarchy != null && !attractableList.Contains(componentInHierarchy) && attractableList.Count < 12)
		{
			if (!componentInHierarchy.isKinematic)
			{
				AddAttractable(componentInHierarchy);
			}
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(componentInHierarchy, isKinematic: false);
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		Rigidbody rigidbody = Utils.FindAncestorWithComponent<Rigidbody>(collider.gameObject);
		if (rigidbody != null)
		{
			int num = attractableList.IndexOf(rigidbody);
			if (num != -1)
			{
				removeList.Add(num);
			}
		}
	}

	private void ApplyGravitation()
	{
		if (!base.gameObject.GetComponent<Pickupable>().attached)
		{
			for (int i = 0; i < attractableList.Count; i++)
			{
				Rigidbody rigidbody = attractableList[i];
				if ((bool)rigidbody)
				{
					float magnitude = (base.gameObject.transform.position - rigidbody.transform.position).magnitude;
					Vector3 vector = Vector3.Normalize(base.gameObject.transform.position - rigidbody.transform.position);
					float num = Mathf.Clamp(0.7f * Mathf.Pow(magnitude, 1.5f), 0.7f, 15f);
					rigidbody.AddForce(vector * num, ForceMode.Acceleration);
					if (rigidbody.mass > 15f)
					{
						rigidBody.AddForce(-vector * (num * rigidbody.mass * 0.003f), ForceMode.Acceleration);
					}
				}
				else
				{
					removeList.Add(i);
				}
			}
			if (currentIndex == 0 && lastIndex != currentIndex)
			{
				for (int j = 0; j < newPadStatus.Length; j++)
				{
					pads[j].active = newPadStatus[j];
					newPadStatus[j] = false;
				}
			}
			lastIndex = currentIndex;
			if (currentIndex >= attractableList.Count)
			{
				currentIndex = 0;
				return;
			}
			Rigidbody rigidbody2 = attractableList[currentIndex];
			if ((bool)rigidbody2)
			{
				ActivateClosestPad(rigidbody2.transform.position);
				if (!IsValidTarget(rigidbody2.gameObject))
				{
					removeList.Add(currentIndex);
				}
				else
				{
					currentIndex++;
				}
			}
			else
			{
				removeList.Add(currentIndex);
			}
		}
		else if (attractableList.Count > 0)
		{
			ClearAll();
		}
	}

	private void ActivateClosestPad(Vector3 position)
	{
		int num = 0;
		float num2 = float.PositiveInfinity;
		for (int i = 0; i < pads.Length; i++)
		{
			float sqrMagnitude = (pads[i].transform.position - position).sqrMagnitude;
			if (sqrMagnitude < num2)
			{
				num2 = sqrMagnitude;
				num = i;
			}
		}
		newPadStatus[num] = true;
	}

	private void FixedUpdate()
	{
		ApplyGravitation();
		CleanAttractablesAndFX();
	}

	private void OnDisable()
	{
		ClearAll();
	}

	private void ClearAll()
	{
		for (int i = 0; i < attractableList.Count; i++)
		{
			removeList.Add(i);
		}
		CleanAttractablesAndFX();
		activeLoop.Stop();
	}

	private void CleanAttractablesAndFX()
	{
		List<Rigidbody> list = new List<Rigidbody>();
		Dictionary<int, VFXElectricLine> dictionary = new Dictionary<int, VFXElectricLine>();
		int num = 0;
		for (int i = 0; i < attractableList.Count; i++)
		{
			if (attractableList[i] == null || removeList.Contains(i))
			{
				DestroyEffect(i);
				continue;
			}
			list.Add(attractableList[i]);
			dictionary.Add(num, effects[i]);
			num++;
		}
		attractableList = list;
		effects = dictionary;
		removeList.Clear();
	}

	private void AddAttractable(Rigidbody r)
	{
		GameObject obj = UnityEngine.Object.Instantiate(vfxPrefab, Vector3.zero, Quaternion.identity);
		obj.transform.parent = r.gameObject.transform;
		obj.transform.localPosition = Vector3.zero;
		VFXElectricLine component = obj.GetComponent<VFXElectricLine>();
		effects.Add(attractableList.Count, component);
		attractableList.Add(r);
	}

	private void DestroyEffect(int index)
	{
		VFXElectricLine vFXElectricLine = effects[index];
		if (vFXElectricLine != null && vFXElectricLine.gameObject != null)
		{
			UnityEngine.Object.Destroy(vFXElectricLine.gameObject);
		}
	}

	private void UpdateEffectPositions()
	{
		foreach (KeyValuePair<int, VFXElectricLine> effect in effects)
		{
			if (attractableList[effect.Key] == null)
			{
				removeList.Add(effect.Key);
			}
			else if (effect.Value != null)
			{
				effect.Value.origin = base.transform.position;
				effect.Value.target = attractableList[effect.Key].transform.position;
			}
		}
	}

	private void UpdatePads()
	{
		for (int i = 0; i < pads.Length; i++)
		{
			float num = (pads[i].active ? 1 : (-1));
			pads[i].value = Mathf.Clamp01(Time.deltaTime * num + pads[i].value);
			animator.SetFloat(pads[i].paramName, pads[i].value);
			Color color = Color.Lerp(Color.clear, Color.white, pads[i].value);
			pads[i].UpdateVFX(color);
		}
	}

	private void Update()
	{
		UpdateEffectPositions();
		UpdatePads();
		if (effects.Count > 0)
		{
			activeLoop.Play();
		}
		else
		{
			activeLoop.Stop();
		}
	}

	protected override void OnDestroy()
	{
		for (int i = 0; i < pads.Length; i++)
		{
			pads[i].DestroyMats();
		}
		base.OnDestroy();
	}
}
