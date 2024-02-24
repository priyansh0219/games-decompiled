using System;
using System.Collections.Generic;
using UWE;
using UnityEngine;

[DisallowMultipleComponent]
public class WaterParkItem : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour
{
	[NonSerialized]
	public Pickupable pickupable;

	[NonSerialized]
	public InfectedMixin infectedMixin;

	protected WaterPark currentWaterPark;

	private Int2 currentWaterParkCell;

	private static List<SkyApplier> sSkyApliers = new List<SkyApplier>();

	public int managedUpdateIndex { get; set; }

	public virtual string GetProfileTag()
	{
		return "WaterParkItem";
	}

	protected virtual void OnAddToWP()
	{
		Rigidbody component = base.gameObject.GetComponent<Rigidbody>();
		if (component != null)
		{
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(component, isKinematic: false);
		}
		currentWaterParkCell = currentWaterPark.GetCell(this);
		base.gameObject.SendMessage("OnAddToWaterPark", this, SendMessageOptions.DontRequireReceiver);
		BehaviourUpdateUtils.Register(this);
	}

	protected virtual void OnRemoveFromWP()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	public virtual void ValidatePosition()
	{
		if (!(currentWaterPark == null))
		{
			Vector3 localPoint = base.transform.localPosition;
			currentWaterPark.EnsureLocalPointIsInside(ref localPoint);
			base.transform.localPosition = localPoint;
		}
	}

	public void SetWaterPark(WaterPark waterPark)
	{
		if (!(waterPark == currentWaterPark))
		{
			WaterPark waterPark2 = currentWaterPark;
			currentWaterPark = waterPark;
			bool flag = waterPark2 != null;
			bool flag2 = currentWaterPark != null;
			if (flag && flag2)
			{
				waterPark2.MoveItemTo(this, currentWaterPark);
				UpdateBaseLighting();
			}
			else if (flag && !flag2)
			{
				waterPark2.RemoveItem(this);
				OnRemoveFromWP();
				DisableBaseLighting();
			}
			else if (!flag && flag2)
			{
				currentWaterPark.AddItem(this);
				OnAddToWP();
				EnableBaseLighting();
			}
			if (flag2)
			{
				ValidatePosition();
			}
		}
	}

	public WaterPark GetWaterPark()
	{
		return currentWaterPark;
	}

	public virtual int GetSize()
	{
		CreatureEgg component = GetComponent<CreatureEgg>();
		if (component != null)
		{
			return component.GetCreatureSize();
		}
		return 0;
	}

	public bool IsInsideWaterPark()
	{
		return currentWaterPark != null;
	}

	public TechType GetTechType()
	{
		if (pickupable != null)
		{
			return pickupable.GetTechType();
		}
		return CraftData.GetTechType(base.gameObject);
	}

	public virtual void ManagedUpdate()
	{
		if (!(currentWaterPark == null))
		{
			Int2 cell = currentWaterPark.GetCell(this);
			if (cell != currentWaterParkCell)
			{
				currentWaterParkCell = cell;
				UpdateBaseLighting();
			}
		}
	}

	private void OnDestroy()
	{
		if (currentWaterPark != null)
		{
			currentWaterPark.RemoveItem(this, unparent: false);
		}
		BehaviourUpdateUtils.Deregister(this);
	}

	private void EnableBaseLighting()
	{
	}

	private void DisableBaseLighting()
	{
	}

	public void UpdateBaseLighting()
	{
	}
}
