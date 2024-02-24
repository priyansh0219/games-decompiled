using System.Collections;

public interface ILoadButtonDelegate
{
	string rightSideGroup { get; }

	SaveLoadManager.GameInfo GetGameInfo(string slotName);

	IEnumerator LoadGameAsync(MainMenuLoadButton button);

	IEnumerator ClearSlotAsync(MainMenuLoadButton button);
}
