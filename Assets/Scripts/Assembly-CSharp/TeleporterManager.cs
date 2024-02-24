using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class TeleporterManager : MonoBehaviour
{
	public delegate void TeleporterActivate(string tpID);

	public static TeleporterManager main;

	private const int currentVersion = 2;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 2;

	[NonSerialized]
	[ProtoMember(2)]
	public readonly HashSet<string> activeTeleporters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	public static event TeleporterActivate TeleporterActivateEvent;

	private void Start()
	{
		main = this;
	}

	public static void ActivateTeleporter(string identifier)
	{
		main.activeTeleporters.Add(identifier);
		if (TeleporterManager.TeleporterActivateEvent != null)
		{
			TeleporterManager.TeleporterActivateEvent(identifier);
		}
	}

	public static bool GetTeleporterActive(string identifier)
	{
		return main.activeTeleporters.Contains(identifier);
	}
}
