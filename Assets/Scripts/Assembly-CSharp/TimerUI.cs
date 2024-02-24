using UnityEngine;

public class TimerUI : MonoBehaviour
{
	public float timeLimit;

	public float timeLeft;

	public GUIText timerGUI;

	public GameObject enableOnExpire;

	public GameObject disableOnExpire;

	public bool disableInEditor;

	private bool countdownEnabled = true;

	private void Awake()
	{
		enableOnExpire.gameObject.SetActive(value: false);
		timeLeft = timeLimit;
	}

	private void Update()
	{
		_ = disableInEditor;
		if (Input.GetKeyDown(KeyCode.F7))
		{
			countdownEnabled = !countdownEnabled;
			timerGUI.gameObject.SetActive(countdownEnabled);
		}
		if (timeLeft < 0f)
		{
			if (!enableOnExpire.gameObject.activeInHierarchy)
			{
				enableOnExpire.gameObject.SetActive(value: true);
				iTween.FadeFrom(enableOnExpire.gameObject, iTween.Hash("alpha", 0, "time", 1.5f));
				if ((bool)disableOnExpire)
				{
					disableOnExpire.gameObject.SetActive(value: false);
				}
			}
		}
		else if (countdownEnabled)
		{
			timeLeft -= Time.deltaTime;
			int num = Mathf.FloorToInt(timeLeft / 60f);
			int num2 = Mathf.FloorToInt((int)timeLeft % 60);
			timerGUI.text = $"Demo time: {num:0}:{num2:00}";
		}
	}
}
