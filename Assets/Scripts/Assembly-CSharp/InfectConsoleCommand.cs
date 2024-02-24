using UWE;
using UnityEngine;

public class InfectConsoleCommand : MonoBehaviour
{
	private void Awake()
	{
		DevConsole.RegisterConsoleCommand(this, "infect");
		DevConsole.RegisterConsoleCommand(this, "cure");
	}

	private void OnConsoleCommand_infect(NotificationCenter.Notification n)
	{
		if (ParseFloat(n, 0, out var value))
		{
			float value2 = ((!ParseFloat(n, 1, out value2)) ? 1f : Mathf.Clamp01(value2));
			if (value > 0f)
			{
				SetInfection(value, value2);
				return;
			}
		}
		ErrorMessage.AddDebug("Usage: infect radius [amount]");
	}

	private void OnConsoleCommand_cure(NotificationCenter.Notification n)
	{
		if (ParseFloat(n, 0, out var value) && value > 0f)
		{
			SetInfection(value, 0f);
		}
		else
		{
			ErrorMessage.AddDebug("Usage: cure radius");
		}
	}

	private void SetInfection(float radius, float amount)
	{
		int num = UWE.Utils.OverlapSphereIntoSharedBuffer(Player.main.transform.position, radius);
		for (int i = 0; i < num; i++)
		{
			InfectedMixin componentInHierarchy = UWE.Utils.GetComponentInHierarchy<InfectedMixin>(UWE.Utils.sharedColliderBuffer[i].gameObject);
			if (componentInHierarchy != null && componentInHierarchy.SetInfectedAmount(amount))
			{
				ErrorMessage.AddDebug(componentInHierarchy.gameObject.name + " => " + amount);
			}
		}
	}

	private bool ParseFloat(NotificationCenter.Notification n, int index, out float value)
	{
		value = 0f;
		if (n == null)
		{
			return false;
		}
		if (n.data == null)
		{
			return false;
		}
		if (index < 0)
		{
			return false;
		}
		if (n.data.Count < index + 1)
		{
			return false;
		}
		string text = (string)n.data[index];
		if (float.TryParse(text, out var result))
		{
			value = result;
			return true;
		}
		ErrorMessage.AddDebug($"Can't parse '{text}' as float");
		return false;
	}
}
