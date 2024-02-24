using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Sign : HandTarget, IHandTarget, IProtoEventListener
{
	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public string text = "";

	[NonSerialized]
	[ProtoMember(3, OverwriteList = true)]
	public bool[] elements;

	[NonSerialized]
	[ProtoMember(4)]
	public int scaleIndex;

	[NonSerialized]
	[ProtoMember(5)]
	public int colorIndex;

	[NonSerialized]
	[ProtoMember(6)]
	public bool backgroundEnabled = true;

	[AssertNotNull]
	public uGUI_SignInput signInput;

	[AssertNotNull]
	public uGUI_RectTransformCallback baseRect;

	[AssertNotNull]
	public BoxCollider boxCollider;

	[AssertLocalization]
	private const string editLabelHandText = "SignEditLabel";

	private void OnEnable()
	{
		signInput.gameObject.SetActive(value: true);
		UpdateCollider();
		baseRect.onTransformChange += UpdateCollider;
		baseRect.onDimensionsChange += UpdateCollider;
	}

	private void OnDisable()
	{
		signInput.gameObject.SetActive(value: false);
		baseRect.onTransformChange -= UpdateCollider;
		baseRect.onDimensionsChange -= UpdateCollider;
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		text = signInput.text;
		elements = signInput.elementsState;
		scaleIndex = signInput.scaleIndex;
		colorIndex = signInput.colorIndex;
		backgroundEnabled = signInput.backgroundToggle.isOn;
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		signInput.text = text;
		signInput.elementsState = elements;
		signInput.scaleIndex = scaleIndex;
		signInput.colorIndex = colorIndex;
		signInput.backgroundToggle.isOn = backgroundEnabled;
	}

	public void OnHandHover(GUIHand hand)
	{
		if (base.enabled)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "SignEditLabel", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Rename);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (base.enabled)
		{
			signInput.Select(lockMovement: true);
		}
	}

	public void UpdateCollider()
	{
		RectTransform rt = baseRect.rt;
		Rect rect = rt.rect;
		Vector3 lossyScale = rt.lossyScale;
		boxCollider.size = new Vector3(rect.width * lossyScale.x, rect.height * lossyScale.y, boxCollider.size.z);
	}
}
