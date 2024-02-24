using System;
using System.Collections.Generic;
using UnityEngine;

public class CyclopsHUDSonarPing : MonoBehaviour
{
	public bool isCreaturePing;

	[AssertNotNull]
	public Transform pingBase;

	[AssertNotNull]
	public Transform pingTop;

	[AssertNotNull]
	public LineRenderer pingLine;

	[NonSerialized]
	public GameObject entity;

	[NonSerialized]
	public AttackCyclops attackCyclops;

	[NonSerialized]
	public GameObject cyclopsObject;

	public float radarScanSize = 65f;

	public float radarDisplaySize = 1.5f;

	public Color passiveColor;

	public Color aggressiveColor;

	private Transform subTransform;

	private Color currentColor;

	private bool colorChangeCD;

	private BehaviourLOD LOD;

	private readonly List<Renderer> renderers = new List<Renderer>();

	private MaterialPropertyBlock materialPropertyBlock;

	private Vector3[] linePos = new Vector3[2];

	private void Awake()
	{
		materialPropertyBlock = new MaterialPropertyBlock();
	}

	private void Start()
	{
		SubRoot componentInParent = base.gameObject.GetComponentInParent<SubRoot>();
		subTransform = componentInParent.transform;
		LOD = componentInParent.gameObject.GetComponent<BehaviourLOD>();
		if (isCreaturePing)
		{
			MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
			LineRenderer[] componentsInChildren2 = GetComponentsInChildren<LineRenderer>();
			renderers.AddRange(componentsInChildren);
			renderers.AddRange(componentsInChildren2);
			ChangePingColor(passiveColor);
		}
	}

	private void ChangePingColor(Color color)
	{
		if (colorChangeCD || renderers.Count == 0 || currentColor == color)
		{
			return;
		}
		materialPropertyBlock.SetColor(ShaderPropertyID._Color, color);
		foreach (Renderer renderer in renderers)
		{
			renderer.SetPropertyBlock(materialPropertyBlock);
		}
		currentColor = color;
	}

	private void HideVisuals()
	{
		if (pingBase.gameObject.activeSelf)
		{
			pingBase.gameObject.SetActive(value: false);
		}
		if (pingTop.gameObject.activeSelf)
		{
			pingTop.gameObject.SetActive(value: false);
		}
		if (pingLine.gameObject.activeSelf)
		{
			pingLine.gameObject.SetActive(value: false);
		}
	}

	private void ShowVisuals()
	{
		if (!pingBase.gameObject.activeSelf)
		{
			pingBase.gameObject.SetActive(value: true);
		}
		if (!pingTop.gameObject.activeSelf)
		{
			pingTop.gameObject.SetActive(value: true);
		}
	}

	private void LateUpdate()
	{
		if (!LOD.IsFull())
		{
			HideVisuals();
			return;
		}
		bool flag = false;
		if (entity != null && (bool)entity.GetComponent<LiveMixin>() && !entity.GetComponent<LiveMixin>().IsAlive())
		{
			flag = true;
		}
		if (entity == null)
		{
			flag = true;
		}
		if (flag)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		ShowVisuals();
		Vector3 position = entity.transform.position;
		float num = radarDisplaySize / radarScanSize;
		Vector3 localPosition = subTransform.InverseTransformPoint(position) * num;
		base.transform.localPosition = localPosition;
		Vector3 position2 = base.transform.position;
		base.transform.localPosition = new Vector3(localPosition.x, 0f, localPosition.z);
		pingTop.position = position2;
		_ = MainCamera.camera.transform.position - pingTop.transform.position;
		float z = ((!(localPosition.y > 0f)) ? 180 : 0);
		pingTop.transform.LookAt(MainCamera.camera.transform.position);
		pingTop.transform.rotation = Quaternion.Euler(new Vector3(0f, pingTop.transform.rotation.eulerAngles.y, z));
		if (Mathf.Abs(localPosition.y) < 0.05f)
		{
			pingLine.gameObject.SetActive(value: false);
		}
		else
		{
			pingLine.gameObject.SetActive(value: true);
			float y = ((localPosition.y > 0f) ? (-0.05f) : 0.05f);
			linePos[0] = base.transform.position;
			linePos[1] = pingTop.position + new Vector3(0f, y, 0f);
			pingLine.useWorldSpace = true;
			pingLine.SetPositions(linePos);
		}
		if (isCreaturePing && !IsInvoking("ResetColorChangeCD"))
		{
			if (attackCyclops.IsAggressiveTowardsCyclops(cyclopsObject))
			{
				ChangePingColor(aggressiveColor);
				colorChangeCD = true;
				Invoke("ResetColorChangeCD", 15f);
			}
			else
			{
				ChangePingColor(passiveColor);
			}
		}
	}

	private void ResetColorChangeCD()
	{
		colorChangeCD = false;
	}
}
