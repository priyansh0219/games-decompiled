using UnityEngine;
using UnityEngine.EventSystems;

public static class CursorManager
{
	private static RaycastResult lastRaycast = default(RaycastResult);

	private static int frameUpdated = -1;

	public static void SetRaycastResult(RaycastResult raycastResult)
	{
		lastRaycast = raycastResult;
	}

	private static void TryUpdateRaycast()
	{
		if (frameUpdated != Time.frameCount)
		{
			if (ManagedUpdate.main.lastQueue > ManagedUpdate.Queue.PreCanvasCanvasScaler)
			{
				frameUpdated = Time.frameCount;
			}
			GamepadInputModule current = GamepadInputModule.current;
			if (current != null && current.isControlling && current.TryEmulateRaycast(out var raycastResult))
			{
				lastRaycast = raycastResult;
			}
		}
	}

	public static bool GetPointerInfo(ref RectTransform rt, out Vector2 localPosition, out Vector3 worldPosition, out Transform aimingTransform, out float alpha)
	{
		TryUpdateRaycast();
		localPosition = Vector3.zero;
		worldPosition = Vector3.zero;
		aimingTransform = null;
		alpha = 1f;
		GameObject gameObject = lastRaycast.gameObject;
		if (gameObject == null)
		{
			return false;
		}
		alpha = RectTransformExtensions.CalculateNestedAlpha(gameObject.transform);
		Canvas componentInParent = gameObject.GetComponentInParent<Canvas>();
		if (componentInParent == null)
		{
			return false;
		}
		BaseRaycaster module = lastRaycast.module;
		if (module == null)
		{
			return false;
		}
		Camera eventCamera = module.eventCamera;
		if (eventCamera == null)
		{
			return false;
		}
		if (rt == null)
		{
			rt = componentInParent.GetComponent<RectTransform>();
		}
		if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, lastRaycast.screenPosition, eventCamera, out worldPosition))
		{
			return false;
		}
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, lastRaycast.screenPosition, eventCamera, out localPosition))
		{
			return false;
		}
		uGUI_GraphicRaycaster uGUI_GraphicRaycaster2 = module as uGUI_GraphicRaycaster;
		Camera camera = ((uGUI_GraphicRaycaster2 != null) ? uGUI_GraphicRaycaster2.eventCamera : MainCamera.camera);
		aimingTransform = camera.transform;
		return true;
	}
}
