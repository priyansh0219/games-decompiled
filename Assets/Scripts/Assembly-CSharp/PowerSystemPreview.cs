using UnityEngine;

public class PowerSystemPreview : MonoBehaviour
{
	public PowerFX previewPowerFX;

	public PowerRelay powerRelay;

	private void Start()
	{
	}

	private void Update()
	{
		GameObject ghostModel = Builder.GetGhostModel();
		if (ghostModel != null)
		{
			PowerRelay component = ghostModel.GetComponent<PowerRelay>();
			if (component != null && powerRelay.IsNearestValidRelay(component))
			{
				previewPowerFX.SetTarget(ghostModel);
			}
			else
			{
				previewPowerFX.SetTarget(null);
			}
		}
		else
		{
			previewPowerFX.SetTarget(null);
		}
	}
}
