using UnityEngine;

public class DisableWhenPiloting : MonoBehaviour
{
	public GameObject[] disableGameObjects;

	public bool _disable;

	public bool disable
	{
		set
		{
			if (value == _disable)
			{
				return;
			}
			for (int i = 0; i < disableGameObjects.Length; i++)
			{
				GameObject gameObject = disableGameObjects[i];
				if (!(gameObject == null))
				{
					gameObject.SetActive(!value);
				}
			}
			_disable = value;
		}
	}

	private void Start()
	{
		Utils.GetLocalPlayerComp().playerModeChanged.AddHandler(base.gameObject, OnPlayerModeChanged);
		disable = false;
	}

	private void OnPlayerModeChanged(Player.Mode mode)
	{
		disable = mode != Player.Mode.Normal;
	}
}
