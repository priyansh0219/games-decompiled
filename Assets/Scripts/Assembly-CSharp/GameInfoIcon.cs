using System.Collections.Generic;
using UnityEngine;

public class GameInfoIcon : MonoBehaviour
{
	public TechType techType;

	private static readonly Dictionary<TechType, int> presentTypes = new Dictionary<TechType, int>(TechTypeExtensions.sTechTypeComparer);

	private void OnEnable()
	{
		Add(techType);
	}

	private void OnDisable()
	{
		Remove(techType);
	}

	private static void Add(TechType techType)
	{
		int orDefault = presentTypes.GetOrDefault(techType, 0);
		presentTypes[techType] = orDefault + 1;
	}

	private static void Remove(TechType techType)
	{
		int orDefault = presentTypes.GetOrDefault(techType, 1);
		presentTypes[techType] = orDefault - 1;
	}

	public static bool Has(TechType techType)
	{
		return presentTypes.GetOrDefault(techType, 0) > 0;
	}

	public static void Deinitialize()
	{
		presentTypes.Clear();
	}
}
