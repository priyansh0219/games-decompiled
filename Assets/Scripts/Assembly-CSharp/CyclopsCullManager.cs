using System.Collections.Generic;
using UnityEngine;

public class CyclopsCullManager : MonoBehaviour
{
	private enum PlayerCyclopsState
	{
		Unknown = 0,
		Outside = 1,
		LowerFloor = 2,
		UpperFloor = 3,
		Destroyed = 4
	}

	private PlayerCyclopsState playerCyclopsState;

	[SerializeField]
	private SubRoot subRoot;

	[SerializeField]
	private List<GameObject> hudDisplays;

	[SerializeField]
	private Transform floorDivider;

	private List<Canvas> allInteriorCanvases = new List<Canvas>();

	private List<Canvas> canvasesOnLowerFloor = new List<Canvas>();

	private List<Canvas> canvasesOnUpperFloor = new List<Canvas>();

	[SerializeField]
	private List<Canvas> canvasesToExcludeFromCulling = new List<Canvas>();

	private void Start()
	{
		UpdateCanvasLists();
	}

	private void UpdateCanvasLists()
	{
		allInteriorCanvases.Clear();
		canvasesOnLowerFloor.Clear();
		canvasesOnUpperFloor.Clear();
		subRoot.GetComponentsInChildren(includeInactive: true, allInteriorCanvases);
		foreach (Canvas allInteriorCanvase in allInteriorCanvases)
		{
			if (!canvasesToExcludeFromCulling.Contains(allInteriorCanvase))
			{
				if (IsPointOnUpperFloor(allInteriorCanvase.transform.position))
				{
					canvasesOnUpperFloor.Add(allInteriorCanvase);
				}
				else
				{
					canvasesOnLowerFloor.Add(allInteriorCanvase);
				}
			}
		}
	}

	private PlayerCyclopsState DeterminePlayerCyclopsState()
	{
		Player main = Player.main;
		if (main == null)
		{
			return PlayerCyclopsState.Unknown;
		}
		if (subRoot.subDestroyed)
		{
			return PlayerCyclopsState.Destroyed;
		}
		if (main.currentSub != subRoot)
		{
			return PlayerCyclopsState.Outside;
		}
		if (IsPointOnUpperFloor(main.transform.position))
		{
			return PlayerCyclopsState.UpperFloor;
		}
		return PlayerCyclopsState.LowerFloor;
	}

	private bool IsPointOnUpperFloor(Vector3 point)
	{
		return new Plane(floorDivider.up, floorDivider.position).GetSide(point);
	}

	private void Update()
	{
		PlayerCyclopsState playerCyclopsState = DeterminePlayerCyclopsState();
		if (this.playerCyclopsState != playerCyclopsState)
		{
			this.playerCyclopsState = playerCyclopsState;
			bool flag = this.playerCyclopsState == PlayerCyclopsState.Outside || this.playerCyclopsState == PlayerCyclopsState.UpperFloor;
			bool flag2 = this.playerCyclopsState == PlayerCyclopsState.Outside || this.playerCyclopsState == PlayerCyclopsState.LowerFloor;
			if (this.playerCyclopsState == PlayerCyclopsState.Destroyed)
			{
				flag = false;
				flag2 = false;
			}
			SetObjectsActive(hudDisplays, !flag2);
			SetBehavioursEnabled(canvasesOnUpperFloor, !flag2);
			SetBehavioursEnabled(canvasesOnLowerFloor, !flag);
			if (playerCyclopsState == PlayerCyclopsState.Outside)
			{
				UpdateCanvasLists();
			}
		}
	}

	private void SetObjectsActive(List<GameObject> objects, bool active)
	{
		foreach (GameObject @object in objects)
		{
			if (@object != null)
			{
				@object.SetActive(active);
			}
		}
	}

	private void SetBehavioursEnabled<T>(List<T> behaviours, bool enabled) where T : Behaviour
	{
		foreach (T behaviour in behaviours)
		{
			if (behaviour != null)
			{
				behaviour.enabled = enabled;
			}
		}
	}
}
