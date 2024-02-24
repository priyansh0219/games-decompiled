using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public sealed class ConstructableBase : Constructable
{
	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int protoVersion = 1;

	[ProtoMember(2)]
	public TechType faceLinkedModuleType;

	[ProtoMember(3)]
	public Vector3 faceLinkedModulePosition = Vector3.zero;

	[ProtoMember(4)]
	public Base.Face? moduleFace;

	public bool rotatableBasePiece;

	private List<Renderer> ghostRenderers;

	private bool ghostRenderersVisible = true;

	private Base ghostBase;

	public override void Awake()
	{
		base.Awake();
		InitGhostRenderers();
	}

	public void LinkModule(Base.Face? moduleFace)
	{
		this.moduleFace = moduleFace;
	}

	private void InitGhostRenderers()
	{
		if (ghostRenderers == null)
		{
			ghostRenderers = new List<Renderer>();
			if (model != null)
			{
				model.GetComponentsInChildren(ghostRenderers);
			}
		}
	}

	private void SetGhostVisible(bool visible)
	{
		if (ghostRenderersVisible == visible)
		{
			return;
		}
		ghostRenderersVisible = visible;
		InitGhostRenderers();
		int i = 0;
		for (int count = ghostRenderers.Count; i < count; i++)
		{
			Renderer renderer = ghostRenderers[i];
			if (renderer != null)
			{
				renderer.enabled = ghostRenderersVisible;
			}
		}
	}

	private void SetModuleConstructAmount(Base targetBase, float amount)
	{
		if (!moduleFace.HasValue)
		{
			return;
		}
		if (targetBase != null)
		{
			Base.Face value = moduleFace.Value;
			value.cell = targetBase.GetAnchor() + value.cell;
			IBaseModule module = targetBase.GetModule(value);
			if (module != null)
			{
				module.constructed = amount;
				return;
			}
			Debug.LogErrorFormat(this, "IBaseModule not found in targetBase at cell [{0}]", moduleFace.Value);
		}
		else
		{
			Debug.LogError("targetBase is null", this);
		}
	}

	public void OnGlobalEntitiesLoaded()
	{
		StartCoroutine(ReplaceMaterialsAsync());
	}

	private IEnumerator ReplaceMaterialsAsync()
	{
		yield return null;
		ReplaceMaterials(model);
		UpdateMaterial();
	}

	public void OnGhostBasePostRebuildGeometry(Base b)
	{
		ReplaceMaterials(model);
		UpdateMaterial();
	}

	public override bool UpdateGhostModel(Transform aimTransform, GameObject ghostModel, RaycastHit hit, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
	{
		if (ghostModel == null)
		{
			geometryChanged = false;
			return false;
		}
		bool positionFound;
		bool result = ghostModel.GetComponent<BaseGhost>().UpdatePlacement(aimTransform, placeMaxDistance, out positionFound, out geometryChanged, ghostModelParentConstructableBase);
		SetGhostVisible(!positionFound);
		return result;
	}

	public override bool Construct()
	{
		BaseGhost component = model.GetComponent<BaseGhost>();
		Base targetBase = ((component != null) ? component.TargetBase : null);
		bool result = base.Construct();
		SetModuleConstructAmount(targetBase, base.amount);
		return result;
	}

	protected override IEnumerator ProgressDeconstruction()
	{
		if (model != null)
		{
			BaseGhost component = model.GetComponent<BaseGhost>();
			Base @base = ((component != null) ? component.TargetBase : null);
			SetModuleConstructAmount(@base, base.amount);
			if (@base != null && constructedAmount <= 0f)
			{
				@base.DestroyIfEmpty(component);
			}
		}
		else
		{
			yield return null;
		}
	}

	public override bool SetState(bool value, bool setAmount = true)
	{
		if (_constructed != value && value)
		{
			using (ListPool<ConstructableBounds> listPool = Pool<ListPool<ConstructableBounds>>.Get())
			{
				List<ConstructableBounds> list = listPool.list;
				GetComponentsInChildren(includeInactive: true, list);
				using (ListPool<GameObject> listPool2 = Pool<ListPool<GameObject>>.Get())
				{
					List<GameObject> list2 = listPool2.list;
					for (int i = 0; i < list.Count; i++)
					{
						ConstructableBounds constructableBounds = list[i];
						OrientedBounds orientedBounds = OrientedBounds.ToWorldBounds(constructableBounds.transform, constructableBounds.bounds);
						list2.Clear();
						Builder.GetOverlappedObjects(orientedBounds.position, orientedBounds.rotation, orientedBounds.extents, list2);
						int j = 0;
						for (int count = list2.Count; j < count; j++)
						{
							GameObject gameObject = list2[j];
							if (Builder.CanDestroyObject(gameObject))
							{
								UnityEngine.Object.Destroy(gameObject);
							}
						}
					}
				}
			}
			model.GetComponent<BaseGhost>().Finish();
		}
		bool result = base.SetState(value, setAmount);
		if (_constructed)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return result;
		}
		UpdateMaterial();
		return result;
	}

	public override void OnConstructedChanged(bool constructed)
	{
	}

	protected override bool InitializeModelCopy()
	{
		ReplaceMaterials(model);
		modelCopy = model;
		return true;
	}

	protected override void DestroyModelCopy()
	{
		BaseGhost component = modelCopy.GetComponent<BaseGhost>();
		Base @base = ((component != null) ? component.TargetBase : null);
		if (@base != null)
		{
			@base.DeregisterBaseGhost(component);
		}
		modelCopy.transform.SetParent(null, worldPositionStays: true);
		RestoreInitialMaterials(modelCopy);
		base.DestroyModelCopy();
		model = null;
	}

	public override void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		base.OnProtoDeserialize(serializer);
		SetGhostVisible(visible: false);
	}
}
