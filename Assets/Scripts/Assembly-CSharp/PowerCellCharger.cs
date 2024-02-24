using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class PowerCellCharger : Charger
{
	private static readonly HashSet<TechType> compatibleTech = new HashSet<TechType>
	{
		TechType.PowerCell,
		TechType.PrecursorIonPowerCell
	};

	protected override HashSet<TechType> allowedTech => compatibleTech;

	protected override string labelInteract => "PowerCellChargerInteract";

	protected override string labelStorage => "PowerCellChargerLabel";

	protected override string labelIncompatibleItem => "PowerCellChargerIncompatibleItem";

	protected override string labelCantDeconstruct => "PowerCellChargerCantDeconstruct";

	protected override float animTimeOpen => 0.5f;

	protected override bool Initialize()
	{
		if (base.Initialize())
		{
			animParamOpen = Animator.StringToHash("opened");
		}
		return false;
	}
}
