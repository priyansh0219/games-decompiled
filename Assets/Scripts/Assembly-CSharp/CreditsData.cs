using UnityEngine;

[CreateAssetMenu(fileName = "CreditsData.asset", menuName = "Subnautica/Create CreditsData Asset")]
public class CreditsData : ScriptableObject
{
	public CreditsEntry[] entries;
}
