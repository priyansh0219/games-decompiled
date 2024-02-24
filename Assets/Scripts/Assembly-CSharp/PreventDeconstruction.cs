using UnityEngine;

public class PreventDeconstruction : MonoBehaviour
{
	public bool inCyclops;

	public bool inBase;

	public bool inEscapePod;

	private void Start()
	{
		Constructable component = GetComponent<Constructable>();
		if (!(component != null))
		{
			return;
		}
		if (inEscapePod && GetComponentInParent<EscapePod>() != null)
		{
			component.deconstructionAllowed = false;
		}
		if (inBase)
		{
			SubRoot componentInParent = GetComponentInParent<SubRoot>();
			if (componentInParent != null && componentInParent.isBase)
			{
				component.deconstructionAllowed = false;
			}
		}
		if (inCyclops)
		{
			SubRoot componentInParent2 = GetComponentInParent<SubRoot>();
			if (componentInParent2 != null && !componentInParent2.isBase)
			{
				component.deconstructionAllowed = false;
			}
		}
	}
}
