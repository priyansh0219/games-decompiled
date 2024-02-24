using System;
using System.Collections;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class WeldableWallPanelGeneric : HandTarget, IHandTarget
{
	public GameObject doorObject;

	public GameObject doorEndPosObject;

	public LiveMixin liveMixin;

	public Animation animator;

	private Vector3 doorEndPos;

	private Quaternion doorEndRot;

	private float repairAmountNormalized;

	[NonSerialized]
	[ProtoMember(1)]
	public bool repaired;

	private GameObject sendMessageFrom;

	private bool updatePanel = true;

	[AssertLocalization]
	private const string damagedWiresGenericHandText = "DamagedWiresGeneric";

	[AssertLocalization]
	private const string weldToFixHandText = "WeldToFix";

	private void Start()
	{
		if ((bool)base.transform.parent)
		{
			sendMessageFrom = base.transform.parent.gameObject;
		}
		else
		{
			sendMessageFrom = base.gameObject;
		}
		if (liveMixin.health >= liveMixin.maxHealth)
		{
			repaired = true;
			sendMessageFrom.BroadcastMessage("UnlockDoor");
		}
		animator["Repair"].speed = 0f;
		if (repaired)
		{
			animator["Repair"].normalizedTime = 1f;
			return;
		}
		repairAmountNormalized = liveMixin.GetHealthFraction();
		animator["Repair"].time = repairAmountNormalized;
	}

	public void UnlockDoor()
	{
		if ((bool)liveMixin)
		{
			liveMixin.health = liveMixin.maxHealth;
		}
	}

	public void OnHandHover(GUIHand hand)
	{
		if (liveMixin.health < liveMixin.maxHealth)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "DamagedWiresGeneric", translate: true);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "WeldToFix", translate: true);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
	}

	private void Update()
	{
		if (!updatePanel)
		{
			return;
		}
		float healthFraction = liveMixin.GetHealthFraction();
		repairAmountNormalized = Mathf.MoveTowards(repairAmountNormalized, healthFraction, Time.deltaTime / 5f);
		animator["Repair"].normalizedTime = repairAmountNormalized;
		if (liveMixin.IsFullHealth())
		{
			doorEndPos = doorEndPosObject.transform.position;
			doorEndRot = doorEndPosObject.transform.rotation;
			Vector3 position = doorObject.transform.position;
			Quaternion rotation = doorObject.transform.rotation;
			doorObject.transform.position = Vector3.Lerp(position, doorEndPos, Time.deltaTime * 20f);
			doorObject.transform.rotation = Quaternion.Slerp(rotation, doorEndRot, Time.deltaTime * 20f);
			if (!repaired)
			{
				repaired = true;
				sendMessageFrom.BroadcastMessage("UnlockDoor");
				StartCoroutine(DisablePanel());
			}
		}
	}

	private IEnumerator DisablePanel()
	{
		yield return new WaitForSeconds(5f);
		updatePanel = false;
	}
}
