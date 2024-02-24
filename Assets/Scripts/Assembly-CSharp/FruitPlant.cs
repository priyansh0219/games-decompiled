using System;
using System.Collections.Generic;
using Gendarme;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class FruitPlant : MonoBehaviour, IShouldSerialize
{
	public PickPrefab[] fruits;

	public float fruitSpawnInterval = 50f;

	private const bool defaultFruitSpawnEnabled = false;

	private const float defaultTimeNextFruit = -1f;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public bool fruitSpawnEnabled;

	[NonSerialized]
	[ProtoMember(3)]
	public float timeNextFruit = -1f;

	private List<PickPrefab> inactiveFruits = new List<PickPrefab>();

	private bool initialized;

	private void Start()
	{
		if (fruitSpawnEnabled)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		if (initialized)
		{
			return;
		}
		inactiveFruits.Clear();
		for (int i = 0; i < fruits.Length; i++)
		{
			fruits[i].pickedEvent.AddHandler(this, OnFruitHarvest);
			if (fruits[i].GetPickedState())
			{
				inactiveFruits.Add(fruits[i]);
			}
		}
		initialized = true;
	}

	private void Update()
	{
		if (fruitSpawnEnabled)
		{
			while (inactiveFruits.Count != 0 && DayNightCycle.main.timePassed >= (double)timeNextFruit)
			{
				PickPrefab random = inactiveFruits.GetRandom();
				random.SetPickedState(newPickedState: false);
				inactiveFruits.Remove(random);
				timeNextFruit += fruitSpawnInterval;
			}
		}
	}

	private void OnFruitHarvest(PickPrefab fruit)
	{
		if (inactiveFruits.Count == 0)
		{
			timeNextFruit = DayNightCycle.main.timePassedAsFloat + fruitSpawnInterval;
		}
		inactiveFruits.Add(fruit);
	}

	private void OnGrown()
	{
		for (int i = 0; i < fruits.Length; i++)
		{
			fruits[i].SetPickedState(newPickedState: true);
		}
		initialized = false;
		Initialize();
		fruitSpawnEnabled = true;
		timeNextFruit = DayNightCycle.main.timePassedAsFloat;
	}

	[SuppressMessage("Gendarme.Rules.Correctness", "AvoidFloatingPointEqualityRule")]
	public bool ShouldSerialize()
	{
		if (version == 1 && !fruitSpawnEnabled)
		{
			return timeNextFruit != -1f;
		}
		return true;
	}
}
