using UnityEngine;

public class BaseSurfaceModel : MonoBehaviour, IBaseGhostModel
{
	[AssertNotNull]
	public GameObject aboveWaterModel;

	[AssertNotNull]
	public GameObject underWaterModel;

	public const float threshHold = 1f;

	public void Start()
	{
		((IBaseGhostModel)this).BuildModel(isGhost: false);
	}

	void IBaseGhostModel.BuildModel(bool isGhost)
	{
		bool flag = base.transform.position.y >= 1f;
		if ((bool)aboveWaterModel)
		{
			aboveWaterModel.SetActive(flag);
		}
		if ((bool)underWaterModel)
		{
			underWaterModel.SetActive(!flag);
		}
	}
}
