using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[RequireComponent(typeof(Sealed))]
public class LaserCutObject : MonoBehaviour
{
	[SerializeField]
	private bool useBackFacingNodes = true;

	[SerializeField]
	private bool useMaterialFading = true;

	[SerializeField]
	private bool showTrail = true;

	[SerializeField]
	private bool destroyCutObjectOnLoad = true;

	[SerializeField]
	[AssertNotNull]
	private GameObject nodeHolderFront;

	[SerializeField]
	private GameObject nodeHolderBack;

	[SerializeField]
	[AssertNotNull]
	private GameObject laserCutStreak;

	[SerializeField]
	[AssertNotNull]
	private GameObject laserCutFX;

	[SerializeField]
	[AssertNotNull]
	private Sealed sealedScript;

	[SerializeField]
	[AssertNotNull]
	private GameObject trailRenderer;

	[SerializeField]
	[AssertNotNull]
	private GameObject uncutObject;

	[SerializeField]
	[AssertNotNull]
	private GameObject cutObject;

	[SerializeField]
	[AssertNotNull]
	private GameObject cutFloatAwayObject;

	[SerializeField]
	private bool drawDebugLines = true;

	private const float debugWireRadius = 0.025f;

	private Renderer cutObjectRenderer;

	private Renderer cutFloatAwayRenderer;

	private MaterialPropertyBlock cutObjectPropertyBlock;

	private MaterialPropertyBlock cutFloatAwayPropertyBlock;

	private bool cutting;

	private int totalNodes;

	private float lastCutValue;

	private Vector3 matchingNodePos;

	private float doorTimer;

	[NonSerialized]
	[ProtoMember(1)]
	public bool isCutOpen;

	private void OnEnable()
	{
		totalNodes = nodeHolderFront.transform.childCount;
		matchingNodePos = Vector3.zero;
		if (isCutOpen)
		{
			CutOpenDoor();
		}
		if (isCutOpen && destroyCutObjectOnLoad && (bool)cutFloatAwayObject)
		{
			UnityEngine.Object.Destroy(cutFloatAwayObject);
		}
		if (useMaterialFading)
		{
			cutObjectRenderer = cutObject.GetComponent<MeshRenderer>();
			cutObjectPropertyBlock = new MaterialPropertyBlock();
			if ((bool)cutFloatAwayObject)
			{
				cutFloatAwayRenderer = cutFloatAwayObject.GetComponent<MeshRenderer>();
				cutFloatAwayPropertyBlock = new MaterialPropertyBlock();
			}
		}
		if (nodeHolderFront.transform.childCount > 0)
		{
			SetCutFxPosition();
		}
		trailRenderer.SetActive(showTrail);
	}

	private void Update()
	{
		if (isCutOpen)
		{
			if (useMaterialFading)
			{
				cutObjectRenderer.GetPropertyBlock(cutObjectPropertyBlock);
				float @float = cutObjectPropertyBlock.GetFloat(ShaderPropertyID._GlowStrength);
				if (@float > 0f)
				{
					@float = Mathf.MoveTowards(@float, 0f, Time.deltaTime / 9f);
					cutObjectPropertyBlock.SetFloat(ShaderPropertyID._GlowStrength, @float);
					cutObjectPropertyBlock.SetFloat(ShaderPropertyID._GlowStrengthNight, @float);
					cutObjectRenderer.SetPropertyBlock(cutObjectPropertyBlock);
					if ((bool)cutFloatAwayObject)
					{
						cutFloatAwayRenderer.GetPropertyBlock(cutFloatAwayPropertyBlock);
						cutFloatAwayPropertyBlock.SetFloat(ShaderPropertyID._GlowStrength, @float);
						cutFloatAwayPropertyBlock.SetFloat(ShaderPropertyID._GlowStrengthNight, @float);
						cutFloatAwayRenderer.SetPropertyBlock(cutFloatAwayPropertyBlock);
					}
				}
			}
			if ((bool)cutFloatAwayObject && Time.time - doorTimer > 300f)
			{
				float x = cutFloatAwayObject.transform.localScale.x;
				if (x > 0.05f)
				{
					x = Mathf.Lerp(x, 0f, Time.deltaTime * 5f);
					cutFloatAwayObject.transform.localScale = new Vector3(x, x, x);
				}
				else
				{
					cutFloatAwayObject.SetActive(value: false);
				}
			}
		}
		if (cutting && !isCutOpen)
		{
			if (MiscSettings.flashes)
			{
				SetCutFXState(state: true);
			}
			SetCutFxPosition();
		}
		else
		{
			SetCutFXState(state: false);
		}
		if (!sealedScript.IsSealed() && !isCutOpen)
		{
			CutOpenDoor();
		}
	}

	public void CutOpenDoor()
	{
		isCutOpen = true;
		uncutObject.SetActive(value: false);
		cutObject.SetActive(value: true);
		doorTimer = Time.time;
	}

	public void ActivateFX()
	{
		cutting = true;
		CancelInvoke("StopCutting");
		Invoke("StopCutting", 1f);
	}

	private void SetCutFXState(bool state)
	{
		for (int i = 0; i < laserCutFX.transform.childCount; i++)
		{
			ParticleSystem component = laserCutFX.transform.GetChild(i).GetComponent<ParticleSystem>();
			if ((bool)component)
			{
				ParticleSystem.EmissionModule emission = component.emission;
				emission.enabled = state;
				if (state && !component.isPlaying)
				{
					component.Play(withChildren: true);
				}
				else if (!state && component.isPlaying)
				{
					component.Stop(withChildren: true);
				}
			}
		}
	}

	private void SetCutFxPosition()
	{
		float openedAmount = sealedScript.openedAmount;
		float maxOpenedAmount = sealedScript.maxOpenedAmount;
		int num = (int)Mathf.Floor(openedAmount / maxOpenedAmount * (float)totalNodes);
		int num2 = num + 1;
		if (num2 >= totalNodes)
		{
			num2 = 0;
		}
		if (num >= totalNodes)
		{
			num = 0;
		}
		Transform transform = nodeHolderFront.transform;
		if (useBackFacingNodes)
		{
			bool num3 = Utils.CheckObjectInFront(base.transform, Player.main.transform);
			float y = (num3 ? 0f : 180f);
			laserCutFX.transform.localRotation = Quaternion.Euler(new Vector3(0f, y, 0f));
			laserCutFX.transform.Rotate(new Vector3(0f, -90f, 0f));
			transform = (num3 ? nodeHolderFront.transform : nodeHolderBack.transform);
		}
		Transform transform2 = transform.GetChild(num2).transform;
		Transform transform3 = transform.GetChild(num).transform;
		if (transform2 != null && transform3 != null)
		{
			Vector3 position = transform2.position;
			if (matchingNodePos == Vector3.zero)
			{
				matchingNodePos = position;
			}
			else
			{
				matchingNodePos = Vector3.Lerp(matchingNodePos, position, Time.deltaTime * 2f);
			}
			laserCutStreak.transform.position = matchingNodePos;
			laserCutFX.transform.position = matchingNodePos;
			if ((bool)Player.main.gameObject)
			{
				Player.main.armsController.lookTargetTransform.position = matchingNodePos;
			}
		}
		lastCutValue = openedAmount;
	}

	private void StopCutting()
	{
		cutting = false;
	}
}
