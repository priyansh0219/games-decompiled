using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
[RequireComponent(typeof(MovePoint))]
public class Stalker : Creature
{
	public float feedFriendlinessIncrement = 1f;

	public float playFriendlinessIncrement = 1f;

	[AssertNotNull]
	public Transform loseToothDropLocation;

	[AssertNotNull]
	public GameObject toothPrefab;

	[AssertNotNull]
	public Collider stalkerBodyCollider;

	[AssertNotNull]
	public FMODAsset loseToothSound;

	private bool frozen;

	private bool FindNest()
	{
		bool result = false;
		Vector3 origin = base.transform.position + Random.onUnitSphere + Vector3.up;
		if (UWE.Utils.TraceForTerrain(new Ray(origin, Vector3.down), 3000f, out var hitInfo))
		{
			leashPosition = hitInfo.point + Vector3.up * 1f;
			result = true;
		}
		return result;
	}

	protected override void InitializeOnce()
	{
		base.InitializeOnce();
		InvokeRepeating("UpdateFindNest", Random.value, 0.2f);
	}

	private void UpdateFindNest()
	{
		if (FindNest())
		{
			CancelInvoke("UpdateFindNest");
		}
	}

	private bool LoseTooth()
	{
		if (true)
		{
			GameObject gameObject = Object.Instantiate(toothPrefab);
			gameObject.transform.position = loseToothDropLocation.transform.position;
			gameObject.transform.rotation = loseToothDropLocation.transform.rotation;
			if (gameObject.activeSelf && base.isActiveAndEnabled)
			{
				Collider[] componentsInChildren = gameObject.GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					Physics.IgnoreCollision(stalkerBodyCollider, componentsInChildren[i]);
				}
			}
			Utils.PlayFMODAsset(loseToothSound, gameObject.transform);
			LargeWorldEntity.Register(gameObject);
			return true;
		}
		return false;
	}

	private void CheckLoseTooth(GameObject target)
	{
		float hardness = HardnessMixin.GetHardness(target);
		if (Random.value < hardness && Random.value < 0.5f)
		{
			LoseTooth();
		}
	}

	public void OnShinyPickedUp(GameObject target)
	{
		CheckLoseTooth(target);
	}

	public void OnMeleeAttack(GameObject target)
	{
		CheckLoseTooth(target);
	}

	public void OnFreezeByStasisSphere()
	{
		frozen = true;
	}

	public void OnUnfreezeByStasisSphere()
	{
		frozen = false;
	}

	public void OnFishEat(GameObject fish)
	{
		if ((bool)fish)
		{
			Player componentInParent = fish.GetComponentInParent<Player>();
			if ((bool)componentInParent)
			{
				Befriend(componentInParent.gameObject, feedFriendlinessIncrement);
			}
		}
	}

	public void OnShinyPickUp(object target)
	{
		GameObject gameObject = target as GameObject;
		if (!gameObject)
		{
			return;
		}
		Smell component = gameObject.GetComponent<Smell>();
		if ((bool)component)
		{
			Befriend(component.owner, playFriendlinessIncrement * component.strength);
		}
		Player componentInParent = gameObject.GetComponentInParent<Player>();
		if ((bool)componentInParent)
		{
			Pickupable component2 = gameObject.GetComponent<Pickupable>();
			if ((bool)component2)
			{
				component2.Drop();
			}
			Befriend(componentInParent.gameObject, playFriendlinessIncrement);
		}
	}

	private void Befriend(GameObject go, float friendlinessIncrement)
	{
		Friendliness.Add(friendlinessIncrement);
		SetFriend(go, 60f);
	}
}
