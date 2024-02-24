using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BatteryCharger : Charger
{
	private static readonly HashSet<TechType> compatibleTech = new HashSet<TechType>
	{
		TechType.Battery,
		TechType.LithiumIonBattery,
		TechType.PrecursorIonBattery
	};

	protected override HashSet<TechType> allowedTech => compatibleTech;

	protected override string labelInteract => "BatteryChargerInteract";

	protected override string labelStorage => "BatteryChargerStorageLabel";

	protected override string labelIncompatibleItem => "BatteryChargerIncompatibleItem";

	protected override string labelCantDeconstruct => "BatteryChargerCantDeconstruct";

	protected override float animTimeOpen => 0.9f;

	protected override bool Initialize()
	{
		if (base.Initialize())
		{
			animParamOpen = Animator.StringToHash("opened");
		}
		return false;
	}
}
