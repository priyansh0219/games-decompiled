using System;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class OxygenPipe : PlayerTool, IProtoEventListener, IPipeConnection
{
	private const int currentVersion = 1;

	public static List<OxygenPipe> pipes = new List<OxygenPipe>();

	[AssertNotNull]
	public VFXController fxControl;

	[AssertNotNull]
	public GameObject oxygenProvider;

	[AssertNotNull]
	public Transform stretchedPart;

	[AssertNotNull]
	public Transform topSection;

	[AssertNotNull]
	public Transform bottomSection;

	[AssertNotNull]
	public Transform endCap;

	[AssertNotNull]
	public GameObject pipePrefab;

	[AssertNotNull]
	public Rigidbody rigidBody;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter bubblesSound;

	[AssertNotNull]
	public GameObject craftModel;

	private const float maxLength = 10f;

	private static OxygenPipe ghostModel;

	private List<string> children = new List<string>();

	private bool _isGhost;

	private bool gettingOxygen;

	[NonSerialized]
	[ProtoMember(1)]
	public string parentPipeUID;

	[NonSerialized]
	[ProtoMember(2)]
	public string rootPipeUID;

	[NonSerialized]
	[ProtoMember(3)]
	public Vector3 parentPosition;

	[NonSerialized]
	[ProtoMember(4)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(5, OverwriteList = true)]
	public string[] childPipeUID;

	public bool isGhost
	{
		get
		{
			return _isGhost;
		}
		set
		{
			_isGhost = value;
			oxygenProvider.SetActive(!_isGhost);
		}
	}

	private void Start()
	{
		pickupable.pickedUpEvent.AddHandler(base.gameObject, OnPickedUp);
		pickupable.droppedEvent.AddHandler(base.gameObject, OnDropped);
		PrepareGhostModel();
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = !pickupable.attached;
		}
		UpdatePipe();
		craftModel.SetActive(value: false);
		pipes.Add(this);
	}

	protected override void OnDestroy()
	{
		pipes.Remove(this);
		base.OnDestroy();
	}

	public void UpdateOxygen()
	{
		UpdatePipe();
	}

	public bool GetProvidesOxygen()
	{
		if (children.Count == 0 && GetRoot() != null)
		{
			return GetRoot().GetProvidesOxygen();
		}
		return false;
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public Vector3 GetAttachPoint()
	{
		return base.transform.position;
	}

	public void SetParent(IPipeConnection parent)
	{
		IPipeConnection parent2 = GetParent();
		parentPipeUID = null;
		rootPipeUID = null;
		if (parent2 != null && parent != parent2 && !isGhost)
		{
			parent2.RemoveChild(this);
		}
		if (parent != null)
		{
			parentPipeUID = parent.GetGameObject().GetComponent<UniqueIdentifier>().Id;
			rootPipeUID = ((parent.GetRoot() != null) ? parent.GetRoot().GetGameObject().GetComponent<UniqueIdentifier>()
				.Id : null);
			parentPosition = parent.GetAttachPoint();
			if (!isGhost)
			{
				parent.AddChild(this);
			}
		}
		UpdatePipe();
	}

	public IPipeConnection GetParent()
	{
		if (!string.IsNullOrEmpty(parentPipeUID) && UniqueIdentifier.TryGetIdentifier(parentPipeUID, out var uid))
		{
			return uid.GetComponent<IPipeConnection>();
		}
		return null;
	}

	public void AddChild(IPipeConnection child)
	{
		children.Add(child.GetGameObject().GetComponent<UniqueIdentifier>().Id);
		UpdatePipe();
	}

	public void RemoveChild(IPipeConnection child)
	{
		children.Remove(child.GetGameObject().GetComponent<UniqueIdentifier>().Id);
		UpdatePipe();
	}

	public void SetRoot(IPipeConnection root)
	{
		if (root == null)
		{
			rootPipeUID = null;
		}
		else
		{
			rootPipeUID = root.GetGameObject().GetComponent<UniqueIdentifier>().Id;
		}
		UpdatePipe();
	}

	public IPipeConnection GetRoot()
	{
		if (!string.IsNullOrEmpty(rootPipeUID) && UniqueIdentifier.TryGetIdentifier(rootPipeUID, out var uid))
		{
			return uid.GetComponent<IPipeConnection>();
		}
		return null;
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		childPipeUID = children.ToArray();
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (childPipeUID != null)
		{
			children = new List<string>(childPipeUID);
		}
		else
		{
			children = new List<string>();
		}
	}

	public void UpdatePipe()
	{
		bool flag = !string.IsNullOrEmpty(parentPipeUID);
		endCap.gameObject.SetActive(flag && children.Count == 0);
		oxygenProvider.SetActive(GetProvidesOxygen());
		bottomSection.gameObject.SetActive(flag);
		stretchedPart.gameObject.SetActive(flag);
		bool flag2 = fxControl.emitters[0].instanceGO != null;
		if (GetProvidesOxygen())
		{
			if (!flag2)
			{
				fxControl.Play();
			}
			bubblesSound.Play();
		}
		else
		{
			bubblesSound.Stop();
			if (flag2)
			{
				fxControl.StopAndDestroy(0f);
			}
		}
		if (flag)
		{
			Vector3 vector = Vector3.Normalize(parentPosition - base.transform.position);
			float magnitude = (parentPosition - base.transform.position).magnitude;
			topSection.rotation = Quaternion.LookRotation(vector, Vector3.up);
			endCap.rotation = topSection.rotation;
			bottomSection.rotation = Quaternion.LookRotation(vector, Vector3.up);
			bottomSection.position = parentPosition;
			stretchedPart.position = topSection.position + vector;
			Vector3 localScale = stretchedPart.localScale;
			localScale.z = magnitude - 2f;
			stretchedPart.localScale = localScale;
			stretchedPart.rotation = topSection.rotation;
		}
		else
		{
			topSection.localRotation = Quaternion.identity;
		}
		pickupable.isPickupable = children.Count == 0 && !isGhost;
	}

	public override void OnDraw(Player p)
	{
		base.OnDraw(p);
		UpdatePipe();
	}

	public override void OnHolster()
	{
		base.OnHolster();
		PrepareGhostModel();
		ghostModel.gameObject.SetActive(value: false);
	}

	public override bool OnRightHandDown()
	{
		if (Player.main.IsBleederAttached())
		{
			return true;
		}
		return ghostModel.PlaceInWorld(this);
	}

	private void OnPickedUp(Pickupable p)
	{
		SetParent(null);
		SetRoot(null);
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		bubblesSound.Stop();
	}

	private void OnDropped(Pickupable p)
	{
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
	}

	private void UpdateAsPlaced()
	{
		bool flag = gettingOxygen;
		gettingOxygen = GetProvidesOxygen();
		if (gettingOxygen != flag)
		{
			UpdatePipe();
		}
	}

	public void Update()
	{
		if (usingPlayer != null)
		{
			UpdatePlacement();
		}
		else
		{
			UpdateAsPlaced();
		}
	}

	private void PrepareGhostModel()
	{
		if (ghostModel == null)
		{
			ghostModel = UnityEngine.Object.Instantiate(pipePrefab).GetComponent<OxygenPipe>();
			ghostModel.isGhost = true;
			ghostModel.name = "PipeGhostModel";
			ghostModel.pickupable.isPickupable = false;
			UnityEngine.Object.Destroy(ghostModel.GetComponent<LargeWorldEntity>());
			UnityEngine.Object.Destroy(ghostModel.GetComponent<WorldForces>());
			UnityEngine.Object.Destroy(ghostModel.GetComponent<Rigidbody>());
			UnityEngine.Object.Destroy(ghostModel.GetComponent<PrefabIdentifier>());
			FPModel component = ghostModel.GetComponent<FPModel>();
			component.propModel.SetActive(value: true);
			component.viewModel.SetActive(value: false);
			UnityEngine.Object.Destroy(component);
			Collider[] componentsInChildren = ghostModel.GetComponentsInChildren<Collider>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				UnityEngine.Object.Destroy(componentsInChildren[i]);
			}
			ghostModel.gameObject.SetActive(value: false);
		}
	}

	private void UpdatePlacement()
	{
		PrepareGhostModel();
		ghostModel.gameObject.SetActive(value: true);
		ghostModel.transform.position = MainCamera.camera.transform.forward * 2f + MainCamera.camera.transform.position;
		ghostModel.UpdateGhostAttach();
		IPipeConnection parent = ghostModel.GetParent();
		if (parent != null)
		{
			ghostModel.gameObject.SetActive(value: true);
			ghostModel.transform.position = GetPipePosition(parent);
			ghostModel.UpdatePipe();
		}
		else
		{
			ghostModel.gameObject.SetActive(value: false);
		}
	}

	private Vector3 GetPipePosition(IPipeConnection parent)
	{
		Vector3 value = parent.GetAttachPoint() - ghostModel.transform.position;
		float num = Mathf.Clamp(value.magnitude, 2.1f, 10f);
		return parent.GetGameObject().transform.position - Vector3.Normalize(value) * num;
	}

	private bool IsInSight(IPipeConnection parent)
	{
		bool result = true;
		Vector3 value = GetPipePosition(parent) - parent.GetAttachPoint();
		int num = UWE.Utils.RaycastIntoSharedBuffer(new Ray(parent.GetAttachPoint(), Vector3.Normalize(value)), value.magnitude + 0.3f);
		for (int i = 0; i < num; i++)
		{
			if (UWE.Utils.GetEntityRoot(UWE.Utils.sharedHitBuffer[i].collider.gameObject) != parent.GetGameObject())
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void UpdateGhostAttach()
	{
		IPipeConnection parent = null;
		float num = 1000f;
		int num2 = UWE.Utils.OverlapSphereIntoSharedBuffer(base.transform.position, 11f);
		for (int i = 0; i < num2; i++)
		{
			GameObject entityRoot = UWE.Utils.GetEntityRoot(UWE.Utils.sharedColliderBuffer[i].gameObject);
			if (entityRoot == null)
			{
				continue;
			}
			IPipeConnection component = entityRoot.GetComponent<IPipeConnection>();
			if (component != null && component.GetRoot() != null && IsInSight(component))
			{
				float magnitude = (entityRoot.transform.position - base.transform.position).magnitude;
				if (magnitude < num)
				{
					parent = component;
					num = magnitude;
				}
			}
		}
		SetParent(parent);
	}

	private bool PlaceInWorld(OxygenPipe pipe)
	{
		if (ghostModel.GetParent() != null)
		{
			pipe.pickupable.Drop(ghostModel.transform.position);
			pipe.SetParent(ghostModel.GetParent());
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(pipe.rigidBody, isKinematic: true);
			ghostModel.gameObject.SetActive(value: false);
			return true;
		}
		return false;
	}
}
