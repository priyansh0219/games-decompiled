using System.Collections.Generic;

public class LoadingStage
{
	public const string root = "Root";

	public const string scenePrefix = "Scene";

	public const string saveFilesLoad = "SaveFilesLoad";

	public const string sceneMain = "SceneMain";

	public const string sceneEssentials = "SceneEssentials";

	public const string sceneCyclops = "SceneCyclops";

	public const string sceneEscapePod = "SceneEscapePod";

	public const string sceneAurora = "SceneAurora";

	public const string builder = "Builder";

	public const string worldMount = "WorldMount";

	public const string worldTiles = "WorldTiles";

	public const string batches = "Batches";

	public const string octrees = "Octrees";

	public const string terrain = "Terrain";

	public const string clipmap = "Clipmap";

	public const string updatingVisibility = "UpdatingVisibility";

	public const string entityCells = "EntityCells";

	public const string worldSettle = "WorldSettle";

	public const string equipment = "Equipment";

	private static readonly HashSet<string> common = new HashSet<string>
	{
		"SceneMain", "SceneEssentials", "SceneCyclops", "SceneEscapePod", "SceneAurora", "Builder", "WorldMount", "WorldTiles", "Batches", "Octrees",
		"Terrain", "Clipmap", "UpdatingVisibility", "EntityCells"
	};

	private static readonly Dictionary<string, float> durations = new Dictionary<string, float>
	{
		{ "SaveFilesLoad", 0.217f },
		{ "SceneMain", 2.664f },
		{ "SceneEssentials", 0.597f },
		{ "SceneCyclops", 2.07f },
		{ "SceneEscapePod", 1.738f },
		{ "SceneAurora", 1.061f },
		{ "Builder", 2.126f },
		{ "WorldMount", 3.648f },
		{ "WorldTiles", 1.856f },
		{ "Batches", 0.004f },
		{ "Octrees", 0f },
		{ "Terrain", 0f },
		{ "Clipmap", 0.29f },
		{ "UpdatingVisibility", 0.006f },
		{ "EntityCells", 6.673f },
		{ "WorldSettle", 0f },
		{ "Equipment", 1.764f }
	};

	public static float GetDuration(string stage)
	{
		if (!durations.TryGetValue(stage, out var value))
		{
			return 0f;
		}
		return value;
	}

	public static void FillStages(Dictionary<string, float> stages)
	{
		foreach (string item in common)
		{
			if (!stages.ContainsKey(item))
			{
				stages[item] = 0f;
			}
		}
		if (Utils.GetContinueMode())
		{
			if (!stages.ContainsKey("SaveFilesLoad"))
			{
				stages["SaveFilesLoad"] = 0f;
			}
			if (!stages.ContainsKey("WorldSettle"))
			{
				stages["WorldSettle"] = 0f;
			}
		}
		if (GameModeUtils.SpawnsInitialItems() && !stages.ContainsKey("Equipment"))
		{
			stages["Equipment"] = 0f;
		}
	}
}
