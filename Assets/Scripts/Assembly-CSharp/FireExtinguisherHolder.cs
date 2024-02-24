using System;
using System.Collections;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class FireExtinguisherHolder : MonoBehaviour, IHandTarget
{
	[AssertNotNull]
	public GameObject tankObject;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version;

	[NonSerialized]
	[ProtoMember(2)]
	public bool hasTank = true;

	[NonSerialized]
	[ProtoMember(3)]
	public float fuel = 100f;

	private bool isTakeTankAsyncInProgress;

	[AssertLocalization]
	private const string takeHandText = "TakeFireExtinguisher";

	[AssertLocalization]
	private const string replaceHandText = "ReplaceFireExtinguisher";

	private void Start()
	{
		tankObject.SetActive(hasTank);
	}

	private IEnumerator TakeTankAsync()
	{
		isTakeTankAsyncInProgress = true;
		TaskResult<GameObject> result = new TaskResult<GameObject>();
		yield return CraftData.AddToInventoryAsync(TechType.FireExtinguisher, result, 1, noMessage: false, spawnIfCantAdd: false);
		GameObject gameObject = result.Get();
		if (gameObject != null)
		{
			hasTank = false;
			tankObject.SetActive(value: false);
			gameObject.GetComponent<FireExtinguisher>().fuel = fuel;
		}
		isTakeTankAsyncInProgress = false;
	}

	private void TryStoreTank()
	{
		Pickupable pickupable = Inventory.main.container.RemoveItem(TechType.FireExtinguisher);
		if (pickupable != null)
		{
			FireExtinguisher component = pickupable.GetComponent<FireExtinguisher>();
			if (component != null)
			{
				fuel = component.fuel;
			}
			hasTank = true;
			tankObject.SetActive(value: true);
			UnityEngine.Object.Destroy(pickupable.gameObject);
		}
	}

	public void OnHandHover(GUIHand hand)
	{
		string text = (hasTank ? "TakeFireExtinguisher" : "ReplaceFireExtinguisher");
		HandReticle.main.SetText(HandReticle.TextType.Hand, text, translate: true, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Hand);
	}

	public void OnHandClick(GUIHand hand)
	{
		if (!isTakeTankAsyncInProgress)
		{
			if (hasTank)
			{
				StartCoroutine(TakeTankAsync());
			}
			else
			{
				TryStoreTank();
			}
		}
	}
}
