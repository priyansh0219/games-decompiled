using System;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("SpecimenAnalyzer is a prototype class, not for the shipping game!", false)]
public class SpecimenAnalyzer : MonoBehaviour
{
	public StorageContainer storageContainer;

	public FMOD_CustomLoopingEmitter processingLoop;

	private TechType analyzingTech;

	private float analyzingTime;

	private bool subscribed;

	private bool isDirty;

	private static readonly Dictionary<TechType, TechType> researchList = new Dictionary<TechType, TechType>(TechTypeExtensions.sTechTypeComparer)
	{
		{
			TechType.ReefbackShell,
			TechType.ReefbackEgg
		},
		{
			TechType.ReefbackAdvancedStructure,
			TechType.ReefbackEgg
		},
		{
			TechType.ReefbackTissue,
			TechType.ReefbackEgg
		}
	};

	private void Awake()
	{
	}

	private void OnEnable()
	{
		storageContainer.enabled = true;
		isDirty = true;
	}

	private void OnDisable()
	{
		Subscribe(state: false);
		storageContainer.enabled = false;
		analyzingTech = TechType.None;
		analyzingTime = 0f;
		processingLoop.Stop();
	}

	private void LateUpdate()
	{
		Subscribe(state: true);
		Scan();
		ProcessResearch();
		if (analyzingTech != 0)
		{
			processingLoop.Play();
		}
		else
		{
			processingLoop.Stop();
		}
	}

	public string GetStatusText()
	{
		if (analyzingTech == TechType.None)
		{
			if (storageContainer.container.count == 0)
			{
				return Language.main.Get("SpecimenAnalyzerEmpty");
			}
			return Language.main.Get("SpecimenAnalyzerIdle");
		}
		string arg = "";
		int arg2 = Mathf.FloorToInt(analyzingTime / 300f * 100f);
		return Language.main.GetFormat("SpecimenAnalyzing", Language.main.Get(analyzingTech.AsString()), arg, arg2);
	}

	private void OnItemsChanged(InventoryItem item)
	{
		isDirty = true;
	}

	private void Subscribe(bool state)
	{
		if (subscribed != state)
		{
			if (subscribed)
			{
				storageContainer.container.onAddItem -= OnItemsChanged;
				storageContainer.container.onRemoveItem -= OnItemsChanged;
			}
			else
			{
				storageContainer.container.onAddItem += OnItemsChanged;
				storageContainer.container.onRemoveItem += OnItemsChanged;
			}
			subscribed = state;
		}
	}

	private bool ScanGenericEgg(TechType techType, out TechType eggType)
	{
		bool result = false;
		eggType = TechType.None;
		if (techType == TechType.GrassyPlateausEgg)
		{
			eggType = ((UnityEngine.Random.value > 0.5f) ? TechType.StalkerEgg : TechType.ReefbackEgg);
			result = true;
		}
		return result;
	}

	private TechType GetTechTypeInside()
	{
		TechType result = TechType.None;
		List<TechType> itemTypes = storageContainer.container.GetItemTypes();
		if (itemTypes.Count > 0)
		{
			result = itemTypes[0];
		}
		return result;
	}

	private void SetTechTypeInside(TechType eggType)
	{
		List<TechType> itemTypes = storageContainer.container.GetItemTypes();
		Pickupable item = storageContainer.container.GetItems(itemTypes[0])[0].item;
		storageContainer.container.RemoveItem(item);
		item.SetTechTypeOverride(eggType);
		storageContainer.container.AddItem(item);
	}

	private void Scan()
	{
		if (isDirty)
		{
			isDirty = false;
			analyzingTech = TechType.None;
			TechType techTypeInside = GetTechTypeInside();
			TechType eggType = TechType.None;
			if (ScanGenericEgg(techTypeInside, out eggType))
			{
				SetTechTypeInside(eggType);
			}
		}
	}

	private TechType GetResearchOutput(TechType techType)
	{
		TechType result = TechType.None;
		foreach (KeyValuePair<TechType, TechType> research in researchList)
		{
			if (research.Value == techType && !KnownTech.Contains(research.Key))
			{
				result = research.Key;
				break;
			}
		}
		return result;
	}

	private void ProcessResearch()
	{
		if (analyzingTech == TechType.None)
		{
			TechType techTypeInside = GetTechTypeInside();
			if (techTypeInside != 0 && GetResearchOutput(techTypeInside) != 0)
			{
				analyzingTech = techTypeInside;
				analyzingTime = 0f;
				ErrorMessage.AddError(Language.main.GetFormat("SpecimenAnalysisCommencing", Language.main.Get(techTypeInside.AsString())));
			}
			return;
		}
		float deltaTime = Time.deltaTime;
		analyzingTime += deltaTime;
		float num = 10f;
		if (analyzingTime >= num)
		{
			TechType researchOutput = GetResearchOutput(analyzingTech);
			KnownTech.Add(researchOutput);
			ErrorMessage.AddError(Language.main.GetFormat("SpecimenAnalysisComplete", Language.main.Get(researchOutput.AsString())));
			FMODUWE.PlayOneShot("event:/player/new_tech", base.transform.position);
			analyzingTech = TechType.None;
			analyzingTime = 0f;
		}
	}
}
