using System.Collections;
using UnityEngine;

public class MainMenuLoadScroll : MonoBehaviour
{
	private IEnumerator Start()
	{
		yield return 0;
		string[] activeSlotNames = SaveLoadManager.main.GetActiveSlotNames();
		base.gameObject.SetActive(activeSlotNames.Length >= 5);
	}
}
