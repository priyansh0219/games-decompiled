using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class TechFragment : MonoBehaviour
{
	[Serializable]
	public class RandomTech
	{
		public TechType techType;

		public float chance;
	}

	[AssertNotNull]
	public List<RandomTech> techList;

	public TechType defaultTechType;

	[NonSerialized]
	[ProtoMember(1)]
	public bool techTypeChosen;

	private TechType GetRandomTech()
	{
		TechType techType = defaultTechType;
		for (int i = 0; i < techList.Count; i++)
		{
			RandomTech randomTech = techList[i];
			if (UnityEngine.Random.value <= randomTech.chance)
			{
				techType = randomTech.techType;
				break;
			}
		}
		return techType;
	}

	private IEnumerator Start()
	{
		TechType techType = TechType.None;
		if (techTypeChosen)
		{
			Pickupable component = GetComponent<Pickupable>();
			if (component != null)
			{
				techType = component.GetTechType();
			}
		}
		else
		{
			techType = GetRandomTech();
		}
		if (techType != 0)
		{
			CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
			yield return request;
			GameObject result = request.GetResult();
			if (result != null)
			{
				UnityEngine.Object.Instantiate(result).GetComponent<Transform>().position = base.transform.position;
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else
		{
			Debug.LogError("TechFragment : Start() : Random TechType = None");
		}
	}
}
