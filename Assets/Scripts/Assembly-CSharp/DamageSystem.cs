using System.Collections.Generic;
using UWE;
using UnityEngine;

public class DamageSystem
{
	public static float damageMultiplier = 1f;

	public static bool instagib = false;

	public static TechType[] acidImmune = new TechType[15]
	{
		TechType.Reginald,
		TechType.Spinefish,
		TechType.HoopfishSchool,
		TechType.Bleeder,
		TechType.GhostLeviathan,
		TechType.GhostLeviathanJuvenile,
		TechType.GhostRayBlue,
		TechType.RedGreenTentacle,
		TechType.RedGreenTentacleSeed,
		TechType.Shuttlebug,
		TechType.SpineEel,
		TechType.CrabSquid,
		TechType.CaveCrawler,
		TechType.Exosuit,
		TechType.Cyclops
	};

	private static HashSet<LiveMixin> damagedLiveMixins = new HashSet<LiveMixin>();

	public static bool IsAcidImmune(GameObject go)
	{
		TechType techType = CraftData.GetTechType(go);
		if (techType != 0)
		{
			for (int i = 0; i < acidImmune.Length; i++)
			{
				if (techType == acidImmune[i])
				{
					return true;
				}
			}
		}
		return false;
	}

	public static float CalculateDamage(float damage, DamageType type, GameObject target, GameObject dealer = null)
	{
		damage *= damageMultiplier;
		DamageModifier[] componentsInChildren = target.GetComponentsInChildren<DamageModifier>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			damage = componentsInChildren[i].ModifyDamage(damage, type);
		}
		bool flag = target.GetComponent<Player>();
		Sealed component = target.GetComponent<Sealed>();
		bool flag2 = component != null && component.IsSealed();
		switch (type)
		{
		case DamageType.Heat:
		case DamageType.Fire:
		{
			if ((bool)target.GetComponent<Living>() || flag)
			{
				damage *= 2f;
			}
			HeatResistGene component2 = target.GetComponent<HeatResistGene>();
			if ((bool)component2)
			{
				damage -= 0.75f * component2.Scalar * damage;
			}
			break;
		}
		case DamageType.Radiation:
		{
			if (!flag)
			{
				break;
			}
			if ((bool)(Player.main.GetVehicle() as Exosuit))
			{
				damage = 0f;
				break;
			}
			float num = damage;
			if (Inventory.main.equipment.GetCount(TechType.RadiationSuit) > 0)
			{
				damage -= num * 0.5f;
			}
			if (Inventory.main.equipment.GetCount(TechType.RadiationHelmet) > 0)
			{
				damage -= num * 0.23f;
			}
			if (Inventory.main.equipment.GetCount(TechType.RadiationHelmet) > 0)
			{
				damage -= num * 0.23f;
			}
			break;
		}
		case DamageType.LaserCutter:
			if (!flag2)
			{
				damage *= 0.5f;
			}
			break;
		case DamageType.Poison:
			if (CreatureData.GetBehaviourType(target) == BehaviourType.SmallFish || CraftData.GetTechType(target) == TechType.Gasopod || CraftData.GetTechType(target) == TechType.MapRoomCamera || target.GetComponent<Vehicle>() != null)
			{
				damage = 0f;
			}
			break;
		case DamageType.Acid:
			if (IsAcidImmune(target))
			{
				damage = 0f;
			}
			else if (target.GetComponent<Vehicle>() != null)
			{
				damage *= 0.05f;
			}
			break;
		case DamageType.Collide:
			if (dealer != null && dealer.GetComponent<Vehicle>() != null && target.GetComponentInParent<Base>() != null)
			{
				damage = 0f;
			}
			break;
		}
		if (flag2 && type != DamageType.LaserCutter)
		{
			damage = 0f;
		}
		if (flag && type != DamageType.Radiation && type != DamageType.Starve)
		{
			float num2 = 0f;
			if (Player.main.HasReinforcedSuit())
			{
				num2 += 0.4f;
			}
			if (Player.main.HasReinforcedGloves())
			{
				num2 += 0.12f;
			}
			damage -= damage * num2;
		}
		if ((bool)NoDamageConsoleCommand.main && NoDamageConsoleCommand.main.GetNoDamageCheat())
		{
			damage = 0f;
		}
		if (instagib && damage > 0f)
		{
			LiveMixin component3 = target.GetComponent<LiveMixin>();
			if ((bool)component3)
			{
				damage = component3.maxHealth * 100f;
			}
		}
		return damage;
	}

	public static void RadiusDamage(float maxDamage, Vector3 position, float detonateRadius, DamageType type = DamageType.Normal, GameObject sourceObject = null)
	{
		int num = UWE.Utils.OverlapSphereIntoSharedBuffer(position, detonateRadius);
		Vector3 position2 = default(Vector3);
		if (sourceObject != null)
		{
			position2 = sourceObject.transform.position;
		}
		for (int i = 0; i < num; i++)
		{
			if (UWE.Utils.sharedColliderBuffer[i].gameObject != sourceObject)
			{
				LiveMixin liveMixin = Utils.FindAncestorWithComponent<LiveMixin>(UWE.Utils.sharedColliderBuffer[i].gameObject);
				if ((bool)liveMixin && damagedLiveMixins.Add(liveMixin))
				{
					float magnitude = (position - liveMixin.gameObject.transform.position).magnitude;
					float num2 = Mathf.Max(0f, (detonateRadius - magnitude) / detonateRadius);
					liveMixin.TakeDamage(maxDamage * num2, position2, type);
				}
			}
		}
		damagedLiveMixins.Clear();
		WorldForces.AddExplosion(position, DayNightCycle.main.timePassed, maxDamage / 10f, detonateRadius);
	}

	public static bool GetDamageTypeLoops(DamageType damageType)
	{
		if (damageType != DamageType.Heat && damageType != DamageType.Fire && damageType != DamageType.Pressure && damageType != DamageType.Poison && damageType != DamageType.Acid && damageType != DamageType.Drill)
		{
			return damageType == DamageType.Cold;
		}
		return true;
	}
}
