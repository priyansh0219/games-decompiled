using UnityEngine;

public class uGUI_LegendBar : MonoBehaviour
{
	private struct Data
	{
		public string button;

		public string action;
	}

	private const ManagedUpdate.Queue queue = ManagedUpdate.Queue.LateUpdateAfterInput;

	[AssertNotNull]
	public CanvasGroup canvasGroup;

	[AssertNotNull]
	public uGUI_LegendItem[] items;

	private static bool isDirty = true;

	private static Data[] datas = new Data[4];

	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdateAfterInput, OnUpdate);
	}

	private void OnDisable()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.LateUpdateAfterInput, OnUpdate);
	}

	public static void ClearButtons()
	{
		isDirty = true;
		for (int i = 0; i < datas.Length; i++)
		{
			datas[i] = default(Data);
		}
	}

	public static void ChangeButton(int index, string button, string action)
	{
		if (index < datas.Length)
		{
			isDirty = true;
			datas[index] = new Data
			{
				button = button,
				action = action
			};
		}
	}

	private void OnUpdate()
	{
		if (GameInput.IsPrimaryDeviceGamepad())
		{
			canvasGroup.alpha = 1f;
			if (isDirty)
			{
				isDirty = false;
				for (int i = 0; i < items.Length; i++)
				{
					Data data = datas[i];
					uGUI_LegendItem obj = items[i];
					obj.textButton.text = data.button;
					obj.textAction.text = data.action;
				}
			}
		}
		else
		{
			canvasGroup.alpha = 0f;
		}
	}
}
