using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ProtoBuf;
using Story;
using UnityEngine;

[ProtoContract]
[ProtoInclude(3100, typeof(ConstructableBase))]
[ProtoInclude(3200, typeof(BasePowerDistributor))]
[ProtoInclude(3201, typeof(BaseSpotLight))]
[ProtoInclude(3202, typeof(BasePipeConnector))]
public class Constructable : HandTarget, IProtoEventListener, IConstructable, IObstacle
{
	private const int currentVersion = 3;

	private const float constructInterval = 1f;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 3;

	[NonSerialized]
	[ProtoMember(2)]
	public bool _constructed = true;

	[NonSerialized]
	[ProtoMember(5)]
	public float constructedAmount = 1f;

	[ProtoMember(6)]
	public TechType techType;

	[NonSerialized]
	[ProtoMember(7)]
	public bool isNew = true;

	[NonSerialized]
	[ProtoMember(8)]
	public bool isInside = true;

	[AssertNotNull]
	public GameObject model;

	public GameObject builtBoxFX;

	[AssertNotNull]
	public Material ghostMaterial;

	public bool controlModelState = true;

	[AssertNotNull(AssertNotNullAttribute.Options.AllowEmptyCollection)]
	public MonoBehaviour[] controlledBehaviours;

	public bool allowedOnWall;

	public bool allowedOnGround = true;

	public bool allowedOnCeiling;

	public bool deconstructionAllowed = true;

	public bool allowedInSub = true;

	public bool allowedInBase = true;

	public bool allowedOutside;

	public bool allowedOnConstructables;

	public bool allowedUnderwater = true;

	public bool alignWithSurface;

	public bool forceUpright;

	public bool rotationEnabled;

	public bool attachedToBase;

	public float placeMaxDistance = 5f;

	public float placeMinDistance = 1.2f;

	public float placeDefaultDistance = 2f;

	public VFXSurfaceTypes surfaceType;

	public Texture _EmissiveTex;

	public Texture _NoiseTex;

	protected Transform tr;

	protected GameObject modelCopy;

	private List<TechType> resourceMap;

	private VFXOverlayMaterial ghostOverlay;

	private Shader marmoUberShader;

	private int defaultLayer;

	private int viewModelLayer;

	private List<RendererMaterialsStorage> renderersMaterials = new List<RendererMaterialsStorage>();

	private bool deconstructCoroutineRunning;

	public bool constructed => _constructed;

	public float amount => constructedAmount;

	public List<SurfaceType> allowedSurfaceTypes
	{
		get
		{
			List<SurfaceType> list = new List<SurfaceType>();
			if (allowedOnWall)
			{
				list.Add(SurfaceType.Wall);
			}
			if (allowedOnGround)
			{
				list.Add(SurfaceType.Ground);
			}
			if (allowedOnCeiling)
			{
				list.Add(SurfaceType.Ceiling);
			}
			return list;
		}
	}

	public virtual void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public virtual void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (version < 3 && !_constructed)
		{
			constructedAmount = 0.5f;
		}
		InitResourceMap();
		bool flag = _constructed;
		_constructed = !flag;
		SetState(flag, setAmount: false);
	}

	public override void Awake()
	{
		base.Awake();
		tr = GetComponent<Transform>();
		defaultLayer = LayerMask.NameToLayer("Default");
		viewModelLayer = LayerMask.NameToLayer("Viewmodel");
		marmoUberShader = ShaderManager.preloadedShaders.marmosetUBER;
	}

	private void OnDestroy()
	{
		for (int i = 0; i < renderersMaterials.Count; i++)
		{
			RendererMaterialsStorageManager.TryDestroyCopiedAndRestoreInitialMaterials(renderersMaterials[i], this);
		}
		renderersMaterials.Clear();
	}

	protected virtual void Start()
	{
		InitResourceMap();
	}

	public bool DeconstructionAllowed(out string reason)
	{
		bool result = true;
		reason = null;
		using (ListPool<IConstructable> listPool = Pool<ListPool<IConstructable>>.Get())
		{
			List<IConstructable> list = listPool.list;
			GetComponents(list);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (!list[i].CanDeconstruct(out reason))
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	public virtual bool SetState(bool value, bool setAmount = true)
	{
		if (_constructed == value)
		{
			return false;
		}
		_constructed = value;
		MonoBehaviour[] components = base.gameObject.GetComponents<MonoBehaviour>();
		int i = 0;
		for (int num = components.Length; i < num; i++)
		{
			MonoBehaviour monoBehaviour = components[i];
			if (!(monoBehaviour == null) && !(monoBehaviour == this) && !(monoBehaviour.GetType() == typeof(SubModuleHandler)) && !(monoBehaviour.GetType() == typeof(LargeWorldEntity)))
			{
				components[i].enabled = _constructed;
			}
		}
		if (controlledBehaviours != null)
		{
			int j = 0;
			for (int num2 = controlledBehaviours.Length; j < num2; j++)
			{
				MonoBehaviour monoBehaviour2 = controlledBehaviours[j];
				if (!(monoBehaviour2 == null) && !(monoBehaviour2 == this))
				{
					controlledBehaviours[j].enabled = _constructed;
				}
			}
		}
		if (setAmount)
		{
			constructedAmount = (_constructed ? 1f : 0f);
		}
		if (_constructed)
		{
			DestroyModelCopy();
			NotifyConstructedChanged(constructed: true);
			SetupRenderers();
			ItemGoalTracker.OnConstruct(techType);
		}
		else
		{
			InitializeModelCopy();
			SetupRenderers();
			NotifyConstructedChanged(constructed: false);
		}
		return true;
	}

	public void SetIsInside(bool inside)
	{
		isInside = inside;
	}

	public bool IsInside()
	{
		return isInside;
	}

	public virtual bool UpdateGhostModel(Transform aimTransform, GameObject ghostModel, RaycastHit hit, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
	{
		geometryChanged = false;
		return CheckFlags(allowedInBase, allowedInSub, allowedOutside, allowedUnderwater, hit.point);
	}

	public virtual bool Construct()
	{
		if (_constructed)
		{
			return false;
		}
		int count = resourceMap.Count;
		int resourceID = GetResourceID();
		constructedAmount += Time.deltaTime / ((float)count * GetConstructInterval());
		constructedAmount = Mathf.Clamp01(constructedAmount);
		int resourceID2 = GetResourceID();
		if (resourceID2 != resourceID)
		{
			TechType destroyTechType = resourceMap[resourceID2 - 1];
			if (!Inventory.main.DestroyItem(destroyTechType) && GameModeUtils.RequiresIngredients())
			{
				constructedAmount = (float)resourceID / (float)count;
				return false;
			}
		}
		UpdateMaterial();
		if (constructedAmount >= 1f)
		{
			SetState(value: true);
		}
		return true;
	}

	public IEnumerator DeconstructAsync(IOut<bool> result, IOut<string> reason)
	{
		if (_constructed || deconstructCoroutineRunning)
		{
			result.Set(deconstructCoroutineRunning);
			reason.Set(null);
			yield break;
		}
		deconstructCoroutineRunning = true;
		int resourceCount = resourceMap.Count;
		int resourceID = GetResourceID();
		float num = 2f;
		constructedAmount -= Time.deltaTime * num / ((float)resourceCount * GetConstructInterval());
		constructedAmount = Mathf.Clamp01(constructedAmount);
		int nextResID = GetResourceID();
		if (nextResID != resourceID && GameModeUtils.RequiresIngredients())
		{
			TechType techType = resourceMap[nextResID];
			bool resourceCanBePickedUp = Inventory.main.HasRoomFor(techType);
			if (resourceCanBePickedUp)
			{
				TaskResult<GameObject> prefabResult = new TaskResult<GameObject>();
				yield return CraftData.InstantiateFromPrefabAsync(techType, prefabResult);
				Pickupable component = prefabResult.Get().GetComponent<Pickupable>();
				if (!Inventory.main.Pickup(component))
				{
					resourceCanBePickedUp = false;
					UnityEngine.Object.Destroy(component.gameObject);
				}
			}
			if (!resourceCanBePickedUp)
			{
				constructedAmount = ((float)nextResID + 0.001f) / (float)resourceCount;
				result.Set(value: false);
				reason.Set(Language.main.Get("InventoryFull"));
				deconstructCoroutineRunning = false;
				yield break;
			}
		}
		UpdateMaterial();
		yield return ProgressDeconstruction();
		if (constructedAmount <= 0f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		result.Set(value: true);
		reason.Set(null);
		deconstructCoroutineRunning = false;
	}

	protected virtual IEnumerator ProgressDeconstruction()
	{
		yield return null;
	}

	private static float GetConstructInterval()
	{
		if (NoCostConsoleCommand.main.fastBuildCheat)
		{
			return 0.01f;
		}
		if (!GameModeUtils.RequiresIngredients())
		{
			return 0.2f;
		}
		return 1f;
	}

	public virtual Vector3 GetRandomConstructionPoint()
	{
		try
		{
			Vector3 result = base.transform.position;
			Renderer renderer;
			using (ListPool<Renderer> listPool = Pool<ListPool<Renderer>>.Get())
			{
				List<Renderer> list = listPool.list;
				GetComponentsInChildren(includeInactive: false, list);
				for (int num = list.Count - 1; num >= 0; num--)
				{
					renderer = list[num];
					if (!renderer.enabled || (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)))
					{
						list.RemoveAt(num);
					}
				}
				if (list.Count == 0)
				{
					return result;
				}
				renderer = list[UnityEngine.Random.Range(0, list.Count)];
			}
			Mesh mesh = null;
			if (renderer is MeshRenderer)
			{
				MeshFilter component = renderer.GetComponent<MeshFilter>();
				if (component != null)
				{
					mesh = component.sharedMesh;
				}
			}
			else if (renderer is SkinnedMeshRenderer)
			{
				mesh = (renderer as SkinnedMeshRenderer).sharedMesh;
			}
			if (mesh != null)
			{
				Bounds bounds = mesh.bounds;
				Matrix4x4 localToWorldMatrix = renderer.transform.localToWorldMatrix;
				Vector3 extents = bounds.extents;
				result = localToWorldMatrix.MultiplyPoint3x4(bounds.center + new Vector3(UnityEngine.Random.Range(0f - extents.x, extents.x), UnityEngine.Random.Range(0f - extents.y, extents.y), UnityEngine.Random.Range(0f - extents.z, extents.z)));
			}
			return result;
		}
		finally
		{
		}
	}

	private int GetResourceID()
	{
		return Mathf.CeilToInt(constructedAmount * (float)resourceMap.Count);
	}

	protected void InitResourceMap()
	{
		if (resourceMap != null)
		{
			return;
		}
		resourceMap = new List<TechType>();
		if (techType == TechType.None)
		{
			return;
		}
		ReadOnlyCollection<Ingredient> ingredients = TechData.GetIngredients(techType);
		if (ingredients == null)
		{
			return;
		}
		for (int i = 0; i < ingredients.Count; i++)
		{
			Ingredient ingredient = ingredients[i];
			for (int j = 0; j < ingredient.amount; j++)
			{
				resourceMap.Add(ingredient.techType);
			}
		}
	}

	public virtual Dictionary<TechType, int> GetRemainingResources()
	{
		InitResourceMap();
		int resourceID = GetResourceID();
		Dictionary<TechType, int> dictionary = new Dictionary<TechType, int>(TechTypeExtensions.sTechTypeComparer);
		for (int i = resourceID; i < resourceMap.Count; i++)
		{
			TechType key = resourceMap[i];
			int value = 0;
			if (dictionary.TryGetValue(key, out value))
			{
				dictionary[key] = value + 1;
			}
			else
			{
				dictionary.Add(key, 1);
			}
		}
		return dictionary;
	}

	protected virtual bool InitializeModelCopy()
	{
		if (modelCopy != null)
		{
			return false;
		}
		modelCopy = UnityEngine.Object.Instantiate(model);
		modelCopy.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
		modelCopy.SetActive(value: true);
		ReplaceMaterials(modelCopy);
		UpdateMaterial();
		return true;
	}

	protected void ReplaceMaterials(GameObject rootObject)
	{
		if (ghostMaterial != null)
		{
			ghostOverlay = base.gameObject.AddComponent<VFXOverlayMaterial>();
			ghostOverlay.ApplyOverlay(ghostMaterial, "ConstructableGhost", instantiateMaterial: false);
		}
		using (ListPool<Renderer> listPool = Pool<ListPool<Renderer>>.Get())
		{
			List<Renderer> list = listPool.list;
			rootObject.GetComponentsInChildren(includeInactive: true, list);
			for (int i = 0; i < list.Count; i++)
			{
				Renderer renderer = list[i];
				RendererMaterialsStorage rendererMaterialsStorage = null;
				Material[] sharedMaterials = renderer.sharedMaterials;
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					Material material = sharedMaterials[j];
					if (!(material == null) && !(material.shader == null) && material.HasProperty(ShaderPropertyID._BuildLinear))
					{
						if (rendererMaterialsStorage == null)
						{
							rendererMaterialsStorage = RendererMaterialsStorageManager.GetRendererMaterialsStorage(renderer, this);
						}
						Material orCreateCopiedMaterial = rendererMaterialsStorage.GetOrCreateCopiedMaterial(j, applyNowIfCreated: false);
						orCreateCopiedMaterial.EnableKeyword("FX_BUILDING");
						orCreateCopiedMaterial.SetTexture(ShaderPropertyID._EmissiveTex, _EmissiveTex);
						orCreateCopiedMaterial.SetFloat(ShaderPropertyID._Cutoff, 0.42f);
						orCreateCopiedMaterial.SetColor(ShaderPropertyID._BorderColor, new Color(0.7f, 0.7f, 1f, 1f));
						orCreateCopiedMaterial.SetFloat(ShaderPropertyID._Built, 0f);
						orCreateCopiedMaterial.SetVector(ShaderPropertyID._BuildParams, new Vector4(0.1f, 0.25f, 0.2f, -0.2f));
						orCreateCopiedMaterial.SetFloat(ShaderPropertyID._NoiseStr, 1.9f);
						orCreateCopiedMaterial.SetFloat(ShaderPropertyID._NoiseThickness, 0.48f);
						orCreateCopiedMaterial.SetFloat(ShaderPropertyID._BuildLinear, 0f);
					}
				}
				if (rendererMaterialsStorage != null)
				{
					rendererMaterialsStorage.ApplyCurrentMaterials();
					renderersMaterials.Add(rendererMaterialsStorage);
				}
			}
		}
		Shader.SetGlobalFloat(ShaderPropertyID._SubConstructProgress, 0f);
	}

	protected void RestoreInitialMaterials(GameObject rootObject)
	{
		using (ListPool<Renderer> listPool = Pool<ListPool<Renderer>>.Get())
		{
			List<Renderer> list = listPool.list;
			rootObject.GetComponentsInChildren(includeInactive: true, list);
			for (int i = 0; i < list.Count; i++)
			{
				RendererMaterialsStorage rendererMaterialsStorage = RendererMaterialsStorageManager.GetRendererMaterialsStorage(list[i], this);
				if (rendererMaterialsStorage != null)
				{
					RendererMaterialsStorageManager.TryDestroyCopiedAndRestoreInitialMaterials(rendererMaterialsStorage, this);
					renderersMaterials.Remove(rendererMaterialsStorage);
				}
			}
		}
	}

	protected virtual void DestroyModelCopy()
	{
		if (ghostOverlay != null)
		{
			ghostOverlay.RemoveOverlay();
		}
		if (modelCopy != null)
		{
			modelCopy.AddComponent<BuiltEffectController>();
			modelCopy = null;
		}
		if (builtBoxFX != null)
		{
			Vector3 center;
			Vector3 extents;
			using (ListPool<Renderer> listPool = Pool<ListPool<Renderer>>.Get())
			{
				List<Renderer> list = listPool.list;
				GetComponentsInChildren(includeInactive: false, list);
				OrientedBounds.EncapsulateRenderers(base.transform.worldToLocalMatrix, list, out center, out extents);
			}
			if (extents.x > 0f && extents.y > 0f && extents.z > 0f)
			{
				Transform obj = UnityEngine.Object.Instantiate(builtBoxFX).transform;
				OrientedBounds orientedBounds = OrientedBounds.ToWorldBounds(localBounds: new OrientedBounds(center, Quaternion.identity, extents), tr: base.transform);
				obj.position = orientedBounds.position;
				obj.rotation = orientedBounds.rotation;
				obj.localScale = orientedBounds.size;
			}
		}
	}

	protected void UpdateMaterial()
	{
		if (modelCopy == null)
		{
			return;
		}
		for (int i = 0; i < renderersMaterials.Count; i++)
		{
			RendererMaterialsStorage rendererMaterialsStorage = renderersMaterials[i];
			for (int j = 0; j < rendererMaterialsStorage.Count; j++)
			{
				if (rendererMaterialsStorage.TryGetCopiedMaterial(j, out var material))
				{
					material.SetFloat(ShaderPropertyID._Built, constructedAmount);
				}
			}
		}
	}

	protected void SetupRenderers()
	{
		Utils.SetLayerRecursively(newLayer: (!(GetComponentInParent<SubRoot>() != null)) ? defaultLayer : viewModelLayer, obj: base.gameObject);
	}

	protected void NotifyConstructedChanged(bool constructed)
	{
		using (ListPool<IConstructable> listPool = Pool<ListPool<IConstructable>>.Get())
		{
			List<IConstructable> list = listPool.list;
			GetComponents(list);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				list[i].OnConstructedChanged(constructed);
			}
		}
	}

	public void ExcludeFromSubParentRigidbody()
	{
		using (ListPool<Collider> listPool = Pool<ListPool<Collider>>.Get())
		{
			List<Collider> list = listPool.list;
			GetComponentsInChildren(includeInactive: true, list);
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				Collider collider = list[i];
				if (!collider.isTrigger && LayerID.IsMaskContainsLayer(LayerID.DefaultCollisionMask, collider.gameObject.layer))
				{
					collider.gameObject.layer = LayerID.SubRigidbodyExclude;
					flag = true;
				}
			}
			if (flag)
			{
				base.gameObject.EnsureComponent<Rigidbody>().isKinematic = true;
			}
		}
	}

	public bool IsDeconstructionObstacle()
	{
		return true;
	}

	public bool CanDeconstruct(out string reason)
	{
		reason = null;
		if (!deconstructionAllowed)
		{
			return false;
		}
		StorageContainer[] componentsInChildren = GetComponentsInChildren<StorageContainer>();
		foreach (StorageContainer storageContainer in componentsInChildren)
		{
			if (storageContainer.preventDeconstructionIfNotEmpty && !storageContainer.IsEmpty())
			{
				reason = Language.main.Get("DeconstructNonEmptyStorageContainerError");
				return false;
			}
		}
		return true;
	}

	public virtual void OnConstructedChanged(bool constructed)
	{
		if (constructed && isNew)
		{
			isNew = false;
			CrafterLogic.NotifyCraftEnd(base.gameObject, techType);
		}
		if (controlModelState)
		{
			if (model != null)
			{
				model.SetActive(constructed);
				return;
			}
			Debug.LogErrorFormat(this, "controlModelState checkbox is set, but model is not assigned for Constructable component on '{0}'", base.gameObject);
		}
	}

	public static bool CheckFlags(bool allowedInBase, bool allowedInSub, bool allowedOutside, bool allowedUnderwater, Vector3 hitPoint)
	{
		SubRoot currentSub = Player.main.GetCurrentSub();
		if (currentSub != null)
		{
			if (Player.main.currentWaterPark != null)
			{
				return false;
			}
			if (currentSub.isBase)
			{
				if (!allowedInBase)
				{
					return false;
				}
			}
			else if (!allowedInSub)
			{
				return false;
			}
		}
		else
		{
			if (!allowedUnderwater && hitPoint.y < 0f)
			{
				return false;
			}
			if (!allowedOutside)
			{
				return false;
			}
		}
		return true;
	}
}
