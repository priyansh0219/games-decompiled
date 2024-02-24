using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class LavaShell : DamageModifier
{
	public float maxArmorPoints = 100f;

	[Range(0f, 1f)]
	public float armorEfficiency = 0.9f;

	public Transform shellTransform;

	public Transform noShellTransform;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public float armorPoints;

	private void Start()
	{
		SetShellEnabled(IsEnabled());
		DevConsole.RegisterConsoleCommand(this, "lavashell");
	}

	public override float ModifyDamage(float damage, DamageType type)
	{
		if (type == DamageType.Heat || type == DamageType.Fire)
		{
			if (IsEnabled())
			{
				AddArmorPoints(damage);
			}
			return 0f;
		}
		if (!IsEnabled())
		{
			return damage;
		}
		float num = Mathf.Min(damage * armorEfficiency, armorPoints);
		TakeDamage(num);
		return damage - num;
	}

	public float GetArmorPoints()
	{
		return armorPoints;
	}

	public bool IsEnabled()
	{
		return armorPoints > 0f;
	}

	public float GetArmorFraction()
	{
		return armorPoints / maxArmorPoints;
	}

	public void Activate()
	{
		armorPoints = maxArmorPoints;
		SetShellEnabled(enabled: true);
	}

	public void TakeDamage(float damage)
	{
		if (IsEnabled() && damage > 0f)
		{
			AddArmorPoints(0f - damage);
		}
	}

	private void AddArmorPoints(float points)
	{
		armorPoints = Mathf.Clamp(armorPoints + points, 0f, maxArmorPoints);
		if (armorPoints == 0f)
		{
			SetShellEnabled(enabled: false);
		}
	}

	private void SetShellEnabled(bool enabled)
	{
		shellTransform.gameObject.SetActive(enabled);
		noShellTransform.gameObject.SetActive(!enabled);
	}

	private void OnConsoleCommand_lavashell(NotificationCenter.Notification n)
	{
		Activate();
	}
}
