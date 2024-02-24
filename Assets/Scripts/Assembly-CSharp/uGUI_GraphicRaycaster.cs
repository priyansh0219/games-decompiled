using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_GraphicRaycaster : GraphicRaycaster
{
	public delegate void UpdateRaycasterStatus(uGUI_GraphicRaycaster raycaster);

	private static List<uGUI_GraphicRaycaster> allRaycasters = new List<uGUI_GraphicRaycaster>();

	public bool guiCameraSpace;

	public UpdateRaycasterStatus updateRaycasterStatusDelegate;

	[Tooltip("How close the player must be to enable this raycaster. Set to 0 to always be enabled")]
	[SerializeField]
	private float interactionDistance;

	public override Camera eventCamera
	{
		get
		{
			if (SNCameraRoot.main != null)
			{
				if (!guiCameraSpace)
				{
					return SNCameraRoot.main.mainCam;
				}
				return SNCameraRoot.main.guiCam;
			}
			return MainCamera.camera;
		}
	}

	public static void UpdateGraphicRaycasters()
	{
		foreach (uGUI_GraphicRaycaster allRaycaster in allRaycasters)
		{
			if (allRaycaster.updateRaycasterStatusDelegate != null)
			{
				allRaycaster.updateRaycasterStatusDelegate(allRaycaster);
			}
			else if (!(allRaycaster.interactionDistance <= 0f))
			{
				if (Vector3.SqrMagnitude(Player.mainObject.transform.position - allRaycaster.transform.position) < allRaycaster.interactionDistance * allRaycaster.interactionDistance)
				{
					allRaycaster.enabled = true;
				}
				else
				{
					allRaycaster.enabled = false;
				}
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		allRaycasters.Add(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		allRaycasters.Remove(this);
	}
}
