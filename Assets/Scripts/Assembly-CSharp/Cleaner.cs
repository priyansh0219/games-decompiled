using System;
using Gendarme;
using UnityEngine;

public class Cleaner : MonoBehaviour
{
	private void Start()
	{
		InvokeRepeating("DoUnloadUnusedAssets", UnityEngine.Random.value, 1f);
		InvokeRepeating("DoCollectGarbage", UnityEngine.Random.value, 1f);
	}

	private void DoUnloadUnusedAssets()
	{
		Resources.UnloadUnusedAssets();
	}

	[SuppressMessage("Gendarme.Rules.BadPractice", "AvoidCallingProblematicMethodsRule")]
	private void DoCollectGarbage()
	{
		GC.Collect();
	}
}
