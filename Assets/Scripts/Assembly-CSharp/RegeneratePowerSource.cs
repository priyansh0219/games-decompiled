using System.Collections;
using UnityEngine;

public class RegeneratePowerSource : MonoBehaviour
{
	[AssertNotNull]
	public PowerSource powerSource;

	public float regenerationThreshhold = 25f;

	public float regenerationInterval = 20f;

	public float regenerationAmount = 1f;

	[AssertLocalization(2)]
	private const string powerCellStatusFormatKey = "PowerCellStatus";

	[AssertLocalization]
	private const string regenPowerCellKey = "RegenPowerCell";

	private IEnumerator Start()
	{
		while (true)
		{
			yield return new WaitForSeconds(regenerationInterval);
			if (powerSource.GetPower() < regenerationThreshhold)
			{
				powerSource.SetPower(Mathf.Min(regenerationThreshhold, powerSource.GetPower() + regenerationAmount));
			}
		}
	}

	public void OnHover(HandTargetEventData eventData)
	{
		string format = Language.main.GetFormat("PowerCellStatus", Mathf.FloorToInt(powerSource.GetPower()), Mathf.FloorToInt(powerSource.GetMaxPower()));
		HandReticle.main.SetText(HandReticle.TextType.Hand, "RegenPowerCell", translate: true);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, format, translate: false);
	}
}
