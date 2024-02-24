using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class SpecialHullPlate : MonoBehaviour
{
	public TextMeshProUGUI serialNumber;

	public TextMeshProUGUI date;

	private IEnumerator Start()
	{
		PlatformServices services;
		while ((services = PlatformUtils.main.GetServices()) == null)
		{
			yield return null;
		}
		IEconomyItems economyItems = services.GetEconomyItems();
		if (economyItems != null)
		{
			while (!economyItems.IsReady)
			{
				yield return null;
			}
			if (economyItems.HasItem(TechType.SpecialHullPlate))
			{
				serialNumber.text = economyItems.GetItemProperty(TechType.SpecialHullPlate, "serial_number").PadLeft(5, '0');
				DateTime.TryParse(economyItems.GetItemProperty(TechType.SpecialHullPlate, "created_at"), out var result);
				date.text = $"{result:dd/MM/yyyy}";
			}
		}
	}
}
