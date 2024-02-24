using System;
using System.Collections;
using System.Collections.Generic;
using Gendarme;
using UnityEngine;

[SuppressMessage("Gendarme.Rules.Serialization", "MarkAllNonSerializableFieldsRule")]
public class RebreatherDepthWarnings : MonoBehaviour
{
	[Serializable]
	public class DepthAlert
	{
		[NonSerialized]
		public bool alertCooldown;

		public PDANotification alert;

		public float alertDepth;
	}

	public float alertDelay = 20f;

	public List<DepthAlert> alerts;

	private float wasAtDepth;

	private void Update()
	{
		if (!Player.main.IsUnderwater())
		{
			return;
		}
		float depth = Player.main.GetDepth();
		foreach (DepthAlert alert in alerts)
		{
			if (!alert.alertCooldown && depth >= alert.alertDepth && wasAtDepth < alert.alertDepth && Inventory.main.equipment.GetCount(TechType.Rebreather) == 0 && (bool)alert.alert)
			{
				alert.alertCooldown = true;
				alert.alert.Play();
				StartCoroutine(ResetAlertCD(alert));
			}
		}
		wasAtDepth = depth;
	}

	private IEnumerator ResetAlertCD(DepthAlert alert)
	{
		yield return new WaitForSeconds(alertDelay);
		alert.alertCooldown = false;
	}
}
