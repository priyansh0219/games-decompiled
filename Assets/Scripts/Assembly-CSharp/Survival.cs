using System;
using ProtoBuf;
using UWE;
using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(LiveMixin))]
[ProtoContract]
public class Survival : MonoBehaviour
{
	private Player player;

	private LiveMixin liveMixin;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[ProtoMember(2)]
	public float food;

	[ProtoMember(3)]
	public float water;

	[ProtoMember(4)]
	public float stomach;

	private float kUpdateHungerInterval = 10f;

	public PDANotification[] foodWarningSounds;

	public PDANotification[] waterWarningSounds;

	[AssertNotNull]
	public PDANotification vitalsOkNotification;

	public bool freezeStats;

	public Event<float> onEat = new Event<float>();

	public Event<float> onDrink = new Event<float>();

	[AssertNotNull]
	public FMODAsset curedSound;

	[AssertLocalization]
	private const string healthFullMessage = "HealthFull";

	private void Start()
	{
		player = base.gameObject.GetComponent<Player>();
		player.playerRespawnEvent.AddHandler(base.gameObject, OnRespawn);
		liveMixin = base.gameObject.GetComponent<LiveMixin>();
		liveMixin.onHealTempDamage.AddHandler(base.gameObject, OnHealTempDamage);
		InvokeRepeating("UpdateHunger", 0f, kUpdateHungerInterval);
		ResetStats();
	}

	private void UpdateHunger()
	{
		if (GameModeUtils.RequiresSurvival() && !freezeStats)
		{
			float num = UpdateStats(kUpdateHungerInterval);
			if ((bool)liveMixin && num > float.Epsilon)
			{
				liveMixin.TakeDamage(num, player.transform.position, DamageType.Starve);
			}
			if (food + water >= 150f && (bool)liveMixin)
			{
				float num2 = 1f / 24f;
				liveMixin.AddHealth(num2 * kUpdateHungerInterval);
			}
		}
	}

	private void OnRespawn(Player p)
	{
		ResetStats();
	}

	public void ResetStats()
	{
		food = 50.5f;
		water = 90.5f;
	}

	public void OnHealTempDamage(float damage)
	{
		food = Mathf.Clamp(food - damage * 0.25f, 0f, 200f);
	}

	public float UpdateStats(float timePassed)
	{
		float num = 0f;
		if (timePassed > float.Epsilon)
		{
			float prevVal = food;
			float prevVal2 = water;
			float num2 = timePassed / 2520f * 100f;
			if (num2 > food)
			{
				num += (num2 - food) * 25f;
			}
			food = Mathf.Clamp(food - num2, 0f, 200f);
			float num3 = timePassed / 1800f * 100f;
			if (num3 > water)
			{
				num += (num3 - water) * 25f;
			}
			water = Mathf.Clamp(water - num3, 0f, 100f);
			UpdateWarningSounds(foodWarningSounds, food, prevVal, 20f, 10f);
			UpdateWarningSounds(waterWarningSounds, water, prevVal2, 20f, 10f);
		}
		return num;
	}

	private void UpdateWarningSounds(PDANotification[] soundList, float val, float prevVal, float threshold1, float threshold2)
	{
		int num = -1;
		if (val <= 0f && prevVal > 0f)
		{
			num = 2;
		}
		else if (val < threshold2 && prevVal >= threshold2)
		{
			num = 1;
		}
		else if (val < threshold1 && prevVal >= threshold1)
		{
			num = 0;
		}
		if (num != -1 && soundList != null && num >= 0 && num < soundList.Length && soundList[num] != null)
		{
			soundList[num].Play();
		}
	}

	public bool Eat(GameObject useObj)
	{
		bool flag = false;
		if (useObj != null)
		{
			Eatable component = useObj.GetComponent<Eatable>();
			if (component != null)
			{
				if (component.GetFoodValue() != 0f)
				{
					if (food <= 99f)
					{
						food = Mathf.Clamp(food + component.GetFoodValue(), 0f, 200f);
					}
					onEat.Trigger(component.GetFoodValue());
					if (component.GetFoodValue() > 0f)
					{
						GoalManager.main.OnCustomGoalEvent("Eat_Something");
					}
					flag = true;
				}
				if (component.GetWaterValue() != 0f)
				{
					water = Mathf.Clamp(water + component.GetWaterValue(), 0f, 100f);
					onDrink.Trigger(component.GetWaterValue());
					if (component.GetWaterValue() > 0f)
					{
						GoalManager.main.OnCustomGoalEvent("Drink_Something");
					}
					flag = true;
				}
				if ((food > 20f && food - component.GetFoodValue() < 20f) || (water > 20f && water - component.GetWaterValue() < 20f))
				{
					vitalsOkNotification.Play();
				}
			}
			if (flag)
			{
				TechType techType = CraftData.GetTechType(useObj);
				if (techType == TechType.None)
				{
					Pickupable component2 = useObj.GetComponent<Pickupable>();
					if ((bool)component2)
					{
						techType = component2.GetTechType();
					}
				}
				FMODUWE.PlayOneShot(TechData.GetSoundUse(techType), Player.main.transform.position);
				if (techType == TechType.Bladderfish)
				{
					Player.main.GetComponent<OxygenManager>().AddOxygen(15f);
				}
			}
		}
		return flag;
	}

	public float GetWeaknessSpeedScalar()
	{
		float num = 1f;
		if (food < 20f)
		{
			num -= (20f - food) * 0.02f;
		}
		if (water < 20f)
		{
			num -= (20f - water) * 0.02f;
		}
		return num;
	}

	public bool Use(GameObject useObj)
	{
		bool flag = false;
		if (useObj != null)
		{
			TechType techType = CraftData.GetTechType(useObj);
			if (techType == TechType.None)
			{
				Pickupable component = useObj.GetComponent<Pickupable>();
				if ((bool)component)
				{
					techType = component.GetTechType();
				}
			}
			if (techType == TechType.FirstAidKit)
			{
				if (Player.main.GetComponent<LiveMixin>().AddHealth(50f) > 0.1f)
				{
					flag = true;
				}
				else
				{
					ErrorMessage.AddMessage(Language.main.Get("HealthFull"));
				}
			}
			if (techType == TechType.EnzymeCureBall)
			{
				Debug.LogWarningFormat(this, "Code should be unreachable for the time being.");
				InfectedMixin component2 = Utils.GetLocalPlayer().gameObject.GetComponent<InfectedMixin>();
				if (component2.IsInfected())
				{
					component2.RemoveInfection();
					Utils.PlayFMODAsset(curedSound, base.transform);
					flag = true;
				}
			}
			if (flag)
			{
				FMODUWE.PlayOneShot(TechData.GetSoundUse(techType), Player.main.transform.position);
			}
		}
		return flag;
	}
}
