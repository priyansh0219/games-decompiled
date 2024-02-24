using UnityEngine;

public class RocketBuilderTooltip : MonoBehaviour, ITooltip
{
	private TechType rocketTechType = TechType.RocketBaseLadder;

	public bool showTooltipOnDrag => false;

	public void SetTooltipTech(int stage)
	{
		stage--;
		switch (stage)
		{
		case 1:
			rocketTechType = TechType.RocketStage1;
			break;
		case 2:
			rocketTechType = TechType.RocketStage2;
			break;
		case 3:
			rocketTechType = TechType.RocketStage3;
			break;
		}
	}

	void ITooltip.GetTooltip(TooltipData data)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, string.Empty, translate: false, GameInput.button0);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Interact);
		bool locked = !CrafterLogic.IsCraftRecipeUnlocked(rocketTechType);
		TooltipFactory.BuildTech(rocketTechType, locked, data);
	}
}
