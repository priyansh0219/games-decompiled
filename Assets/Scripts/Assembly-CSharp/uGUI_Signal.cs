using UnityEngine;

public class uGUI_Signal : MonoBehaviour
{
	private static readonly string[] slotNames = new string[2] { "Chip1", "Chip2" };

	private Signal[] equipppedSignals = new Signal[2];

	private void Start()
	{
		InvokeRepeating("CheckForSignals", 0f, 1f);
	}

	private void CheckForSignals()
	{
		Inventory main = Inventory.main;
		if (main == null)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			InventoryItem itemInSlot = main.equipment.GetItemInSlot(slotNames[i]);
			if (itemInSlot != null)
			{
				Signal component = itemInSlot.item.gameObject.GetComponent<Signal>();
				if (component != equipppedSignals[i])
				{
					Equip(i, component);
				}
			}
			else
			{
				Equip(i, null);
			}
		}
	}

	private void Equip(int i, Signal newSignal)
	{
		Signal signal = equipppedSignals[i];
		if (signal != null)
		{
			equipppedSignals[i] = null;
			signal.CleanUp();
		}
		if (newSignal != null)
		{
			equipppedSignals[i] = newSignal;
			newSignal.Initialize();
		}
	}
}
