using System;
using System.Collections;
using System.Collections.Generic;
using UWE;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BreakableResource : HandTarget, IPropulsionCannonAmmo, IHandTarget
{
	[Serializable]
	public class RandomPrefab
	{
		public AssetReferenceGameObject prefabReference;

		public TechType prefabTechType;

		public float chance;
	}

	[AssertNotNull(AssertNotNullAttribute.Options.AllowEmptyCollection)]
	public List<RandomPrefab> prefabList;

	[AssertNotNull]
	public AssetReferenceGameObject defaultPrefabReference;

	public TechType defaultPrefabTechType;

	public float verticalSpawnOffset = 0.2f;

	public int numChances = 1;

	public int hitsToBreak;

	[AssertNotNull]
	public FMODAsset hitSound;

	[AssertNotNull]
	public FMODAsset breakSound;

	public GameObject hitFX;

	public GameObject breakFX;

	[AssertLocalization]
	public string breakText = "BreakRock";

	public string customGoalText = "";

	private bool broken;

	void IPropulsionCannonAmmo.OnGrab()
	{
	}

	void IPropulsionCannonAmmo.OnShoot()
	{
	}

	void IPropulsionCannonAmmo.OnImpact()
	{
		BreakIntoResources();
	}

	void IPropulsionCannonAmmo.OnRelease()
	{
	}

	bool IPropulsionCannonAmmo.GetAllowedToGrab()
	{
		return true;
	}

	bool IPropulsionCannonAmmo.GetAllowedToShoot()
	{
		return true;
	}

	private void BashHit()
	{
		BreakIntoResources();
	}

	public void OnHandHover(GUIHand hand)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, breakText, translate: true, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Hand);
	}

	public void OnHandClick(GUIHand hand)
	{
		if (Utils.GetLocalPlayerComp().GetInMechMode())
		{
			BreakIntoResources();
		}
		else if (!Player.main.PlayBash())
		{
			Player.main.PlayGrab();
			FMODUWE.PlayOneShot(hitSound, base.transform.position);
			if ((bool)hitFX)
			{
				Utils.PlayOneShotPS(hitFX, base.transform.position, Quaternion.Euler(new Vector3(270f, 0f, 0f)));
			}
			HitResource();
		}
	}

	public void HitResource()
	{
		hitsToBreak--;
		if (hitsToBreak == 0)
		{
			BreakIntoResources();
		}
	}

	public void BreakIntoResources()
	{
		if (broken)
		{
			return;
		}
		broken = true;
		SendMessage("OnBreakResource", null, SendMessageOptions.DontRequireReceiver);
		if ((bool)base.gameObject.GetComponent<VFXBurstModel>())
		{
			base.gameObject.BroadcastMessage("OnKill");
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (customGoalText != "")
		{
			GoalManager.main.OnCustomGoalEvent(customGoalText);
		}
		bool flag = false;
		for (int i = 0; i < numChances; i++)
		{
			AssetReferenceGameObject assetReferenceGameObject = ChooseRandomResource();
			if (assetReferenceGameObject != null)
			{
				SpawnResourceFromPrefab(assetReferenceGameObject);
				flag = true;
			}
		}
		if (!flag)
		{
			SpawnResourceFromPrefab(defaultPrefabReference);
		}
		FMODUWE.PlayOneShot(breakSound, base.transform.position);
		if ((bool)hitFX)
		{
			Utils.PlayOneShotPS(breakFX, base.transform.position, Quaternion.Euler(new Vector3(270f, 0f, 0f)));
		}
	}

	private void SpawnResourceFromPrefab(AssetReferenceGameObject breakPrefab)
	{
		CoroutineHost.StartCoroutine(SpawnResourceFromPrefab(breakPrefab, base.transform.position + base.transform.up * verticalSpawnOffset, base.transform.up));
	}

	private static IEnumerator SpawnResourceFromPrefab(AssetReferenceGameObject breakPrefab, Vector3 position, Vector3 up)
	{
		CoroutineTask<GameObject> result = AddressablesUtility.InstantiateAsync(breakPrefab.RuntimeKey as string, null, position);
		yield return result;
		GameObject result2 = result.GetResult();
		if (result2 == null)
		{
			Debug.LogErrorFormat("Failed to spawn {0}" + breakPrefab.RuntimeKey);
			yield break;
		}
		Rigidbody rigidbody = result2.EnsureComponent<Rigidbody>();
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidbody, isKinematic: false);
		rigidbody.AddTorque(Vector3.right * UnityEngine.Random.Range(3, 6));
		rigidbody.AddForce(up * 0.1f);
	}

	private AssetReferenceGameObject ChooseRandomResource()
	{
		AssetReferenceGameObject result = null;
		for (int i = 0; i < prefabList.Count; i++)
		{
			RandomPrefab randomPrefab = prefabList[i];
			if (Player.main.gameObject.GetComponent<PlayerEntropy>().CheckChance(randomPrefab.prefabTechType, randomPrefab.chance))
			{
				result = randomPrefab.prefabReference;
				break;
			}
		}
		return result;
	}
}
