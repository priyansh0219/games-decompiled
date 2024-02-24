using System.Collections;
using UWE;
using UnityEngine;

[RequireComponent(typeof(CrushDamage))]
[RequireComponent(typeof(ConditionRules))]
public class DepthAlarms : MonoBehaviour
{
	[AssertNotNull]
	public VoiceNotification crushDepthNotification;

	[AssertNotNull]
	public CrushDamage crushDamage;

	[AssertNotNull]
	public ConditionRules conditionRules;

	private IEnumerator Start()
	{
		yield return null;
		conditionRules.AddCondition(() => crushDamage.GetCanTakeCrushDamage() && crushDamage.GetDepth() > crushDamage.crushDepth).WhenBecomesTrue(delegate
		{
			crushDepthNotification.Play(crushDamage.crushDepth);
		});
	}
}
