using System.Collections.Generic;
using UnityEngine;

public class EnterExitHelper : MonoBehaviour
{
	public bool isForEscapePod;

	public bool isForWaterPark;

	private static List<WaterParkGeometry> sWaterParkGeometry = new List<WaterParkGeometry>();

	public void CinematicEnter(CinematicModeEventData eventData)
	{
		Enter(base.gameObject, eventData.player, isForEscapePod, setCurrentSubForced: true);
	}

	public void CinematicExit(CinematicModeEventData eventData)
	{
		Exit(base.transform, eventData.player, isForEscapePod, isForWaterPark);
	}

	public static void Enter(GameObject gameObject, Player player, bool isForEscapePod, bool setCurrentSubForced = false)
	{
		if (!(player == null))
		{
			if (isForEscapePod)
			{
				player.escapePod.Update(newValue: true);
				player.currentEscapePod = Utils.FindAncestorWithComponent<EscapePod>(gameObject);
			}
			SubRoot subRoot = Utils.FindAncestorWithComponent<SubRoot>(gameObject);
			if ((bool)subRoot)
			{
				player.SetCurrentSub(subRoot, setCurrentSubForced);
			}
			player.currentWaterPark = null;
		}
	}

	public static void Exit(Transform transform, Player player, bool isForEscapePod, bool isForWaterPark)
	{
		if (!(player == null))
		{
			if (isForEscapePod)
			{
				player.escapePod.Update(newValue: false);
				player.currentEscapePod = null;
			}
			if (isForWaterPark)
			{
				player.currentWaterPark = GetWaterPark(transform, player);
			}
			else
			{
				player.SetCurrentSub(null);
			}
		}
	}

	private static WaterPark GetWaterPark(Transform transform, Player player)
	{
		if (transform.parent == null)
		{
			return null;
		}
		transform.parent.GetComponentsInChildren(includeInactive: false, sWaterParkGeometry);
		WaterPark result = null;
		foreach (WaterParkGeometry item in sWaterParkGeometry)
		{
			WaterPark module = item.GetModule();
			if (module != null && module.IsPointInside(player.transform.position))
			{
				result = module;
				break;
			}
		}
		sWaterParkGeometry.Clear();
		return result;
	}
}
