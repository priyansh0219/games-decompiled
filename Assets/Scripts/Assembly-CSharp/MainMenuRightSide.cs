using System.Collections.Generic;
using UnityEngine;

public class MainMenuRightSide : MonoBehaviour
{
	public static MainMenuRightSide main;

	public List<MainMenuGroup> groups;

	public MainMenuGroup homeGroup;

	private string liveGroup;

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
		homeGroup.gameObject.SetActive(value: true);
	}

	public void OpenGroup(string target)
	{
		MainMenuGroup mainMenuGroup = null;
		foreach (MainMenuGroup group in groups)
		{
			if (group.gameObject.name == target)
			{
				group.gameObject.SetActive(value: true);
				if (group.ChangeLegendOnOpen)
				{
					group.SyncLegendToGroup();
				}
				mainMenuGroup = group;
			}
			else if (group.gameObject.activeSelf)
			{
				group.gameObject.SetActive(value: false);
			}
		}
		if (mainMenuGroup != null)
		{
			uGUI_MainMenu.main.OnRightSideOpened(mainMenuGroup.gameObject);
		}
	}

	public void HideRightSide()
	{
		foreach (MainMenuGroup group in groups)
		{
			if (group.gameObject.activeSelf)
			{
				liveGroup = group.gameObject.name;
				group.gameObject.SetActive(value: false);
			}
			else
			{
				group.gameObject.SetActive(value: false);
			}
		}
	}

	public void UnhideRightSide()
	{
		foreach (MainMenuGroup group in groups)
		{
			if (group.gameObject.name == liveGroup)
			{
				group.gameObject.SetActive(value: true);
			}
			else
			{
				group.gameObject.SetActive(value: false);
			}
		}
	}
}
