using System;
using System.Collections.Generic;
using Gendarme;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[RequireComponent(typeof(EnergyMixin))]
public class DiveReel : PlayerTool, IEquippable, IProtoEventListener
{
	[NonSerialized]
	[ProtoMember(1)]
	public int state;

	public const int _version = 2;

	[NonSerialized]
	[ProtoMember(2)]
	public int version = 2;

	[NonSerialized]
	[ProtoMember(3, OverwriteList = true)]
	public List<Vector3> nodePositions;

	[AssertNotNull]
	public GameObject nodePrefab;

	[AssertNotNull]
	public Transform nodeDeployPos;

	[AssertNotNull]
	public Animator animationController;

	[AssertNotNull]
	public FMOD_CustomEmitter equipSFX;

	[AssertNotNull]
	public FMOD_CustomEmitter fireSFX;

	[AssertNotNull]
	public FMOD_CustomEmitter resetNodesSFX;

	[AssertNotNull]
	public Transform baseMatTrans;

	public Color baseColor;

	public Color lowAmmoColor;

	public float blinkDelay = 5f;

	public int maxNodes = 20;

	public float energyCostPerDisc = 0.5f;

	public float nodeRadius = 0.3f;

	private readonly List<DiveReelNode> nodes = new List<DiveReelNode>();

	private bool readyToPlace;

	private Vector3 lastNodePos;

	private Transform lastNodeTransform;

	private string primaryUseCachedString = string.Empty;

	private string altUseCachedString = string.Empty;

	private string customUseCachedString = string.Empty;

	private Vector3 deployPosLate;

	private bool cooldown;

	private Material matInstance;

	private Color desiredColor;

	[AssertLocalization]
	private const string createNodeButtonFormat = "DiveReelCreateNode";

	[AssertLocalization]
	private const string resetNodesButtonFormat = "DiveReelResetNodes";

	private void Start()
	{
		if (nodePositions == null)
		{
			nodePositions = new List<Vector3>();
		}
		InvokeRepeating("ProcBlink", 0f, blinkDelay);
		matInstance = baseMatTrans.GetComponent<SkinnedMeshRenderer>().material;
	}

	public override void OnDraw(Player p)
	{
		base.OnDraw(p);
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	private void ProcBlink()
	{
		if (nodes.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (DiveReelNode node in nodes)
		{
			if (node != null)
			{
				node.Blink((float)num / 2f);
			}
			num++;
		}
	}

	private void CreateNewNode(Vector3 createPos, bool isFirst, bool loadingNode)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(nodePrefab, createPos, MainCamera.camera.transform.rotation);
		gameObject.transform.Rotate(new Vector3(90f, 0f, 0f), Space.Self);
		DiveReelNode component = gameObject.GetComponent<DiveReelNode>();
		if (!loadingNode)
		{
			component.rb.AddForce(MainCamera.camera.transform.forward * 800f);
		}
		if (isFirst)
		{
			component.firstArrow = true;
		}
		else if ((bool)lastNodeTransform && (bool)component)
		{
			component.previousArrowPos = lastNodeTransform;
		}
		if (!loadingNode)
		{
			nodePositions.Add(createPos);
		}
		lastNodePos = createPos;
		nodes.Add(component);
		lastNodeTransform = gameObject.transform;
		if (nodes.Count == maxNodes)
		{
			SetDiveReelOutOfAmmoAnimator(state: true);
		}
		else
		{
			SetDiveReelOutOfAmmoAnimator(state: false);
		}
	}

	private void SetDiveReelOutOfAmmoAnimator(bool state)
	{
		animationController.SetBool("divereel_outofammo", state);
		Player.main.playerAnimator.SetBool("divereel_outofammo", state);
	}

	private void Update()
	{
		deployPosLate = nodeDeployPos.position;
		if (nodes.Count == maxNodes)
		{
			desiredColor = lowAmmoColor;
		}
		else if (nodes.Count >= maxNodes - maxNodes / 4)
		{
			if (!IsInvoking("CycleColors"))
			{
				InvokeRepeating("CycleColors", 0f, 1f);
			}
		}
		else
		{
			desiredColor = baseColor;
			if (IsInvoking("CycleColors"))
			{
				CancelInvoke("CycleColors");
			}
		}
		Color color = matInstance.GetColor(ShaderPropertyID._GlowColor);
		matInstance.SetColor(ShaderPropertyID._GlowColor, Color.Lerp(color, desiredColor, Time.deltaTime * 4f));
		UpdateControllerLightbarToToolBarValue();
	}

	private void CycleColors()
	{
		if (desiredColor == baseColor)
		{
			desiredColor = lowAmmoColor;
		}
		else if (desiredColor == lowAmmoColor)
		{
			desiredColor = baseColor;
		}
	}

	private void ResetNodes()
	{
		for (int i = 0; i < nodePositions.Count; i++)
		{
			if (nodePositions[i] != Vector3.zero)
			{
				nodePositions[i] = Vector3.zero;
			}
		}
		int num = 0;
		foreach (DiveReelNode node in nodes)
		{
			if (node != null)
			{
				node.DestroySelf((float)num * 0.2f);
				num++;
			}
		}
		nodePositions.Clear();
		nodes.Clear();
	}

	public override void OnToolUseAnim(GUIHand guiHand)
	{
		if (Player.main.currentSub != null)
		{
			return;
		}
		cooldown = true;
		Invoke("ResetCooldown", 3f);
		if (!Player.main.IsBleederAttached() && !energyMixin.IsDepleted() && nodes.Count < maxNodes)
		{
			Vector3 vector = deployPosLate;
			Vector3 position = Player.main.transform.position;
			Vector3 vector2 = position - vector;
			if (Physics.Raycast(position, -vector2.normalized, out var hitInfo, vector2.magnitude, ~(1 << LayerID.Player), QueryTriggerInteraction.Ignore))
			{
				vector = hitInfo.point + vector2.normalized * nodeRadius;
			}
			CreateNewNode(vector, nodePositions.Count == 0, loadingNode: false);
			energyMixin.ConsumeEnergy(energyCostPerDisc);
			fireSFX.Play();
			animationController.SetTrigger("divereel_fire");
		}
	}

	private void ResetCooldown()
	{
		cooldown = false;
	}

	public void OnEquip(GameObject sender, string slot)
	{
		animationController.SetBool("using_tool", value: true);
		equipSFX.Play();
	}

	public void OnUnequip(GameObject sender, string slot)
	{
	}

	public void UpdateEquipped(GameObject sender, string slot)
	{
		if (usingPlayer != null && nodes.Count > 0 && !cooldown && GameInput.GetButtonHeld(GameInput.Button.AltTool))
		{
			resetNodesSFX.Play();
			ResetNodes();
			Player.main.playerAnimator.SetTrigger("divereel_reset");
			animationController.SetTrigger("divereel_reset");
			SetDiveReelOutOfAmmoAnimator(state: false);
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (nodePositions == null)
		{
			return;
		}
		int num = 0;
		foreach (Vector3 nodePosition in nodePositions)
		{
			if (nodePosition == Vector3.zero)
			{
				break;
			}
			bool isFirst = num == 0;
			CreateNewNode(nodePosition, isFirst, loadingNode: true);
			num++;
		}
	}

	public override string GetCustomUseText()
	{
		string text = string.Empty;
		string text2 = string.Empty;
		if (usingPlayer != null && !cooldown)
		{
			if (usingPlayer.currentSub == null && !usingPlayer.IsBleederAttached() && !energyMixin.IsDepleted() && nodes.Count < maxNodes)
			{
				text = LanguageCache.GetButtonFormat("DiveReelCreateNode", GameInput.Button.RightHand);
			}
			if (nodes.Count > 0)
			{
				text2 = LanguageCache.GetButtonFormat("DiveReelResetNodes", GameInput.Button.AltTool);
			}
		}
		bool flag = string.IsNullOrEmpty(text);
		bool flag2 = string.IsNullOrEmpty(text2);
		if (flag && flag2)
		{
			return base.GetCustomUseText();
		}
		if (text != primaryUseCachedString || text2 != altUseCachedString)
		{
			if (flag2 && !flag)
			{
				customUseCachedString = text;
			}
			else if (flag && flag2)
			{
				customUseCachedString = text2;
			}
			else
			{
				customUseCachedString = $"{text}, {text2}";
			}
			primaryUseCachedString = text;
			altUseCachedString = text2;
		}
		return customUseCachedString;
	}
}
