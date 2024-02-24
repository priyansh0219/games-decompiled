using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UWE;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class Builder
{
	public static readonly float additiveRotationSpeed = 90f;

	public static readonly GameInput.Button buttonRotateCW = GameInput.Button.CyclePrev;

	public static readonly GameInput.Button buttonRotateCCW = GameInput.Button.CycleNext;

	private static readonly Vector3[] checkDirections = new Vector3[4]
	{
		Vector3.up,
		Vector3.down,
		Vector3.left,
		Vector3.right
	};

	private static readonly Color placeColorAllow = new Color(0f, 1f, 0f, 1f);

	private static readonly Color placeColorDeny = new Color(1f, 0f, 0f, 1f);

	private static readonly string denyBuildingTag = "DenyBuilding";

	private static BuildModeInputHandler inputHandler = new BuildModeInputHandler();

	private static Collider[] sColliders = new Collider[2];

	public static float additiveRotation = 0f;

	private static List<Collider> sCollidersList = new List<Collider>();

	private static List<Renderer> sRenderers = new List<Renderer>();

	private static List<MountingBounds> sMounts = new List<MountingBounds>();

	private static GameObject prefab;

	private static float placeMaxDistance;

	private static float placeMinDistance;

	private static float placeDefaultDistance;

	private static TechType constructableTechType;

	private static List<SurfaceType> allowedSurfaceTypes;

	private static bool forceUpright;

	private static bool allowedInSub;

	private static bool allowedInBase;

	private static bool allowedOutside;

	private static bool allowedOnConstructables;

	private static bool allowedUnderwater;

	private static bool rotationEnabled;

	private static bool rotatableBasePiece;

	private static bool alignWithSurface;

	private static bool attachedToBase;

	private static Renderer[] renderers;

	private static GameObject ghostModel;

	private static Vector3 ghostModelPosition;

	private static Quaternion ghostModelRotation;

	private static Vector3 ghostModelScale;

	private static List<OrientedBounds> bounds = new List<OrientedBounds>();

	private static Bounds _aaBounds = default(Bounds);

	private static Vector3 placePosition;

	private static Quaternion placeRotation;

	public static Material originalGhostStructureMaterial;

	private static Material originalBuilderObstacleMaterial;

	private static Material ghostStructureMaterial;

	private static Material builderObstacleMaterial;

	private static LayerMask placeLayerMask;

	private static GameObject placementTarget;

	private const string soundPlace = "event:/tools/builder/place";

	private const string ghostModelMaterialPath = "Materials/ghostmodel.mat";

	private const string builderObstacleMaterialPath = "Materials/builderobstacle.mat";

	private static HashSet<Shader> shadersToExclude;

	private static CommandBuffer obstaclesBuffer;

	private static float startScrollTime;

	private const float scrollCooldownTime = 0.25f;

	public static Bounds aaBounds => _aaBounds;

	public static bool isPlacing => prefab != null;

	public static bool canPlace { get; private set; }

	public static bool canRotate
	{
		get
		{
			if (isPlacing)
			{
				if (!rotationEnabled)
				{
					return rotatableBasePiece;
				}
				return true;
			}
			return false;
		}
	}

	public static bool inputHandlerActive
	{
		get
		{
			if (InputHandlerStack.main != null)
			{
				return InputHandlerStack.main.IsFocused(inputHandler);
			}
			return false;
		}
	}

	public static TechType lastTechType { get; private set; }

	public static int lastRotation { get; private set; }

	public static IEnumerator InitializeAsync()
	{
		placeLayerMask = ~((1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Trigger")));
		obstaclesBuffer = new CommandBuffer();
		obstaclesBuffer.name = "Builder Obstacles";
		MainCamera.camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, obstaclesBuffer);
		if (originalGhostStructureMaterial == null)
		{
			AsyncOperationHandle<Material> resourceRequest2 = AddressablesUtility.LoadAsync<Material>("Materials/ghostmodel.mat");
			yield return resourceRequest2;
			resourceRequest2.LogExceptionIfFailed("Materials/ghostmodel.mat");
			originalGhostStructureMaterial = resourceRequest2.Result;
		}
		ghostStructureMaterial = new Material(originalGhostStructureMaterial);
		if (originalBuilderObstacleMaterial == null)
		{
			AsyncOperationHandle<Material> resourceRequest2 = AddressablesUtility.LoadAsync<Material>("Materials/builderobstacle.mat");
			yield return resourceRequest2;
			resourceRequest2.LogExceptionIfFailed("Materials/builderobstacle.mat");
			originalBuilderObstacleMaterial = resourceRequest2.Result;
		}
		builderObstacleMaterial = new Material(originalBuilderObstacleMaterial);
		if (shadersToExclude == null)
		{
			shadersToExclude = new HashSet<Shader>();
			AsyncOperationHandle<Shader> op = AddressablesUtility.LoadAsync<Shader>("Assets/Waterscape/Clip/WaterClip.shader");
			yield return op;
			if (op.Status == AsyncOperationStatus.Succeeded)
			{
				shadersToExclude.Add(op.Result);
			}
		}
	}

	public static IEnumerator BeginAsync(TechType techType)
	{
		CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
		yield return request;
		if (InputHandlerStack.main.IsDefaultHandlerFocused() && !uGUI_BuilderMenu.IsOpen())
		{
			Begin(techType, request.GetResult());
		}
	}

	public static void Begin(TechType techType, GameObject modulePrefab)
	{
		uGUI_BuilderMenu.Hide();
		if (modulePrefab == null)
		{
			Debug.LogError("Module prefab is null!");
			return;
		}
		if (modulePrefab != prefab)
		{
			End();
		}
		prefab = modulePrefab;
		if (lastTechType != techType)
		{
			lastTechType = techType;
			lastRotation = 0;
		}
		Update();
	}

	public static void ResetLast()
	{
		lastTechType = TechType.None;
	}

	public static void End()
	{
		inputHandler.canHandleInput = false;
		if (ghostModel != null)
		{
			ConstructableBase componentInParent = ghostModel.GetComponentInParent<ConstructableBase>();
			if (componentInParent != null)
			{
				UnityEngine.Object.Destroy(componentInParent.gameObject);
			}
			UnityEngine.Object.Destroy(ghostModel);
		}
		prefab = null;
		ghostModel = null;
		canPlace = false;
		placementTarget = null;
		additiveRotation = 0f;
		obstaclesBuffer.Clear();
	}

	public static void Update()
	{
		obstaclesBuffer.Clear();
		canPlace = false;
		if (!(prefab == null))
		{
			if (CreateGhost())
			{
				inputHandler.canHandleInput = true;
				InputHandlerStack.main.Push(inputHandler);
			}
			canPlace = UpdateAllowed();
			Transform transform = ghostModel.transform;
			transform.position = placePosition + placeRotation * ghostModelPosition;
			transform.rotation = placeRotation * ghostModelRotation;
			transform.localScale = ghostModelScale;
			Color color = (canPlace ? placeColorAllow : placeColorDeny);
			IBuilderGhostModel[] components = ghostModel.GetComponents<IBuilderGhostModel>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].UpdateGhostModelColor(canPlace, ref color);
			}
			ghostStructureMaterial.SetColor(ShaderPropertyID._Tint, color);
		}
	}

	private static bool CreateGhost()
	{
		if (ghostModel != null)
		{
			return false;
		}
		Constructable component = prefab.GetComponent<Constructable>();
		ConstructableBase component2 = prefab.GetComponent<ConstructableBase>();
		constructableTechType = component.techType;
		placeMinDistance = component.placeMinDistance;
		placeMaxDistance = component.placeMaxDistance;
		placeDefaultDistance = component.placeDefaultDistance;
		allowedSurfaceTypes = component.allowedSurfaceTypes;
		forceUpright = component.forceUpright;
		allowedInSub = component.allowedInSub;
		allowedInBase = component.allowedInBase;
		allowedOutside = component.allowedOutside;
		allowedOnConstructables = component.allowedOnConstructables;
		allowedUnderwater = component.allowedUnderwater;
		rotationEnabled = component.rotationEnabled;
		rotatableBasePiece = component2 != null && component2.rotatableBasePiece;
		alignWithSurface = component.alignWithSurface;
		attachedToBase = component.attachedToBase;
		if (component2 != null)
		{
			component2 = UnityEngine.Object.Instantiate(prefab).GetComponent<ConstructableBase>();
			ghostModel = component2.model;
			ghostModel.GetComponent<BaseGhost>().SetupGhost();
			ghostModelPosition = Vector3.zero;
			ghostModelRotation = Quaternion.identity;
			ghostModelScale = Vector3.one;
			renderers = MaterialExtensions.AssignMaterial(ghostModel, ghostStructureMaterial, includeDisabled: true);
			InitBounds(ghostModel);
		}
		else
		{
			ghostModel = UnityEngine.Object.Instantiate(component.model);
			ghostModel.SetActive(value: true);
			Transform component3 = component.GetComponent<Transform>();
			Transform component4 = component.model.GetComponent<Transform>();
			Quaternion quaternion = Quaternion.Inverse(component3.rotation);
			ghostModelPosition = quaternion * (component4.position - component3.position);
			ghostModelRotation = quaternion * component4.rotation;
			ghostModelScale = component4.lossyScale;
			Collider[] componentsInChildren = ghostModel.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				UnityEngine.Object.Destroy(componentsInChildren[i]);
			}
			renderers = MaterialExtensions.AssignMaterial(ghostModel, ghostStructureMaterial, includeDisabled: true);
			string poweredPrefabName = TechData.GetPoweredPrefabName(constructableTechType);
			if (!string.IsNullOrEmpty(poweredPrefabName))
			{
				CoroutineHost.StartCoroutine(CreatePowerPreviewAsync(ghostModel, poweredPrefabName));
			}
			InitBounds(prefab);
		}
		return true;
	}

	private static bool UpdateAllowed()
	{
		SetDefaultPlaceTransform(ref placePosition, ref placeRotation);
		bool geometryChanged = false;
		bool flag = false;
		using (ListPool<GameObject> listPool = Pool<ListPool<GameObject>>.Get())
		{
			List<GameObject> list = listPool.list;
			ConstructableBase componentInParent = ghostModel.GetComponentInParent<ConstructableBase>();
			if (componentInParent != null)
			{
				Transform transform = componentInParent.transform;
				transform.position = placePosition;
				transform.rotation = placeRotation;
				flag = componentInParent.UpdateGhostModel(GetAimTransform(), ghostModel, default(RaycastHit), out geometryChanged, componentInParent);
				placePosition = transform.position;
				placeRotation = transform.rotation;
				if (geometryChanged)
				{
					renderers = MaterialExtensions.AssignMaterial(ghostModel, ghostStructureMaterial, includeDisabled: true);
					InitBounds(ghostModel);
				}
				if (flag)
				{
					GetObstacles(placePosition, placeRotation, bounds, null, list);
				}
			}
			else
			{
				flag = CheckAsSubModule(out var hitCollider);
				CheckSpace(placePosition, placeRotation, bounds, placeLayerMask.value, hitCollider, list);
			}
			flag &= list.Count == 0;
			if (list.Count > 0)
			{
				using (ListPool<Material> listPool2 = Pool<ListPool<Material>>.Get())
				{
					List<Material> list2 = listPool2.list;
					for (int i = 0; i < list.Count; i++)
					{
						sRenderers.Clear();
						GameObject gameObject = list[i];
						if (gameObject.GetComponent<BaseCell>() != null)
						{
							continue;
						}
						gameObject.GetComponentsInChildren(sRenderers);
						for (int j = 0; j < sRenderers.Count; j++)
						{
							Renderer renderer = sRenderers[j];
							if (!renderer.enabled || renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly || renderer is ParticleSystemRenderer)
							{
								continue;
							}
							renderer.GetSharedMaterials(list2);
							int count = list2.Count;
							for (int k = 0; k < count; k++)
							{
								Material material = list2[k];
								if (!(material == null) && !shadersToExclude.Contains(material.shader) && material.renderQueue < 2450 && (!material.HasProperty(ShaderPropertyID._EnableCutOff) || !(material.GetFloat(ShaderPropertyID._EnableCutOff) > 0f)) && !material.IsKeywordEnabled("FX_BUILDING"))
								{
									obstaclesBuffer.DrawRenderer(renderer, builderObstacleMaterial, k);
								}
							}
						}
					}
				}
			}
		}
		if (flag)
		{
			ghostModel.GetComponentsInChildren(sMounts);
			int l = 0;
			for (int count2 = sMounts.Count; l < count2; l++)
			{
				if (!sMounts[l].IsMounted())
				{
					flag = false;
					break;
				}
			}
			sMounts.Clear();
		}
		return flag;
	}

	public static bool TryPlace()
	{
		if (prefab == null || !canPlace)
		{
			return false;
		}
		RuntimeManager.PlayOneShot("event:/tools/builder/place", ghostModel.transform.position);
		ConstructableBase componentInParent = ghostModel.GetComponentInParent<ConstructableBase>();
		if (componentInParent != null)
		{
			BaseGhost component = ghostModel.GetComponent<BaseGhost>();
			component.Place();
			if (component.TargetBase != null)
			{
				componentInParent.transform.SetParent(component.TargetBase.transform, worldPositionStays: true);
			}
			componentInParent.SetState(value: false);
		}
		else
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
			bool flag = false;
			bool flag2 = false;
			SubRoot currentSub = Player.main.GetCurrentSub();
			if (currentSub != null)
			{
				flag = currentSub.isBase;
				flag2 = currentSub.isCyclops;
				gameObject.transform.parent = currentSub.GetModulesRoot();
			}
			else if (placementTarget != null && allowedOutside)
			{
				SubRoot componentInParent2 = placementTarget.GetComponentInParent<SubRoot>();
				if (componentInParent2 != null)
				{
					gameObject.transform.parent = componentInParent2.GetModulesRoot();
				}
			}
			Transform transform = gameObject.transform;
			transform.position = placePosition;
			transform.rotation = placeRotation;
			Constructable componentInParent3 = gameObject.GetComponentInParent<Constructable>();
			componentInParent3.SetState(value: false);
			if (ghostModel != null)
			{
				UnityEngine.Object.Destroy(ghostModel);
			}
			componentInParent3.SetIsInside(flag || flag2);
			SkyEnvironmentChanged.Send(gameObject, currentSub);
			if (flag2)
			{
				componentInParent3.ExcludeFromSubParentRigidbody();
			}
		}
		ghostModel = null;
		prefab = null;
		canPlace = false;
		return true;
	}

	public static void ShowRotationControlsHint()
	{
		ErrorMessage.AddError(Language.main.GetFormat("GhostRotateInputHint", GameInput.FormatButton(buttonRotateCW, allBindingSets: true), GameInput.FormatButton(buttonRotateCCW, allBindingSets: true)));
	}

	private static void InitBounds(GameObject gameObject)
	{
		CacheBounds(gameObject.transform, gameObject, bounds);
		_aaBounds.center = Vector3.zero;
		_aaBounds.extents = Vector3.zero;
		int count = bounds.Count;
		if (count > 0)
		{
			Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			for (int i = 0; i < count; i++)
			{
				OrientedBounds orientedBounds = bounds[i];
				OrientedBounds.MinMaxBounds(OrientedBounds.TransformMatrix(orientedBounds.position, orientedBounds.rotation), Vector3.zero, orientedBounds.extents, ref min, ref max);
			}
			_aaBounds.extents = (max - min) * 0.5f;
			_aaBounds.center = min + _aaBounds.extents;
		}
	}

	public static void OnDrawGizmos()
	{
		Matrix4x4 matrix = Gizmos.matrix;
		Color color = Gizmos.color;
		Gizmos.matrix = OrientedBounds.TransformMatrix(placePosition, placeRotation);
		Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
		Gizmos.DrawCube(aaBounds.center, aaBounds.extents * 2f);
		Gizmos.matrix = matrix;
		Gizmos.color = color;
		OnDrawGizmos();
	}

	public static void CacheBounds(Transform transform, GameObject target, List<OrientedBounds> results, bool append = false)
	{
		if (!append)
		{
			results.Clear();
		}
		if (target == null)
		{
			return;
		}
		using (ListPool<ConstructableBounds> listPool = Pool<ListPool<ConstructableBounds>>.Get())
		{
			List<ConstructableBounds> list = listPool.list;
			target.GetComponentsInChildren(includeInactive: true, list);
			for (int i = 0; i < list.Count; i++)
			{
				ConstructableBounds constructableBounds = list[i];
				OrientedBounds orientedBounds = OrientedBounds.ToWorldBounds(localBounds: constructableBounds.bounds, tr: constructableBounds.transform);
				if (transform != null)
				{
					orientedBounds = OrientedBounds.ToLocalBounds(transform, orientedBounds);
				}
				results.Add(orientedBounds);
			}
		}
		results.Sort();
	}

	public static void GetOverlappedColliders(Vector3 position, Quaternion rotation, Vector3 extents, List<Collider> results)
	{
		GetOverlappedColliders(position, rotation, extents, -1, QueryTriggerInteraction.Collide, results);
	}

	public static void GetOverlappedColliders(Vector3 position, Quaternion rotation, Vector3 extents, int layerMask, QueryTriggerInteraction trigger, List<Collider> results)
	{
		results.Clear();
		int num = UWE.Utils.OverlapBoxIntoSharedBuffer(position, extents, rotation, layerMask, trigger);
		for (int i = 0; i < num; i++)
		{
			Collider collider = UWE.Utils.sharedColliderBuffer[i];
			GameObject gameObject = collider.gameObject;
			if (!collider.isTrigger || gameObject.layer == LayerID.Useable || gameObject.CompareTag(denyBuildingTag))
			{
				results.Add(collider);
			}
		}
	}

	public static void GetRootObjects(List<Collider> colliders, List<GameObject> results)
	{
		for (int i = 0; i < colliders.Count; i++)
		{
			GameObject gameObject = colliders[i].gameObject;
			Transform transform = gameObject.transform;
			while (transform != null)
			{
				if (transform.GetComponent<IBaseModuleGeometry>() != null)
				{
					gameObject = transform.gameObject;
					break;
				}
				if (transform.GetComponent<PrefabIdentifier>() != null)
				{
					gameObject = transform.gameObject;
					break;
				}
				if (transform.GetComponent<SceneObjectIdentifier>() != null)
				{
					gameObject = transform.gameObject;
					break;
				}
				transform = transform.parent;
			}
			if (!results.Contains(gameObject))
			{
				results.Add(gameObject);
			}
		}
	}

	public static void GetOverlappedObjects(Vector3 position, Quaternion rotation, Vector3 extents, List<GameObject> results)
	{
		results.Clear();
		GetOverlappedColliders(position, rotation, extents, sCollidersList);
		GetRootObjects(sCollidersList, results);
		sCollidersList.Clear();
	}

	public static bool CheckSpace(Vector3 position, Quaternion rotation, List<OrientedBounds> localBounds, int layerMask, Collider allowedCollider)
	{
		using (ListPool<GameObject> listPool = Pool<ListPool<GameObject>>.Get())
		{
			List<GameObject> list = listPool.list;
			CheckSpace(position, rotation, localBounds, layerMask, allowedCollider, list);
			return list.Count == 0;
		}
	}

	public static void CheckSpace(Vector3 position, Quaternion rotation, List<OrientedBounds> localBounds, int layerMask, Collider allowedCollider, List<GameObject> obstacles)
	{
		obstacles.Clear();
		if (rotation.IsDistinguishedIdentity())
		{
			rotation = Quaternion.identity;
		}
		for (int i = 0; i < localBounds.Count; i++)
		{
			OrientedBounds orientedBounds = localBounds[i];
			if (orientedBounds.rotation.IsDistinguishedIdentity())
			{
				orientedBounds.rotation = Quaternion.identity;
			}
			orientedBounds.position = position + rotation * orientedBounds.position;
			orientedBounds.rotation = rotation * orientedBounds.rotation;
			if (!(orientedBounds.extents.x > 0f) || !(orientedBounds.extents.y > 0f) || !(orientedBounds.extents.z > 0f))
			{
				continue;
			}
			GetOverlappedColliders(orientedBounds.position, orientedBounds.rotation, orientedBounds.extents, layerMask, QueryTriggerInteraction.Ignore, sCollidersList);
			if (allowedCollider != null)
			{
				for (int num = sCollidersList.Count - 1; num >= 0; num--)
				{
					if (sCollidersList[num] == allowedCollider)
					{
						sCollidersList.RemoveAt(num);
					}
				}
			}
			GetRootObjects(sCollidersList, obstacles);
			sCollidersList.Clear();
		}
	}

	private static GameObject FindObstacle(GameObject go)
	{
		Transform transform = go.transform;
		Base @base = null;
		if ((bool)ghostModel)
		{
			BaseGhost componentInChildren = ghostModel.GetComponentInChildren<BaseGhost>();
			if ((bool)componentInChildren)
			{
				@base = componentInChildren.TargetBase;
			}
		}
		while (transform != null)
		{
			if (transform.gameObject.layer == LayerID.TerrainCollider)
			{
				return transform.gameObject;
			}
			if (transform.gameObject.GetComponent<IObstacle>() != null)
			{
				return transform.gameObject;
			}
			Base component = transform.gameObject.GetComponent<Base>();
			if (component != null && @base != null && component != @base)
			{
				return transform.gameObject;
			}
			transform = transform.parent;
		}
		return null;
	}

	public static void GetObstacles(Vector3 position, Quaternion rotation, List<OrientedBounds> localBounds, Func<Collider, bool> filter, List<GameObject> results)
	{
		results.Clear();
		if (rotation.IsDistinguishedIdentity())
		{
			rotation = Quaternion.identity;
		}
		using (ListPool<GameObject> listPool = Pool<ListPool<GameObject>>.Get())
		{
			List<GameObject> list = listPool.list;
			for (int i = 0; i < localBounds.Count; i++)
			{
				OrientedBounds orientedBounds = localBounds[i];
				if (orientedBounds.rotation.IsDistinguishedIdentity())
				{
					orientedBounds.rotation = Quaternion.identity;
				}
				orientedBounds.position = position + rotation * orientedBounds.position;
				orientedBounds.rotation = rotation * orientedBounds.rotation;
				GetOverlappedColliders(orientedBounds.position, orientedBounds.rotation, orientedBounds.extents, sCollidersList);
				if (filter != null)
				{
					int num = 0;
					for (int num2 = sCollidersList.Count - 1; num2 >= 0; num2--)
					{
						Collider arg = sCollidersList[num2];
						if (filter(arg))
						{
							sCollidersList.RemoveAt(num2);
						}
						num++;
					}
				}
				list.Clear();
				foreach (Collider sColliders in sCollidersList)
				{
					GameObject gameObject = FindObstacle(sColliders.gameObject);
					if (gameObject != null && !list.Contains(gameObject))
					{
						list.Add(gameObject);
					}
				}
				sCollidersList.Clear();
				for (int j = 0; j < list.Count; j++)
				{
					GameObject item = list[j];
					if (!results.Contains(item))
					{
						results.Add(item);
					}
				}
			}
		}
	}

	public static bool CanDestroyObject(GameObject go)
	{
		if (go.GetComponentInParent<Player>() != null)
		{
			return false;
		}
		LargeWorldEntity component = go.GetComponent<LargeWorldEntity>();
		if (component != null && component.cellLevel >= LargeWorldEntity.CellLevel.Global)
		{
			return false;
		}
		if (go.layer == LayerID.TerrainCollider)
		{
			return false;
		}
		if (go.GetComponentInParent<SubRoot>() != null)
		{
			return false;
		}
		if (go.GetComponentInParent<Constructable>() != null)
		{
			return false;
		}
		if (go.GetComponent<IObstacle>() != null)
		{
			return false;
		}
		Pickupable component2 = go.GetComponent<Pickupable>();
		if (component2 != null && component2.attached)
		{
			return false;
		}
		if (go.GetComponent<PlaceTool>() != null)
		{
			return false;
		}
		return true;
	}

	public static bool IsObstacle(Collider collider)
	{
		if (collider != null && collider.gameObject.layer == LayerID.TerrainCollider)
		{
			return true;
		}
		return false;
	}

	public static bool IsObstacle(GameObject go)
	{
		if (go.GetComponent<IObstacle>() != null)
		{
			return true;
		}
		return false;
	}

	public static Transform GetAimTransform()
	{
		return MainCamera.camera.transform;
	}

	public static GameObject GetGhostModel()
	{
		return ghostModel;
	}

	public static float CalculateAdditiveRotationFromInput(float additiveRotation)
	{
		if (GameInput.GetButtonHeld(buttonRotateCW))
		{
			additiveRotation = MathExtensions.RepeatAngle(additiveRotation - GetDeltaTimeForAdditiveRotation() * additiveRotationSpeed);
		}
		else if (GameInput.GetButtonHeld(buttonRotateCCW))
		{
			additiveRotation = MathExtensions.RepeatAngle(additiveRotation + GetDeltaTimeForAdditiveRotation() * additiveRotationSpeed);
		}
		return additiveRotation;
	}

	private static float GetDeltaTimeForAdditiveRotation()
	{
		float result = 0f;
		switch (GameInput.PrimaryDevice)
		{
		case GameInput.Device.Controller:
			result = Time.deltaTime;
			break;
		case GameInput.Device.Keyboard:
		{
			float time = Time.time;
			if (time - startScrollTime > 0.25f)
			{
				startScrollTime = time - Time.deltaTime;
			}
			result = time - startScrollTime;
			break;
		}
		}
		return result;
	}

	private static bool CheckAsSubModule(out Collider hitCollider)
	{
		hitCollider = null;
		placementTarget = null;
		Transform aimTransform = GetAimTransform();
		if (!Physics.Raycast(aimTransform.position, aimTransform.forward, out var hitInfo, placeMaxDistance, placeLayerMask.value, QueryTriggerInteraction.Ignore))
		{
			return false;
		}
		if (!Constructable.CheckFlags(allowedInBase, allowedInSub, allowedOutside, allowedUnderwater, hitInfo.point))
		{
			return false;
		}
		hitCollider = hitInfo.collider;
		placementTarget = hitCollider.gameObject;
		SetPlaceOnSurface(hitInfo, ref placePosition, ref placeRotation);
		if (!CheckTag(hitCollider))
		{
			return false;
		}
		if (!CheckSurfaceType(GetSurfaceType(hitInfo.normal)))
		{
			return false;
		}
		if (!CheckDistance(hitInfo.point, placeMinDistance))
		{
			return false;
		}
		if (!allowedOnConstructables && HasComponent<Constructable>(hitCollider.gameObject))
		{
			return false;
		}
		if (attachedToBase && !HasComponent<Base>(hitCollider.gameObject))
		{
			return false;
		}
		if (!Player.main.IsInSub())
		{
			GameObject entityRoot = UWE.Utils.GetEntityRoot(placementTarget);
			if (!entityRoot)
			{
				entityRoot = placementTarget;
			}
			if (!ValidateOutdoor(entityRoot))
			{
				return false;
			}
		}
		return true;
	}

	public static SurfaceType GetSurfaceType(Vector3 hitNormal)
	{
		if ((double)hitNormal.y < -0.33)
		{
			return SurfaceType.Ceiling;
		}
		if ((double)hitNormal.y < 0.33)
		{
			return SurfaceType.Wall;
		}
		return SurfaceType.Ground;
	}

	private static bool CheckTag(Collider c)
	{
		if (c == null)
		{
			return false;
		}
		GameObject gameObject = c.gameObject;
		if (gameObject == null)
		{
			return false;
		}
		if (gameObject.CompareTag(denyBuildingTag))
		{
			return false;
		}
		return true;
	}

	private static bool CheckSurfaceType(SurfaceType surfaceType)
	{
		return allowedSurfaceTypes.Contains(surfaceType);
	}

	private static bool CheckDistance(Vector3 worldPosition, float minDistance)
	{
		Transform aimTransform = GetAimTransform();
		return (worldPosition - aimTransform.position).magnitude >= minDistance;
	}

	private static bool HasComponent<T>(GameObject go) where T : Component
	{
		return go.GetComponentInParent<T>() != null;
	}

	private static void SetDefaultPlaceTransform(ref Vector3 position, ref Quaternion rotation)
	{
		Transform aimTransform = GetAimTransform();
		position = aimTransform.position + aimTransform.forward * placeDefaultDistance;
		Vector3 forward;
		Vector3 up;
		if (forceUpright)
		{
			forward = -aimTransform.forward;
			forward.y = 0f;
			forward.Normalize();
			up = Vector3.up;
		}
		else
		{
			forward = -aimTransform.forward;
			up = aimTransform.up;
		}
		rotation = Quaternion.LookRotation(forward, up);
		if (rotationEnabled)
		{
			rotation = Quaternion.AngleAxis(additiveRotation, up) * rotation;
		}
	}

	private static void SetPlaceOnSurface(RaycastHit hit, ref Vector3 position, ref Quaternion rotation)
	{
		Transform aimTransform = GetAimTransform();
		Vector3 vector = Vector3.forward;
		Vector3 vector2 = Vector3.up;
		if (forceUpright)
		{
			vector = -aimTransform.forward;
			vector.y = 0f;
			vector.Normalize();
			vector2 = Vector3.up;
		}
		else if (alignWithSurface)
		{
			vector = Vector3.ProjectOnPlane(-aimTransform.forward, hit.normal);
			vector2 = hit.normal;
		}
		else
		{
			switch (GetSurfaceType(hit.normal))
			{
			case SurfaceType.Wall:
				vector = hit.normal;
				vector2 = Vector3.up;
				break;
			case SurfaceType.Ceiling:
				vector = hit.normal;
				vector2 = -aimTransform.forward;
				vector2.y -= Vector3.Dot(vector2, vector);
				vector2.Normalize();
				break;
			case SurfaceType.Ground:
				vector2 = hit.normal;
				vector = -aimTransform.forward;
				vector.y -= Vector3.Dot(vector, vector2);
				vector.Normalize();
				break;
			}
		}
		position = hit.point;
		rotation = Quaternion.LookRotation(vector, vector2);
		if (rotationEnabled)
		{
			rotation = Quaternion.AngleAxis(additiveRotation, vector2) * rotation;
		}
	}

	public static bool ValidateOutdoor(GameObject hitObject)
	{
		Rigidbody component = hitObject.GetComponent<Rigidbody>();
		if ((bool)component && !component.isKinematic)
		{
			return false;
		}
		SubRoot component2 = hitObject.GetComponent<SubRoot>();
		Base component3 = hitObject.GetComponent<Base>();
		if (component2 != null && component3 == null)
		{
			return false;
		}
		if (hitObject.GetComponent<Pickupable>() != null)
		{
			return false;
		}
		LiveMixin component4 = hitObject.GetComponent<LiveMixin>();
		if (component4 != null && component4.destroyOnDeath)
		{
			return false;
		}
		return true;
	}

	private static IEnumerator CreatePowerPreviewAsync(GameObject ghostModel, string poweredPrefabName)
	{
		AsyncOperationHandle<GameObject> loadRequest = AddressablesUtility.LoadAsync<GameObject>(poweredPrefabName);
		yield return loadRequest;
		GameObject result = loadRequest.Result;
		if (result != null)
		{
			PowerRelay component = result.GetComponent<PowerRelay>();
			if (component.powerFX != null && component.powerFX.attachPoint != null)
			{
				PowerFX powerFX = ghostModel.AddComponent<PowerFX>();
				GameObject gameObject = new GameObject();
				gameObject.transform.parent = ghostModel.transform;
				gameObject.transform.localPosition = component.powerFX.attachPoint.localPosition;
				powerFX.attachPoint = gameObject.transform;
			}
			PowerRelay powerRelay = ghostModel.AddComponent<PowerRelay>();
			powerRelay.powerSystemPreviewPrefab = component.powerSystemPreviewPrefab;
			powerRelay.maxOutboundDistance = component.maxOutboundDistance;
			powerRelay.dontConnectToRelays = component.dontConnectToRelays;
			if (component.internalPowerSource != null)
			{
				PowerSource powerSource = ghostModel.AddComponent<PowerSource>();
				powerSource.maxPower = 0f;
				powerRelay.internalPowerSource = powerSource;
			}
		}
		AddressablesUtility.QueueRelease(ref loadRequest);
	}

	public static void ClampRotation(int max)
	{
		if (lastRotation >= max)
		{
			lastRotation = 0;
		}
	}

	public static bool UpdateRotation(int max)
	{
		if (GameInput.GetButtonDown(buttonRotateCW))
		{
			lastRotation = (lastRotation + max - 1) % max;
			return true;
		}
		if (GameInput.GetButtonDown(buttonRotateCCW))
		{
			lastRotation = (lastRotation + 1) % max;
			return true;
		}
		return false;
	}
}
