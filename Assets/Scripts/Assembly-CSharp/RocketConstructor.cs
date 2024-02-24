using System.Collections.Generic;
using Story;
using UWE;
using UnityEngine;

public class RocketConstructor : MonoBehaviour
{
	[AssertNotNull]
	public CrafterLogic crafterLogic;

	[AssertNotNull]
	public Rocket rocket;

	public PlayerDistanceTracker playerDistanceTracker;

	public GameObject buildBotPrefab;

	public Transform[] buildbotSpawnPoints;

	private bool buildBotsSpawned;

	private List<GameObject> buildBots = new List<GameObject>();

	private GameObject buildTarget;

	public bool usingMenu
	{
		set
		{
			for (int i = 0; i < buildBots.Count; i++)
			{
				buildBots[i].GetComponent<ConstructorBuildBot>().usingMenu = value;
			}
		}
	}

	private void OnEnable()
	{
		if (!buildBotsSpawned)
		{
			SpawnBuildBots();
			buildBotsSpawned = true;
		}
	}

	private void OnDisable()
	{
		DestroyBuildBots();
	}

	private void SpawnBuildBots()
	{
		for (int i = 0; i < buildbotSpawnPoints.Length; i++)
		{
			GameObject gameObject = Object.Instantiate(buildBotPrefab);
			gameObject.transform.parent = buildbotSpawnPoints[i];
			gameObject.GetComponent<ConstructorBuildBot>().botId = i + 1;
			UWE.Utils.ZeroTransform(gameObject.transform);
			buildBots.Add(gameObject);
		}
	}

	private void DestroyBuildBots()
	{
		for (int i = 0; i < buildBots.Count; i++)
		{
			Object.Destroy(buildBots[i]);
		}
		buildBots.Clear();
		buildBotsSpawned = false;
	}

	private void Update()
	{
		bool flag = playerDistanceTracker.distanceToPlayer < 7f || buildTarget != null;
		for (int i = 0; i < buildBots.Count; i++)
		{
			buildBots[i].GetComponent<ConstructorBuildBot>().launch = flag || buildBots[i].transform.localPosition != Vector3.zero;
		}
	}

	public void OnConstructionDone(GameObject constructedObject)
	{
		for (int i = 0; i < buildBots.Count; i++)
		{
			buildBots[i].GetComponent<ConstructorBuildBot>().FinishConstruction();
			buildBots[i].transform.parent = buildbotSpawnPoints[i];
		}
		buildTarget = null;
	}

	private void SendBuildBots(GameObject toBuild)
	{
		buildTarget = toBuild;
		BuildBotPath[] componentsInChildren = buildTarget.GetComponentsInChildren<BuildBotPath>();
		if (componentsInChildren.Length != 0)
		{
			for (int i = 0; i < buildBots.Count; i++)
			{
				int num = i % componentsInChildren.Length;
				buildBots[i].GetComponent<ConstructorBuildBot>().SetPath(componentsInChildren[num], buildTarget);
			}
		}
		else
		{
			Debug.Log("found no build bot path for " + toBuild.name);
		}
	}

	public void StartRocketConstruction()
	{
		if (crafterLogic == null || crafterLogic.inProgress || !rocket.IsRocketReady() || rocket.IsFinished())
		{
			return;
		}
		TechType currentStageTech = rocket.GetCurrentStageTech();
		if (CrafterLogic.ConsumeResources(currentStageTech))
		{
			float craftTime = 5f;
			if (crafterLogic.Craft(currentStageTech, craftTime))
			{
				GameObject toBuild = rocket.StartRocketConstruction();
				ItemGoalTracker.OnConstruct(currentStageTech);
				SendBuildBots(toBuild);
			}
		}
	}
}
