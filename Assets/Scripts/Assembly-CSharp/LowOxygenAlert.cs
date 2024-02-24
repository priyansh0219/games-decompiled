using System;
using System.Collections.Generic;
using Gendarme;
using UnityEngine;

[SuppressMessage("Gendarme.Rules.Serialization", "MarkAllNonSerializableFieldsRule")]
public class LowOxygenAlert : MonoBehaviour
{
	[Serializable]
	public class Alert
	{
		public PDANotification notification;

		public FMOD_CustomEmitter soundSFX;

		public int oxygenTriggerSeconds = 50;

		public float minO2Capacity = 60f;

		public float minDepth;
	}

	public List<Alert> alertList = new List<Alert>();

	private Utils.ScalarMonitor secondsMonitor = new Utils.ScalarMonitor(100f);

	private Player player;

	private float lastOxygenCapacity;

	private void Start()
	{
		player = Utils.GetLocalPlayerComp();
		lastOxygenCapacity = 0f;
	}

	private void Update()
	{
		secondsMonitor.Update(player.GetOxygenAvailable());
		float oxygenCapacity = player.GetOxygenCapacity();
		if (!GameModeUtils.IsOptionActive(GameModeOption.NoHints) && (Utils.NearlyEqual(oxygenCapacity, lastOxygenCapacity) || oxygenCapacity < lastOxygenCapacity))
		{
			for (int num = alertList.Count - 1; num >= 0; num--)
			{
				Alert alert = alertList[num];
				if (oxygenCapacity >= alert.minO2Capacity && secondsMonitor.JustDroppedBelow(alert.oxygenTriggerSeconds) && Ocean.GetDepthOf(Utils.GetLocalPlayer()) > alert.minDepth && (player.IsSwimming() || (player.GetMode() == Player.Mode.LockedPiloting && !player.IsInsidePoweredVehicle()) || (player.IsInSub() && !player.CanBreathe())))
				{
					Subtitles.Add(alert.notification.text);
					alert.soundSFX.Play();
					break;
				}
			}
		}
		lastOxygenCapacity = oxygenCapacity;
	}
}
