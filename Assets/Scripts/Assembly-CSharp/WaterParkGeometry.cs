using UnityEngine;

public class WaterParkGeometry : MonoBehaviour, IBaseModuleGeometry, IObstacle
{
	[AssertLocalization]
	private const string playerObstacle = "PlayerObstacle";

	[AssertLocalization]
	private const string deconstructNonEmptyMessage = "DeconstructNonEmptyWaterParkError";

	private Base.Face _geometryFace;

	public Base.Face geometryFace
	{
		get
		{
			return _geometryFace;
		}
		set
		{
			_geometryFace = value;
		}
	}

	public virtual WaterPark GetModule()
	{
		Base componentInParent = GetComponentInParent<Base>();
		if (componentInParent != null)
		{
			Base.Face face = geometryFace;
			Base.Face face2;
			do
			{
				face2 = face;
				face.cell.y--;
			}
			while (componentInParent.GetFace(face) == Base.FaceType.WaterPark);
			return componentInParent.GetModule(face2) as WaterPark;
		}
		return null;
	}

	public bool IsDeconstructionObstacle()
	{
		return true;
	}

	public bool CanDeconstruct(out string reason)
	{
		reason = null;
		WaterPark module = GetModule();
		if (module != null)
		{
			if (Player.main.currentWaterPark == module)
			{
				reason = Language.main.Get("PlayerObstacle");
				return false;
			}
			if (!module.IsConnected() && module.HasItemsInside())
			{
				reason = Language.main.Get("DeconstructNonEmptyWaterParkError");
				return false;
			}
		}
		return true;
	}
}
