using UnityEngine;

public class PlayerAnimationEventManager : MonoBehaviour
{
	public void DeathCut()
	{
		uGUI_PlayerDeath.main.SendMessage("TriggerDeathVignette", uGUI_PlayerDeath.DeathTypes.CutToBlack, SendMessageOptions.RequireReceiver);
	}

	public void DeathFade()
	{
		uGUI_PlayerDeath.main.SendMessage("TriggerDeathVignette", uGUI_PlayerDeath.DeathTypes.FadeToBlack, SendMessageOptions.RequireReceiver);
	}
}
