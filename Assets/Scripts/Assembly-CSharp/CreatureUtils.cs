using UnityEngine;

[DisallowMultipleComponent]
public class CreatureUtils : MonoBehaviour
{
	public bool setupEcoTarget = true;

	public bool setupEcoBehaviours = true;

	[AssertNotNull(AssertNotNullAttribute.Options.AllowEmptyCollection)]
	public Component[] addedComponents;
}
