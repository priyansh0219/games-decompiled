using System;
using System.Collections.Generic;
using UnityEngine;

public class LootSpawner : MonoBehaviour
{
	public static LootSpawner main;

	public List<TechType> escapePodTechTypes;

	public List<TechType> supplyTechTypes;

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
		DevConsole.RegisterConsoleCommand(this, "randomloot");
		DevConsole.RegisterConsoleCommand(this, "randomsupplyloot");
	}

	public TechType[] GetEscapePodStorageTechTypes()
	{
		return escapePodTechTypes.ToArray();
	}

	public TechType[] GetSupplyTechTypes(int count)
	{
		return GetRandomTechTypes(supplyTechTypes, count);
	}

	private TechType[] GetRandomTechTypes(List<TechType> list, int count)
	{
		int count2 = list.Count;
		count = Math.Min(count, count2);
		TechType[] array = new TechType[count];
		List<int> numbers = new List<int>();
		MathExtensions.UniqueRandomNumbersInRange(0, count2, count, ref numbers);
		for (int i = 0; i < numbers.Count; i++)
		{
			array[i] = list[numbers[i]];
		}
		return array;
	}
}
