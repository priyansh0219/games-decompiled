using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BaseAddConnectorGhost : BaseGhost
{
	public override void SetupGhost()
	{
		base.SetupGhost();
		ghostBase.SetSize(new Int3(1));
		ghostBase.SetConnector(Int3.zero);
		RebuildGhostGeometry();
	}

	public override bool UpdatePlacement(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
	{
		positionFound = false;
		geometryChanged = false;
		targetBase = BaseGhost.FindBase(camera);
		if (targetBase == null)
		{
			return false;
		}
		targetBase.SetPlacementGhost(this);
		float placeDefaultDistance = ghostModelParentConstructableBase.placeDefaultDistance;
		Vector3 position = camera.position;
		Vector3 forward = camera.forward;
		Vector3 point = position + forward * placeDefaultDistance;
		new Int3(1);
		Int3 @int = targetBase.WorldToGrid(point);
		if (targetBase.GetCell(@int) != 0 && targetBase.WorldToGrid(camera.position) == @int && targetBase.PickFace(camera, out var face))
		{
			@int = Base.GetAdjacent(face);
		}
		if (!targetBase.CanSetConnector(@int))
		{
			return false;
		}
		ghostModelParentConstructableBase.transform.position = targetBase.GridToWorld(@int);
		ghostModelParentConstructableBase.transform.rotation = targetBase.transform.rotation;
		positionFound = true;
		targetOffset = @int;
		if (ghostModelParentConstructableBase.transform.position.y > float.PositiveInfinity && BaseGhost.GetDistanceToGround(ghostModelParentConstructableBase.transform.position) > 25f)
		{
			return false;
		}
		return true;
	}
}
