using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CyclopsExternalDamageManager : MonoBehaviour, IOnTakeDamage
{
	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public LiveMixin subLiveMixin;

	[AssertNotNull]
	public CyclopsDamagePoint[] damagePoints;

	[AssertNotNull]
	public GameObject[] fxPrefabs;

	[AssertNotNull]
	public SubFire subFire;

	[AssertNotNull]
	public GameObject subLeaksRoot;

	[AssertNotNull]
	public GameObject[] subLeaks;

	public float overshieldPercentage = 10f;

	private List<CyclopsDamagePoint> unusedDamagePoints;

	private IOnTakeDamage[] damageRecievers = new IOnTakeDamage[0];

	private void Start()
	{
		damageRecievers = GetComponents<IOnTakeDamage>();
		overshieldPercentage = Mathf.Clamp(overshieldPercentage, 0f, 100f);
		for (int i = 0; i < damagePoints.Length; i++)
		{
			damagePoints[i].SetManager(this);
			damagePoints[i].gameObject.SetActive(value: false);
		}
		unusedDamagePoints = damagePoints.ToList();
		for (int j = 0; j < damagePoints.Length; j++)
		{
			if (EvaluatePlaceNewPoint())
			{
				CreatePoint();
			}
		}
		OnTakeDamage(null);
	}

	private void OnEnable()
	{
		InvokeRepeating("UpdateOvershield", 0f, 1f);
	}

	private void OnDisable()
	{
		CancelInvoke();
	}

	private void Update()
	{
		if (subLeaksRoot.activeSelf && Player.main.currentSub != subRoot)
		{
			subLeaksRoot.SetActive(value: false);
		}
		else if (!subLeaksRoot.activeSelf && Player.main.currentSub == subRoot)
		{
			subLeaksRoot.SetActive(value: true);
		}
	}

	private void UpdateOvershield()
	{
		if (subFire.GetFireCount() <= 0)
		{
			float healthFraction = subLiveMixin.GetHealthFraction();
			float num = (100f - overshieldPercentage) / 100f;
			if (healthFraction > num)
			{
				subLiveMixin.AddHealth(3f);
			}
		}
	}

	private bool EvaluatePlaceNewPoint()
	{
		int num = damagePoints.Length;
		int num2 = num - unusedDamagePoints.Count;
		float healthFrac = ReturnHealthFractionWithOvershield();
		int num3 = CalculateDesiredPoints(healthFrac, num);
		return num2 < num3;
	}

	public void RepairPoint(CyclopsDamagePoint point)
	{
		float healthBack = subLiveMixin.maxHealth / (float)damagePoints.Length;
		unusedDamagePoints.Add(point);
		if (damagePoints.Length - unusedDamagePoints.Count == 0)
		{
			subLiveMixin.AddHealth(subLiveMixin.maxHealth);
		}
		else
		{
			subLiveMixin.AddHealth(healthBack);
		}
		ToggleLeakPointsBasedOnDamage();
	}

	public void NotifyAllOfDamage()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		for (int i = 0; i < damageRecievers.Length; i++)
		{
			IOnTakeDamage onTakeDamage = damageRecievers[i];
			if (onTakeDamage != null && ((MonoBehaviour)onTakeDamage).isActiveAndEnabled)
			{
				onTakeDamage.OnTakeDamage(null);
			}
		}
	}

	public void OnTakeDamage(DamageInfo damageInfo)
	{
		if (EvaluatePlaceNewPoint())
		{
			CreatePoint();
		}
		ToggleLeakPointsBasedOnDamage();
	}

	private void CreatePoint()
	{
		int index = Random.Range(0, unusedDamagePoints.Count - 1);
		unusedDamagePoints[index].gameObject.SetActive(value: true);
		unusedDamagePoints[index].RestoreHealth();
		GameObject prefabGo = fxPrefabs[Random.Range(0, fxPrefabs.Length)];
		unusedDamagePoints[index].SpawnFx(prefabGo);
		unusedDamagePoints.RemoveAt(index);
	}

	private int CalculateDesiredPoints(float healthFrac, int totalPts)
	{
		return Mathf.CeilToInt(Mathf.Clamp((1f - healthFrac) * (float)totalPts, 0f, totalPts));
	}

	private float ReturnHealthFractionWithOvershield()
	{
		float num = (100f - overshieldPercentage) / 100f;
		float num2 = subLiveMixin.maxHealth * num;
		return Mathf.Clamp01(subLiveMixin.health / num2);
	}

	private void ToggleLeakPointsBasedOnDamage()
	{
		int num = subLeaks.Length;
		float num2 = ReturnHealthFractionWithOvershield();
		int num3 = Mathf.CeilToInt(Mathf.Clamp((1f - num2) * (float)num, 0f, num));
		int num4 = 0;
		GameObject[] array = subLeaks;
		foreach (GameObject gameObject in array)
		{
			if (num4 < num3 && !gameObject.activeSelf)
			{
				gameObject.SetActive(value: true);
			}
			else if (num4 >= num3 && gameObject.activeSelf)
			{
				gameObject.SetActive(value: false);
			}
			num4++;
		}
	}
}
