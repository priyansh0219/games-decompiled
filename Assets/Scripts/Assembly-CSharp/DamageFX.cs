using System;
using System.Collections.Generic;
using UnityEngine;

public class DamageFX : MonoBehaviour
{
	[Serializable]
	public class DamageEffect
	{
		public Texture texture;

		public float minDamage;

		public float maxDamage;

		public float holdTime;

		public float fadeTime;
	}

	public static DamageFX main;

	private DamageScreenFX[] damageScreenFXs;

	[AssertNotNull]
	public Texture bloodTexture;

	[AssertNotNull]
	public Texture coldTexture;

	[AssertNotNull]
	public Texture heatTexture;

	[AssertNotNull]
	public List<DamageEffect> damageEffectList;

	public Damage damagePrefab;

	private void Awake()
	{
		main = this;
	}

	public void AddHudDamage(float damageScalar, Vector3 damageSource, DamageInfo damageInfo)
	{
		CreateImpactEffect(damageScalar, damageSource, damageInfo.type);
		PlayScreenFX(damageInfo);
	}

	public void CreateImpactEffect(float damageScalar, Vector3 damageSource, DamageType type)
	{
		if (type != 0 && type != DamageType.Pressure && type != DamageType.Collide && type != DamageType.Explosive)
		{
			return;
		}
		Damage component = Utils.SpawnFromPrefab(damagePrefab.gameObject, base.transform).GetComponent<Damage>();
		DamageEffect damageEffect = damageEffectList[0];
		for (int i = 1; i < damageEffectList.Count; i++)
		{
			DamageEffect damageEffect2 = damageEffectList[i];
			if (damageEffect2 != null && damageScalar >= damageEffect2.minDamage && damageScalar <= damageEffect2.maxDamage)
			{
				damageEffect = damageEffect2;
			}
		}
		component.Init(damageEffect.holdTime, damageEffect.fadeTime, damageEffect.texture, damageSource);
	}

	public void PlayScreenFX(DamageInfo damageInfo)
	{
		if (damageScreenFXs == null && SNCameraRoot.main != null)
		{
			damageScreenFXs = SNCameraRoot.main.gameObject.GetComponentsInChildren<DamageScreenFX>(includeInactive: true);
		}
		for (int i = 0; i < damageScreenFXs.Length; i++)
		{
			damageScreenFXs[i].Play(damageInfo);
		}
	}

	public void ClearHudDamage()
	{
		foreach (Transform item in base.transform)
		{
			Damage component = item.gameObject.GetComponent<Damage>();
			if ((bool)component)
			{
				UnityEngine.Object.DestroyObject(component.gameObject);
			}
		}
		if (damageScreenFXs != null)
		{
			for (int i = 0; i < damageScreenFXs.Length; i++)
			{
				damageScreenFXs[i].ClearAll();
			}
		}
	}
}
