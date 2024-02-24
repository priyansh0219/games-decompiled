using ProtoBuf;
using UnityEngine;

[ProtoContract]
[ProtoInclude(1000, typeof(Pickupable))]
[ProtoInclude(1100, typeof(StorageContainer))]
[ProtoInclude(1200, typeof(DiveReelAnchor))]
[ProtoInclude(1300, typeof(UpgradeConsole))]
[ProtoInclude(1400, typeof(Sign))]
[ProtoInclude(1500, typeof(ColoredLabel))]
[ProtoInclude(1600, typeof(PickPrefab))]
[ProtoInclude(1800, typeof(ThermalPlant))]
[ProtoInclude(1900, typeof(GrownPlant))]
[ProtoInclude(1950, typeof(GrowingPlant))]
[ProtoInclude(2000, typeof(Vehicle))]
[ProtoInclude(3000, typeof(Constructable))]
[ProtoInclude(4000, typeof(SupplyCrate))]
[ProtoInclude(5000, typeof(Crafter))]
[ProtoInclude(6000, typeof(StarshipDoor))]
[ProtoInclude(7000, typeof(MedicalCabinet))]
[ProtoInclude(8000, typeof(MapRoomScreen))]
[ProtoInclude(9000, typeof(CyclopsLightingPanel))]
[ProtoInclude(10000, typeof(GhostPickupable))]
[ProtoInclude(11000, typeof(WeldableWallPanelGeneric))]
[ProtoInclude(12000, typeof(PrecursorDoorKeyColumn))]
[ProtoInclude(13000, typeof(PrecursorKeyTerminal))]
[ProtoInclude(14000, typeof(PrecursorTeleporterActivationTerminal))]
[ProtoInclude(15000, typeof(PrecursorDisableGunTerminal))]
public class HandTarget : MonoBehaviour
{
	private float creationTime;

	private bool _isvalid = true;

	public bool isValidHandTarget
	{
		get
		{
			if (_isvalid)
			{
				return Time.time - creationTime > 2f;
			}
			return false;
		}
		set
		{
			_isvalid = value;
		}
	}

	public virtual void Awake()
	{
		creationTime = Time.time;
	}
}
