using Microsoft.Xbox;
using UnityEngine;
using UnityEngine.UI;

public class UnlockAchievementSampleLogic : MonoBehaviour
{
	public Text output;

	public void UnlockAchievement()
	{
		Gdk.Helpers.UnlockAchievement("1");
		output.text = "Unlocking achievement...";
	}
}
