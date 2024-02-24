using System.Collections.Generic;
using Gendarme;
using UnityEngine;
using UnityEngine.UI;

[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidLackOfCohesionOfMethodsRule")]
public class CyclopsHolographicHUD : MonoBehaviour
{
	private class FireIcon
	{
		public GameObject fireIcon;

		public GameObject refFire;
	}

	private class DamageIcon
	{
		public GameObject damageIcon;

		public GameObject refDamage;
	}

	private sealed class LavaLarvaIcon
	{
		public GameObject creatureIcon;

		public GameObject refGo;
	}

	[AssertNotNull]
	public LiveMixin subRootLiveMixin;

	[AssertNotNull]
	public CyclopsExternalDamageManager externalDamage;

	[AssertNotNull]
	public SubFire subFire;

	[AssertNotNull]
	public Transform iconHolder;

	[AssertNotNull]
	public Image healthBar;

	[AssertNotNull]
	public GameObject fireSuppressionSystem;

	[AssertNotNull]
	public GameObject fireIconPrefab;

	[AssertNotNull]
	public GameObject damageIconPrefab;

	[AssertNotNull]
	public GameObject lavaLarvaIconPrefab;

	[AssertNotNull]
	public BehaviourLOD LOD;

	public float uiScale = 0.5f;

	private Transform subRootTransform;

	private List<FireIcon> fireIcons = new List<FireIcon>();

	private List<DamageIcon> damageIcons = new List<DamageIcon>();

	private List<LavaLarvaIcon> lavaLarvaIcons = new List<LavaLarvaIcon>();

	private int oldFireCount;

	private void Start()
	{
		subRootTransform = base.transform.GetComponentInParent<SubRoot>().transform;
		InvokeRepeating("UpdateFires", 2f, 2f);
		InvokeRepeating("UpdateDamageIcons", 2.5f, 2.5f);
	}

	public void AttachedLavaLarva(GameObject go)
	{
		Vector3 localPosition = subRootTransform.InverseTransformPoint(go.transform.position) * uiScale;
		GameObject gameObject = Object.Instantiate(lavaLarvaIconPrefab);
		gameObject.transform.parent = iconHolder;
		gameObject.transform.localPosition = localPosition;
		LavaLarvaIcon lavaLarvaIcon = new LavaLarvaIcon();
		lavaLarvaIcon.creatureIcon = gameObject;
		lavaLarvaIcon.refGo = go;
		lavaLarvaIcons.Add(lavaLarvaIcon);
	}

	public void DetachedLavaLarva(GameObject go)
	{
		LavaLarvaIcon lavaLarvaIcon = null;
		foreach (LavaLarvaIcon lavaLarvaIcon2 in lavaLarvaIcons)
		{
			if (lavaLarvaIcon2.refGo.Equals(go))
			{
				Object.Destroy(lavaLarvaIcon2.creatureIcon);
				lavaLarvaIcon = lavaLarvaIcon2;
				break;
			}
		}
		if (lavaLarvaIcon != null)
		{
			lavaLarvaIcons.Remove(lavaLarvaIcon);
		}
	}

	public void RefreshUpgradeConsoleIcons(TechType[] upgrades)
	{
		bool flag = false;
		for (int i = 0; i < upgrades.Length; i++)
		{
			if (upgrades[i] == TechType.CyclopsFireSuppressionModule)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			fireSuppressionSystem.SetActive(value: true);
		}
		else
		{
			fireSuppressionSystem.SetActive(value: false);
		}
	}

	private void UpdateFires()
	{
		if (LOD.IsFull())
		{
			int fireCount = subFire.GetFireCount();
			if (fireCount != oldFireCount)
			{
				UpdateFireIcons();
				oldFireCount = fireCount;
			}
		}
	}

	private void UpdateDamageIcons()
	{
		if (!LOD.IsFull())
		{
			return;
		}
		List<DamageIcon> list = new List<DamageIcon>();
		foreach (DamageIcon damageIcon2 in damageIcons)
		{
			if (!damageIcon2.refDamage.gameObject.activeSelf)
			{
				list.Add(damageIcon2);
			}
		}
		foreach (DamageIcon item in list)
		{
			item.damageIcon.GetComponent<CyclopsHolographicHUD_WarningPings>().DespawnIcon();
			damageIcons.Remove(item);
		}
		CyclopsDamagePoint[] damagePoints = externalDamage.damagePoints;
		foreach (CyclopsDamagePoint cyclopsDamagePoint in damagePoints)
		{
			if (!cyclopsDamagePoint.gameObject.activeSelf)
			{
				continue;
			}
			bool flag = true;
			foreach (DamageIcon damageIcon3 in damageIcons)
			{
				if (damageIcon3.refDamage.gameObject.Equals(cyclopsDamagePoint.gameObject))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				Vector3 localPosition = subRootTransform.InverseTransformPoint(cyclopsDamagePoint.transform.position) * uiScale;
				GameObject gameObject = Object.Instantiate(damageIconPrefab);
				gameObject.transform.parent = iconHolder;
				gameObject.transform.localPosition = localPosition;
				DamageIcon damageIcon = new DamageIcon();
				damageIcon.damageIcon = gameObject;
				damageIcon.refDamage = cyclopsDamagePoint.gameObject;
				damageIcons.Add(damageIcon);
			}
		}
	}

	private void Update()
	{
		if (LOD.IsFull())
		{
			float healthFraction = subRootLiveMixin.GetHealthFraction();
			healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, healthFraction, Time.deltaTime * 2f);
		}
	}

	private void UpdateFireIcons()
	{
		List<GameObject> allFires = subFire.GetAllFires();
		if (allFires.Count == 0)
		{
			DeleteAllFireIcons();
			return;
		}
		List<FireIcon> list = new List<FireIcon>();
		foreach (FireIcon fireIcon2 in fireIcons)
		{
			if (fireIcon2.refFire == null)
			{
				list.Add(fireIcon2);
			}
		}
		foreach (FireIcon item in list)
		{
			item.fireIcon.SendMessage("DespawnIcon");
			fireIcons.Remove(item);
		}
		foreach (GameObject item2 in allFires)
		{
			bool flag = true;
			foreach (FireIcon fireIcon3 in fireIcons)
			{
				if (fireIcon3.refFire.Equals(item2))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				Vector3 localPosition = subRootTransform.InverseTransformPoint(item2.transform.position) * uiScale;
				GameObject gameObject = Object.Instantiate(fireIconPrefab);
				gameObject.transform.parent = iconHolder;
				gameObject.transform.localPosition = localPosition;
				FireIcon fireIcon = new FireIcon();
				fireIcon.fireIcon = gameObject;
				fireIcon.refFire = item2;
				fireIcons.Add(fireIcon);
			}
		}
	}

	private void DeleteAllFireIcons()
	{
		foreach (FireIcon fireIcon in fireIcons)
		{
			Object.Destroy(fireIcon.fireIcon);
		}
		fireIcons.Clear();
	}

	public void CyclopsDeathEvent()
	{
		CancelInvoke();
		base.gameObject.SetActive(value: false);
	}
}
