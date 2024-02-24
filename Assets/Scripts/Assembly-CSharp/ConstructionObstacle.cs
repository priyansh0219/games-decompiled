using UnityEngine;

public class ConstructionObstacle : MonoBehaviour, IObstacle
{
	[AssertLocalization(AssertLocalizationAttribute.Options.AllowEmptyString)]
	public string reason;

	public bool deconstructionObstacle;

	public bool IsDeconstructionObstacle()
	{
		return deconstructionObstacle;
	}

	public bool CanDeconstruct(out string r)
	{
		r = (string.IsNullOrEmpty(reason) ? null : Language.main.Get(reason));
		return false;
	}
}
