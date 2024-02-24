using System.Collections.Generic;
using System.Text;
using UWE;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WaitScreen : MonoBehaviour
{
	public interface IWaitItem
	{
		string stage { get; }

		double pastSecs { get; }

		float progress { get; }
	}

	public abstract class WaitItemBase : IWaitItem
	{
		private readonly double startMS;

		public string stage { get; private set; }

		public double pastSecs => (UWE.Utils.GetSystemTime() - startMS) / 1000.0;

		public abstract float progress { get; }

		public WaitItemBase(string stage)
		{
			this.stage = stage;
			startMS = UWE.Utils.GetSystemTime();
		}
	}

	public class AsyncOperationItem : WaitItemBase
	{
		private readonly AsyncOperationHandle operation;

		public override float progress => operation.PercentComplete;

		public AsyncOperationItem(string stage, AsyncOperationHandle operation)
			: base(stage)
		{
			this.operation = operation;
		}
	}

	public class ManualWaitItem : WaitItemBase
	{
		private float _progress;

		public override float progress => _progress;

		public ManualWaitItem(string stage)
			: base(stage)
		{
		}

		public void SetProgress(float p)
		{
			_progress = p;
		}

		public void SetProgress(int i, int total)
		{
			_progress = (float)i / (float)total;
		}
	}

	private static WaitScreen main;

	private readonly List<IWaitItem> items = new List<IWaitItem>();

	private bool isWaiting;

	private readonly Dictionary<string, double> stageDurations = new Dictionary<string, double>();

	private readonly Dictionary<string, float> stageProgress = new Dictionary<string, float>();

	public static bool IsWaiting
	{
		get
		{
			if (main != null)
			{
				return main.isWaiting;
			}
			return false;
		}
	}

	private void Awake()
	{
		main = this;
	}

	private void Update()
	{
		if (isWaiting)
		{
			if (items.Count != 0)
			{
				foreach (IWaitItem item in items)
				{
					stageProgress[item.stage] = item.progress;
				}
				return;
			}
			isWaiting = false;
			stageProgress.Clear();
			FreezeTime.End(FreezeTime.Id.WaitScreen);
		}
		else if (items.Count > 0)
		{
			isWaiting = true;
			FreezeTime.Begin(FreezeTime.Id.WaitScreen);
		}
	}

	public static ManualWaitItem Add(string stage)
	{
		if (main == null)
		{
			return null;
		}
		ManualWaitItem manualWaitItem = new ManualWaitItem(stage);
		main.items.Add(manualWaitItem);
		return manualWaitItem;
	}

	public static AsyncOperationItem Add(string stage, AsyncOperationHandle operation)
	{
		if (main == null)
		{
			return null;
		}
		AsyncOperationItem asyncOperationItem = new AsyncOperationItem(stage, operation);
		main.items.Add(asyncOperationItem);
		return asyncOperationItem;
	}

	public static void Remove(IWaitItem item)
	{
		if (!(main == null))
		{
			main.items.Remove(item);
			main.stageProgress[item.stage] = 1f;
			main.stageDurations[item.stage] = item.pastSecs;
			Debug.LogFormat("'{0}' took {1:0.00} seconds", item.stage, item.pastSecs);
		}
	}

	public static float CalcProgress()
	{
		if (main == null)
		{
			return 0f;
		}
		Dictionary<string, float> dictionary = main.stageProgress;
		LoadingStage.FillStages(dictionary);
		float num = 0f;
		float num2 = 0f;
		foreach (KeyValuePair<string, float> item in dictionary)
		{
			float duration = LoadingStage.GetDuration(item.Key);
			num += item.Value * duration;
			num2 += duration;
		}
		if (!(num2 > 0f))
		{
			return 0f;
		}
		return Mathf.Clamp01(num / num2);
	}

	public static void ReportStageDurations()
	{
		if (main == null)
		{
			Debug.LogError("Attempt to call WaitScreen.ReportStageDurations(), but WaitScreen singleton is not initialized!");
			return;
		}
		Dictionary<string, double> dictionary = main.stageDurations;
		if (dictionary.Count == 0)
		{
			Debug.LogError("WaitScreen.stageDurations data is empty. Nothing to report!");
			return;
		}
		List<string> list = new List<string>
		{
			"SaveFilesLoad", "SceneMain", "SceneEssentials", "SceneCyclops", "SceneEscapePod", "SceneAurora", "Builder", "WorldMount", "WorldTiles", "Batches",
			"Octrees", "Terrain", "Clipmap", "UpdatingVisibility", "EntityCells", "WorldSettle", "Equipment"
		};
		using (GameAnalytics.EventData eventData = GameAnalytics.CustomEvent(GameAnalytics.Event.LoadingDurations))
		{
			foreach (string item in list)
			{
				if (dictionary.TryGetValue(item, out var value))
				{
					eventData.Add(item, value);
				}
			}
		}
		using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
		{
			StringBuilder sb = stringBuilderPool.sb;
			for (int i = 0; i < list.Count; i++)
			{
				string format = list[i];
				if (i > 0)
				{
					sb.Append(',');
				}
				sb.AppendFormat(format);
			}
			sb.Append('\n');
			for (int j = 0; j < list.Count; j++)
			{
				string key = list[j];
				if (j > 0)
				{
					sb.Append(',');
				}
				if (dictionary.TryGetValue(key, out var value2))
				{
					sb.AppendFormat("{0:0.000}", value2);
				}
			}
			sb.Insert(0, "Loading stage durations:\n");
			sb.Append('\n');
			Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, sb.ToString());
		}
		dictionary.Clear();
	}
}
