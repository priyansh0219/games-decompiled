using System.Collections;

public interface IEconomyItems
{
	bool IsReady { get; }

	IEnumerator InitializeAsync();

	bool HasItem(TechType techType);

	string GetItemProperty(TechType techType, string key);
}
