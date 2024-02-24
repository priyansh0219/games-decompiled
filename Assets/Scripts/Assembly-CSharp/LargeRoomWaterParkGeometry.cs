public class LargeRoomWaterParkGeometry : WaterParkGeometry
{
	public override WaterPark GetModule()
	{
		Base componentInParent = GetComponentInParent<Base>();
		if (componentInParent != null)
		{
			return componentInParent.GetModule(base.geometryFace) as WaterPark;
		}
		return null;
	}
}
