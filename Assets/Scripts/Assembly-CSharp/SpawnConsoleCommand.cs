using System.Collections;
using UWE;
using UnityEngine;

public class SpawnConsoleCommand : MonoBehaviour
{
	private void Awake()
	{
		DevConsole.RegisterConsoleCommand(this, "spawn");
	}

	private void OnConsoleCommand_spawn(NotificationCenter.Notification n)
	{
		if (n != null && n.data != null && n.data.Count > 0)
		{
			StartCoroutine(SpawnAsync(n));
		}
	}

	private IEnumerator SpawnAsync(NotificationCenter.Notification n)
	{
		string text = (string)n.data[0];
		if (UWE.Utils.TryParseEnum<TechType>(text, out var techType))
		{
			if (!CraftData.IsAllowed(techType))
			{
				yield break;
			}
			CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
			yield return request;
			GameObject result = request.GetResult();
			if (result != null)
			{
				int num = 1;
				if (n.data.Count > 1 && int.TryParse((string)n.data[1], out var result2))
				{
					num = result2;
				}
				float maxDist = 12f;
				if (n.data.Count > 2)
				{
					maxDist = float.Parse((string)n.data[2]);
				}
				Debug.LogFormat("Spawning {0} {1}", num, techType);
				for (int i = 0; i < num; i++)
				{
					GameObject obj = Utils.CreatePrefab(result, maxDist, i > 0);
					LargeWorldEntity.Register(obj);
					CrafterLogic.NotifyCraftEnd(obj, techType);
					obj.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
				}
			}
			else
			{
				ErrorMessage.AddDebug("Could not find prefab for TechType = " + techType);
			}
		}
		else
		{
			ErrorMessage.AddDebug("Could not parse " + text + " as TechType");
		}
	}
}
