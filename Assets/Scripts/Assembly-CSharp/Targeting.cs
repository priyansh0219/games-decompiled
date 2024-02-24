using System.Collections.Generic;
using UWE;
using UnityEngine;

public class Targeting
{
	public delegate bool FilterRaycast(RaycastHit hit);

	private static readonly float[] standardRadiuses = new float[2] { 0.15f, 0.3f };

	private static readonly float[] gamepadRadiuses = new float[2] { 0.15f, 0.5f };

	private static List<Transform> ignoreList = new List<Transform>();

	public static readonly HashSet<TechType> techTypeIgnoreList = new HashSet<TechType>();

	public static void AddToIgnoreList(GameObject ignoreGameObject)
	{
		if (ignoreGameObject != null)
		{
			AddToIgnoreList(ignoreGameObject.transform);
		}
	}

	public static void AddToIgnoreList(Transform ignoreTransform)
	{
		if (ignoreTransform != null && !ignoreList.Contains(ignoreTransform))
		{
			ignoreList.Add(ignoreTransform);
		}
	}

	public static bool GetRoot(GameObject candidate, out TechType techType, out GameObject gameObject)
	{
		techType = TechType.None;
		gameObject = null;
		if (candidate == null)
		{
			return false;
		}
		GameObject go;
		TechType techType2 = CraftData.GetTechType(candidate, out go);
		if (techType2 == TechType.None || go == null)
		{
			Pickupable componentInParent = candidate.GetComponentInParent<Pickupable>();
			if (componentInParent != null)
			{
				techType2 = componentInParent.GetTechType();
				go = componentInParent.gameObject;
			}
		}
		if (techType2 != 0 && go != null)
		{
			techType = techType2;
			gameObject = go;
			return true;
		}
		return false;
	}

	public static bool GetTarget(GameObject ignoreObj, float maxDistance, out GameObject result, out float distance)
	{
		if (ignoreObj != null)
		{
			AddToIgnoreList(ignoreObj.transform);
		}
		return GetTarget(maxDistance, out result, out distance);
	}

	public static bool GetTarget(float maxDistance, out GameObject result, out float distance)
	{
		bool flag = false;
		Transform transform = MainCamera.camera.transform;
		Vector3 position = transform.position;
		Vector3 forward = transform.forward;
		Ray ray = new Ray(position, forward);
		int layerMask = ~((1 << LayerID.Trigger) | (1 << LayerID.OnlyVehicle));
		QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide;
		int numHits = UWE.Utils.RaycastIntoSharedBuffer(ray, maxDistance, layerMask, queryTriggerInteraction);
		DebugTargetConsoleCommand.radius = -1f;
		if (Filter(UWE.Utils.sharedHitBuffer, numHits, out var resultHit))
		{
			flag = true;
		}
		if (!flag)
		{
			float[] array = (GameInput.IsPrimaryDeviceGamepad() ? gamepadRadiuses : standardRadiuses);
			for (int i = 0; i < array.Length; i++)
			{
				float num = (DebugTargetConsoleCommand.radius = array[i]);
				ray.origin = position + forward * num;
				numHits = UWE.Utils.SpherecastIntoSharedBuffer(ray, num, maxDistance, layerMask, queryTriggerInteraction);
				if (Filter(UWE.Utils.sharedHitBuffer, numHits, out resultHit))
				{
					flag = true;
					break;
				}
			}
		}
		Reset();
		DebugTargetConsoleCommand.Stop();
		result = ((resultHit.collider != null) ? resultHit.collider.gameObject : null);
		distance = resultHit.distance;
		return flag;
	}

	private static bool Filter(RaycastHit[] hits, int numHits, out RaycastHit resultHit)
	{
		resultHit = default(RaycastHit);
		for (int i = 0; i < numHits; i++)
		{
			RaycastHit raycastHit = hits[i];
			Collider collider = raycastHit.collider;
			if (collider == null)
			{
				continue;
			}
			GameObject gameObject = collider.gameObject;
			Transform transform = collider.transform;
			if (gameObject == null || transform == null)
			{
				continue;
			}
			int layer = gameObject.layer;
			Transform transform2 = null;
			for (int j = 0; j < ignoreList.Count; j++)
			{
				Transform transform3 = ignoreList[j];
				if (transform.IsAncestorOf(transform3))
				{
					transform2 = transform3;
					break;
				}
			}
			if (transform2 != null)
			{
				DebugTargetConsoleCommand.Log(DebugTargetConsoleCommand.Reason.AncestorOfIgnoredParent, raycastHit);
				continue;
			}
			if (collider.isTrigger)
			{
				if (layer != LayerID.Useable)
				{
					DebugTargetConsoleCommand.Log(DebugTargetConsoleCommand.Reason.TriggerNotUseable, raycastHit);
					continue;
				}
			}
			else if (layer == LayerID.NotUseable)
			{
				DebugTargetConsoleCommand.Log(DebugTargetConsoleCommand.Reason.ColliderNotUseable, raycastHit);
				continue;
			}
			if (!(resultHit.collider == null) && !(raycastHit.distance < resultHit.distance))
			{
				continue;
			}
			if (techTypeIgnoreList.Count > 0)
			{
				TechType techType = CraftData.GetTechType(gameObject);
				if (techTypeIgnoreList.Contains(techType))
				{
					DebugTargetConsoleCommand.Log(DebugTargetConsoleCommand.Reason.FromTechTypeExcludeList, raycastHit);
					continue;
				}
			}
			resultHit = raycastHit;
		}
		if (resultHit.collider != null)
		{
			DebugTargetConsoleCommand.Log(DebugTargetConsoleCommand.Reason.Accept, resultHit);
			return true;
		}
		return false;
	}

	private static void Reset()
	{
		ignoreList.Clear();
	}
}
